using SFramework.Core.Module.Task;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.Editor.Task
{
    /// <summary>
    /// 任务管理器编辑器
    /// </summary>
    [CustomEditor(typeof(SongTaskManager))]
    public class SongTaskManagerEditor:UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var taskManager = (SongTaskManager) target;
            if (GUILayout.Button("添加任务点"))
            {
                var songTaskManager = target as SongTaskManager;
                var gameObject = new GameObject();
                gameObject.transform.SetParent(songTaskManager.transform);
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localScale = Vector3.one;
                gameObject.name = $"SongTaskPoint{songTaskManager.taskPoints.Count+1}";
                songTaskManager.taskPoints.Add(gameObject.AddComponent<SongTaskPoint>());
                gameObject.SetActive(false);
            }
            if (GUILayout.Button("获取全部子任务节点"))
            {
                var songTaskManager = target as SongTaskManager;
                songTaskManager.taskPoints.Clear();
                songTaskManager.gameObject.SetActive(true);
                var points = songTaskManager.GetComponentsInChildren<SongTaskPoint>(true);
                for (var i = 0; i < points.Length; i++)
                {
                    var songTaskPoint = points[i];
                    songTaskPoint.taskPointID = i;
                    songTaskManager.taskPoints.Add(songTaskPoint);
                    songTaskPoint.gameObject.SetActive(false);
                }
                EditorUtility.SetDirty(songTaskManager);
                songTaskManager.gameObject.SetActive(false);
            }
        }
    }
}