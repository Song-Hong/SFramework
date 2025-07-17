using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Song.Core.Process;
using Song.Scripts.Core.Mono;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Song.Core.Editor
{
    /// <summary>
    /// 流程管理类编辑器
    /// </summary>
    [CustomEditor(typeof(SongProcessMono))]
    public class SongProcessMonoEditor:UnityEditor.Editor
    {
        /// <summary>
        /// 全部流程
        /// </summary>
        private List<string> _processList = new List<string>();
        // private Dictionary<Type,string> _processNames;
        /// <summary>
        /// 当前选择的流程选项
        /// </summary>
        private int index = 0;
        
        private void OnEnable()
        {
            foreach (var item in SongEditorUtil.FindAllSubclassesOf<SongProcessBase>())
            {
                _processList.Add(item.FullName);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("开始流程：");
            if (_processList.Count > 0)
            {
                var displayedOptions = _processList.ToArray();
                index = EditorGUILayout.Popup(index,displayedOptions);
                ((SongProcessMono)target).StartProcess = displayedOptions[index];
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                GUILayout.Label("当前没有流程");
            }
            GUILayout.EndHorizontal();
        }
    }
}