using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.Module.Task
{
    /// <summary>
    /// 任务列表管理器
    /// </summary>
    public class SongTaskManager:MonoBehaviour
    {
        /// <summary>
        /// 所有任务点
        /// </summary>
        [Header("全部任务点")]
        public List<SongTaskPoint> taskPoints = new List<SongTaskPoint>();

        [Header("自动开始任务")]
        public bool autoStart = true;
        
        /// <summary>
        /// 开始任务
        /// </summary>
        private void OnEnable()
        {
            if (autoStart)
                StartTask();
        }
        
        /// <summary>
        /// 开始任务
        /// </summary>
        public void StartTask()
        {
            var taskList = new SongTaskList();
            foreach (var taskPoint in taskPoints)
            {
                taskList.AddTask(taskPoint);
            }
            taskList.StartFristTask();
            taskList.OnAllTaskFinished += () =>
            {
                Debug.Log("所有任务完成");
                this.gameObject.SetActive(false);
            };
        }
    }
}