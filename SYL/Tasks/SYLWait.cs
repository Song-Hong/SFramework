using System.Threading.Tasks;
using SFramework.SFTask.Module;
using UnityEngine;

namespace SFramework.SYL.Tasks
{
    public class SYLWait : SfTaskNode, ISfTaskNodeHelper
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns>任务节点名称</returns>
        public override string GetTaskNodeName() => "等待任务";
        
        /// <summary>
        /// 等待时间
        /// </summary>
        [Header("等待时间(秒)")]
        public int waitTime = 1;
        
        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns>返回任务执行结果</returns>
        public override async Task<int> Start()
        {
            await Task.Delay(waitTime*1000);
            return await base.Start();
        }
    }
}