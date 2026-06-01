using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SFramework.SFNet.Module.Udp;
using UnityEngine;

namespace SFramework.SFNet.Module.Kcp
{
    /// <summary>
    /// KCP 服务端：UDP 监听，按远端 EndPoint 维护独立 KCP 会话
    /// </summary>
    public class SfKcpServer
    {
        #region 会话

        class SfKcpSession
        {
            public IPEndPoint Remote;
            public SfKcp Kcp;
            public readonly byte[] RecvBuffer = new byte[1024 * 64];
        }

        #endregion

        #region 变量

        public string IP { get; private set; }
        public int Port { get; private set; }
        public int FrameInterval { get; set; } = 10;
        public bool IsRunning { get; private set; }

        Socket _socket;
        volatile bool _running;
        Thread _receiveThread;
        Thread _updateThread;
        readonly byte[] _recvBuffer = new byte[SfKcpConst.MtuDef];
        EndPoint _remoteEp;

        readonly Dictionary<string, SfKcpSession> _sessions = new Dictionary<string, SfKcpSession>();
        readonly object _sessionsLock = new object();

        #endregion

        #region 事件

        /// <summary>新客户端连接 (ip, port, conv)</summary>
        public event Action<string, int, uint> OnClientConnected;
        /// <summary>客户端断开</summary>
        public event Action<string, int> OnClientDisconnected;
        /// <summary>收到可靠消息</summary>
        public event Action<string, int, byte[]> OnReceived;

        #endregion

        #region 启动

        private SfKcpServer() { }

        public static SfKcpServer Start(int port) =>
            Start(SfUDPServer.GetMainLocalIP(), port);

        public static SfKcpServer Start(string ip, int port)
        {
            var server = new SfKcpServer();
            return server.StartInternal(ip, port) ? server : null;
        }

        bool StartInternal(string ip, int port)
        {
            if (IsRunning)
            {
                Debug.LogWarning("[SfKcpServer] 已在运行");
                return false;
            }

            try
            {
                IP = ip;
                Port = port;
                _remoteEp = new IPEndPoint(IPAddress.Any, 0);

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

                _running = true;
                IsRunning = true;

                _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                _receiveThread.Start();

                _updateThread = new Thread(UpdateLoop) { IsBackground = true };
                _updateThread.Start();

                Debug.Log($"[SfKcpServer] 启动成功 {ip}:{port}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpServer] 启动失败: {e.Message}");
                Stop();
                return false;
            }
        }

        #endregion

        #region 收发

        static string SessionKey(string ip, int port) => $"{ip}:{port}";

        SfKcpSession GetOrCreateSession(IPEndPoint remote, uint conv)
        {
            var key = SessionKey(remote.Address.ToString(), remote.Port);
            lock (_sessionsLock)
            {
                if (_sessions.TryGetValue(key, out var session))
                    return session;

                session = new SfKcpSession
                {
                    Remote = remote,
                    Kcp = new SfKcp(conv, (data, len) =>
                    {
                        if (_socket == null || len <= 0) return;
                        _socket.SendTo(data, 0, len, SocketFlags.None, remote);
                    })
                };
                session.Kcp.SetNoDelay(1, (uint)FrameInterval, 2, true);
                _sessions[key] = session;

                OnClientConnected?.Invoke(remote.Address.ToString(), remote.Port, conv);
                Debug.Log($"[SfKcpServer] 新会话 {key} conv={conv}");
                return session;
            }
        }

        void ReceiveLoop()
        {
            while (_running)
            {
                try
                {
                    if (_socket == null) break;
                    int len = _socket.ReceiveFrom(_recvBuffer, ref _remoteEp);
                    if (len < SfKcpConst.Overhead) continue;
                    if (_remoteEp is not IPEndPoint remote) continue;

                    uint conv = SfKcpSegment.GetConv(_recvBuffer);
                    if (conv == 0) continue;

                    var session = GetOrCreateSession(remote, conv);
                    session.Kcp.Input(_recvBuffer, 0, len);
                    DrainSession(session, remote.Address.ToString(), remote.Port);
                }
                catch (SocketException)
                {
                    if (!_running) break;
                }
                catch (Exception e)
                {
                    if (_running)
                        Debug.LogError($"[SfKcpServer] 接收异常: {e.Message}");
                }
            }
        }

        void UpdateLoop()
        {
            while (_running)
            {
                try
                {
                    uint now = (uint)Environment.TickCount;
                    List<SfKcpSession> snapshot;
                    lock (_sessionsLock)
                    {
                        snapshot = new List<SfKcpSession>(_sessions.Values);
                    }

                    foreach (var session in snapshot)
                    {
                        session.Kcp.Update(now);
                        DrainSession(session, session.Remote.Address.ToString(), session.Remote.Port);

                        if (session.Kcp.State < 0)
                            RemoveSession(session.Remote.Address.ToString(), session.Remote.Port);
                    }

                    Thread.Sleep(FrameInterval);
                }
                catch (Exception e)
                {
                    if (_running)
                        Debug.LogError($"[SfKcpServer] Update 异常: {e.Message}");
                }
            }
        }

        void DrainSession(SfKcpSession session, string ip, int port)
        {
            while (true)
            {
                int size = session.Kcp.PeekSize();
                if (size <= 0) break;
                if (size > session.RecvBuffer.Length)
                {
                    Debug.LogError($"[SfKcpServer] 消息过大: {size}");
                    break;
                }

                int received = session.Kcp.Receive(session.RecvBuffer, size);
                if (received < 0) break;

                var data = new byte[received];
                Buffer.BlockCopy(session.RecvBuffer, 0, data, 0, received);
                OnReceived?.Invoke(ip, port, data);
            }
        }

        public void Send(string ip, int port, byte[] data)
        {
            if (!IsRunning || data == null) return;
            var key = SessionKey(ip, port);
            lock (_sessionsLock)
            {
                if (!_sessions.TryGetValue(key, out var session)) return;
                session.Kcp.Send(data);
            }
        }

        public void Send(string ip, int port, string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            Send(ip, port, System.Text.Encoding.UTF8.GetBytes(msg));
        }

        public void Broadcast(byte[] data, string excludeIp = null, int excludePort = -1)
        {
            if (!IsRunning || data == null) return;
            lock (_sessionsLock)
            {
                foreach (var kv in _sessions)
                {
                    var session = kv.Value;
                    var ip = session.Remote.Address.ToString();
                    var port = session.Remote.Port;
                    if (excludeIp != null && ip == excludeIp && port == excludePort) continue;
                    session.Kcp.Send(data);
                }
            }
        }

        void RemoveSession(string ip, int port)
        {
            var key = SessionKey(ip, port);
            lock (_sessionsLock)
            {
                if (_sessions.Remove(key))
                {
                    OnClientDisconnected?.Invoke(ip, port);
                    Debug.Log($"[SfKcpServer] 会话移除 {key}");
                }
            }
        }

        #endregion

        #region 停止

        public void Stop()
        {
            if (!IsRunning && !_running && _socket == null) return;

            _running = false;
            IsRunning = false;

            try { _socket?.Close(); } catch { /* ignore */ }
            _socket = null;

            lock (_sessionsLock) _sessions.Clear();

            if (_receiveThread != null && _receiveThread.IsAlive)
                _receiveThread.Join(1000);
            if (_updateThread != null && _updateThread.IsAlive)
                _updateThread.Join(1000);

            _receiveThread = null;
            _updateThread = null;

            Debug.Log("[SfKcpServer] 已停止");
        }

        #endregion
    }
}
