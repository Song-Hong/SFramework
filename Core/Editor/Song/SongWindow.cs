using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Song.Core.Editor.Song
{
    /// <summary>
    /// SongTools窗口
    /// </summary>
    public partial class SongWindow:EditorWindow
    {
        // [MenuItem("Song/SongTools")]
        public static void ShowSongWindow()
        {
            //显示窗口
            var songWindow = GetWindow<SongWindow>();
            songWindow.titleContent = new GUIContent("SongTools");
            songWindow.Show();
        }

        /// <summary>
        /// 内容区域
        /// </summary>
        private VisualElement _content;
        
        /// <summary>
        /// 创建GUI
        /// </summary>
        private void CreateGUI()
        {
            //加载UI
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Song/Core/Editor/Song/Song.uxml")
                .CloneTree(rootVisualElement);

            //获取内容区域
            _content  = rootVisualElement.Q<VisualElement>("Content");
            
            //初始化左侧面板
            InitLeftMenu();
        }
        
        /// <summary>
        /// 切换页面
        /// </summary>
        /// <param name="pageName"></param>
        public void ChangePage(string pageName)
        {
            var assetPath = $"Assets/Song/Core/Editor/Song/{pageName}.uxml";
            var page = new VisualElement();
            try
            {
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath).CloneTree(page);
            }
            catch (Exception e)
            {
                Console.Write(e);
                page = null;
            }
            finally
            {
                _content.Clear();
                if (page != null)
                    _content.Add(page);
            }
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        /// <param name="pageName">页面名</param>
        public void InitPage(string pageName)
        {
            switch (pageName)
            {
                case "Extends":
                    InitExtendsPage();
                    break;
            }
        }
    }
}