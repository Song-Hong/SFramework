using Song.Scripts.Core.Mono;

namespace Song.Core.Process
{
    /// <summary>
    /// 流程基类
    /// </summary>
    public class SongProcessBase
    {
        public virtual void Enter()
        {
            
        }

        public virtual void Exit()
        {
            
        }

        public virtual void Update()
        {
            
        }

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <typeparam name="T">流程状态</typeparam>
        public void ChangeProcess<T>() where T : SongProcessBase
        =>SongProcessMono.Instance.ChangeProcess<T>();
    }
}