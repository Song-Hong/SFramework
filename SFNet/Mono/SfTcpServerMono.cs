using System;
using System.Net;
using System.Threading;
using SFramework.Core.Mono;
using SFramework.SFNet.Module.Tcp;
using UnityEngine;

namespace SFramework.SFNet.Mono
{
    /// <summary>
    /// TCP服务器/客户端 开箱即用工具
    /// </summary>
    public class SfTcpServerMono : SfMonoSingleton<SfTcpServerMono>
    {
        public enum ServerState
        {
            Server,
            Client
        }

        [Header("服务器/客户端")] public ServerState serverState;
        [Header("IP")] public string ip;
        [Header("端口号")] public int port = 8888;
        [Header("打印消息日志")]public bool printLog = true;
        
        /// <summary>
        /// 服务器
        /// </summary>
        private SfTcpServer _server;
        /// <summary>
        /// 客户端
        /// </summary>
        private SfTcpClient _client;
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
            var mainThread = SynchronizationContext.Current;

            if (serverState == ServerState.Client)
            {
                try
                {
                    _client = SfTcpClient.Connect(ip, port, (msg) =>
                    {
                        if(printLog)
                            Debug.Log("接收到服务端消息:" + msg);
                        mainThread.Post(_ =>
                        {
                            Received?.Invoke(msg);
                        }, null);
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                try
                {
                    _server = SfTcpServer.Connect(ip, port, (socket, msg) =>
                    {
                        mainThread.Post(_ =>
                        {
                            Received?.Invoke(msg);
                        }, null);
                        // 获取远程端点信息
                        var remoteEndPoint = socket.RemoteEndPoint;
                        if (remoteEndPoint is not IPEndPoint endPoint) return;
                        var ipAddress = endPoint.Address;
                        var pointPort = endPoint.Port;
                        if(printLog) Debug.Log($"接收到{ipAddress}:{pointPort}消息: {msg}");
                        mainThread.Post(_ =>
                        {
                            ReceivedIPPort?.Invoke(ipAddress.ToString(),pointPort,msg);
                        }, null);
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Reset()
        {
            ip = SfTcpServer.GetMainLocalIP();
        }

        /// <summary>
        /// 关闭客户端断开连接
        /// </summary>
        private void OnDestroy()
        {
            if (serverState == ServerState.Client)
            {
                _client?.Disconnect();
            }
            else
            {
                _server?.Disconnect();
            }
        }
        
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息</param>
        public void Send(string msg)
        {
            if (serverState == ServerState.Client)
            {
                _client.Send(msg);
            }
            else
            {
                _server.Send(msg);
            }
        }

        /// <summary>
        /// 发送消息至指定客户端
        /// </summary>
        /// <param name="clientID">消息序号</param>
        /// <param name="msg">消息</param>
        public void Send(int clientID, string msg)
        {
            if (serverState == ServerState.Client)
            {
                _client.Send(msg);
            }
            else
            {
                _server.Send(clientID,msg);
            }
        }
        
        /// <summary>
        /// 广播给所有客户端
        /// </summary>
        /// <param name="msg">消息</param>
        public void SendAll(string msg)
        {
            if (serverState == ServerState.Client)
            {
                _client.Send(msg);
            }
            else
            {
                _server.SendAll(msg);
            }
        }
    }
}