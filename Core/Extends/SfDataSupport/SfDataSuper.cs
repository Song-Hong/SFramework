using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFramework.Core.Extends.SfDataSupport
{
    /// <summary>
    /// 指定 SFDS 文件的压缩算法。默认优先 Brotli，不可用时回退到 Deflate。
    /// </summary>
    public enum SfDataSuperCompression { Brotli = 1, Deflate = 2 }

    /// <summary>
    /// 表示写入/读取过程中的进度信息。
    /// </summary>
    public struct SfDataSuperProgress
    {
        /// <summary>已处理的字节数</summary>
        public long bytesProcessed;
        /// <summary>总字节数（若可用）</summary>
        public long totalBytes;
        /// <summary>整体百分比（0~1），若可用</summary>
        public float percent;
        /// <summary>当前阶段，例如 "compress" 或 "decompress"</summary>
        public string stage;
    }

    /// <summary>
    /// 控制 SFDS 的分块大小、压缩算法与恢复策略。
    /// </summary>
    public sealed class SfDataSuperOptions
    {
        /// <summary>分块大小（字节），默认 64MB</summary>
        public int chunkSizeBytes = 64 * 1024 * 1024;
        /// <summary>压缩算法</summary>
        public SfDataSuperCompression compression = SfDataSuperCompression.Brotli;
        /// <summary>分块校验失败时是否允许跳过继续恢复</summary>
        public bool allowRecovery = true;
    }

    /// <summary>
    /// 表示 SFDS 相关的错误。
    /// </summary>
    public sealed class SfDataSuperException : Exception
    {
        /// <summary>
        /// 构造异常。
        /// </summary>
        public SfDataSuperException(string message) : base(message) { }
    }

    internal sealed class Crc32
    {
        private readonly uint[] table;
        private uint crc = 0xFFFFFFFFu;
        public Crc32()
        {
            table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int k = 0; k < 8; k++) c = ((c & 1) != 0) ? (0xEDB88320u ^ (c >> 1)) : (c >> 1);
                table[i] = c;
            }
        }
        /// <summary>
        /// 增量更新校验。
        /// </summary>
        public void Update(byte[] buf, int offset, int count)
        {
            for (int i = 0; i < count; i++) crc = table[(crc ^ buf[offset + i]) & 0xFF] ^ (crc >> 8);
        }
        /// <summary>
        /// 获取最终校验值。
        /// </summary>
        public uint Digest() => crc ^ 0xFFFFFFFFu;
        /// <summary>
        /// 一次性计算校验值。
        /// </summary>
        public static uint Compute(byte[] buf, int offset, int count)
        {
            var c = new Crc32();
            c.Update(buf, offset, count);
            return c.Digest();
        }
    }

    /// <summary>
    /// 提供变长整型（Varint）与 ZigZag 编码，降低整数存储开销。
    /// </summary>
    internal static class Varint
    {
        /// <summary>
        /// 写入无符号 64 位 Varint。
        /// </summary>
        public static void WriteU64(Stream s, ulong v)
        {
            // 高位存在则持续写出低 7 位并置 1 标记继续
            while (v >= 0x80)
            {
                s.WriteByte((byte)(v | 0x80));
                v >>= 7;
            }
            s.WriteByte((byte)v);
        }
        /// <summary>
        /// 使用 ZigZag 编码写入有符号 64 位整数。
        /// </summary>
        public static void WriteS64(Stream s, long v)
        {
            ulong zz = (ulong)((v << 1) ^ (v >> 63));
            WriteU64(s, zz);
        }
        /// <summary>
        /// 读取无符号 64 位 Varint。
        /// </summary>
        public static ulong ReadU64(Stream s)
        {
            int shift = 0;
            ulong result = 0;
            while (true)
            {
                int b = s.ReadByte();
                if (b < 0) throw new EndOfStreamException();
                result |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }
        /// <summary>
        /// 读取 ZigZag 编码的有符号 64 位整数。
        /// </summary>
        public static long ReadS64(Stream s)
        {
            ulong zz = ReadU64(s);
            long v = (long)((zz >> 1) ^ (ulong)-(long)(zz & 1));
            return v;
        }
    }

    /// <summary>
    /// 描述每个压缩分块的元信息。
    /// </summary>
    internal sealed class ChunkDescriptor
    {
        public uint compLen;
        public uint rawLen;
        public uint crc;
    }

    /// <summary>
    /// SFDS 文件头，包含版本、压缩、分块与校验信息。
    /// </summary>
    internal sealed class Header
    {
        public const uint Magic = 0x53464453;
        public byte Version = 1;
        public byte Flags = 1;
        public byte Compression = 1;
        public uint ChunkSize;
        public uint ChunkCount;
        public ulong TotalRawLen;
        public ulong DescriptorOffset;
        public ulong DescriptorLength;
        public uint HeaderCrc;
    }

    /// <summary>
    /// 将原始字节流分块压缩并生成分块描述。
    /// </summary>
    internal sealed class ChunkedCompressor : IDisposable
    {
        private readonly Stream outStream;
        private readonly SfDataSuperOptions options;
        private readonly List<ChunkDescriptor> descriptors = new List<ChunkDescriptor>();
        private readonly byte[] buffer;
        private int bufPos;
        private ulong totalRaw;
        private readonly Action<SfDataSuperProgress> progress;

        public ChunkedCompressor(Stream outStream, SfDataSuperOptions options, Action<SfDataSuperProgress> progress)
        {
            this.outStream = outStream;
            this.options = options ?? new SfDataSuperOptions();
            buffer = new byte[this.options.chunkSizeBytes];
            bufPos = 0;
            progress ??= (_)=>{};
            this.progress = progress;
        }

        /// <summary>
        /// 写入原始数据，内部累积到分块缓冲并在满块时压缩输出。
        /// </summary>
        public void Write(byte[] data, int offset, int count)
        {
            int remaining = count;
            int idx = offset;
            while (remaining > 0)
            {
                int toCopy = Math.Min(remaining, buffer.Length - bufPos);
                Buffer.BlockCopy(data, idx, buffer, bufPos, toCopy);
                bufPos += toCopy;
                idx += toCopy;
                remaining -= toCopy;
                if (bufPos == buffer.Length) FlushChunk();
            }
        }

        /// <summary>
        /// 刷新剩余缓冲，输出最后一个分块。
        /// </summary>
        public void Flush()
        {
            if (bufPos > 0) FlushChunk();
        }

        private void FlushChunk()
        {
            // 计算当前块的 CRC 与原始长度
            var crc = Crc32.Compute(buffer, 0, bufPos);
            var rawLen = (uint)bufPos;
            long start = outStream.Position;
            // 创建压缩流并写入块数据
            using (var comp = CreateCompressionStream(outStream, options.compression, true))
            {
                comp.Write(buffer, 0, bufPos);
                comp.Flush();
            }
            // 通过位置差获取压缩后长度
            long compLen = outStream.Position - start;
            var desc = new ChunkDescriptor { compLen = (uint)compLen, rawLen = rawLen, crc = crc };
            descriptors.Add(desc);
            totalRaw += rawLen;
            bufPos = 0;
            progress(new SfDataSuperProgress { bytesProcessed = (long)totalRaw, totalBytes = (long)totalRaw, percent = 1f, stage = "compress" });
        }

        /// <summary>
        /// 创建压缩流，优先 Brotli，不可用时退回 Deflate。
        /// </summary>
        private static Stream CreateCompressionStream(Stream baseStream, SfDataSuperCompression compression, bool leaveOpen)
        {
            if (compression == SfDataSuperCompression.Brotli)
            {
                try
                {
                    return new BrotliStream(baseStream, CompressionLevel.Optimal, leaveOpen);
                }
                catch
                {
                    return new DeflateStream(baseStream, CompressionLevel.Optimal, leaveOpen);
                }
            }
            return new DeflateStream(baseStream, CompressionLevel.Optimal, leaveOpen);
        }

        public IReadOnlyList<ChunkDescriptor> Descriptors => descriptors;
        public ulong TotalRaw => totalRaw;
        public void Dispose() { }
    }

    /// <summary>
    /// 读取压缩分块并顺序解压为原始字节流，逐块校验。
    /// </summary>
    internal sealed class ChunkedDecompressor
    {
        private readonly Stream inStream;
        private readonly IReadOnlyList<ChunkDescriptor> descriptors;
        private readonly SfDataSuperOptions options;
        private readonly Action<SfDataSuperProgress> progress;

        public ChunkedDecompressor(Stream inStream, IReadOnlyList<ChunkDescriptor> descriptors, SfDataSuperOptions options, Action<SfDataSuperProgress> progress)
        {
            this.inStream = inStream;
            this.descriptors = descriptors;
            this.options = options ?? new SfDataSuperOptions();
            progress ??= (_)=>{};
            this.progress = progress;
        }

        /// <summary>
        /// 解压所有分块到内存流并返回。
        /// </summary>
        public Stream ReadAllToStream()
        {
            var ms = new MemoryStream();
            ulong processed = 0;
            foreach (var d in descriptors)
            {
                var compSlice = new Substream(inStream, inStream.Position, d.compLen);
                using (var ds = CreateDecompressionStream(compSlice))
                {
                    var buf = new byte[64 * 1024];
                    int n;
                    var crc = new Crc32();
                    while ((n = ds.Read(buf, 0, buf.Length)) > 0)
                    {
                        ms.Write(buf, 0, n);
                        crc.Update(buf, 0, n);
                        processed += (uint)n;
                        progress(new SfDataSuperProgress { bytesProcessed = (long)processed, totalBytes = (long)processed, percent = 1f, stage = "decompress" });
                    }
                    var dig = crc.Digest();
                    if (dig != d.crc)
                    {
                        if (!options.allowRecovery) throw new SfDataSuperException("CRC mismatch");
                    }
                }
                // Substream 已在读取过程中推进了基础流位置，无需再次移动
            }
            ms.Position = 0;
            return ms;
        }

        private Stream CreateDecompressionStream(Stream src)
        {
            if (options.compression == SfDataSuperCompression.Brotli)
            {
                try
                {
                    return new BrotliStream(src, CompressionMode.Decompress, true);
                }
                catch
                {
                    return new DeflateStream(src, CompressionMode.Decompress, true);
                }
            }
            return new DeflateStream(src, CompressionMode.Decompress, true);
        }
    }

    /// <summary>
    /// 在基础流上提供只读的子区段视图，用于单块解压。
    /// </summary>
    internal sealed class Substream : Stream
    {
        private readonly Stream baseStream;
        private readonly long start;
        private readonly long length;
        private long position;
        public Substream(Stream baseStream, long start, long length)
        {
            this.baseStream = baseStream;
            this.start = start;
            this.length = length;
            position = 0;
        }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => length;
        public override long Position { get => position; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count)
        {
            long remaining = length - position;
            if (remaining <= 0) return 0;
            int toRead = (int)Math.Min(count, remaining);
            baseStream.Position = start + position;
            int n = baseStream.Read(buffer, offset, toRead);
            position += n;
            return n;
        }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
    }

    /// <summary>
    /// 将编码器的写入直接导向分块压缩器，避免大内存缓冲。
    /// </summary>
    internal sealed class EncodingSinkStream : Stream
    {
        private readonly ChunkedCompressor compressor;
        public EncodingSinkStream(ChunkedCompressor compressor) { this.compressor = compressor; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { compressor.Flush(); }
        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { compressor.Write(buffer, offset, count); }
        public override void WriteByte(byte value) { compressor.Write(new[] { value }, 0, 1); }
    }

    /// <summary>
    /// 在紧凑二进制与 SfData 树之间进行编码/解码。
    /// </summary>
    internal static class BinaryCodec
    {
        /// <summary>
        /// 将 <paramref name="node"/> 编码到二进制流。
        /// </summary>
        public static void Encode(SfData node, Stream s)
        {
            switch (node.Type)
            {
                case SfDataType.String:
                    // 标记 0x01 + UTF8 长度 + 数据
                    s.WriteByte(0x01);
                    var str = node.Value?.ToString() ?? string.Empty;
                    var bytes = Encoding.UTF8.GetBytes(str);
                    Varint.WriteU64(s, (ulong)bytes.Length);
                    s.Write(bytes, 0, bytes.Length);
                    break;
                case SfDataType.Int:
                    // 标记 0x02 + ZigZag Varint
                    s.WriteByte(0x02);
                    Varint.WriteS64(s, (int)node.Value);
                    break;
                case SfDataType.Double:
                    // 标记 0x03 + 8 字节 IEEE754
                    s.WriteByte(0x03);
                    var dbl = BitConverter.GetBytes((double)node.Value);
                    s.Write(dbl, 0, dbl.Length);
                    break;
                case SfDataType.Boolean:
                    // 标记 0x04 + 1 字节（0/1）
                    s.WriteByte(0x04);
                    s.WriteByte(((bool)node.Value) ? (byte)1 : (byte)0);
                    break;
                case SfDataType.Array:
                    // 标记 0x11 + 元素数 + 逐元素编码
                    s.WriteByte(0x11);
                    Varint.WriteU64(s, (ulong)node.ArrayList.Count);
                    for (int i = 0; i < node.ArrayList.Count; i++) Encode(node.ArrayList[i], s);
                    break;
                case SfDataType.Object:
                    // 标记 0x10 + 键数量 + [键Len+键Bytes+值] * N
                    s.WriteByte(0x10);
                    ulong c = (ulong)node.Count;
                    Varint.WriteU64(s, c);
                    foreach (var k in node.Keys)
                    {
                        var kb = Encoding.UTF8.GetBytes(k);
                        Varint.WriteU64(s, (ulong)kb.Length);
                        s.Write(kb, 0, kb.Length);
                        Encode(node[k], s);
                    }
                    break;
                default:
                    s.WriteByte(0x00);
                    break;
            }
        }

        /// <summary>
        /// 从二进制流解码为 <see cref="SfData"/>。
        /// </summary>
        /// <summary>
        /// 从二进制流解码为 <see cref="SfData"/>。
        /// </summary>
        public static SfData Decode(Stream s)
        {
            int m = s.ReadByte();
            if (m < 0) throw new EndOfStreamException();
            switch (m)
            {
                case 0x00:
                    return new SfData();
                case 0x01:
                {
                    ulong len = Varint.ReadU64(s);
                    var buf = new byte[len];
                    ReadExact(s, buf, 0, (int)len);
                    var str = Encoding.UTF8.GetString(buf);
                    return new SfData(str);
                }
                case 0x02:
                {
                    long v = Varint.ReadS64(s);
                    return new SfData((int)v);
                }
                case 0x03:
                {
                    var buf = new byte[8];
                    ReadExact(s, buf, 0, 8);
                    double d = BitConverter.ToDouble(buf, 0);
                    return new SfData(d);
                }
                case 0x04:
                {
                    int b = s.ReadByte();
                    if (b < 0) throw new EndOfStreamException();
                    return new SfData(b != 0);
                }
                case 0x11:
                {
                    ulong n = Varint.ReadU64(s);
                    var arr = new SfData();
                    for (ulong i = 0; i < n; i++) arr.Add(Decode(s));
                    return arr;
                }
                case 0x10:
                {
                    ulong n = Varint.ReadU64(s);
                    var obj = new SfData();
                    for (ulong i = 0; i < n; i++)
                    {
                        ulong klen = Varint.ReadU64(s);
                        var kbuf = new byte[klen];
                        ReadExact(s, kbuf, 0, (int)klen);
                        var key = Encoding.UTF8.GetString(kbuf);
                        obj[key] = Decode(s);
                    }
                    return obj;
                }
                default:
                    throw new SfDataSuperException("Invalid marker");
            }
        }

        private static void ReadExact(Stream s, byte[] buffer, int offset, int count)
        {
            int read = 0;
            while (read < count)
            {
                int n = s.Read(buffer, offset + read, count - read);
                if (n <= 0) throw new EndOfStreamException();
                read += n;
            }
        }
    }

    /// <summary>
    /// 面向文件与流的 SFDS 读写接口，支持同步与异步。
    /// </summary>
    public static class SfDataSuper
    {
        /// <summary>
        /// 保存为 .sfds 文件。
        /// </summary>
        /// <param name="path">目标文件路径。</param>
        /// <param name="data">待保存的数据树。</param>
        /// <param name="onProgress">进度回调（可选）。</param>
        /// <param name="options">写入选项（可选）。</param>
        public static void SaveToFile(string path, SfData data, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            saveToFile(path, data, onProgress, options);
        }
        /// <summary>
        /// 异步保存为 .sfds 文件。
        /// </summary>
        public static Task SaveToFileAsync(string path, SfData data, IProgress<SfDataSuperProgress> progress = null, SfDataSuperOptions options = null, CancellationToken ct = default)
        {
            return saveToFileAsync(path, data, progress, options, ct);
        }
        /// <summary>
        /// 从 .sfds 文件加载。
        /// </summary>
        public static SfData LoadFromFile(string path, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            return loadFromFile(path, onProgress, options);
        }
        /// <summary>
        /// 异步从 .sfds 文件加载。
        /// </summary>
        public static Task<SfData> LoadFromFileAsync(string path, IProgress<SfDataSuperProgress> progress = null, SfDataSuperOptions options = null, CancellationToken ct = default)
        {
            return loadFromFileAsync(path, progress, options, ct);
        }
        /// <summary>
        /// 将数据写入可寻址流，自动分块与压缩。
        /// </summary>
        public static void StreamWrite(Stream stream, SfData data, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            streamWrite(stream, data, onProgress, options);
        }
        /// <summary>
        /// 从流读取并解码为 <see cref="SfData"/>。
        /// </summary>
        public static SfData StreamRead(Stream stream, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            return streamRead(stream, onProgress, options);
        }

        /// <summary>
        /// 保存为 .sfds 文件。
        /// </summary>
        /// <param name="path">目标文件路径。</param>
        /// <param name="data">待保存的数据树。</param>
        /// <param name="onProgress">进度回调（可选）。</param>
        /// <param name="options">写入选项（可选）。</param>
        public static void saveToFile(string path, SfData data, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            options ??= new SfDataSuperOptions();
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            streamWrite(fs, data, onProgress, options);
        }

        /// <summary>
        /// 异步保存为 .sfds 文件。
        /// </summary>
        /// <param name="path">目标文件路径。</param>
        /// <param name="data">待保存的数据树。</param>
        /// <param name="progress">进度报告接口（可选）。</param>
        /// <param name="options">写入选项（可选）。</param>
        /// <param name="ct">取消令牌（可选）。</param>
        public static async Task saveToFileAsync(string path, SfData data, IProgress<SfDataSuperProgress> progress = null, SfDataSuperOptions options = null, CancellationToken ct = default)
        {
            await Task.Run(() =>
            {
                Action<SfDataSuperProgress> cb = progress != null ? p => progress.Report(p) : (Action<SfDataSuperProgress>)null;
                saveToFile(path, data, cb, options);
            }, ct);
        }

        /// <summary>
        /// 从 .sfds 文件加载。
        /// </summary>
        public static SfData loadFromFile(string path, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            options ??= new SfDataSuperOptions();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return streamRead(fs, onProgress, options);
        }

        /// <summary>
        /// 异步从 .sfds 文件加载。
        /// </summary>
        public static async Task<SfData> loadFromFileAsync(string path, IProgress<SfDataSuperProgress> progress = null, SfDataSuperOptions options = null, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                Action<SfDataSuperProgress> cb = progress != null ? p => progress.Report(p) : (Action<SfDataSuperProgress>)null;
                return loadFromFile(path, cb, options);
            }, ct);
        }

        /// <summary>
        /// 将数据写入可寻址流，自动分块与压缩。
        /// </summary>
        public static void streamWrite(Stream stream, SfData data, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            options ??= new SfDataSuperOptions();
            onProgress ??= (_)=>{};
            if (!stream.CanSeek) throw new SfDataSuperException("Stream must be seekable");

            var headerPos = stream.Position;
            WriteEmptyHeader(stream);

            var compressor = new ChunkedCompressor(stream, options, onProgress);
            using (var sink = new EncodingSinkStream(compressor))
            {
                BinaryCodec.Encode(data, sink);
                sink.Flush();
            }

            var descOffset = stream.Position;
            var descLen = WriteDescriptors(stream, compressor.Descriptors);

            var hdr = new Header
            {
                Compression = (byte)options.compression,
                ChunkSize = (uint)options.chunkSizeBytes,
                ChunkCount = (uint)compressor.Descriptors.Count,
                TotalRawLen = compressor.TotalRaw,
                DescriptorOffset = (ulong)descOffset,
                DescriptorLength = (ulong)descLen
            };

            stream.Position = headerPos;
            WriteHeader(stream, hdr);
        }

        /// <summary>
        /// 异步写入到流。
        /// </summary>
        public static async Task streamWriteAsync(Stream stream, SfData data, IProgress<SfDataSuperProgress> progress = null, SfDataSuperOptions options = null, CancellationToken ct = default)
        {
            await Task.Run(() =>
            {
                Action<SfDataSuperProgress> cb = progress != null ? p => progress.Report(p) : (Action<SfDataSuperProgress>)null;
                streamWrite(stream, data, cb, options);
            }, ct);
        }

        /// <summary>
        /// 从流读取并解码为 <see cref="SfData"/>。
        /// </summary>
        public static SfData streamRead(Stream stream, Action<SfDataSuperProgress> onProgress = null, SfDataSuperOptions options = null)
        {
            options ??= new SfDataSuperOptions();
            onProgress ??= (_)=>{};
            var hdr = ReadHeader(stream);
            if (hdr.Version != 1) throw new SfDataSuperException("Version mismatch");
            var descriptors = ReadDescriptors(stream, hdr);
            var decomp = new ChunkedDecompressor(stream, descriptors, options, onProgress);
            using var raw = decomp.ReadAllToStream();
            return BinaryCodec.Decode(raw);
        }

        /// <summary>
        /// 异步从流读取。
        /// </summary>
        public static async Task<SfData> streamReadAsync(Stream stream, IProgress<SfDataSuperProgress> progress = null, SfDataSuperOptions options = null, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                Action<SfDataSuperProgress> cb = progress != null ? p => progress.Report(p) : (Action<SfDataSuperProgress>)null;
                return streamRead(stream, cb, options);
            }, ct);
        }

        /// <summary>
        /// 写入占位头，后续回填真实值与 CRC。
        /// </summary>
        private static void WriteEmptyHeader(Stream s)
        {
            var bw = new BinaryWriter(s, Encoding.UTF8, true);
            bw.Write(Header.Magic);
            bw.Write((byte)1);
            bw.Write((byte)1);
            bw.Write((byte)1);
            bw.Write((uint)0);
            bw.Write((uint)0);
            bw.Write((ulong)0);
            bw.Write((ulong)0);
            bw.Write((ulong)0);
            bw.Write((uint)0);
        }

        /// <summary>
        /// 按顺序写入头部并计算 CRC 校验。
        /// </summary>
        /// <param name="s">可寻址流，用于写入头部。</param>
        /// <param name="h">要写入的头部结构体。</param>
        private static void WriteHeader(Stream s, Header h)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms, Encoding.UTF8, true);
            bw.Write(Header.Magic);
            bw.Write(h.Version);
            bw.Write(h.Flags);
            bw.Write(h.Compression);
            bw.Write(h.ChunkSize);
            bw.Write(h.ChunkCount);
            bw.Write(h.TotalRawLen);
            bw.Write(h.DescriptorOffset);
            bw.Write(h.DescriptorLength);
            // 预留 CRC 字段位置（写 0）
            bw.Write((uint)0);
            bw.Flush();
            var buf = ms.ToArray();
            // 对除最后 4 字节外的头部计算 CRC32
            var crc = Crc32.Compute(buf, 0, buf.Length - 4);
            var crcBytes = BitConverter.GetBytes(crc);
            Buffer.BlockCopy(crcBytes, 0, buf, buf.Length - 4, 4);
            s.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// 读取头部为结构体（不在此处校验 CRC）。
        /// </summary>
        private static Header ReadHeader(Stream s)
        {
            var br = new BinaryReader(s, Encoding.UTF8, true);
            var h = new Header();
            uint magic = br.ReadUInt32();
            if (magic != Header.Magic) throw new SfDataSuperException("Bad magic");
            h.Version = br.ReadByte();
            h.Flags = br.ReadByte();
            h.Compression = br.ReadByte();
            h.ChunkSize = br.ReadUInt32();
            h.ChunkCount = br.ReadUInt32();
            h.TotalRawLen = br.ReadUInt64();
            h.DescriptorOffset = br.ReadUInt64();
            h.DescriptorLength = br.ReadUInt64();
            h.HeaderCrc = br.ReadUInt32();
            return h;
        }

        /// <summary>
        /// 写入分块描述表，返回写入长度。
        /// </summary>
        private static long WriteDescriptors(Stream s, IReadOnlyList<ChunkDescriptor> descs)
        {
            var bw = new BinaryWriter(s, Encoding.UTF8, true);
            long start = s.Position;
            bw.Write((uint)descs.Count);
            for (int i = 0; i < descs.Count; i++)
            {
                bw.Write(descs[i].compLen);
                bw.Write(descs[i].rawLen);
                bw.Write(descs[i].crc);
            }
            return s.Position - start;
        }

        /// <summary>
        /// 读取分块描述表并恢复读取位置。
        /// </summary>
        private static List<ChunkDescriptor> ReadDescriptors(Stream s, Header h)
        {
            long pos = s.Position;
            s.Position = (long)h.DescriptorOffset;
            var br = new BinaryReader(s, Encoding.UTF8, true);
            var count = br.ReadUInt32();
            var list = new List<ChunkDescriptor>((int)count);
            for (int i = 0; i < count; i++)
            {
                var d = new ChunkDescriptor();
                d.compLen = br.ReadUInt32();
                d.rawLen = br.ReadUInt32();
                d.crc = br.ReadUInt32();
                list.Add(d);
            }
            s.Position = pos;
            return list;
        }
    }
}
