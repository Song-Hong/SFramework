using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.SFTask.Module
{
    /// <summary>
    /// 任务点
    /// </summary>
    [Serializable]
    public class SfTaskPoint
    {
        /// <summary>
        /// 任务点标题
        /// </summary>
        public string title;
        
        /// <summary>
        /// 任务列表
        /// </summary>
        [SerializeReference]
        public List<SfTaskNode> Tasks = new List<SfTaskNode>();
        
        /// <summary>
        /// 任务点任务执行类型
        /// </summary>
        public SfTaskPointType Type;
        
        /// <summary>
        /// 任务点执行函数
        /// </summary>
        public async Task Start()
        {
            switch (Type)
            {
                case SfTaskPointType.Sequential: // 顺序执行
                    foreach (var task in Tasks)
                    {
                        await task.Start();
                    }
                    break;
                case SfTaskPointType.Parallel: // 并行执行
                    var runningTasks = new List<Task>();
                    foreach (var task in Tasks)
                    {
                        runningTasks.Add(task.Start()); 
                    }
                    await Task.WhenAll(runningTasks);
                    break;
            }
        }
    }
}