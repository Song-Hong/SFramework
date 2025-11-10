using System.Threading.Tasks;
using SFramework.SFTask.Module;
using UnityEngine;

namespace SFramework.SFTask.TaskPackages.SYLTasks
{
    /// <summary>
    /// 播放音频任务
    /// </summary>
    public class SylPlayAudio:SfTaskNode,ISfTaskNodeHelper
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns>任务节点名称</returns>
        public override string GetTaskNodeName() => "播放语音";

        /// <summary>
        /// 日志内容
        /// </summary>
        public AudioClip AudioClip;

        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns>返回任务执行结果</returns>
        public override async Task<int> Start()
        {
            
            return await base.Start();
        }
    }
}