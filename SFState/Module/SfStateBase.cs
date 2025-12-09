namespace SFramework.SFState.Module
{
    /// <summary>
    /// 状态模块基类
    /// </summary>
    public abstract class SfStateBase<T> : ISfState<T> where T : ISfStateMachine<T>
    {
        /// <summary>
        /// 状态机所有者
        /// </summary>
        public ISfStateMachine<T> Owner { get; set; }
        
        /// <summary>
        /// 进入状态
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// 退出状态
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// 更新状态
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        public void ChangeState<TState>() where TState : ISfState<T> =>Owner.ChangeState<TState>();
    }
}