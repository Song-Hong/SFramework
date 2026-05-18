using System;
using System.Threading;
using SFramework.Core.Mono;
using SFramework.SFNet.Data;
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
        /// <summary>
        /// UDP服务器
        /// </summary>
        private SfUDPServer _server;
        
        /// <summary>
        /// 服务器数据
        /// </summary>
        public SfServerData serverData;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Start()
        {
            //自动开启服务器
            if(serverData.autoStart){Init();}
            //获取并执行所有组件的初始化方法
            foreach (var support in GetComponentsInChildren<SfUdpSupport>())
            {
                support.Init(_server);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            //当 IP 为空时 自动获取本地IP
            if(string.IsNullOrWhiteSpace(serverData.ip))
            {
                serverData.ip = SfUDPServer.GetMainLocalIP();
            }
            
            // 主线程
            var mainThread = SynchronizationContext.Current;
            try
            {
                _server = SfUDPServer.Start(serverData.ip, serverData.port, (clientIp,clientPort,msg) =>
                {
                    if(serverData.printLog)
                        Debug.Log($"接收到{clientIp}:{clientPort}消息: {msg}");
                    mainThread.Post(_ => { serverData.Received?.Invoke(msg); }, null);
                    mainThread.Post(_ => { serverData.ReceivedIPPort?.Invoke(clientIp,clientPort,msg); }, null);
                });
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
            serverData.ip = SfUDPServer.GetMainLocalIP();
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