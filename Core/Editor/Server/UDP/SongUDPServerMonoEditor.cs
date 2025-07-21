using System;
using SFramework.Core.Editor.SongConfig;
using SFramework.Core.Mono;
using Song.Core.Mono;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.Editor.Server
{

    [CustomEditor(typeof(SongUDPServerMono))]
    public class SongUDPServerMonoEditor : UnityEditor.Editor
    {
        private string msg = "Hello World";
        private string ip = "";
        private int port = 12345;
        private SongUDPServerMono server;

        private GUIStyle background;
        private GUIStyle title;
        
        private void OnEnable()
        {
            try
            {
                //自定义背景颜色
                background = new GUIStyle();
                var texture2D = new Texture2D(1, 1);
                texture2D.SetPixel(1,1,new Color(0.06f,0.06f,0.06f));
                texture2D.Apply();
                background.normal.background = texture2D;
                
                //自定义标题颜色
                title = new GUIStyle();
                title.alignment = TextAnchor.MiddleCenter;
                title.fontSize = 16;
                title.normal.textColor = Color.white;
                
                var activeGameObject = Selection.activeGameObject;
                server = activeGameObject.GetComponent<SongUDPServerMono>();
                ip = server.ip;
                port = 9999;
            }
            catch (Exception e)
            {
               
            }
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("快速创建UDP服务");
            base.OnInspectorGUI();
            
            if (server && Application.isPlaying && server.isActiveAndEnabled)
            {
                UDPTest();
            }
            
            if(GUILayout.Button("生成配置文件"))
            {
                //生成配置文件
                SongConfigEditor.ShowWindow();
            }
        }

        /// <summary>
        /// UDP测试工具
        /// </summary>
        public void UDPTest()
        {
            GUILayout.Space(10);

            GUILayout.BeginVertical(background);
            GUILayout.Space(10);
            GUILayout.Label("UDP测试工具",title);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("目标IP地址:");
            ip = GUILayout.TextField(ip);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("目标端口号:");
            int.TryParse(GUILayout.TextField(port.ToString()), out port);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("消息:");
            msg = GUILayout.TextField(msg);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("发送UDP",GUILayout.Width(160)))
            {
                server.Send(ip, port, msg);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("广播消息",GUILayout.Width(160)))
            {
                server.SendBroadcast(port,msg);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("遍历广播消息",GUILayout.Width(160)))
            {
                server.SendBroadcastWithForeach(ip,port,msg);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }
}