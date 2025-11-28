using System;
using SFramework.Core.Mono;
using SFramework.SFHardware.Module;
using UnityEngine;

namespace SFramework.SFHardware.Mono
{
    /// <summary>
    /// 麦克风模块单例
    /// </summary>
    public class SfMicrophoneMono : SfMonoSingleton<SfMicrophoneMono>
    {
        /// <summary>
        /// 麦克风模块实例
        /// </summary>
        public SfMicrophone SfMicrophone;

        /// <summary>
        /// 当录制结束 (无论是手动停止还是VAD自动停止)
        /// </summary>
        public event Action<AudioClip> OnRecodingFinished;

        /// <summary>
        /// 是否正在录音 (VAD或手动)
        /// </summary>
        public bool IsRecording => SfMicrophone.IsRecording();
        
        /// <summary>
        /// 初始化麦克风模块实例
        /// </summary>
        private void Start()
        {
            SfMicrophone = new SfMicrophone();
            _vadSampleData = new float[vadWindowSize];
        }

        /// <summary>
        /// 当销毁组件时，结束录音
        /// </summary>
        private void OnDestroy()
        {
            StopRecoding(); // 停止手动录音
            StopVoiceDetection(); // 停止VAD
        }

        /// <summary>
        /// 当禁用组件时，结束录音
        /// </summary>
        private void OnDisable()
        {
            StopRecoding(); // 停止手动录音
            StopVoiceDetection(); // 停止VAD
        }

        #region 手动录音

        /// <summary>
        /// 开始录音 (手动)
        /// </summary>
        public bool StartRecoding(bool isLoop = false, int maxRecordTimeSeconds = 60, int sampleRate = 44100)
            => StartRecoding(0, isLoop, maxRecordTimeSeconds, sampleRate);

        /// <summary>
        /// 开始录音 (手动)
        /// </summary>
        public bool StartRecoding(int deviceIndex, bool isLoop = false, int maxRecordTimeSeconds = 60, int sampleRate = 44100)
        {
            StopVoiceDetection(); // VAD 和手动录音互斥
            return SfMicrophone.Start(deviceIndex, isLoop, maxRecordTimeSeconds, sampleRate);
        }

        /// <summary>
        /// 开始录音 (手动)
        /// </summary>
        public bool StartRecoding(string deviceName, bool isLoop = false, int maxRecordTimeSeconds = 60, int sampleRate = 44100)
        {
            StopVoiceDetection(); // VAD 和手动录音互斥
            return SfMicrophone.Start(deviceName, isLoop, maxRecordTimeSeconds, sampleRate);
        }

        /// <summary>
        /// 结束录制 (手动)
        /// </summary>
        public void StopRecoding()
        {
            if (_vadState != VadState.Stopped)
            {
                Debug.LogWarning("VAD 正在运行，请调用 StopVoiceDetection()。");
                return;
            }
            
            if (SfMicrophone.IsRecording())
            {
                // 1. 获取设备名称
                string deviceName = SfMicrophone.SelectedDevice;
                
                // 2. 获取当前录音指针的位置（这是关键！必须在 Stop 之前获取）
                // 如果是手动录音且非循环，GetPosition 返回的是当前录了多少采样点
                int position = Microphone.GetPosition(deviceName);
                
                // 3. 获取原始的长 Clip
                var originalClip = SfMicrophone.RecordingClip;
                
                // 4. 停止录音
                SfMicrophone.Stop();

                // 5. 剪裁 AudioClip
                AudioClip trimmedClip = TrimClip(originalClip, position);

                // 6. 触发事件，传递剪裁后的 Clip
                OnRecodingFinished?.Invoke(trimmedClip);
            }
        }

        /// <summary>
        /// 辅助方法：剪裁 AudioClip
        /// </summary>
        private AudioClip TrimClip(AudioClip originalClip, int position)
        {
            // 如果 position <= 0，通常意味着录音刚开始或者循环了一整圈(但在非Loop模式下很少见)
            // 为了安全，如果获取不到位置，就返回原片段，或者如果确实很短就返回空
            if (position <= 0) 
            {
                return originalClip; 
            }

            // 创建一个新的 AudioClip，长度只有实际录制那么长
            float[] soundData = new float[position * originalClip.channels];
            originalClip.GetData(soundData, 0);

            AudioClip newClip = AudioClip.Create(originalClip.name, position, originalClip.channels, originalClip.frequency, false);
            newClip.SetData(soundData, 0);

            Debug.Log($"[SfMicrophoneMono] 录音结束。原始长度: {originalClip.length:F2}s, 实际录制: {newClip.length:F2}s");

            return newClip;
        }
        #endregion
        

