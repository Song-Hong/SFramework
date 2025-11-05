using System;
using System.Collections.Generic;
using SFramework.Core.Mono;
using SFramework.SFTask.Module;

namespace SFramework.SFTask.Mono
{
    /// <summary>
    /// 任务模块Mono单例
    /// </summary>
    public class SfTaskMono:SfMonoSingleton<SfTaskMono>
    {
        /// <summary>
        /// 任务点列表
        /// </summary>
        public List<SfTaskPoint> tasks = new List<SfTaskPoint>();
        
        /// <summary>
        /// 
        /// </summary>
        public bool autoStart = true;
        
        /// <summary>
        /// 启动任务模块Mono单例
        /// </summary>
        private async void Start()
        {
            if (!autoStart) return;
            StartTask();
        }

        /// <summary>
        /// 启动所有任务
        /// </summary>
        public async void StartTask()
        {
            foreach (var sfTaskPoint in tasks)
            {
                await sfTaskPoint.Start();
            }
        }
    }
}