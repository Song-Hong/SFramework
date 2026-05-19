using System;
using System.Collections.Generic;

namespace SFramework.SFMultiple.Data
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 心跳包
        /// </summary>
        Heartbeat = 0,
        /// <summary>
        /// 玩家加入
        /// </summary>
        PlayerJoin = 1,
        /// <summary>
        /// 玩家离开
        /// </summary>
        PlayerLeave = 2,
        /// <summary>
        /// 聊天消息
        /// </summary>
        Chat = 3,
        /// <summary>
        /// 玩家状态同步
        /// </summary>
        PlayerState = 4,
        /// <summary>
        /// 自定义消息
        /// </summary>
        Custom = 100,
    }
    
    /// <summary>
    /// 玩家信息
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public int playerId;
        /// <summary>
        /// 玩家名称
        /// </summary>
        public string playerName;
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool isOnline;
        /// <summary>
        /// 加入时间
        /// </summary>
        public long joinTime;
        
        public PlayerInfo() { }
        
        public PlayerInfo(int id, string name)
        {
            playerId = id;
            playerName = name;
            isOnline = true;
            joinTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
    
    /// <summary>
    /// 网络消息
    /// </summary>
    [Serializable]
    public class NetworkMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType type;
        /// <summary>
        /// 发送者ID
        /// </summary>
        public int senderId;
        /// <summary>
        /// 消息内容 (JSON格式)
        /// </summary>
        public string content;
        /// <summary>
        /// 时间戳
        /// </summary>
        public long timestamp;
        
        public NetworkMessage()
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        public NetworkMessage(MessageType type, int senderId, string content)
        {
            this.type = type;
            this.senderId = senderId;
            this.content = content;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
    
    /// <summary>
    /// 房间信息
    /// </summary>
    [Serializable]
    public class RoomInfo
    {
        /// <summary>
        /// 房间ID
        /// </summary>
        public string roomId;
        /// <summary>
        /// 房间名称
        /// </summary>
        public string roomName;
        /// <summary>
        /// 最大玩家数
        /// </summary>
        public int maxPlayers;
        /// <summary>
        /// 当前玩家列表
        /// </summary>
        public List<PlayerInfo> players = new List<PlayerInfo>();
        
        public RoomInfo() { }
        
        public RoomInfo(string id, string name, int maxPlayers = 10)
        {
            roomId = id;
            roomName = name;
            this.maxPlayers = maxPlayers;
        }
        
        /// <summary>
        /// 当前玩家数
        /// </summary>
        public int PlayerCount => players.Count;
        
        /// <summary>
        /// 房间是否已满
        /// </summary>
        public bool IsFull => players.Count >= maxPlayers;
    }
}
