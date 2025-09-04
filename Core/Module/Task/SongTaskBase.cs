using System;
using System.Linq;
using UnityEngine;

namespace SFramework.Core.Module.Task
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public abstract class SongTaskBase:MonoBehaviour
    {
        /// <summary>
        /// 拥有者任务点
        /// </summary>
        public SongTaskPoint owner;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="own">拥有者</param>
        public void Init(SongTaskPoint own)
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
            var taskPoint = this.GetComponent<SongTaskPoint>();
            // if (!taskPoint)
            // {
            //     taskPoint = this.gameObject.AddComponent<SongTaskPoint>();
            // }
            owner = taskPoint;
            taskPoint.tasks.Clear();
            taskPoint.tasks = taskPoint.GetComponents<SongTaskBase>().ToList() ;
        }

        /// <summary>
        /// 任务开始
        /// </summary>
        public abstract void OnEnable();
    }
}