using System;
using System.IO;
using UnityEngine;

namespace SFramework.SFIo.Module
{
    /// <summary>
    /// WAV 音频文件支持类
    /// </summary>
    public static class SfWav
    {
        // WAV 文件头的最小大小 (来自 SavWav)
        private const int HeaderSize = 44;

        /// <summary>
        /// 加载 WAV 音频文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>加载的音频文件, 失败则返回 null</returns>
        public static AudioClip Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("WAV file not found at path: " + path);
                return null;
            }

            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                using (var reader = new BinaryReader(fileStream))
                {
                    // --- 读取并验证文件头 ---
                    
                    // RIFF 块
                    string riff = new string(reader.ReadChars(4));
                    if (riff != "RIFF") throw new FormatException("Invalid WAV file: Missing RIFF header.");
                    
                    /* int fileSize = */ reader.ReadInt32(); // 文件总大小 (我们不太需要)
                    
                    string wave = new string(reader.ReadChars(4));
                    if (wave != "WAVE") throw new FormatException("Invalid WAV file: Missing WAVE header.");

                    // Format 块
                    string fmt = new string(reader.ReadChars(4));
                    if (fmt != "fmt ") throw new FormatException("Invalid WAV file: Missing 'fmt ' chunk.");

                    int fmtSize = reader.ReadInt32();
                    if (fmtSize < 16) throw new FormatException("Unsupported WAV format: 'fmt ' chunk size is too small.");

                    short audioFormat = reader.ReadInt16();
                    if (audioFormat != 1) throw new FormatException("Unsupported WAV format: Only PCM (Format 1) is supported.");

                    short channels = reader.ReadInt16();
                    int frequency = reader.ReadInt32();
                    /* int byteRate = */ reader.ReadInt32();
                    /* short blockAlign = */ reader.ReadInt16();
                    short bitsPerSample = reader.ReadInt16();

                    if (bitsPerSample != 16)
                    {
                        throw new FormatException($"Unsupported WAV format: Only 16-bit PCM is supported (file has {bitsPerSample}-bit).");
                    }

                    // 跳过 'fmt ' 块中可能存在的额外参数
                    if (fmtSize > 16)
                    {
                        reader.ReadBytes(fmtSize - 16);
                    }

                    // --- 寻找 'data' 块 ---
                    // 文件头中可能包含 "LIST", "INFO" 等其他块
                    var dataChunkId = new string(reader.ReadChars(4));
                    while (dataChunkId != "data" && fileStream.Position < fileStream.Length)
                    {
                        var junkChunkSize = reader.ReadInt32();
                        reader.ReadBytes(junkChunkSize); // 跳过这个块
                        dataChunkId = new string(reader.ReadChars(4));
                    }

                    if (dataChunkId != "data")
                    {
                        throw new FormatException("Invalid WAV file: 'data' chunk not found.");
                    }

                    var dataSize = reader.ReadInt32(); // 'data' 块的大小 (字节)
                    
                    // --- 读取音频数据 ---
                    var byteData = reader.ReadBytes(dataSize);
                    
                    // --- 转换数据 ---
                    var sampleCount = dataSize / 2; // 16位 = 2字节/样本
                    var floatData = new float[sampleCount];
                    
                    // 转换因子 (1 / short.MaxValue)
                    const float rescaleFactor = 1.0f / 32767.0f;

                    for (var i = 0; i < sampleCount; i++)
                    {
                        // 从字节数组中读取 16 位有符号整数 (小端)
                        var intSample = BitConverter.ToInt16(byteData, i * 2);
                        // 转换为 float [-1, 1]
                        floatData[i] = intSample * rescaleFactor;
                    }
                    
                    // --- 创建 AudioClip ---
                    int totalSamplesPerChannel = sampleCount / channels;
                    AudioClip clip = AudioClip.Create(Path.GetFileNameWithoutExtension(path), totalSamplesPerChannel, channels, frequency, false);
                    clip.SetData(floatData, 0);
                    
                    return clip;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load WAV file at '{path}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 保存音频文件为 WAV 格式
        /// </summary>
        /// <param name="clip">要保存的 AudioClip</param>
        /// <param name="path">要保存的完整文件路径</param>
        /// <returns>是否保存成功</returns>
        public static bool Save(AudioClip clip, string path)
        {
            if (clip == null)
            {
                Debug.LogError("AudioClip is null. Cannot save.");
                return false;
            }

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Save path is null or empty.");
                return false;
            }

            // 确保文件名以 .wav 结尾
            if (!path.ToLower().EndsWith(".wav"))
            {
                path += ".wav";
            }

            Debug.Log("正在保存 WAV 到: " + path);
            
            // 确保目录存在
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("创建目录失败: " + ex.Message);
                return false;
            }


