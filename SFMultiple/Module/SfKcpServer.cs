using System;
using System.Collections.Generic;
using System.Net.Sockets.Kcp.Simple;
using System.Threading.Tasks;
using SFramework.SFMultiple.Data;
using SFramework.SFMultiple.Support;
using UnityEngine;

namespace SFramework.SFMultiple.Module
{
    /// <summary>
    /// KCP 服务器
    /// </summary>
    public class SfKcpServer
    {
        #region 变量
        /// <summary>
        /// 端口号
        /// </summary>
        public int port = 40001;
        /// <summary>
        /// 帧间隔 (毫秒)
        /// </summary>
        public int frameInterval = 10;
        /// <summary>
        /// 是否正在运行
        /// </summary>
        private volatile bool _isRunning;
        /// <summary>
        /// KCP 客户端
        /// </summary>
        private SimpleKcpClient _kcpClient;
        /// <summary>
        /// 玩家列表 (连接ID -> 玩家信息)
        /// </summary>
        private Dictionary<int, PlayerInfo> _players = new Dictionary<int, PlayerInfo>();
        /// <summary>
        /// 下一个玩家ID
        /// </summary>
        private int _nextPlayerId = 1;
        #endregion
        
        #region 事件
        /// <summary>
        /// 接收到消息回调
        /// </summary>
        public Action<int, NetworkMessage> OnMessageReceived;
        /// <summary>
        /// 玩家加入回调
        /// </summary>
        public Action<PlayerInfo> OnPlayerJoined;
        /// <summary>
        /// 玩家离开回调
        /// </summary>
        public Action<PlayerInfo> OnPlayerLeft;
        /// <summary>
        /// 服务器启动回调
        /// </summary>
        public Action OnServerStarted;
        /// <summary>
        /// 服务器停止回调
        /// </summary>
        public Action OnServerStopped;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数：初始化 KCP 服务器
        /// </summary>
        public SfKcpServer() { }
        
        /// <summary>
        /// 启动服务器
        /// </summary>
        public bool Start(int port = 40001)
        {
            if (_isRunning)
            {
                Debug.LogWarning("[SfKcpServer] 服务器已在运行");
                return false;
            }
            
            try
            {
                this.port = port;
                _kcpClient = new SimpleKcpClient(port);
                _isRunning = true;
                
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
                StartReceiveLoop(_kcpClient);
                
                OnServerStarted?.Invoke();
                Debug.Log($"[SfKcpServer] KCP服务器启动成功，监听端口: {port}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpServer] 服务器启动失败: {e.Message}");
                return false;
            }
        }
        #endregion

        #region 监听
        /// <summary>
        /// 接收循环
        /// </summary>
        private async void StartReceiveLoop(SimpleKcpClient client)
        {
            while (_isRunning)
            {
                try
                {
                    byte[] data = await client.ReceiveAsync();
                    if (data == null || data.Length == 0) continue;
                    
                    var message = SfMessageSerializer.Deserialize(data);
                    if (message == null) continue;
                    
                    // 处理不同类型的消息
                    HandleMessage(message);
                    
                    OnMessageReceived?.Invoke(message.senderId, message);
                }
                catch (Exception e)
                {
                    if (_isRunning)
                    {
                        Debug.LogError($"[SfKcpServer] 接收异常: {e.Message}");
                    }
                    await Task.Delay(100);
                }
            }
        }
        
        /// <summary>
        /// 处理消息
        /// </summary>
        private void HandleMessage(NetworkMessage message)
        {
            switch (message.type)
            {
                case MessageType.Heartbeat:
                    HandleHeartbeat(message);
                    break;
                case MessageType.PlayerJoin:
                    HandlePlayerJoin(message);
                    break;
                case MessageType.PlayerLeave:
                    HandlePlayerLeave(message);
                    break;
                case MessageType.Chat:
                    HandleChat(message);
                    break;
                case MessageType.PlayerState:
                    HandlePlayerState(message);
                    break;
            }
        }
        
        /// <summary>
        /// 处理心跳
        /// </summary>
        private void HandleHeartbeat(NetworkMessage message)
        {
            // 回复心跳
            var ack = new NetworkMessage(MessageType.Heartbeat, 0, "ACK");
            Broadcast(ack);
        }
        
