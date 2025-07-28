using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.Core.Module.Task
{
    /// <summary>
    /// 任务点
    /// </summary>
    public class SongTaskPoint : MonoBehaviour
    {
        /// <summary>
        /// 拥有者
        /// </summary>
        [Header("拥有者")] public SongTaskList Owner;
        
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
        [FormerlySerializedAs("taskPointType")] [Header("任务点执行类型")] public SongTaskPointType songTaskPointType;

        /// <summary>
        /// 任务点全部任务
        /// </summary>
        [Header("任务点执行列表")] public List<SongTaskBase> tasks = new List<SongTaskBase>();
        
        /// <summary>
        /// 当前任务执行索引
        /// </summary>
        private int index = 0;
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner"></param>
        public void Init(SongTaskList owner)
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

            //当前任务点没有任务,任务点执行完毕
            if (tasks.Count <= 0)
            {
                Owner.TaskPointFinished(this);
                return;
            }
            
            //顺序执行
            if(SongTaskPointType.Order == songTaskPointType)
            {
                tasks[index].enabled = true;
            }
            //并行执行
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
        public void TaskFinished(SongTaskBase task)
        {
            //关闭当前任务
            task.enabled = false;
            //计数器增加
            index++;
            
            //顺序执行
            if(songTaskPointType == SongTaskPointType.Order)
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