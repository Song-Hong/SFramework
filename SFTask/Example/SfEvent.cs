using System.Threading.Tasks;
using SFramework.SFTask.Module;
using UnityEngine;
using UnityEngine.Events;

namespace SFramework.SFTask.Example
{
    /// <summary>
    /// 事件任务节点
    /// </summary>
    public class SfEvent:SfTaskNode
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns></returns>
        public override string GetTaskNodeName() => "Unity事件";
        
        /// <summary>
        /// Unity事件
        /// </summary>
        [Header("Unity事件")]
        public UnityEvent unityEvent;
        
        public override Task<int> Start()
        {
            unityEvent?.Invoke();
            return base.Start();
        }
    }
}