using System;
using System.Collections.Generic;

namespace Song.Core.Module.State
{
    /// <summary>
    /// 状态机管理器接口
    /// </summary>
    public interface ISongStateManager
    {
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<ISongStateBase> OnStateChangeEvent;

        /// <summary>
        /// 当前状态
        /// </summary>
        public ISongStateBase CurrentState {get;set;}
        
        /// <summary>
        /// 全部状态
        /// </summary>
        public Dictionary<string, ISongStateBase> States { get;set;}

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="state"></param>
        public void AddState(ISongStateBase state);

        /// <summary>
        /// 删除状态
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(ISongStateBase state);

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeState<T>() where T : ISongStateBase;
    }
}