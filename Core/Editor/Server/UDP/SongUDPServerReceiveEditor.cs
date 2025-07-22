using SFramework.Core.Mono;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using SFramework.Core.Support.UDP;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;

namespace SFramework.Core.Editor.Server.UDP
{
    /// <summary>
    /// UDP 接受指令样式
    /// </summary>
    [CustomEditor(typeof(SongUDPReceiveSupport))]
    public class SongUDPServerReceiveEditor:UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("UDP 接口 负责消息的接受执行");
            base.OnInspectorGUI();

            if (GUILayout.Button("生成配置文件"))
            {
                var filePath = Application.streamingAssetsPath + "/SFConfig/UDPReceive.json";
                
                // 如果文件不存在则创建
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var songUDPReceive = target as SongUDPReceiveSupport;
                if (songUDPReceive != null)
                {
                    var config = new Dictionary<string,string>();
                    var receiveEvents = songUDPReceive.receives;
                    
                    foreach (var receiveEvent in receiveEvents)
                    {
                        if (!config.ContainsKey(receiveEvent.receiveName))
                            config.Add(receiveEvent.receiveName, receiveEvent.receive);
                    }

                    var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    Debug.Log("生成UDP接口配置文件 "+filePath);
                }
                
                AssetDatabase.Refresh();
            }
        }
    }
}