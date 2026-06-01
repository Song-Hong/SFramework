/*
   ┌────────┬─────┬──────┬──────┬───────┬───────┬──────┬──────┬────────┐
   │  conv  │ cmd │ frg  │  wnd │  ts   │  sn   │  una │ len  │  data  │
   │ 会话ID  │命令 │ 分片号│ 窗口  │时间戳  │序列号  │ 未确  │长度  │ 数据    │
   │ 4字节   │1字节│ 1字节 │ 2字节 │4字节   │4字节  │4字节  │4字节 │        │ 
   └────────┴─────┴──────┴──────┴───────┴───────┴──────┴──────┴────────┘
   conv：会话ID，收发两端必须一致，用来识别"这个包属于哪个连接"
   cmd：包类型，有4种（数据包 / ACK / 窗口探测 / 窗口通知）
   frg：分片编号，大数据被拆成多片时用，0表示最后一片
   wnd：发送方的接收窗口大小，告诉对方"我还能收多少"
   ts：发送时间戳，用来计算RTT
   sn：序列号，每个包唯一递增
   una：未确认序号，"我这边 una 之前的包都收到了"
   len + data：有效载荷(实际数据)
 */
using System;

namespace SFramework.SFNet.Module.Kcp
{
    /// <summary>
    /// KCP 命令类型常量
    /// 对应 ikcp.c 中的 IKCP_CMD_* 定义
    /// </summary>
    public static class SfKcpCmd
    {
        public const byte Push = 81; // 数据包：携带实际payload
        public const byte Ack  = 82; // 确认包：告知对方已收到哪些包
        public const byte Wask = 83; // 窗口探测：问对方"你的接收窗口还剩多少"
        public const byte Wins = 84; // 窗口通知：回答对方"我的接收窗口大小"
    }

    /// <summary>
    /// KCP 数据段
    /// 
    /// 网络包结构（24字节固定头 + 可变data）：
    /// ┌────────┬─────┬──────┬──────┬───────┬───────┬──────┬──────┬──────────┐
    /// │  conv  │ cmd │ frg  │  wnd │  ts   │  sn   │  una │  len │  data... │
    /// │ 4字节  │1字节│1字节 │2字节 │4字节  │4字节  │4字节 │4字节 │          │
    /// └────────┴─────┴──────┴──────┴───────┴───────┴──────┴──────┴──────────┘
    /// 
    /// 注意：resendts / rto / fastack / xmit 是本地控制字段，不进入网络包
    /// </summary>
    public class SfKcpSegment
    {
        // ── 网络头部字段 ────────────────────────────────────────────

        /// <summary>会话ID，收发两端必须一致，用于区分不同连接</summary>
        public uint Conv;

        /// <summary>命令类型，取值见 KcpCmd</summary>
        public byte Cmd;

        /// <summary>
        /// 分片编号（倒序）
        /// 大数据被拆成多片时使用，frg=0 表示最后一片（或未分片）
        /// </summary>
        public byte Frg;

        /// <summary>接收窗口大小，告诉对方"我还能接收多少个包"</summary>
        public ushort Wnd;

        /// <summary>发送时间戳（毫秒），用于RTT计算</summary>
        public uint Ts;

        /// <summary>序列号，每发一个Push包递增，用于排序和去重</summary>
        public uint Sn;

        /// <summary>
        /// 未确认序号（unacknowledged）
        /// 表示"我这边 una 之前的所有包都已收到"，对方可据此清理发送缓冲
        /// </summary>
        public uint Una;

        /// <summary>data 字段的字节长度</summary>
        public uint Len => (uint)(Data?.Length ?? 0);

        /// <summary>payload 数据</summary>
        public byte[] Data;

        /// <summary>从 UDP 包头部读取 conv（会话 ID）</summary>
        public static uint GetConv(byte[] buffer, int offset = 0)
        {
            if (buffer == null || buffer.Length - offset < 4) return 0;
            return ReadUInt32(buffer, offset);
        }

        /// <summary>重置段，便于对象池复用</summary>
        public void Reset()
        {
            Conv = 0;
            Cmd = 0;
            Frg = 0;
            Wnd = 0;
            Ts = 0;
            Sn = 0;
            Una = 0;
            Data = Array.Empty<byte>();
            Resendts = 0;
            Rto = 0;
            FastAck = 0;
            Xmit = 0;
        }

