using System;
using System.Linq;
using UnityEngine;

// 用于简化设备查找

namespace SFramework.SFHardware.Module
{
    /// <summary>
    /// 相机模块
    /// 用于管理和访问设备的物理摄像头 (WebCamTexture)
    /// </summary>
    public class SfCamera
    {
        #region 变量與属性
        /// <summary>
        /// 当前活动的摄像头纹理
        /// </summary>
        public WebCamTexture ActiveTexture { get; private set; }

        /// <summary>
        /// 摄像头是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 获取所有可用的摄像头设备
        /// </summary>
        public WebCamDevice[] Devices { get; private set; }

        /// <summary>
        /// 当前正在使用的设备
        /// </summary>
        public WebCamDevice CurrentDevice { get; private set; }

        /// <summary>
        /// 当摄像头成功启动时触发的事件
        /// </summary>
        public event Action OnCameraStarted;

        /// <summary>
        /// 当摄像头停止时触发的事件
        /// </summary>
        public event Action OnCameraStopped;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数：初始化相机模块
        /// </summary>
        public SfCamera()
        {
            // 初始化时刷新一次设备列表
            RefreshDevices();
            IsRunning = false;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 刷新可用的设备列表
        /// (当有新设备插入或拔出时，或者在请求权限后调用)
        /// </summary>
        public void RefreshDevices()
        {
            Devices = WebCamTexture.devices;
            if (Devices == null)
            {
                // 确保 Devices 数组永不为 null
                Devices = new WebCamDevice[0];
            }
        }

        /// <summary>
        /// 启动默认的摄像头
        /// (优先尝试启动后置摄像头)
        /// </summary>
        /// <param name="requestedWidth">请求的宽度</param>
        /// <param name="requestedHeight">请求的高度</param>
        /// <param name="requestedFPS">请求的帧率</param>
        /// <returns>是否成功启动</returns>
        public bool StartDefault(int requestedWidth = 1920, int requestedHeight = 1080, int requestedFPS = 30)
        {
            if (Devices.Length == 0)
            {
                Debug.LogWarning("SfCamera: 未找到任何摄像头设备。");
                return false;
            }

            // 1. 尝试查找后置摄像头
            var defaultDevice = Devices.FirstOrDefault(d => !d.isFrontFacing);

            // 2. 如果没有后置摄像头 (例如在笔记本上)，则使用第一个可用的设备
            if (string.IsNullOrEmpty(defaultDevice.name))
            {
                defaultDevice = Devices[0];
            }

            return Start(defaultDevice, requestedWidth, requestedHeight, requestedFPS);
        }

        /// <summary>
        /// 启动指定的摄像头（通过设备索引）
        /// </summary>
        /// <param name="deviceIndex">设备索引 (来自 Devices 列表)</param>
        /// <param name="requestedWidth">请求的宽度</param>
        /// <param name="requestedHeight">请求的高度</param>
        /// <param name="requestedFPS">请求的帧率</param>
        /// <returns>是否成功启动</returns>
        public bool Start(int deviceIndex, int requestedWidth = 1920, int requestedHeight = 1080, int requestedFPS = 30)
        {
            if (deviceIndex < 0 || deviceIndex >= Devices.Length)
            {
                Debug.LogError($"SfCamera: 无效的设备索引 {deviceIndex}。");
                return false;
            }

            return Start(Devices[deviceIndex], requestedWidth, requestedHeight, requestedFPS);
        }
        
        /// <summary>
        /// 启动指定的摄像头（通过设备名称）
        /// </summary>
        /// <param name="deviceName">设备名称 (来自 Devices 列表)</param>
        /// <param name="requestedWidth">请求的宽度</param>
        /// <param name="requestedHeight">请求的高度</param>
        /// <param name="requestedFPS">请求的帧率</param>
        /// <returns>是否成功启动</returns>
        public bool Start(string deviceName, int requestedWidth = 1920, int requestedHeight = 1080, int requestedFPS = 30)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                Debug.LogError("SfCamera: 设备名称不能为空。");
                return false;
            }

