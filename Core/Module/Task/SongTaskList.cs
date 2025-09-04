using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.Module.Task
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class SongTaskList
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        public List<SongTaskPoint> Tasks = new List<SongTaskPoint>();
        
        /// <summary>
        /// 当前执行的任务
        /// </summary>
        public SongTaskPoint CurrentSongTask;

        /// <summary>
        /// 当准备切换下一个任务时
        /// </summary>
        public event Action<SongTaskPoint> OnTaskPointChangedStart;
        
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
        /// <param name="songTaskPoint">当前任务节点</param>
        public void TaskPointFinished(SongTaskPoint songTaskPoint)
        {
            //关闭当前任务
            songTaskPoint.gameObject.SetActive(false);
            
            //计数器增加
            _index++;
            if(_index+1 < Tasks.Count)
            {
                //执行下一个任务
                Tasks[_index].gameObject.SetActive(true);
                CurrentSongTask = Tasks[_index]; // CurrentTask is updated here first.
                
                // Now fire the event after CurrentTask is updated
                OnTaskPointChangedStart?.Invoke(songTaskPoint); // Invoked with the *just finished* taskPoint
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
                CurrentSongTask = Tasks[0];
            }
        }

        #region 添加任务
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="songTaskPoint">任务点</param>
        public void AddTask(SongTaskPoint songTaskPoint)
        {
            songTaskPoint.gameObject.SetActive(false);
            songTaskPoint.Init(this);
            Tasks.Add(songTaskPoint);
        }
        #endregion
    }
}