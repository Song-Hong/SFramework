using System;
using System.Collections;
using System.Collections.Generic;
using Song.Core.Module.Task;
using UnityEngine;

namespace Song.Scripts.Core.Mono
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class SongTaskMono : MonoBehaviour
    {
        /// <summary>
        /// 所有任务点
        /// </summary>
        public List<TaskPoint> taskPoints = new List<TaskPoint>();

        private void Start()
        {
            var taskList = new TaskList();
            foreach (var taskPoint in taskPoints)
            {
                taskList.AddTask(taskPoint);
            }
            taskList.StartFristTask();
            taskList.OnAllTaskFinished += () =>
            {
                Debug.Log("所有任务完成");
            };
        }
    }
}
