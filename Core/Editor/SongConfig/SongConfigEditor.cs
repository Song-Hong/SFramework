using System;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.Editor.SongConfig
{
    /// <summary>
    /// SongConfig
    /// </summary>
    public class SongConfigEditorWindow:EditorWindow
    {
        [MenuItem("Song/配置文件编辑器")]
        public static void ShowWindow()
        {
            var songConfigEditorWindow = GetWindow<SongConfigEditorWindow>();
            songConfigEditorWindow.titleContent = new GUIContent("配置文件编辑器");
            songConfigEditorWindow.Show();
            // var configPath = Application.dataPath + "/SFramework/Core/Editor/SongConfig/config.xlsx";
            // SongExcelEditorWindow.SongExcelEditorWindow.OpenExcelEditor(configPath).OnWindowClose += () =>
            // {
            //     
            // };
        }

        /// <summary>
        /// 窗口初始化
        /// </summary>
        private void CreateGUI()
        {

        }
    }
}