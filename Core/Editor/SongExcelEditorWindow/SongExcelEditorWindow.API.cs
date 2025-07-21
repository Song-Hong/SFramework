using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.Editor.SongExcelEditorWindow
{
    public partial class SongExcelEditorWindow:EditorWindow
    {
        /// <summary>
        /// 当编辑窗口关闭
        /// </summary>
        public event Action OnWindowClose;
        
        /// <summary>
        /// 打开Excel编辑器
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static SongExcelEditorWindow OpenExcelEditor(string excelPath)
        {
            var songEditorWindow = GetWindow<SongExcelEditorWindow>();
            if(!File.Exists(excelPath))
                Debug.LogError("文件不存在:"+excelPath);
            if (!excelPath.EndsWith(".xlsx"))
            {
                Debug.LogError("文件不是xlsx文件格式:"+excelPath);
                return null;
            }
            songEditorWindow.Show();
            songEditorWindow.excelFilePath = excelPath;
            songEditorWindow.CreateGUI();
            return songEditorWindow;
        }

        private void OnDestroy()
        {
            OnWindowClose?.Invoke();
        }
    }
}