using System.Threading.Tasks;
using SFramework.SFTask.Module;
using UnityEngine;

namespace SFramework.SFTask.Example
{
    /// <summary>
    /// 打印日志任务节点
    /// </summary>
    public class SfLog:SfTaskNode
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns></returns>
        public override string GetTaskNodeName() => "打印日志";

        /// <summary>
        /// 日志内容
        /// </summary>
        [Header("日志内容")]
        public string content;
        
        public override Task<int> Start()
        {
            Debug.Log(content);
            return base.Start();
        }
    }
}