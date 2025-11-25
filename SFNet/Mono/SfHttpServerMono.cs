using System;
using System.Net;
using System.Threading;
using SFramework.Core.Mono;
using SFramework.SFNet.Module.Http;
using UnityEngine;

namespace SFramework.SFNet.Mono
{
    /// <summary>
    /// HTTP 
    /// </summary>
    public class SfHttpServerMono:SfMonoSingleton<SfHttpServerMono>
    {
        /// <summary>
        /// http 服务 
        /// </summary>
        private SfHttpServer _server;
        
        /// <summary>
        /// IP 地址
        /// </summary>
        public string ip;
        
        /// <summary>
        /// 端口号
        /// </summary>
        public int port = 8989;
        
        /// <summary>
        /// 打印消息日志
        /// </summary>
        public bool printLog = true;

        /// <summary>
        /// 接收到HTTP请求时的回调事件
        /// 参数1: HttpListenerRequest - 客户端的请求对象
        /// 参数2: HttpListenerResponse - 用于向客户端发送响应的对象
        /// </summary>
        public event Action<HttpListenerRequest, HttpListenerResponse> OnRequest;
        
        /// <summary>
        /// 接收到HTTP请求时的回调事件
        /// 参数1: HttpListenerRequest - 客户端的请求对象
        /// 参数2: HttpListenerResponse - 用于向客户端发送响应的对象
        /// </summary>
        public event Action<HttpListenerRequest, HttpListenerResponse> MainThreadOnRequest;
        private void Start()
        {
            //当 IP 为空时 自动获取本地IP
            if(string.IsNullOrWhiteSpace(ip))
            {
                ip = SfHttpServer.GetMainLocalIP();
            }
            
            var mainThread = SynchronizationContext.Current;
            _server = SfHttpServer.Start(ip, port,(request, response) => {
                // 处理请求
                if (printLog)
                {
                    Debug.Log(
                        $"HTTP:{request.Url}"+
                        $" Method:{request.HttpMethod}"+
                        $" IP:{request.RemoteEndPoint}"+
                        $" Headers:{request.Headers}"+
                        $" Body:{request.HasEntityBody}"
                        );
                }
                
                // 触发事件
                OnRequest?.Invoke(request, response);
                
                // 触发事件
                mainThread.Post(_ =>
                {
                    MainThreadOnRequest?.Invoke(request, response);
                }, null);
            });
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void Reset()
        {
            ip = SfHttpServer.GetMainLocalIP();
        }
        
        /// <summary>
        /// 关闭客户端断开连接
        /// </summary>
        private void OnDestroy()
        {
            _server?.Stop();
        }
    }
}