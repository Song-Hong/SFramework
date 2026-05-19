using UnityEngine;
using System.Text;
using System.Threading;
using SFramework.SFMultiple.Module;
using SFramework.SFMultiple.Data;

namespace SFramework.SFMultiple.Mono
{
    /// <summary>
    /// KCP 管理器 (客户端示例)
    /// </summary>
    public class KcpManager : MonoBehaviour
    {
        [Tooltip("服务器IP")]
        public string ServerIp = "127.0.0.1";
        [Tooltip("服务器端口")]
        public int Port = 40001;
        [Tooltip("玩家名称")]
        public string PlayerName = "Player1";
        
        private SfKcpClient _kcpClient;

        void Awake()
        {
            _kcpClient = new SfKcpClient();
            _kcpClient.OnConnected += OnConnected;
            _kcpClient.OnDisconnected += OnDisconnected;
            _kcpClient.OnMessageReceived += HandleReceivedMessage;
            
            Debug.Log($"[KcpManager] 客户端初始化完成，准备连接 {ServerIp}:{Port}");
        }

        void Start()
        {
            _kcpClient.Connect(ServerIp, Port);
        }
        
        void Update()
        {
            // 定期发送心跳
            if (_kcpClient.IsConnected)
            {
                // 可以在这里处理心跳逻辑
            }
        }

        /// <summary>
        /// 连接成功回调
        /// </summary>
        private void OnConnected()
        {
            Debug.Log($"[KcpManager] 连接成功，发送加入请求: {PlayerName}");
            _kcpClient.SendJoinRequest(PlayerName);
        }
        
        /// <summary>
        /// 断开连接回调
        /// </summary>
        private void OnDisconnected()
        {
            Debug.Log("[KcpManager] 已断开连接");
        }

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        private void HandleReceivedMessage(NetworkMessage message)
        {
            string threadInfo = $"(Thread: {Thread.CurrentThread.ManagedThreadId})";

            switch (message.type)
            {
                case MessageType.PlayerJoin:
                    var joinPlayer = SfMessageSerializer.DeserializePlayer(message.content);
                    if (joinPlayer != null)
                    {
                        Debug.Log($"[KcpManager {threadInfo}] 玩家加入: {joinPlayer.playerName} (ID: {joinPlayer.playerId})");
                    }
                    break;
                case MessageType.PlayerLeave:
                    var leavePlayer = SfMessageSerializer.DeserializePlayer(message.content);
                    if (leavePlayer != null)
                    {
                        Debug.Log($"[KcpManager {threadInfo}] 玩家离开: {leavePlayer.playerName}");
                    }
                    break;
                case MessageType.Chat:
                    Debug.Log($"[KcpManager {threadInfo}] 聊天消息 [ID:{message.senderId}]: {message.content}");
                    break;
                case MessageType.PlayerState:
                    Debug.Log($"[KcpManager {threadInfo}] 状态同步 [ID:{message.senderId}]: {message.content}");
                    break;
                case MessageType.Heartbeat:
                    // 心跳回复，不需要处理
                    break;
            }
        }

        /// <summary>
        /// 发送聊天消息 (供外部调用)
        /// </summary>
        public void SendChat(string content)
        {
            if (_kcpClient != null && _kcpClient.IsConnected)
            {
                _kcpClient.SendChat(content);
            }
        }
        
        /// <summary>
        /// 发送状态同步 (供外部调用)
        /// </summary>
        public void SendState(string stateJson)
        {
            if (_kcpClient != null && _kcpClient.IsConnected)
            {
                _kcpClient.SendState(stateJson);
            }
        }

        void OnApplicationQuit()
        {
            if (_kcpClient != null)
            {
                _kcpClient.Disconnect();
            }
            Debug.Log("[KcpManager] 客户端已关闭");
        }
    }
}
