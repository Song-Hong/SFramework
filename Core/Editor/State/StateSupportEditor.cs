using System;
using Song.Core.Module.State;
using Song.Core.Support;
using UnityEditor;
using UnityEngine;

namespace Song.Core.Editor.State
{
    [CustomEditor(typeof(StateSupport))]
    public class StateSupportEditor : UnityEditor.Editor
    {
        private int startIndex = 0;
        
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var stateSupport = target as StateSupport;
            if (stateSupport == null) return;

            var statesCount = stateSupport.states.Count;
            if (statesCount <= 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("初始状态：");
                GUILayout.Label("当前没有状态");
                GUILayout.EndHorizontal();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            var states = new string[statesCount];
            for (var i = 0; i < statesCount; i++)
            {
                var state = stateSupport.states[i].GetType().Name;
                states[i] = state;
                if(stateSupport.startState == state)
                    startIndex = i;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("初始状态：");
            startIndex = EditorGUILayout.Popup(startIndex, states);
            GUILayout.EndHorizontal();
            stateSupport.startState = states[startIndex];
            serializedObject.ApplyModifiedProperties();
        }
    }
}