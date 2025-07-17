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
            
        }

        /// <summary>
        /// 窗口初始化
        /// </summary>
        private void CreateGUI()
        {
            var configPath = Application.dataPath + "/SFramework/Core/Editor/SongConfig/config.xlsx";
            
        }
    }
}