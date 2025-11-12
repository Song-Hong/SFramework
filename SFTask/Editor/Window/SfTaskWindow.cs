using System;
using System.IO;
using SFramework.SFTask.Editor.View;
using SFramework.SFTask.Module;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.Window
{
    /// <summary>
    /// 任务窗口
    /// </summary>
    public class SfTaskWindow:EditorWindow
    {
        /// <summary>
        /// 任务图视图
        /// </summary>
        private static SfTaskWindow  taskWindow;
        /// <summary>
        /// 任务图视图
        /// </summary>
        private SfTaskGraphView graphView;
        
        /// <summary>
        /// 任务文件图标路径
        /// </summary>
        private const string IconPath = "Assets/SFramework/SfTask/Editor/Data/TaskFile.png"; 
        
        /// <summary>
        /// 关闭图标路径
        /// </summary>
        public const string CloseIconPath = "Assets/SFramework/SFTask/Editor/Data/Close.png";
        
        /// <summary>
        /// 关闭图标
        /// </summary>
        public static Texture2D CloseIcon;
        
        /// <summary>
        /// 打开任务窗口
        /// </summary>
        [MenuItem("SFramework/SfTaskWindow")]
        public static void OpenWindow()
        {
            // 确保只有一个窗口实例
            taskWindow = GetWindow<SfTaskWindow>();
            taskWindow.titleContent = 
                new GUIContent("新任务", AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath));
            taskWindow.Show();
        }

        /// <summary>
        /// 初始化窗口内容
        /// </summary>
        private void OnEnable()
        {
            // 初始化任务图视图
            graphView = new SfTaskGraphView();
            var styleSheetPath = "Assets/SFramework/SFTask/Editor/Style/SfTask.uss"; 
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if (styleSheet != null)
            {
                graphView.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogError($"无法加载样式表: {styleSheetPath}");
            }
            // 初始化关闭图标
            if(File.Exists(CloseIconPath))
                CloseIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CloseIconPath);
            // 将任务图视图添加到窗口的根元素中
            rootVisualElement.Add(graphView);
        }
        
        /// <summary>
        /// 获取任务图视图
        /// </summary>
        /// <returns>任务图视图</returns>
        public SfTaskGraphView GetGraphView()
        {
            return graphView;
        }
    }
}