        #region 实时监听 VAD
        /// <summary>
        /// VAD 状态
        /// </summary>
        private enum VadState
        {
            Stopped,    // VAD 未运行
            Monitoring, // 正在监听，等待语音
            Recording   // 检测到语音，正在录制
        }
        
        [Header("语音活动检测 (VAD)")]
        [Tooltip("触发录音的声音阈值 (0 到 1)")]
        [SerializeField] private float vadThreshold = 0.02f;
        
        [Tooltip("语音结束后，等待多少秒自动停止录音")]
        [SerializeField] private float silenceDurationToStop = 1.5f;

        [Tooltip("VAD使用的循环缓冲区长度（秒）。必须大于 silenceDurationToStop")]
        [SerializeField] private int vadBufferLengthSeconds = 10;
        
        [Tooltip("VAD分析窗口大小（样本数）")]
        [SerializeField] private int vadWindowSize = 1024;

        private VadState _vadState = VadState.Stopped;
        private float[] _vadSampleData;       // VAD 用的样本数据窗口
        private int _vadLastPosition;     // VAD 上次读取的录音头位置
        private float _timeSinceLastSound; // 上次检测到声音以来的时间
        private int _vadStartSamplePosition; // VAD 录音开始的采样点
        private AudioClip _vadMonitoringClip; // VAD 用的循环录音Clip
        private string _vadMonitoringDevice;  // VAD 正在监听的设备
        
        /// <summary>
        /// 开始语音活动检测 (VAD)
        /// </summary>
        /// <param name="deviceIndex">麦克风设备索引</param>
        /// <param name="sampleRate">采样率</param>
        /// <returns>是否开启成功</returns>
        public bool StartVoiceDetection(int deviceIndex = 0, int sampleRate = 44100)
        {
            if (IsRecording) StopRecoding(); // VAD 和手动录音互斥

            if (silenceDurationToStop > vadBufferLengthSeconds)
            {
                Debug.LogError("VAD 缓冲区长度 (vadBufferLengthSeconds) 必须大于静音停止时间 (silenceDurationToStop)！");
                return false;
            }
            
            // 启动一个 *循环* 的录音作为VAD的监听缓冲区
            bool success = SfMicrophone.Start(deviceIndex, true, vadBufferLengthSeconds, sampleRate);
            if (success)
            {
                _vadMonitoringClip = SfMicrophone.RecordingClip;
                _vadMonitoringDevice = SfMicrophone.SelectedDevice;
                _vadLastPosition = 0;
                _timeSinceLastSound = 0;
                _vadState = VadState.Monitoring;
                Debug.Log($"[VAD] 开始监听设备: {_vadMonitoringDevice}");
            }
            return success;
        }

        /// <summary>
        /// 开始语音活动检测 (VAD)
        /// </summary>
        /// <param name="deviceName">麦克风设备名称</param>
        /// <param name="sampleRate">采样率</param>
        /// <returns>是否开启成功</returns>
        public bool StartVoiceDetection(string deviceName, int sampleRate = 44100)
        {
            if (IsRecording) StopRecoding(); // VAD 和手动录音互斥
            
            if (silenceDurationToStop > vadBufferLengthSeconds)
            {
                Debug.LogError("VAD 缓冲区长度 (vadBufferLengthSeconds) 必须大于静音停止时间 (silenceDurationToStop)！");
                return false;
            }

            // 启动一个 *循环* 的录音作为VAD的监听缓冲区
            bool success = SfMicrophone.Start(deviceName, true, vadBufferLengthSeconds, sampleRate);
            if (success)
            {
                _vadMonitoringClip = SfMicrophone.RecordingClip;
                _vadMonitoringDevice = SfMicrophone.SelectedDevice;
                _vadLastPosition = 0;
                _timeSinceLastSound = 0;
                _vadState = VadState.Monitoring;
                Debug.Log($"[VAD] 开始监听设备: {_vadMonitoringDevice}");
            }
            return success;
        }

        /// <summary>
        /// 停止语音活动检测
        /// </summary>
        public void StopVoiceDetection()
        {
            if (_vadState == VadState.Stopped) return;

            // 如果在停止时仍在录制，则强行处理最后一段
            if (_vadState == VadState.Recording)
            {
                int endPosition = Microphone.GetPosition(_vadMonitoringDevice);
                ProcessAndFireRecording(_vadStartSamplePosition, endPosition);
            }
            
            SfMicrophone.Stop();
            _vadMonitoringClip = null;
            _vadState = VadState.Stopped;
            Debug.Log("[VAD] 停止监听。");
        }
        
