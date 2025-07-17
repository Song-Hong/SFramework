using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Song.Core.Module.Task
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class TaskList
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        public List<TaskPoint> Tasks = new List<TaskPoint>();
        
        /// <summary>
        /// 当前执行的任务
        /// </summary>
        public TaskPoint CurrentTask;

        /// <summary>
        /// 当准备切换下一个任务时
        /// </summary>
        public event Action<TaskPoint> OnTaskPointChangedStart;
        
        /// <summary>
        /// 当任务切换时
        /// </summary>
        public event Action OnTaskPointChanged;
        
        /// <summary>
        /// 所有任务执行完毕
        /// </summary>
        public event Action OnAllTaskFinished;
        
        /// <summary>
        /// 任务计数器
        /// </summary>
        private int _index = 0;
        
        /// <summary>
        /// 当前节点执行完毕
        /// </summary>
        /// <param name="taskPoint">当前任务节点</param>
        public void TaskPointFinished(TaskPoint taskPoint)
        {
            //关闭当前任务
            taskPoint.gameObject.SetActive(false);
            
            //计数器增加
            _index++;
            if(_index < Tasks.Count)
            {
                //执行下一个任务
                Tasks[_index].gameObject.SetActive(true);
                CurrentTask = Tasks[_index]; // CurrentTask is updated here first.
                
                // Now fire the event after CurrentTask is updated
                OnTaskPointChangedStart?.Invoke(taskPoint); // Invoked with the *just finished* taskPoint
                OnTaskPointChanged?.Invoke();
            }
            else
            {
                //所有任务执行完毕
                OnAllTaskFinished?.Invoke();
            }
        }
        
        /// <summary>
        /// 执行任务1
        /// </summary>
        public void StartFristTask()
        {
            if(Tasks.Count > 0)
            {
                Tasks[0].gameObject.SetActive(true);
                CurrentTask = Tasks[0];
            }
        }

        #region 添加任务
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="taskPoint">任务点</param>
        public void AddTask(TaskPoint taskPoint)
        {
            taskPoint.gameObject.SetActive(false);
            taskPoint.Init(this);
            Tasks.Add(taskPoint);
        }
        #endregion
    }
}