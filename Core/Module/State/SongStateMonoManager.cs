using System;
using System.Collections.Generic;
using UnityEngine;

namespace Song.Core.Module.State
{
    /// <summary>
    /// 状态机管理器 Mono版
    /// </summary>
    public class SongStateMonoManager:MonoBehaviour,ISongStateManager
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
        {
            if (States.TryAdd(state.GetType().Name, state))
            {
                state.StateManager = this;
            }
        }

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

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Start()
        {
            CurrentState?.StateEnter();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public virtual void Update()
        {
            CurrentState?.StateUpdate();
        }
    }
}