using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace SFramework.SFNet.Module.Kcp
{
    /// <summary>
    /// KCP 客户端：基于 UDP + KCP 可靠传输
    /// </summary>
    public class SfKcpClient
    {
        #region 变量

        public string IP { get; private set; }
        public int Port { get; private set; }
        public uint Conv { get; set; } = 1;
        public int FrameInterval { get; set; } = 10;
        public bool IsConnected { get; private set; }

        Socket _socket;
        SfKcp _kcp;
        volatile bool _running;
        Thread _receiveThread;
        Thread _updateThread;
        readonly byte[] _recvBuffer = new byte[SfKcpConst.MtuDef];
        readonly byte[] _kcpRecvBuffer = new byte[1024 * 64];

        #endregion

        #region 事件

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<byte[]> OnReceived;

        #endregion

        #region 连接

        private SfKcpClient() { }

        public static SfKcpClient Connect(string ip, int port, uint conv = 1, int frameInterval = 10)
        {
            var client = new SfKcpClient { Conv = conv, FrameInterval = frameInterval };
            return client.ConnectInternal(ip, port) ? client : null;
        }

        bool ConnectInternal(string ip, int port)
        {
            if (IsConnected)
            {
                Debug.LogWarning("[SfKcpClient] 已连接");
                return false;
            }

            try
            {
                IP = ip;
                Port = port;

                var remote = new IPEndPoint(IPAddress.Parse(ip), port);
                _socket = new Socket(remote.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                _socket.Connect(remote);

                _kcp = new SfKcp(Conv, (data, len) =>
                {
                    if (_socket == null || len <= 0) return;
                    _socket.Send(data, 0, len, SocketFlags.None);
                });

                _kcp.SetNoDelay(1, (uint)FrameInterval, 2, true);

                _running = true;
                IsConnected = true;

                _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                _receiveThread.Start();

                _updateThread = new Thread(UpdateLoop) { IsBackground = true };
                _updateThread.Start();

                OnConnected?.Invoke();
                Debug.Log($"[SfKcpClient] 已连接 {ip}:{port} conv={Conv}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpClient] 连接失败: {e.Message}");
                Stop();
                return false;
            }
        }

        #endregion

        #region 收发

        void ReceiveLoop()
        {
            while (_running)
            {
                try
                {
                    if (_socket == null) break;
                    int len = _socket.Receive(_recvBuffer);
                    if (len <= 0) continue;

                    _kcp?.Input(_recvBuffer, 0, len);
                    DrainReceiveQueue();
                }
                catch (SocketException)
                {
                    if (!_running) break;
                }
                catch (Exception e)
                {
                    if (_running)
                        Debug.LogError($"[SfKcpClient] 接收异常: {e.Message}");
                }
            }
        }

        void UpdateLoop()
        {
            while (_running)
            {
                try
                {
                    _kcp?.Update((uint)Environment.TickCount);
                    DrainReceiveQueue();
                    Thread.Sleep(FrameInterval);
                }
                catch (Exception e)
                {
                    if (_running)
                        Debug.LogError($"[SfKcpClient] Update 异常: {e.Message}");
                }
            }
        }

        void DrainReceiveQueue()
        {
            if (_kcp == null) return;

            while (true)
            {
                int size = _kcp.PeekSize();
                if (size <= 0) break;
                if (size > _kcpRecvBuffer.Length)
                {
                    Debug.LogError($"[SfKcpClient] 消息过大: {size}");
                    break;
                }

                int received = _kcp.Receive(_kcpRecvBuffer, size);
                if (received < 0) break;

                var data = new byte[received];
                Buffer.BlockCopy(_kcpRecvBuffer, 0, data, 0, received);
                OnReceived?.Invoke(data);
            }
        }

        public void Send(byte[] data)
        {
            if (!IsConnected || _kcp == null || data == null) return;
            try
            {
                _kcp.Send(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpClient] 发送失败: {e.Message}");
            }
        }

        public void Send(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            Send(System.Text.Encoding.UTF8.GetBytes(msg));
        }

        #endregion

        #region 断开

        public void Stop()
        {
            if (!IsConnected && !_running && _socket == null) return;

            _running = false;
            IsConnected = false;

            try { _socket?.Close(); } catch { /* ignore */ }
            _socket = null;
            _kcp = null;

            if (_receiveThread != null && _receiveThread.IsAlive)
                _receiveThread.Join(1000);
            if (_updateThread != null && _updateThread.IsAlive)
                _updateThread.Join(1000);

            _receiveThread = null;
            _updateThread = null;

            OnDisconnected?.Invoke();
            Debug.Log("[SfKcpClient] 已断开");
        }

        #endregion
    }
}
