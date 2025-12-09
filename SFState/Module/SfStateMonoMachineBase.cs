using System;
using UnityEngine;

namespace SFramework.SFState.Module
{
    /// <summary>
    /// 状态机模块基类
    /// </summary>
    public abstract class SfStateMonoMachineBase<T> : MonoBehaviour, ISfStateMachine<T> where T : ISfStateMachine<T>
    {
        /// <summary>
        /// 当前状态
        /// </summary>
        public ISfState<T> CurrentState { get; set; }
        
        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        public void ChangeState<TState>() where TState : ISfState<T>
        {
            CurrentState?.Exit();
            CurrentState= (TState)Activator.CreateInstance(typeof(TState));
            CurrentState.Owner = this;
            CurrentState?.Enter();
        }
    }
}