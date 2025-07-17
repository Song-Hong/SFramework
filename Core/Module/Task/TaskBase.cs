using System;
using UnityEngine;

namespace Song.Core.Module.Task
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public class TaskBase:MonoBehaviour
    {
        /// <summary>
        /// 拥有者任务点
        /// </summary>
        public TaskPoint owner;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="own">拥有者</param>
        public void Init(TaskPoint own)
        {
            this.owner = own;
        }
        
        /// <summary>
        /// 任务执行完毕
        /// </summary>
        public void TaskFinished()=>owner.TaskFinished(this);

        /// <summary>
        /// 初始化
        /// </summary>
        private void Reset()
        {
            var taskPoint = this.GetComponent<TaskPoint>();
            if (!taskPoint)
            {
                taskPoint = this.gameObject.AddComponent<TaskPoint>();
            }
            owner = taskPoint;
            taskPoint.tasks.Add(this);
        }
    }
}