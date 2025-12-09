using System;

namespace SFramework.SFState.Module
{
    /// <summary>
    /// 状态机基类接口
    /// </summary>
    /// <typeparam name="T">状态类型</typeparam>
    public interface ISfStateMachine<T> where T : ISfStateMachine<T>
    {
        /// <summary>
        /// 当前状态
        /// </summary>
        public ISfState<T> CurrentState { get; set; }
        
        /// <summary>
        /// 更新状态
        /// </summary>
        public void Update();
        
        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        public void ChangeState<TState>() where TState : ISfState<T>;
    }
}