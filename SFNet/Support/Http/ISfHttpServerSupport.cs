using UnityEngine;

namespace SFramework.SFNet.Support.Http
{
    /// <summary>
    /// HTTP服务器支持接口
    /// </summary>
    public abstract class SfHttpServerSupportBase:MonoBehaviour
    {
        /// <summary>
        /// 支持开始
        /// </summary>
        public abstract void SupportStart();
    }
}