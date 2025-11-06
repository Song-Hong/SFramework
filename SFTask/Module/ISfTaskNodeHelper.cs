using System.Threading.Tasks;

namespace SFramework.SFTask.Module
{
    /// <summary>
    /// 任务节点助手接口
    /// </summary>
    public interface ISfTaskNodeHelper
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns></returns>
        public string GetTaskNodeName();

        /// <summary>
        /// 任务节点执行函数
        /// </summary>
        /// <returns>任务执行结果</returns>
        public Task<int> Start();
    }
}