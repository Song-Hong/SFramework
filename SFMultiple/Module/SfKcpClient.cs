using System;
using System.Threading.Tasks;
using SFramework.SFMultiple.Data;
using SFramework.SFMultiple.Support;
using UnityEngine;

namespace SFramework.SFMultiple.Module
{
    /// <summary>
    /// KCP 客户端
    /// </summary>
    public class SfKcpClient
    {
        #region 变量
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string serverIp;
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int serverPort;
        /// <summary>
        /// 帧间隔 (毫秒)
        /// </summary>
        public int frameInterval = 10;
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected { get; private set; }
        /// <summary>
        /// 当前玩家ID
        /// </summary>
        public int PlayerId { get; private set; }
        /// <summary>
        /// 是否正在运行
        /// </summary>
        private volatile bool _isRunning;
        /// <summary>
        /// KCP 客户端
        /// </summary>
        private System.Net.Sockets.Kcp.Simple.SimpleKcpClient _kcpClient;
        #endregion
        
        #region 事件
        /// <summary>
        /// 连接到服务器回调
        /// </summary>
        public Action OnConnected;
        /// <summary>
        /// 断开连接回调
        /// </summary>
        public Action OnDisconnected;
        /// <summary>
        /// 接收到消息回调
        /// </summary>
        public Action<NetworkMessage> OnMessageReceived;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public SfKcpClient() { }
        
        /// <summary>
        /// 连接到服务器
        /// </summary>
        public bool Connect(string ip, int port)
        {
            if (IsConnected)
            {
                Debug.LogWarning("[SfKcpClient] 已连接到服务器");
                return false;
            }
            
            try
            {
                serverIp = ip;
                serverPort = port;
                _kcpClient = new System.Net.Sockets.Kcp.Simple.SimpleKcpClient(port);
                _isRunning = true;
                IsConnected = true;
                
                // 启动 KCP Update 循环
                Task.Run(async () =>
                {
                    while (_isRunning)
                    {
                        _kcpClient.kcp.Update(DateTimeOffset.UtcNow);
                        await Task.Delay(frameInterval);
                    }
                });
                
                // 启动接收循环
                StartReceiveLoop();
                
                OnConnected?.Invoke();
                Debug.Log($"[SfKcpClient] 已连接到服务器 {ip}:{port}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpClient] 连接失败: {e.Message}");
                IsConnected = false;
                return false;
            }
        }
        #endregion

        #region 监听
        /// <summary>
        /// 接收循环
        /// </summary>
        private async void StartReceiveLoop()
        {
            while (_isRunning)
            {
                try
                {
                    byte[] data = await _kcpClient.ReceiveAsync();
                    if (data == null || data.Length == 0) continue;
                    
                    var message = SfMessageSerializer.Deserialize(data);
                    if (message == null) continue;
                    
                    OnMessageReceived?.Invoke(message);
                }
                catch (Exception e)
                {
                    if (_isRunning)
                    {
                        Debug.LogError($"[SfKcpClient] 接收异常: {e.Message}");
                    }
                    await Task.Delay(100);
                }
            }
        }
        #endregion
        
        #region 发送消息
        /// <summary>
        /// 发送消息
        /// </summary>
        public void Send(NetworkMessage message)
        {
            if (!IsConnected || _kcpClient == null) return;
            
            try
            {
                var data = SfMessageSerializer.Serialize(message);
                _kcpClient.SendAsync(data, data.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpClient] 发送失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 发送加入请求
        /// </summary>
        public void SendJoinRequest(string playerName)
        {
            var player = new PlayerInfo(0, playerName);
            var message = new NetworkMessage(MessageType.PlayerJoin, 0, 
                SfMessageSerializer.SerializePlayer(player));
            Send(message);
        }
        
        /// <summary>
        /// 发送离开请求
        /// </summary>
        public void SendLeaveRequest()
        {
            var message = new NetworkMessage(MessageType.PlayerLeave, PlayerId, "");
            Send(message);
        }
        
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        public void SendChat(string content)
        {
            var message = new NetworkMessage(MessageType.Chat, PlayerId, content);
            Send(message);
        }
        
        /// <summary>
        /// 发送状态同步
        /// </summary>
        public void SendState(string stateJson)
        {
            var message = new NetworkMessage(MessageType.PlayerState, PlayerId, stateJson);
            Send(message);
        }
        
        /// <summary>
        /// 发送心跳
        /// </summary>
        public void SendHeartbeat()
        {
            var message = new NetworkMessage(MessageType.Heartbeat, PlayerId, "");
            Send(message);
        }
        #endregion
        
        #region 断开连接
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected) return;
            
            _isRunning = false;
            IsConnected = false;
            
            // 发送离开消息
            SendLeaveRequest();
            
            OnDisconnected?.Invoke();
            Debug.Log("[SfKcpClient] 已断开连接");
        }
        #endregion
    }
}
