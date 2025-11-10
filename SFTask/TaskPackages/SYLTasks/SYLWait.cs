using System.Threading.Tasks;
using SFramework.SFTask.Module;

namespace SFramework.SFTask.TaskPackages.SYLTasks
{
    /// <summary>
    /// 等待任务
    /// </summary>
    public class SYLWait:SfTaskNode,ISfTaskNodeHelper
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns>任务节点名称</returns>
        public override string GetTaskNodeName() => "等待时间";

        /// <summary>
        /// 日志内容
        /// </summary>
        public float WaitTime;

        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns>返回任务执行结果</returns>
        public override async Task<int> Start()
        {
            await Task.Delay((int)(WaitTime * 1000));
            return await base.Start();
        }
    }
}