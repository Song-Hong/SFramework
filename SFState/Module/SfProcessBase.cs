using SFramework.SFState.Mono;

namespace SFramework.SFState.Module
{
    /// <summary>
    /// 流程基类
    /// </summary>
    public abstract class SfProcessBase
    {
        /// <summary>
        /// 进入流程
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// 退出流程
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// 更新流程
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <typeparam name="T">流程状态</typeparam>
        public void ChangeProcess<T>() where T : SfProcessBase
            =>SfProcessMono.Instance.ChangeProcess<T>();
    }
}