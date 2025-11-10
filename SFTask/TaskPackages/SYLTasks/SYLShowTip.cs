using System.Threading.Tasks;
using SFramework.SFTask.Module;

namespace SFramework.SFTask.TaskPackages.SYLTasks
{
    /// <summary>
    /// 显示提示任务
    /// </summary>
    public class SylShowTip:SfTaskNode,ISfTaskNodeHelper
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns>任务节点名称</returns>
        public override string GetTaskNodeName() => "显示提示";

        /// <summary>
        /// 日志内容
        /// </summary>
        public string TipContent;

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