using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.Core.Support.UDP
{
    /// <summary>
    /// UDP 消息转发
    /// </summary>
    public class SongUDPTranspondSupport:MonoBehaviour,ISongUDPSupport
    {
        /// <summary>
        /// 消息转发
        /// </summary>
        [Header("转发事件")]
        public List<SongUDPTranspondEvent> transponds = new List<SongUDPTranspondEvent>();
        
        public void Init()
        {
            
        }
    }

    /// <summary>
    /// Song UDP 转发事件
    /// </summary>
    [System.Serializable]
    public class SongUDPTranspondEvent
    {
        /// <summary>
        /// 转发接口名
        /// </summary>
        public string transpondName;
        /// <summary>
        /// 转发指令
        /// </summary>
        public string transpondContent;
    }
}