using System;
using System.Text;
using System.Threading;
using SFramework.Core.Mono;
using SFramework.SFNet.Module.Kcp;
using SFramework.SFNet.Module.Udp;
using UnityEngine;

namespace SFramework.SFNet.Mono
{
    /// <summary>
    /// KCP 服务器/客户端 开箱即用
    /// </summary>
    public class SfKcpServerMono : SfMonoSingleton<SfKcpServerMono>
    {
        public enum RunMode
        {
            Server,
            Client
        }

        [Header("运行模式")] public RunMode runMode = RunMode.Server;
        [Header("IP")] public string ip;
        [Header("端口")] public int port = 40001;
        [Header("会话 ID (conv，两端需一致)")] public uint conv = 1;
        [Header("KCP Update 间隔 (ms)")] public int frameInterval = 10;
        [Header("自动启动")] public bool autoStart = true;
        [Header("打印消息日志")] public bool printLog = true;

        SfKcpServer _server;
        SfKcpClient _client;

        /// <summary>收到消息（UTF-8 文本）</summary>
        public event Action<string> Received;
        /// <summary>收到消息（ip, port, 文本）</summary>
        public event Action<string, int, string> ReceivedIPPort;
        /// <summary>服务端：客户端接入</summary>
        public event Action<string, int> ClientConnected;
        /// <summary>服务端：客户端断开</summary>
        public event Action<string, int> ClientDisconnected;

        public void Start()
        {
            if (autoStart) Init();
        }

        /// <summary>启动 KCP 服务或连接</summary>
        public void Init()
        {
            if (runMode == RunMode.Client)
                StartClient();
            else
                StartServer();
        }

        void StartServer()
        {
            if (_server != null && _server.IsRunning) return;

            if (string.IsNullOrWhiteSpace(ip))
                ip = SfUDPServer.GetMainLocalIP();

            var mainThread = SynchronizationContext.Current;
            try
            {
                _server = SfKcpServer.Start(ip, port);
                if (_server == null) return;
                _server.FrameInterval = frameInterval;

                _server.OnClientConnected += (clientIp, clientPort, _) =>
                {
                    if (printLog)
                        Debug.Log($"[SfKcpServerMono] 客户端接入 {clientIp}:{clientPort}");
                    mainThread?.Post(_ => ClientConnected?.Invoke(clientIp, clientPort), null);
                };

                _server.OnClientDisconnected += (clientIp, clientPort) =>
                {
                    if (printLog)
                        Debug.Log($"[SfKcpServerMono] 客户端断开 {clientIp}:{clientPort}");
                    mainThread?.Post(_ => ClientDisconnected?.Invoke(clientIp, clientPort), null);
                };

                _server.OnReceived += (clientIp, clientPort, data) =>
                {
                    var msg = Encoding.UTF8.GetString(data);
                    if (printLog)
                        Debug.Log($"[SfKcpServerMono] 收到 {clientIp}:{clientPort} -> {msg}");
                    mainThread?.Post(_ =>
                    {
                        Received?.Invoke(msg);
                        ReceivedIPPort?.Invoke(clientIp, clientPort, msg);
                    }, null);
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpServerMono] 服务端启动失败: {e.Message}");
            }
        }

        void StartClient()
        {
            if (_client != null && _client.IsConnected) return;

            var mainThread = SynchronizationContext.Current;
            try
            {
                _client = SfKcpClient.Connect(ip, port, conv, frameInterval);
                if (_client == null) return;

                _client.OnReceived += data =>
                {
                    var msg = Encoding.UTF8.GetString(data);
                    if (printLog)
                        Debug.Log($"[SfKcpServerMono] 收到服务端消息: {msg}");
                    mainThread?.Post(_ =>
                    {
                        Received?.Invoke(msg);
                        ReceivedIPPort?.Invoke(ip, port, msg);
                    }, null);
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpServerMono] 客户端连接失败: {e.Message}");
            }
        }

        void Reset()
        {
            ip = SfUDPServer.GetMainLocalIP();
        }

        void OnDestroy()
        {
            if (runMode == RunMode.Client)
                _client?.Stop();
            else
                _server?.Stop();
        }

        /// <summary>发送文本消息</summary>
        public void Send(string msg)
        {
            if (runMode == RunMode.Client)
                _client?.Send(msg);
            else
                Debug.LogWarning("[SfKcpServerMono] 服务端请使用 Send(ip, port, msg) 指定目标");
        }

        /// <summary>服务端：向指定客户端发送</summary>
        public void Send(string targetIp, int targetPort, string msg)
        {
            if (runMode != RunMode.Server)
            {
                _client?.Send(msg);
                return;
            }
            _server?.Send(targetIp, targetPort, msg);
        }

        /// <summary>服务端：广播给所有已连接客户端</summary>
        public void SendAll(string msg)
        {
            if (runMode != RunMode.Server || _server == null) return;
            var data = Encoding.UTF8.GetBytes(msg);
            _server.Broadcast(data);
        }

        /// <summary>发送二进制数据</summary>
        public void SendBytes(string targetIp, int targetPort, byte[] data)
        {
            if (runMode == RunMode.Client)
                _client?.Send(data);
            else
                _server?.Send(targetIp, targetPort, data);
        }
    }
}
