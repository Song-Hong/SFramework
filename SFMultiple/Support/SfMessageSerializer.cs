using System.Text;
using SFramework.SFMultiple.Data;
using SimpleJSON;
using UnityEngine;

namespace SFramework.SFMultiple.Support
{
    /// <summary>
    /// 消息序列化工具
    /// </summary>
    public static class SfMessageSerializer
    {
        /// <summary>
        /// 序列化网络消息为字节数组
        /// </summary>
        public static byte[] Serialize(NetworkMessage message)
        {
            var json = new JSONObject();
            json["type"] = (int)message.type;
            json["senderId"] = message.senderId;
            json["content"] = message.content;
            json["timestamp"] = message.timestamp;
            
            var jsonString = json.ToString();
            return Encoding.UTF8.GetBytes(jsonString);
        }
        
        /// <summary>
        /// 反序列化字节数组为网络消息
        /// </summary>
        public static NetworkMessage Deserialize(byte[] data)
        {
            try
            {
                var jsonString = Encoding.UTF8.GetString(data);
                var json = JSON.Parse(jsonString) as JSONObject;
                if (json == null) return null;
                
                return new NetworkMessage
                {
                    type = (MessageType)json["type"].AsInt,
                    senderId = json["senderId"].AsInt,
                    content = json["content"].Value,
                    timestamp = json["timestamp"].AsLong,
                };
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// 序列化玩家信息
        /// </summary>
        public static string SerializePlayer(PlayerInfo player)
        {
            var json = new JSONObject();
            json["playerId"] = player.playerId;
            json["playerName"] = player.playerName;
            json["isOnline"] = player.isOnline;
            json["joinTime"] = player.joinTime;
            return json.ToString();
        }
        
        /// <summary>
        /// 反序列化玩家信息
        /// </summary>
        public static PlayerInfo DeserializePlayer(string jsonStr)
        {
            try
            {
                var json = JSON.Parse(jsonStr) as JSONObject;
                if (json == null) return null;
                
                return new PlayerInfo
                {
                    playerId = json["playerId"].AsInt,
                    playerName = json["playerName"].Value,
                    isOnline = json["isOnline"].AsBool,
                    joinTime = json["joinTime"].AsLong,
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
