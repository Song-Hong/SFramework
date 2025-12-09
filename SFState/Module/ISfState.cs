namespace SFramework.SFState.Module
{
    /// <summary>
    /// 状态模块基类接口
    /// </summary>
    /// <typeparam name="T">状态机类型</typeparam>
    public interface ISfState<T> where T : ISfStateMachine<T>
    {
        /// <summary>
        /// 状态机所有者
        /// </summary>
        public ISfStateMachine<T> Owner { get; set; }
        /// <summary>
        /// 进入状态
        /// </summary>
        public void Enter();
        /// <summary>
        /// 退出状态
        /// </summary>
        public void Exit();
        /// <summary>
        /// 更新状态
        /// </summary>
        public void Update();

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T">流程状态</typeparam>
        /// <typeparam name="TState">流程状态类型</typeparam>
        public void ChangeState<TState>() where TState : ISfState<T>;
    }
}