            // 1. 从 AudioClip 获取数据
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            try
            {
                // 使用 using 确保文件流被正确关闭
                using (var fileStream = new FileStream(path, FileMode.Create))
                // 使用 BinaryWriter 简化写入
                using (var writer = new BinaryWriter(fileStream))
                {
                    // 2. 写入 WAV 文件头
                    WriteHeader(writer, clip);

                    // 3. 写入音频数据 (将 float 转换为 Int16)
                    ConvertAndWrite(writer, samples);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("保存 AudioClip 为 WAV 文件失败: " + ex.Message);
                // 如果失败，最好删除不完整的文件
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// 将 AudioClip 转换为完整的 WAV 文件字节数组（在内存中）
        /// </summary>
        public static byte[] ToBytes(AudioClip clip)
        {
            // 1. 获取 PCM 数据
            int numSamples = clip.samples * clip.channels;
            float[] sampleData = new float[numSamples];
            clip.GetData(sampleData, 0);
    
            // 转换 float 为 Int16 bytes (Raw PCM)
            byte[] rawPcmData = ConvertFloatToByte(sampleData);

            // 2. 在内存中构建 WAV 文件
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                // 常量
                int headerSize = 44;
                int bitsPerSample = 16;
        
                // --- 写入 WAV 头 (逻辑同 WavSaver) ---
        
                // RIFF Chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(headerSize - 8 + rawPcmData.Length); // ChunkSize
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt Chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1Size
                writer.Write((short)1); // AudioFormat (PCM)
                writer.Write((short)clip.channels);
                writer.Write(clip.frequency);
        
                // ByteRate
                writer.Write(clip.frequency * clip.channels * (bitsPerSample / 8));
                // BlockAlign
                writer.Write((short)(clip.channels * (bitsPerSample / 8)));
                writer.Write((short)bitsPerSample);

                // data Chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(rawPcmData.Length);

                // --- 写入 PCM 数据 ---
                writer.Write(rawPcmData);

                // 返回完整的字节数组
                return memoryStream.ToArray();
            }
        }
        
        /// <summary>
        /// 辅助方法：将浮点数数组转换为 16 位 PCM 格式的字节数组。
        /// (与上一个回答中的方法相同)
        /// </summary>
        private static byte[] ConvertFloatToByte(float[] floatArray)
        {
            int numBytes = floatArray.Length * 2;
            byte[] byteArray = new byte[numBytes];
        
            for (int i = 0; i < floatArray.Length; i++)
            {
                // 缩放到 Int16 范围
                short int16 = (short)(floatArray[i] * short.MaxValue);
            
                // 将 Int16 转换为两个字节 (Little-Endian)
                byte[] bytes = BitConverter.GetBytes(int16);

                // 写入字节数组
                byteArray[i * 2] = bytes[0];
                byteArray[i * 2 + 1] = bytes[1];
            }

            return byteArray;
        }
        #region WAV 写入辅助方法 (来自 SavWav)

        // (重构为使用 BinaryWriter)
        // 写入浮点数数组到文件流 (转换为 Int16)
        private static void ConvertAndWrite(BinaryWriter writer, float[] samples)
        {
            // WAV 文件的 PCM 数据通常是 16 位的，需要将 float [-1, 1] 转换为 short [-32768, 32767]
            var intData = new short[samples.Length];
            
            // 使用 short.MaxValue 作为缩放因子更准确
            const float rescaleFactor = 32767; 

            for (var i = 0; i < samples.Length; i++)
            {
                // 增加一个钳制，防止数据溢出 short 范围
                var clampedSample = Mathf.Clamp(samples[i], -1.0f, 1.0f);
                intData[i] = (short)(clampedSample * rescaleFactor);
            }
            
            // 写入字节
            // (注意: BinaryWriter 写入 short 数组效率不高，我们还是用 BlockCopy)
            var bytesData = new byte[intData.Length * 2]; // 每个 short 占 2 字节
            Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);

            writer.Write(bytesData);
        }
        
        /// <summary>
        /// 写入 WAV 文件头
        /// </summary>
        /// <param name="writer">用于写入文件的 BinaryWriter</param>
        /// <param name="clip">要写入头信息的 AudioClip</param>
        private static void WriteHeader(BinaryWriter writer, AudioClip clip)
        {
            var hz = clip.frequency;
            var channels = clip.channels;
            var samples = clip.samples; // 注意：这里应该是 *每个通道* 的样本数
            
            var dataSize = samples * channels * 2; // 16位PCM，每个样本2字节
            var fileSize = dataSize + HeaderSize - 8; // 文件总大小 - "RIFF" 和 "WAVE" (8字节)
            
            // RIFF 块
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(fileSize); 
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // Format 块
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);             // Subchunk1Size (16 for PCM)
            writer.Write((short)1);       // AudioFormat (1 for PCM)
            writer.Write((short)channels);// NumChannels
            writer.Write(hz);             // SampleRate
            
            int byteRate = hz * channels * 2;         // ByteRate = SampleRate * NumChannels * BitsPerSample/8
            writer.Write(byteRate); 
            
            short blockAlign = (short)(channels * 2); // BlockAlign = NumChannels * BitsPerSample/8
            writer.Write(blockAlign);
            writer.Write((short)16);      // BitsPerSample

            // Data 块
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);
        }
        #endregion
    }
}

