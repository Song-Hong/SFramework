using UnityEngine;

namespace Song.Core.Module.State
{
    /// <summary>
    /// 状态基类 Mono版
    /// </summary>
    public class SongStateMonoBase:MonoBehaviour,ISongStateBase
    {
        /// <summary>
        /// 状态机管理器
        /// </summary>
        public ISongStateManager StateManager { get; set; }

        /// <summary>
        /// 进入该状态时执行的逻辑
        /// </summary>
        public virtual void StateEnter()
        {
            
        }

        /// <summary>
        /// 状态持续状态
        /// </summary>
        public virtual void StateUpdate()
        {
            
        }

        /// <summary>
        /// 退出该状态时执行的逻辑
        /// </summary>
        public virtual void StateExit()
        {
            
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeState<T>() where T : ISongStateBase
            =>StateManager.ChangeState<T>();
    }
}