using UnityEngine;

namespace SFramework.Core.Support
{
    /// <summary>
    /// SFramework 插件基类
    /// </summary>
    public abstract class SfSupport<T>:MonoBehaviour
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mono">模块</param>
        public abstract void Init(T mono);
    }
}