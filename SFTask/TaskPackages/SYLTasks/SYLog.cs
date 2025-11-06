using System.Threading.Tasks;
using SFramework.SFTask.Module;
using UnityEngine;

namespace SFramework.SFTask.TaskPackages.SYLTasks
{
    /// <summary>
    /// 日志任务
    /// </summary>
    public class SyLog:SfTaskNode,ISfTaskNodeHelper
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns>任务节点名称</returns>
        public override string GetTaskNodeName() => "打印消息";

        /// <summary>
        /// 日志内容
        /// </summary>
        public string content;

        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns>返回任务执行结果</returns>
        public override async Task<int> Start()
        {
            await Task.Delay(1000);
            Debug.Log(content);
            return await base.Start();
        }
    }
}