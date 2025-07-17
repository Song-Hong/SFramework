namespace Song.Core.Module.State
{
    /// <summary>
    /// 状态基类接口
    /// </summary>
    public interface ISongStateBase
    {
        /// <summary>
        /// 状态机管理器
        /// </summary>
        public ISongStateManager StateManager { get; set; }

        // 进入该状态时执行的逻辑
        public void StateEnter();

        // 退出该状态时执行的逻辑
        public void StateExit();
        
        // 状态持续状态
        public void StateUpdate();

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeState<T>() where T : ISongStateBase;
    }
}