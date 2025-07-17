using System;
using System.Collections.Generic;

namespace Song.Core.Module.State
{
    /// <summary>
    /// 状态机管理器
    /// </summary>
    public class SongStateManager:ISongStateManager
    {
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<ISongStateBase> OnStateChangeEvent;

        /// <summary>
        /// 当前状态
        /// </summary>
        public ISongStateBase CurrentState { get; set; }
        
        /// <summary>
        /// 全部状态
        /// </summary>
        public Dictionary<string, ISongStateBase> States { get; set; } = new Dictionary<string, ISongStateBase>();
        

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="state"></param>
        public void AddState(ISongStateBase state)
            => States.TryAdd(state.GetType().Name, state);

        /// <summary>
        /// 删除状态
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(ISongStateBase state)
            => States.Remove(state.GetType().Name);

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeState<T>() where T : ISongStateBase
        {
            if(!States.TryGetValue(typeof(T).Name, out var state))return;
            OnStateChangeEvent?.Invoke(state);
            CurrentState?.StateExit();
            CurrentState = state;
            CurrentState.StateEnter();
        }
        
        
    }
}