        /// <summary>
        /// 处理玩家加入
        /// </summary>
        private void HandlePlayerJoin(NetworkMessage message)
        {
            var player = SfMessageSerializer.DeserializePlayer(message.content);
            if (player == null) return;
            
            player.playerId = _nextPlayerId++;
            player.isOnline = true;
            _players[player.playerId] = player;
            
            // 通知所有玩家有新玩家加入
            var joinMsg = new NetworkMessage(MessageType.PlayerJoin, player.playerId, 
                SfMessageSerializer.SerializePlayer(player));
            Broadcast(joinMsg);
            
            OnPlayerJoined?.Invoke(player);
            Debug.Log($"[SfKcpServer] 玩家加入: {player.playerName} (ID: {player.playerId})");
        }
        
        /// <summary>
        /// 处理玩家离开
        /// </summary>
        private void HandlePlayerLeave(NetworkMessage message)
        {
            if (_players.TryGetValue(message.senderId, out var player))
            {
                player.isOnline = false;
                _players.Remove(message.senderId);
                
                var leaveMsg = new NetworkMessage(MessageType.PlayerLeave, player.playerId,
                    SfMessageSerializer.SerializePlayer(player));
                Broadcast(leaveMsg);
                
                OnPlayerLeft?.Invoke(player);
                Debug.Log($"[SfKcpServer] 玩家离开: {player.playerName} (ID: {player.playerId})");
            }
        }
        
        /// <summary>
        /// 处理聊天消息
        /// </summary>
        private void HandleChat(NetworkMessage message)
        {
            // 广播给所有其他玩家
            Broadcast(message, message.senderId);
        }
        
        /// <summary>
        /// 处理玩家状态同步
        /// </summary>
        private void HandlePlayerState(NetworkMessage message)
        {
            // 广播给所有其他玩家
            Broadcast(message, message.senderId);
        }
        #endregion
        
        #region 发送消息
        /// <summary>
        /// 广播消息给所有玩家
        /// </summary>
        public void Broadcast(NetworkMessage message, int excludeId = -1)
        {
            if (!_isRunning || _kcpClient == null) return;
            
            try
            {
                var data = SfMessageSerializer.Serialize(message);
                _kcpClient.SendAsync(data, data.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpServer] 广播失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 发送消息给指定玩家
        /// </summary>
        public void SendTo(int playerId, NetworkMessage message)
        {
            if (!_isRunning || _kcpClient == null) return;
            
            try
            {
                var data = SfMessageSerializer.Serialize(message);
                _kcpClient.SendAsync(data, data.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfKcpServer] 发送失败: {e.Message}");
            }
        }
        #endregion
        
        #region 玩家管理
        /// <summary>
        /// 获取所有在线玩家
        /// </summary>
        public List<PlayerInfo> GetOnlinePlayers()
        {
            var onlinePlayers = new List<PlayerInfo>();
            foreach (var player in _players.Values)
            {
                if (player.isOnline)
                {
                    onlinePlayers.Add(player);
                }
            }
            return onlinePlayers;
        }
        
        /// <summary>
        /// 获取玩家数量
        /// </summary>
        public int GetPlayerCount() => _players.Count;
        
        /// <summary>
        /// 获取玩家信息
        /// </summary>
        public PlayerInfo GetPlayer(int playerId)
        {
            return _players.TryGetValue(playerId, out var player) ? player : null;
        }
        #endregion
        
        #region 工具
        /// <summary>
        /// 获取本机主要的本地IPv4地址
        /// </summary>
        public static string GetMainLocalIP() 
        {
            var ips = GetLocalIPs();
            foreach (var ip in ips)
            {
                if (ip.StartsWith("192.168."))
                {
                    return ip;
                }
            }
            return ips.Count > 0 ? ips[0] : "127.0.0.1";
        }
        
        /// <summary>
        /// 获取本机所有IPv4地址列表
        /// </summary>
        public static List<string> GetLocalIPs()
        {
            var ipv4Addresses = new List<string>();
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipv4Addresses.Add(ip.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"获取本地IP时发生错误: {ex.Message}");
            }
            return ipv4Addresses;
        }
        #endregion
        
        #region 断开连接
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            
            // 通知所有玩家服务器关闭
            var shutdownMsg = new NetworkMessage(MessageType.PlayerLeave, 0, "ServerShutdown");
            Broadcast(shutdownMsg);
            
            _players.Clear();
            
            OnServerStopped?.Invoke();
            Debug.Log("[SfKcpServer] 服务器已停止");
        }
        #endregion
    }
}
