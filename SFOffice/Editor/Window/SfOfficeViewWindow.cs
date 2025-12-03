using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFOffice.Editor.Window
{
    /// <summary>
    /// Office模块视图窗口
    /// </summary>
    public partial class SfOfficeViewWindow:EditorWindow
    {
        /// <summary>
        /// 内容元素
        /// </summary>
        private VisualElement _content;
        
        /// <summary>
        /// 文件路径
        /// </summary>
        private string _filePath = "";
        
        /// <summary>
        /// 显示Office模块视图窗口
        /// </summary>
        [MenuItem("SFramework/Office编辑器")]
        public static void ShowOfficeViewWindow()
        {
            var window = GetWindow<SfOfficeViewWindow>();
            window.titleContent = new GUIContent("Office模块视图");
            window.Show();
        }

        /// <summary>
        /// 显示Office模块视图窗口
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void OpenOfficeViewWindow(string path)
        {
            var window = GetWindow<SfOfficeViewWindow>();
            window.titleContent = new GUIContent("Office模块视图");
            window._filePath = path;
            window.Show();
            window.InitializeContentFromPath();
        }

        /// <summary>
        /// 创建GUI
        /// </summary>
        private void CreateGUI()
        {
            var visualTreeAsset = 
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SFramework/SFOffice/Editor/Window/OfficeView.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);
            
            // 获取内容元素
            _content = rootVisualElement.Q<VisualElement>("Content");
        }
        
        /// <summary>
        /// 初始化内容元素从文件路径
        /// </summary>
        private void InitializeContentFromPath()
        {
            // 清空内容元素
            _content.Clear();
            // 根据文件扩展名初始化不同的模块
            if (string.IsNullOrWhiteSpace(_filePath)) return;
            var lower = Path.GetExtension(_filePath).ToLower();
            switch (lower)
            {
                case ".xlsx":
                    InitExcel();
                    break;
            }
        }
    }
}