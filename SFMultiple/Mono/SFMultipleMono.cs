using System;
using System.Net.Sockets.Kcp;
using System.Text;
using SFramework.Core.Mono;
using SFramework.SFMultiple.Module;
using UnityEngine;

namespace SFramework.SFMultiple.Mono
{
    /// <summary>
    /// 多个 Mono 单例
    /// </summary>
    public class SfMultipleMono : SfMonoSingleton<SfMultipleMono>
    {
        /// <summary>
        /// 服务器类型
        /// </summary>
        public enum ServerType
        {
            /// KCP 服务器
            KcpServer,
        }
        /// <summary>
        /// 服务器类型
        /// </summary>
        public ServerType serverType;

        private void Start()
        {
            var SfKcpServer = new SfKcpServer();
            // 监听
            SfKcpServer.OnReceive += (res)=>
            {
                Debug.Log(Encoding.UTF8.GetString(res));
            };
        }
    }
}