        // ── 本地控制字段（不序列化进网络包）──────────────────────────

        /// <summary>下次触发重传的时间戳（毫秒）</summary>
        public uint Resendts;

        /// <summary>当前 RTO（重传超时时间，毫秒），会随网络状况动态调整</summary>
        public uint Rto;

        /// <summary>
        /// 被后续包"跳过"的次数
        /// 当这个值超过阈值（默认2），触发快速重传，不等 RTO 超时
        /// </summary>
        public uint FastAck;

        /// <summary>已发送次数，超过上限时报告错误（网络太差）</summary>
        public uint Xmit;

        // ── 常量 ────────────────────────────────────────────────────

        /// <summary>固定头部大小（字节）</summary>
        public const int HeaderSize = 24;

        // ── 编码（序列化进字节流）───────────────────────────────────

        /// <summary>
        /// 将当前 Segment 编码写入 buffer
        /// </summary>
        /// <param name="buffer">目标缓冲区</param>
        /// <param name="offset">从哪个位置开始写</param>
        /// <returns>实际写入的字节数（头部 + data）</returns>
        public int Encode(byte[] buffer, int offset)
        {
            int start = offset;

            offset += WriteUInt32(buffer, offset, Conv);
            buffer[offset++] = Cmd;
            buffer[offset++] = Frg;
            offset += WriteUInt16(buffer, offset, Wnd);
            offset += WriteUInt32(buffer, offset, Ts);
            offset += WriteUInt32(buffer, offset, Sn);
            offset += WriteUInt32(buffer, offset, Una);
            offset += WriteUInt32(buffer, offset, Len);

            if (Data is not { Length: > 0 }) return offset - start;
            Buffer.BlockCopy(Data, 0, buffer, offset, Data.Length);
            offset += Data.Length;

            return offset - start;
        }

        // ── 解码（从字节流反序列化）─────────────────────────────────

        /// <summary>
        /// 从 buffer 的指定位置解码出一个 Segment
        /// </summary>
        /// <param name="buffer">来源缓冲区</param>
        /// <param name="offset">从哪个位置开始读</param>
        /// <returns>解码出的 Segment，以及消费的字节数</returns>
        public static (SfKcpSegment seg, int consumed) Decode(byte[] buffer, int offset)
        {
            // 至少需要一个完整头部
            if (buffer.Length - offset < HeaderSize)
                return (null, 0);

            var seg = new SfKcpSegment
            {
                Conv = ReadUInt32(buffer, offset)
            };

            offset += 4;
            seg.Cmd         = buffer[offset++];
            seg.Frg         = buffer[offset++];
            seg.Wnd         = ReadUInt16(buffer, offset); offset += 2;
            seg.Ts          = ReadUInt32(buffer, offset); offset += 4;
            seg.Sn          = ReadUInt32(buffer, offset); offset += 4;
            seg.Una         = ReadUInt32(buffer, offset); offset += 4;
            uint dataLen    = ReadUInt32(buffer, offset); offset += 4;

            // 检查 data 部分是否完整
            if (buffer.Length - offset < (int)dataLen)
                return (null, 0);

            if (dataLen > 0)
            {
                seg.Data = new byte[dataLen];
                Buffer.BlockCopy(buffer, offset, seg.Data, 0, (int)dataLen);
            }
            else
            {
                seg.Data = Array.Empty<byte>();
            }

            return (seg, HeaderSize + (int)dataLen);
        }

        // ── 字节序工具（小端，与 ikcp.c 一致）──────────────────────

        private static int WriteUInt16(byte[] buf, int offset, ushort v)
        {
            buf[offset]     = (byte)(v);
            buf[offset + 1] = (byte)(v >> 8);
            return 2;
        }

        private static int WriteUInt32(byte[] buf, int offset, uint v)
        {
            buf[offset]     = (byte)(v);
            buf[offset + 1] = (byte)(v >> 8);
            buf[offset + 2] = (byte)(v >> 16);
            buf[offset + 3] = (byte)(v >> 24);
            return 4;
        }

        private static ushort ReadUInt16(byte[] buf, int offset) =>
            (ushort)(buf[offset] | (buf[offset + 1] << 8));

        private static uint ReadUInt32(byte[] buf, int offset) =>
            (uint)(buf[offset] | (buf[offset + 1] << 8) |
                   (buf[offset + 2] << 16) | (buf[offset + 3] << 24));
    }
}