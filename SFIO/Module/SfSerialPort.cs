using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace SFramework.SFIo.Module
{
    /// <summary>
    /// 串口通讯模块
    /// 包含自动主线程分发与协议解析
    /// </summary>
    public class SfSerialPort
    {
        #region 变量与属性
        /// <summary>
        /// 串口对象
        /// </summary>
        private SerialPort _serialPort;
        /// <summary>
        /// 监听线程
        /// </summary>
        private Thread _receiveThread;
        /// <summary>
        /// 是否开启
        /// </summary>
        private volatile bool _isOpen;
        /// <summary>
        /// 端口名称
        /// </summary>
        public string PortName { get; private set; }
        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; private set; }

        /// <summary>
        /// 串口是否已打开
        /// </summary>
        public bool IsOpen => _isOpen && _serialPort != null && _serialPort.IsOpen;
        
        /// <summary>
        /// 原始数据接收事件 (已调度回Unity主线程)
        /// </summary>
        public event Action<byte[]> OnDataReceived;
        #endregion

        #region 构造与连接
        private SfSerialPort() { }

        /// <summary>
        /// 创建并启动串口服务
        /// </summary>
        /// <param name="portName">端口号 (如 COM3)</param>
        /// <param name="baudRate">波特率 (默认9600)</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="handshake">握手协议</param>
        /// <param name="onReceived">数据接收回调</param>
        /// <returns>串口实例</returns>
        public static SfSerialPort Connect(
            string portName, 
            int baudRate = 9600, 
            int dataBits = 8, 
            Parity parity = Parity.None, 
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None,
            Action<byte[]> onReceived = null)
        {
            var sfSerial = new SfSerialPort();
            sfSerial.PortName = portName;
            sfSerial.BaudRate = baudRate;
            
            try
            {
                // 初始化 SerialPort
                sfSerial._serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                sfSerial._serialPort.Handshake = handshake;
                sfSerial._serialPort.ReadTimeout = 500;
                sfSerial._serialPort.WriteTimeout = 500;
                sfSerial._serialPort.Open();

                sfSerial._isOpen = true;

                // 启动后台接收线程
                sfSerial._receiveThread = new Thread(sfSerial.ReceiveLoop)
                {
                    IsBackground = true
                };
                sfSerial._receiveThread.Start();

                // 绑定消息事件
                if (onReceived != null)
                {
                    sfSerial.OnDataReceived += onReceived;
                }
                
                Debug.Log($"SfSerial: 串口开启成功！监听于 {portName} 波特率 {baudRate}");
                return sfSerial;
            }
            catch (Exception e)
            {
                Debug.LogError($"SfSerial: 串口开启失败: {e.Message}");
                sfSerial.Disconnect();
                return null;
            }
        }
        #endregion

        #region 接收循环逻辑
        private void ReceiveLoop()
        {
            byte[] buffer = new byte[1024];
            while (_isOpen)
            {
                try
                {
                    if (_serialPort == null || !_serialPort.IsOpen) break;

                    // 阻塞读取
                    int count = _serialPort.Read(buffer, 0, buffer.Length);
                    if (count <= 0) continue;

                    byte[] data = new byte[count];
                    Array.Copy(buffer, 0, data, 0, count);

                    // 回调
                    OnDataReceived?.Invoke(data);
                }
                catch (TimeoutException) { /* 忽略超时 */ }
                catch (Exception e)
                {
                    if (_isOpen) Debug.LogWarning($"SfSerial 接收异常: {e.Message}");
                    break;
                }
            }
        }
        
        #endregion

        #region 发送方法
        /// <summary>
        /// 发送十六进制指令字符串
        /// </summary>
        public void Send(string hexString)
        {
            if (!IsOpen) return;
            try
            {
                string cleanHex = hexString.Replace(" ", "");
                byte[] buffer = new byte[cleanHex.Length / 2];
                for (int i = 0; i < cleanHex.Length; i += 2)
                {
                    buffer[i / 2] = Convert.ToByte(cleanHex.Substring(i, 2), 16);
                }
                _serialPort.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"SfSerial 发送失败: {e.Message}");
            }
        }

        /// <summary>
        /// 发送字节数组
        /// </summary>
        public void Send(byte[] bytes)
        {
            if (IsOpen) _serialPort.Write(bytes, 0, bytes.Length);
        }
        #endregion

        #region 断开连接
        public void Disconnect()
        {
            if (!_isOpen) return;

            _isOpen = false;
            OnDataReceived = null;

            if (_serialPort != null)
            {
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort = null;
            }

            if (_receiveThread is { IsAlive: true })
            {
                _receiveThread.Join(500);
            }
            _receiveThread = null;

            Debug.Log("SfSerial 串口服务已断开。");
        }
        #endregion

        #region 工具类
        /// <summary>
        /// 获取可用串口端口列表
        /// </summary>
        /// <returns></returns>
        public static string[] GetPortNames() => SerialPort.GetPortNames();
        #endregion
    }
}