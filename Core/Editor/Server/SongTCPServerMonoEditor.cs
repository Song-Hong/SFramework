using System;
using Song.Core.Mono;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.Editor.Server
{
    [CustomEditor(typeof(SongTcpServerMono))]
    public class SongTcpServerMonoEditor:UnityEditor.Editor
    {
        private string msg = "Hello World";
        private SongTcpServerMono server;
        
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
                
                title = new GUIStyle();
                title.alignment = TextAnchor.MiddleCenter;
                title.fontSize = 16;
                title.normal.textColor = Color.white;
                
                var activeGameObject = Selection.activeGameObject;
                server = activeGameObject.GetComponent<SongTcpServerMono>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public override void OnInspectorGUI()
        {
            GUILayout.Label("快速创建TCP服务器/客户端");
            base.OnInspectorGUI();
            
            if (!server || !Application.isPlaying || !server.isActiveAndEnabled) return;
            GUILayout.Space(10);

            GUILayout.BeginVertical(background);
            GUILayout.Space(10);
            GUILayout.Label("TCP测试工具",title);
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("消息:");
            msg = GUILayout.TextField(msg);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("发送消息",GUILayout.Width(160)))
            {
                server.Send(msg);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (server.serverState == SongTcpServerMono.ServerState.服务器)
            {
                if (GUILayout.Button("发送给全部客户端",GUILayout.Width(160)))
                {
                    server.SendAll(msg);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }
}