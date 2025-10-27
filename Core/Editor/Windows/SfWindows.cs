using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace SFramework.Core.Editor.Windows
{
    /// <summary>
    /// SFramework 窗口
    /// </summary>
    public partial class SfWindows : EditorWindow
    {
        /// <summary>
        /// 创建窗口
        /// </summary>
        [MenuItem("SFramework/Windows")]
        public static void ShowWindow()
        {
            var window = GetWindow<SfWindows>("SFramework Windows");
            window.Show();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void CreateGUI()
        {
            //加载文件
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SFramework/Core/Editor/Windows/Windows.uxml");
            uxml.CloneTree(rootVisualElement);
            
            // 初始化侧边栏
            InitSlider();
        }
    }   
}
