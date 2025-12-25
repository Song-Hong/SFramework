using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using SFramework.Core.Mono;
using SFramework.SFIo.Module;
using SFramework.SFIo.Support;
using UnityEngine;

namespace SFramework.SFIo.Mono
{
    /// <summary>
    /// 串口模块Mono
    /// </summary>
    public class SfSerialPortMono:SfMonoSingleton<SfSerialPortMono>
    {
        /// <summary>
        /// 缓存控制器类型
        /// <summary>
        /// 缓存控制器类型
        /// </summary>
        public enum BufferControllerType
        {
            /// <summary>
            /// 无缓存
            /// </summary>
            None,
            /// <summary>
            /// 固定长度缓存
            /// </summary>
            FixedLength,
            /// <summary>
            /// 最小长度限制
            /// </summary>
            MinimumLength,
            /// <summary>
            /// 结束符缓存
            /// </summary>
            EndOfLine,
        }
        
        /// <summary>
        /// 初始化串口模块
        /// </summary>
        [Header("端口名")] public string portName = "COM1";
        /// <summary>
        /// 波特率
        /// </summary>
        [Header("波特率")] public int baudRate = 9600;
        /// <summary>
        /// 数据位
        /// </summary>
        [Header("数据位")] public int dataBits = 8;
        /// <summary>
        /// 校验位
        /// </summary>
        [Header("校验位")] public Parity parity = Parity.None;
        /// <summary>
        /// 停止位
        /// </summary>
        [Header("停止位")] public StopBits stopBits = StopBits.One;
        /// <summary>
        /// 握手协议
        /// </summary>
        [Header("握手协议")] public Handshake handshake = Handshake.None;
        /// <summary>
        /// 是否开启缓存
        /// </summary>
        [Header("是否开启缓存")]public BufferControllerType bufferControllerType = BufferControllerType.None;
        /// <summary>
        /// 缓存字节长度（负数表示不使用缓存）
        /// </summary>
        [Header("缓冲长度")]public int bufferLength = 9;
        /// <summary>
        /// 结束符（仅在EndOfLine模式下生效）
        /// </summary>
        [Header("结束符")] public string endCode;
        /// <summary>
        /// 是否打印消息记录
        /// </summary>
        [Header("打印消息记录")] public bool printLog = true;
        
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event Action<byte[]>  OnDataReceived; 
        
        /// <summary>
        /// 主线程数据接收事件
        /// </summary>
        public event Action<byte[]>  OnDataReceivedMainThread; 
        
        /// <summary>
        /// 串口对象
        /// </summary>
        private SfSerialPort _serialPortPort;
        
        /// <summary>
        /// 主线程上下文
        /// </summary>
        private SynchronizationContext _mainContext;

        /// <summary>
        /// 缓存字节列表
        /// </summary>
        private List<byte> _buffer = new List<byte>();
        
        private void Start()
        {
            //获取Unity主线程上下文，确保回调安全
            _mainContext = SynchronizationContext.Current;
            
            try
            {
                _serialPortPort = SfSerialPort.Connect(portName, baudRate, dataBits, parity, stopBits, handshake, data =>
                {
                    switch (bufferControllerType)
                    {
                        case BufferControllerType.FixedLength:
                            FixedBufferProcessing(data);
                            break;
                        case BufferControllerType.MinimumLength:
                            MinimumLengthBufferProcessing(data);
                            break;
                        case BufferControllerType.EndOfLine:
                            EndOfLineBufferProcessing(data);
                            break;
                        default:
                            SendEvent(data);
                            break;
                    }
                });

                // 初始化所有子组件的串口插件
                foreach (var sfSerialPortSupport in GetComponentsInChildren<SfSerialPortSupport>())
                {
                    sfSerialPortSupport.Init(_serialPortPort);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        /// <summary>
        /// 固定长度缓存处理
        /// <param name="data">数据</param>
        /// </summary>
        private void FixedBufferProcessing(byte[] data)
        {
            _buffer.AddRange(data);

            while (_buffer.Count >= bufferLength) // 假设一条完整指令是9字节
            {
                if (_buffer[0] == 0xAA) // 检查包头
                {
                    byte[] completePackage = _buffer.GetRange(0, bufferLength).ToArray();
                    _buffer.RemoveRange(0, bufferLength);

                    SendEvent(completePackage);
                }
                else
                {
                    // 如果包头不对，删掉第一个字节继续找
                    _buffer.RemoveAt(0);
                }
            }
        }
        
        /// <summary>
        /// 带长度限制的缓存处理
        /// </summary>
        /// <param name="data">接收到的新原始数据</param>
        private void MinimumLengthBufferProcessing(byte[] data)
        {
            // 1. 将新收到的数据加入缓存队列
            _buffer.AddRange(data);

            // 2. 循环处理缓存，直到不满足处理条件为止
            while (_buffer.Count > 0)
            {
                if (_buffer.Count < bufferLength)
                {
                    break; 
                }
                
                // 4. 长度足够，截取完整包
                var completePackage = _buffer.GetRange(0, _buffer.Count).ToArray();
            
                // 5. 从缓存中移除已处理的部分
                _buffer.RemoveRange(0, _buffer.Count);

                // 6. 转发完整包
                SendEvent(completePackage);
            }
        }

        /// <summary>
        /// 结束符缓存处理
        /// </summary>
        /// <param name="data">接收到的新原始数据</param>
        private void EndOfLineBufferProcessing(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            // 1. 将新数据存入缓冲区
            _buffer.AddRange(data);

            // 2. 循环检查缓冲区中是否存在结束符
            // 使用循环是为了处理“一次性接收到多条完整指令”的情况
            int index;
            while ((index = _buffer.IndexOf(Convert.ToByte(endCode, 16))) != -1)
            {
                // 3. 提取从开头到结束符的一整条数据
                int count = index + 1; // 包含结束符本身
                byte[] completeFrame = new byte[count];
                _buffer.CopyTo(0, completeFrame, 0, count);

                // 4. 从缓冲区移除已处理的数据
                _buffer.RemoveRange(0, count);
                
                SendEvent(completeFrame);
            }
        }

        /// <summary>
        /// 发送事件
        /// <param name="data">数据</param>
        /// </summary>
        private void SendEvent(byte[] data)
        {
            if (printLog) Debug.Log($"接收到{portName}[{baudRate}]数据: {BitConverter.ToString(data)}");
            OnDataReceived?.Invoke(data);
            _mainContext.Post(_ => OnDataReceivedMainThread?.Invoke(data), null);
        }
        
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public void Send(byte[] data)
        {
            _serialPortPort.Send(data);
        }
        
        /// <summary>
        /// 获取可用串口端口列表
        /// </summary>
        /// <returns></returns>
        public void GetPortNames() => SerialPort.GetPortNames();

        private void OnDestroy()
        {
            _serialPortPort?.Disconnect();
        }
    }
}