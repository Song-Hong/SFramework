using UnityEngine;

namespace SFramework.SFIo.Module
{
    /// <summary>
    /// 麦克风模块
    /// </summary>
    public class SfMicrophone
    {
        /// <summary>
        /// 麦克风设备列表
        /// </summary>
        public string[] Devices;
        
        /// <summary>
        /// 选中的麦克风设备
        /// </summary>
        public string SelectedDevice;
        
        /// <summary>
        /// 录音剪辑
        /// </summary>
        public AudioClip RecordingClip;
        
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public SfMicrophone()
        {
            Devices = Microphone.devices;
        }
        #endregion

        #region 开始录音
        /// <summary>
        /// 开始录音
        /// </summary>
        /// <returns>是否开启成功</returns>
        public bool Start()
        {
            if (Devices.Length != 0) return Start(0);
            Debug.LogError("没有麦克风设备");
            return false;
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        /// <param name="deviceIndex">麦克风设备索引</param>
        /// <param name="isLoop">是否循环录音</param>
        /// <param name="maxRecordTimeSeconds">最大录音时间（秒）</param>
        /// <param name="sampleRate">采样率</param>
        /// <returns>是否开启成功</returns>
        public bool Start(int deviceIndex, bool isLoop = false, int maxRecordTimeSeconds = 60, int sampleRate = 44100)
        {
            if(deviceIndex<0 || deviceIndex>=Devices.Length)
            {
                Debug.LogError($"无效的麦克风设备索引：{deviceIndex}");
                return false;
            }
            return Start(Devices[deviceIndex],isLoop, maxRecordTimeSeconds, sampleRate);
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        /// <param name="deviceName">麦克风设备名称</param>
        /// <param name="isLoop">是否循环录音</param>
        /// <param name="maxRecordTimeSeconds">最大录音时间（秒）</param>
        /// <param name="sampleRate">采样率</param>
        /// <returns>是否开启成功</returns>
        public bool Start(string deviceName,bool isLoop = false, int maxRecordTimeSeconds=60, int sampleRate=44100)
        {
            // 检查设备名称是否有效
            bool isExists = false;
            foreach (var device in Devices)
            {
                if (device == deviceName)
                {
                    isExists = true;
                }
            }
            if(!isExists || string.IsNullOrEmpty(deviceName))
            {
                    Debug.LogError($"无效的麦克风设备名称：{deviceName}");
                    return false;
            }

            if(Devices.Length == 0)
            {
                Debug.LogError("没有麦克风设备");
                return false;
            }
            
            // 停止之前的任何录制
            if (Microphone.IsRecording(SelectedDevice))
            {
                Microphone.End(SelectedDevice);
            }

            // 开始录制，并将其流式传输到 AudioSource
            RecordingClip =Microphone.Start(deviceName, isLoop, maxRecordTimeSeconds, sampleRate);
            SelectedDevice = deviceName;
            return true;
        }
        #endregion

        #region 结束录音
        /// <summary>
        /// 结束录音
        /// </summary>
        public void Stop()
        {
            if(string.IsNullOrWhiteSpace(SelectedDevice))return;
            if (!Microphone.IsRecording(SelectedDevice)) return;
            Microphone.End(SelectedDevice);
            SelectedDevice = "";
        }
        #endregion

        #region 方法
        /// <summary>
        /// 是否正在录音
        /// </summary>
        /// <returns>是否正在录音</returns>
        public bool IsRecording()
        {
            return !string.IsNullOrWhiteSpace(SelectedDevice) && Microphone.IsRecording(SelectedDevice);
        }
        #endregion
    }
}
