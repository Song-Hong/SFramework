using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using SFramework.Core.Module.Config;
using SFramework.Core.Mono;
using SFramework.Core.Support.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SFramework.Core.Support.UDP
{
    /// <summary>
    /// UDP 接口
    /// </summary>
    public class SongUDPReceiveSupport : MonoBehaviour,ISongUDPSupport
    {
        [Header("UDP 接收事件配置")] public List<SongUDPReceiveEvent> receives = new List<SongUDPReceiveEvent>();

        /// <summary>
        /// 初始化检测
        /// </summary>
        public void Init()
        {
            //获取配置文件，如果存在则使用配置文件
            var config = Application.streamingAssetsPath + "/SFConfig/UDPReceive.json";
            if (File.Exists(config))
            {
                foreach (var readAllLine in File.ReadAllLines(config))
                {
                    if(readAllLine.Contains("{") || readAllLine.Contains("}"))
                        continue;
                    var values = readAllLine.Replace("\"","").Replace(",","").Split(":");
                    if(values.Length < 2)
                        continue;
                    var key = values[0].Trim();
                    var value = values[1].Trim();
            
                    foreach (var receiveEvent in receives)
                    {
                        if (receiveEvent.receiveName == key)
                        {
                            receiveEvent.receive = value;
                        }
                    }
                }
            }

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
    }
    
    /// <summary>
    /// 监听事件
    /// </summary>
    [System.Serializable]
    public class SongUDPReceiveEvent
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        [Header("监听名")]public string receiveName;
        
        /// <summary>
        /// 接收到消息
        /// </summary>
        [Header("监听字段")]public string receive;

        /// <summary>
        /// 触发事件
        /// </summary>
        [Header("执行方法")]public UnityEvent unityEvent;
    }
}