            // 查找匹配名称的设备
            var deviceFound = false;
            var deviceToUse = new WebCamDevice();
            foreach (var dev in Devices)
            {
                if (dev.name == deviceName)
                {
                    deviceToUse = dev;
                    deviceFound = true;
                    break;
                }
            }

            if (!deviceFound)
            {
                Debug.LogError($"SfCamera: 未找到名为 '{deviceName}' 的设备。");
                return false;
            }

            // 调用重载方法
            return Start(deviceToUse, requestedWidth, requestedHeight, requestedFPS);
        }

        /// <summary>
        /// 启动指定的摄像头（通过设备结构体）
        /// </summary>
        /// <param name="device">要启动的设备</param>
        /// <param name="requestedWidth">请求的宽度</param>
        /// <param name="requestedHeight">请求的高度</param>
        /// <param name="requestedFPS">请求的帧率</param>
        /// <returns>是否成功启动</returns>
        public bool Start(WebCamDevice device, int requestedWidth = 1920, int requestedHeight = 1080, int requestedFPS = 30)
        {
            if (IsRunning)
            {
                Debug.LogWarning("SfCamera: 摄像头已在运行。请先调用 Stop()。");
                return false;
            }

            // 检查权限 (在移动端尤其重要)
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogWarning("SfCamera: 缺少摄像头权限。应用可能需要先请求权限。");
                // 注意：请求权限 (Application.RequestUserAuthorization) 是一个异步操作，
                // 通常需要一个 MonoBehaviour 启动协程来处理。本模块不直接处理权限请求。
                return false;
            }

            try
            {
                // 创建 WebCamTexture 实例
                ActiveTexture = new WebCamTexture(device.name, requestedWidth, requestedHeight, requestedFPS);
                CurrentDevice = device;

                // 启动摄像头
                ActiveTexture.Play();

                // WebCamTexture.Play() 是异步的, IsPlaying() 可能需要几帧才会变为 true
                IsRunning = true;
                
                Debug.Log($"SfCamera: 正在启动摄像头 '{device.name}' (请求 {requestedWidth}x{requestedHeight} @ {requestedFPS}fps)...");

                // 触发启动事件
                OnCameraStarted?.Invoke();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SfCamera: 启动摄像头失败: {e.Message}");
                if (ActiveTexture != null)
                {
                    ActiveTexture.Stop(); // 确保资源被释放
                    ActiveTexture = null;
                }
                IsRunning = false;
                return false;
            }
        }

        /// <summary>
        /// 停止当前正在运行的摄像头
        /// </summary>
        public void Stop()
        {
            if (!IsRunning || ActiveTexture == null)
            {
                return;
            }

            try
            {
                ActiveTexture.Stop(); // 停止摄像头并释放硬件资源
                ActiveTexture = null;
                IsRunning = false;
                Debug.Log($"SfCamera: 摄像头 '{CurrentDevice.name}' 已停止。");
                
                // 触发停止事件
                OnCameraStopped?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"SfCamera: 停止摄像头时发生错误: {e.Message}");
                // 即使出错，也强制重置状态
                ActiveTexture = null;
                IsRunning = false;
            }
        }

        /// <summary>
        /// 暂停摄像头纹理 (注意：这不会释放硬件资源，只是暂停更新纹理)
        /// </summary>
        public void Pause()
        {
            if (IsRunning && ActiveTexture != null && ActiveTexture.isPlaying)
            {
                ActiveTexture.Pause();
                 Debug.Log($"SfCamera: 摄像头 '{CurrentDevice.name}' 已暂停。");
            }
        }
        
        /// <summary>
        /// 恢复暂停的摄像头纹理
        /// </summary>
        public void Resume()
        {
             if (IsRunning && ActiveTexture != null && !ActiveTexture.isPlaying)
            {
                ActiveTexture.Play(); // 重新开始播放
                Debug.Log($"SfCamera: 摄像头 '{CurrentDevice.name}' 已恢复。");
            }
        }
        #endregion
    }
}