        /// <summary>
        /// Update 循环，用于处理VAD状态机
        /// </summary>
        private void Update()
        {
            // --- VAD 逻辑开始 ---
            if (_vadState == VadState.Stopped || _vadMonitoringClip == null || !SfMicrophone.IsRecording())
            {
                return;
            }

            int currentPosition = Microphone.GetPosition(_vadMonitoringDevice);
            
            // 检查是否有足够的新数据可供分析 (逻辑来自您的 Editor 脚本)
            if (currentPosition < _vadLastPosition) _vadLastPosition = 0; // 缓冲区循环了

            bool hasNewData = (currentPosition - _vadLastPosition) >= vadWindowSize;

            float currentPeakValue = 0f;
            
            if (hasNewData)
            {
                // 计算数据窗口的起始位置
                int startPosition = (currentPosition - vadWindowSize + _vadMonitoringClip.samples) % _vadMonitoringClip.samples;
                
                // 获取数据
                _vadMonitoringClip.GetData(_vadSampleData, startPosition);
                _vadLastPosition = currentPosition;
                
                // 计算该窗口的峰值 (我们只需要一个总峰值，而不是32条)
                for (int i = 0; i < vadWindowSize; i++)
                {
                    currentPeakValue = Mathf.Max(currentPeakValue, Mathf.Abs(_vadSampleData[i]));
                }
            }

            // VAD 状态机
            bool isSpeaking = currentPeakValue > vadThreshold;

            switch (_vadState)
            {
                case VadState.Monitoring:
                    if (isSpeaking)
                    {
                        // 状态切换：监听到 -> 录制中
                        _vadState = VadState.Recording;
                        // 记录开始录制的数据点 (我们回溯一个窗口，确保录到开头)
                        _vadStartSamplePosition = (currentPosition - vadWindowSize + _vadMonitoringClip.samples) % _vadMonitoringClip.samples;
                        _timeSinceLastSound = 0f;
                        Debug.Log("[VAD] 检测到语音，开始录制片段...");
                    }
                    break;
                
                case VadState.Recording:
                    if (isSpeaking)
                    {
                        // 仍在说话，重置静音计时器
                        _timeSinceLastSound = 0f;
                    }
                    else
                    {
                        // 未检测到声音，开始计时
                        _timeSinceLastSound += Time.deltaTime;
                    }
                    
                    if (_timeSinceLastSound > silenceDurationToStop)
                    {
                        // 状态切换：录制中 -> 监听
                        // 静音时间足够长，处理并发送录音
                        Debug.Log($"[VAD] 语音结束，处理录音... (持续{_timeSinceLastSound:F2}秒静音)");
                        ProcessAndFireRecording(_vadStartSamplePosition, currentPosition);
                        
                        // 重置状态机，返回监听状态
                        _vadState = VadState.Monitoring;
                        _timeSinceLastSound = 0f;
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 处理 VAD 录音数据，裁剪并触发事件
        /// </summary>
        /// <param name="startSample">开始采样点</param>
        /// <param name="endSample">结束采样点</param>
        private void ProcessAndFireRecording(int startSample, int endSample)
        {
            if (_vadMonitoringClip == null) return;
            
            int channels = _vadMonitoringClip.channels;
            int frequency = _vadMonitoringClip.frequency;
            int clipSamples = _vadMonitoringClip.samples;
            
            float[] data;
            int finalSampleCount;

            if (endSample > startSample)
            {
                // --- Case 1: 录音未跨越循环缓冲区末尾 ---
                finalSampleCount = endSample - startSample;
                data = new float[finalSampleCount * channels];
                _vadMonitoringClip.GetData(data, startSample);
            }
            else
            {
                // --- Case 2: 录音跨越了循环缓冲区末尾 ---
                // (例如，从 80% 录到 10%)
                
                // 第1部分：从 startSample 到缓冲区末尾
                int part1Count = (clipSamples - startSample);
                // 第2部分：从缓冲区开头到 endSample
                int part2Count = endSample;
                
                finalSampleCount = part1Count + part2Count;
                data = new float[finalSampleCount * channels];
                
                // 获取第1部分数据
                float[] part1Data = new float[part1Count * channels];
                _vadMonitoringClip.GetData(part1Data, startSample);
                Array.Copy(part1Data, data, part1Data.Length);
                
                // 获取第2部分数据
                float[] part2Data = new float[part2Count * channels];
                _vadMonitoringClip.GetData(part2Data, 0); // 从头开始
                Array.Copy(part2Data, 0, data, part1Data.Length, part2Data.Length);
            }

            if (finalSampleCount == 0)
            {
                Debug.LogWarning("[VAD] 录音片段为空，已丢弃。");
                return;
            }

            // --- 创建新的、裁剪过的 AudioClip ---
            AudioClip finalClip = AudioClip.Create("SpeechSegment", finalSampleCount, channels, frequency, false);
            finalClip.SetData(data, 0);
            
            // 触发事件
            OnRecodingFinished?.Invoke(finalClip);
            Debug.Log($"[VAD] 成功创建 {finalClip.length:F2} 秒的录音片段。");
        }
        #endregion
    }
}