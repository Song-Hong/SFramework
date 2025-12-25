using System;
using System.Threading;
using SFramework.Core.Mono;
using SFramework.SFNet.Module.Udp;
using SFramework.SFNet.Support.UDP;
using UnityEngine;

namespace SFramework.SFNet.Mono
{
    /// <summary>
    /// UDP 开箱即用
    /// </summary>
    public class SfUdpServerMono:SfMonoSingleton<SfUdpServerMono>
    {
        [Header("IP")] public string ip;
        [Header("端口号")] public int port = 8787;
        [Header("打印消息日志")]public bool printLog = true;
        
        /// <summary>
        /// UDP服务器
        /// </summary>
        private SfUDPServer _server;
        
        /// <summary>
        /// 接收到消息
        /// </summary>
        public event Action<string> Received;
        /// <summary>
        /// 接收到消息 IP Port 消息
        /// </summary>
        public event Action<string,int,string> ReceivedIPPort;
        
        public void Start()
        {
            //当 IP 为空时 自动获取本地IP
            if(string.IsNullOrWhiteSpace(ip))
            {
                ip = SfUDPServer.GetMainLocalIP();
            }
            
            var mainThread = SynchronizationContext.Current;
            try
            {
                _server = SfUDPServer.Start(ip, port, (clientIp,clientPort,msg) =>
                {
                    if(printLog)
                        Debug.Log($"接收到{clientIp}:{clientPort}消息: {msg}");
                    mainThread.Post(_ =>
                    {
                        Received?.Invoke(msg);
                    }, null);
                    mainThread.Post(_ =>
                    {
                        ReceivedIPPort?.Invoke(clientIp,clientPort,msg);
                    }, null);
                });

                //获取并执行所有组件的初始化方法
                foreach (var support in GetComponentsInChildren<SfUdpSupport>())
                {
                    support.Init(_server);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void Reset()
        {
            ip = SfUDPServer.GetMainLocalIP();
        }

        /// <summary>
        /// 关闭客户端断开连接
        /// </summary>
        private void OnDestroy()
        {
            _server?.Stop();
        }
        
        /// <summary>
        /// 发送消息至指定客户端
        /// </summary>
        /// <param name="targetIp">IP地址</param>
        /// <param name="targetPort">端口号</param>
        /// <param name="msg">消息</param>
        public void Send(string targetIp, int targetPort, string msg)
        {
            _server.Send(targetIp,targetPort,msg);
        }

        /// <summary>
        /// 向同网段广播消息
        /// </summary>
        /// <param name="targetPort">端口号</param>
        /// <param name="msg">消息</param>
        public void SendBroadcast(int targetPort,string msg)
        {
            _server.SendBroadcast(targetPort,msg);
        }
        
        /// <summary>
        /// 发送广播消息使用遍历的方式
        /// </summary>
        /// <param name="targetIp">IP地址</param>
        /// <param name="targetPort">端口号</param>
        /// <param name="msg">消息</param>
        public void SendBroadcastWithForeach(string targetIp, int targetPort, string msg)
        {
            _server.SendBroadcastWithForeach(targetIp,targetPort, msg);
        }
    }
}