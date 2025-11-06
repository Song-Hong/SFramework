using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Serialization;

namespace SFramework.SFTask.Module
{
    /// <summary>
    /// 任务
    /// </summary>
    [Serializable]
    public class SfTaskNode
    {
        #region 字段
        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; private set; }
        
        /// <summary>
        /// 任务是否完成
        /// </summary>
        public bool isComplete;
        #endregion

        #region 构造函数
        /// <summary>
        /// 任务构造函数
        /// </summary>
        public SfTaskNode()
        {
            
        }
        #endregion

        #region 公开方法
        /// <summary>
        /// 任务执行函数
        /// </summary>
        public virtual Task<int> Start( )
        {
            isComplete = true;
            return Task.FromResult(isComplete?1:0);
        }
        
        /// <summary>
        /// 获取任务节点名称
        /// </summary>
        /// <returns></returns>
        public virtual string GetTaskNodeName()=>GetType().Name;
        #endregion
    }
}