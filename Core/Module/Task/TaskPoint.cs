using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Song.Core.Module.Task
{
    /// <summary>
    /// 任务点
    /// </summary>
    public class TaskPoint : MonoBehaviour
    {
        /// <summary>
        /// 拥有者
        /// </summary>
        [Header("拥有者")] public TaskList Owner;
        
        /// <summary>
        /// 任务介绍
        /// </summary>
        [Header("任务介绍")]public string taskDescription;
        
        /// <summary>
        /// 任务点ID
        /// </summary>
        [Header("任务点ID")] public int taskPointID;

        /// <summary>
        /// 任务点执行类型
        /// </summary>
        [Header("任务点执行类型")] public TaskPointType taskPointType;

        /// <summary>
        /// 任务点全部任务
        /// </summary>
        [Header("任务点执行列表")] public List<TaskBase> tasks = new List<TaskBase>();
        
        /// <summary>
        /// 当前任务执行索引
        /// </summary>
        private int index = 0;
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner"></param>
        public void Init(TaskList owner)
        {
            this.Owner = owner;
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        { 
            index = 0;
            
            //初始化任务
            foreach (var taskBase in tasks)
            {
                taskBase.enabled = false;
            }
            
            if(TaskPointType.Order == taskPointType)
            {
                //顺序执行
                tasks[index].enabled = true;
            }
            else
            {
                foreach (var taskBase in tasks)
                {
                    taskBase.enabled = true;
                }
            }
        }
        
        /// <summary>
        /// 任务执行完毕
        /// </summary>
        /// <param name="task"></param>
        public void TaskFinished(TaskBase task)
        {
            //关闭当前任务
            task.enabled = false;
            //计数器增加
            index++;
            
            //顺序执行
            if(taskPointType == TaskPointType.Order)
            {
                if (index < tasks.Count)
                {
                    //执行下一个任务
                    tasks[index].enabled = true;
                }
                else
                {
                    //任务执行完毕
                    Owner.TaskPointFinished(this);
                }
            }
            else
            {
                if(index >= tasks.Count)
                {
                    //任务执行完毕
                    Owner.TaskPointFinished(this);
                }
            }
        }
    }
}