using System;
using System.Collections.Generic;
using SFramework.Core.Module.Config;
using SFramework.Core.Mono;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SFramework.Core.Support.UDP
{
    /// <summary>
    /// UDP 接受配置文件
    /// </summary>
    public class SongUDPReceive : MonoBehaviour
    {
        [Header("UDP")] public SongUDPServerMono songUDPServerMono;
        [Header("UDP 接收事件配置")] public List<ReceiveEvent> receives = new List<ReceiveEvent>();
        
        /// <summary>
        /// 初始化检测
        /// </summary>
        public void Start()
        {
            Reset();

            //设置监听事件
            SongUDPServerMono.Instance.Received += msg =>
            {
                var trim = msg.Trim();
                foreach (var receiveEvent in receives)
                {
                    if(receiveEvent.receive==trim)
                        receiveEvent.unityEvent?.Invoke();
                }
            };
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Reset()
        {
            if (songUDPServerMono == null)
                songUDPServerMono = GetComponent<SongUDPServerMono>();
        }
    }

    /// <summary>
    /// 监听事件
    /// </summary>
    [System.Serializable]
    public class ReceiveEvent
    {
        /// <summary>
        /// 接收到消息
        /// </summary>
        public string receive;

        /// <summary>
        /// 触发事件
        /// </summary>
        public UnityEvent unityEvent;
    }
}