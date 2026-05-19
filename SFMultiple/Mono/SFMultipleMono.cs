using System;
using System.Text;
using SFramework.Core.Mono;
using SFramework.SFMultiple.Data;
using SFramework.SFMultiple.Module;
using UnityEngine;

namespace SFramework.SFMultiple.Mono
{
    /// <summary>
    /// 多人模式管理器
    /// </summary>
    public class SfMultipleMono : SfMonoSingleton<SfMultipleMono>
    {
        #region 配置
        /// <summary>
        /// 运行模式
        /// </summary>
        public enum RunMode
        {
            /// 服务器模式
            Server,
            /// 客户端模式
            Client,
        }
        
        [Header("运行模式")] public RunMode runMode = RunMode.Server;
        [Header("服务器IP")] public string serverIp = "127.0.0.1";
        [Header("端口号")] public int port = 40001;
        [Header("玩家名称")] public string playerName = "Player";
        [Header("打印日志")] public bool printLog = true;
        #endregion
        
        #region 变量
        /// <summary>
        /// KCP 服务器
        /// </summary>
        private SfKcpServer _server;
        /// <summary>
        /// KCP 客户端
        /// </summary>
        private SfKcpClient _client;
        #endregion
        
        #region 事件
        /// <summary>
        /// 接收到消息
        /// </summary>
        public event Action<NetworkMessage> OnMessageReceived;
        /// <summary>
        /// 玩家加入
        /// </summary>
        public event Action<PlayerInfo> OnPlayerJoined;
        /// <summary>
        /// 玩家离开
        /// </summary>
        public event Action<PlayerInfo> OnPlayerLeft;
        #endregion
        
        #region 生命周期
        private void Start()
        {
            if (runMode == RunMode.Server)
            {
                StartServer();
            }
            else
            {
                StartClient();
            }
        }
        
        private void Update()
        {
            // 心跳发送 (每3秒)
            if (_client != null && _client.IsConnected)
            {
                // 心跳逻辑可以在这里处理
            }
        }
        
        private void OnDestroy()
        {
            Stop();
        }
        #endregion
        
        #region 服务器
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer()
        {
            _server = new SfKcpServer();
            _server.OnMessageReceived += (senderId, message) =>
            {
                if (printLog) Debug.Log($"[Server] 收到消息: {message.type} from {senderId}");
                OnMessageReceived?.Invoke(message);
            };
            _server.OnPlayerJoined += (player) =>
            {
                if (printLog) Debug.Log($"[Server] 玩家加入: {player.playerName}");
                OnPlayerJoined?.Invoke(player);
            };
            _server.OnPlayerLeft += (player) =>
            {
                if (printLog) Debug.Log($"[Server] 玩家离开: {player.playerName}");
                OnPlayerLeft?.Invoke(player);
            };
            
            _server.Start(port);
        }
        #endregion
        
        #region 客户端
        /// <summary>
        /// 启动客户端
        /// </summary>
        public void StartClient()
        {
            _client = new SfKcpClient();
            _client.OnConnected += () =>
            {
                if (printLog) Debug.Log("[Client] 已连接到服务器");
                // 发送加入请求
                _client.SendJoinRequest(playerName);
            };
            _client.OnDisconnected += () =>
            {
                if (printLog) Debug.Log("[Client] 已断开连接");
            };
            _client.OnMessageReceived += (message) =>
            {
                if (printLog) Debug.Log($"[Client] 收到消息: {message.type} content: {message.content}");
                OnMessageReceived?.Invoke(message);
                
                // 处理加入成功消息，设置玩家ID
                if (message.type == MessageType.PlayerJoin)
                {
                    var player = SfMessageSerializer.DeserializePlayer(message.content);
                    if (player != null)
                    {
                        _client.PlayerId = player.playerId;
                    }
                }
            };
            
            _client.Connect(serverIp, port);
        }
        #endregion
        
        #region 公共方法
        /// <summary>
        /// 发送消息
        /// </summary>
        public void Send(NetworkMessage message)
        {
            if (runMode == RunMode.Client && _client != null)
            {
                _client.Send(message);
            }
            else if (runMode == RunMode.Server && _server != null)
            {
                _server.Broadcast(message);
            }
        }
        
        /// <summary>
        /// 发送聊天消息
        /// </summary>
        public void SendChat(string content)
        {
            if (runMode == RunMode.Client && _client != null)
            {
                _client.SendChat(content);
            }
            else if (runMode == RunMode.Server && _server != null)
            {
                var msg = new NetworkMessage(MessageType.Chat, 0, content);
                _server.Broadcast(msg);
            }
        }
        
        /// <summary>
        /// 发送状态同步
        /// </summary>
        public void SendState(string stateJson)
        {
            if (runMode == RunMode.Client && _client != null)
            {
                _client.SendState(stateJson);
            }
        }
        
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (runMode == RunMode.Client && _client != null)
            {
                _client.Disconnect();
            }
            else if (runMode == RunMode.Server && _server != null)
            {
                _server.Stop();
            }
        }
        #endregion
        
        #region 编辑器
        private void Reset()
        {
            serverIp = SfKcpServer.GetMainLocalIP();
        }
        #endregion
    }
}
