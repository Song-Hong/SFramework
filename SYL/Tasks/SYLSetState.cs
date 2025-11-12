using System.Threading.Tasks;
using SFramework.SFTask.Module;
using UnityEngine;

namespace SFramework.SYL.Tasks
{
    /// <summary>
    /// 设置物体状态
    /// </summary>
    public class SylSetState:SfTaskNode,ISfTaskNodeHelper
    {
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns>任务节点名称</returns>
        public override string GetTaskNodeName() => "设置物体状态";
        
        [Header("物体")]
        public GameObject objectState;
        [Header("物体状态")]
        public bool state;
        
        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns>返回任务执行结果</returns>
        public override async Task<int> Start()
        {
            objectState.SetActive(state);
            return await base.Start();
        }
    }
}