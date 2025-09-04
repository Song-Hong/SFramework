using System;
using System.IO;
using System.Net;
using System.Text;
using SFramework.Core.Module.Server.Http;
using Song.Core.Module;
using Song.Core.Support.HTTP;
using UnityEngine;

namespace SFramework.Core.Mono
{
    /// <summary>
    /// Http服务器
    /// </summary>
    public class SongHttpServerMono : MonoSingleton<SongHttpServerMono>
    {
        /// <summary>
        /// ip地址
        /// </summary>
        [Header("IP地址")] public string ip;

        /// <summary>
        /// 端口号
        /// </summary>
        [Header("端口号")] public int port = 8866;

        /// <summary>
        /// 接收到Http请求时的回调事件
        /// </summary>
        public event Action<HttpListenerRequest,HttpListenerResponse> OnRequest; 
        
        /// <summary>
        /// http服务器
        /// </summary>
        private SongHttpServer httpServer;

        /// <summary>
        /// 启动服务器
        /// </summary>
        private void Start()
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = SongHttpServer.GetMainLocalIP();
            }
            httpServer = SongHttpServer.Start(ip,port, HandleHttpRequest);
            Debug.Log($"Http服务器已启动，http://{ip}:{port}");
            
            //获取并执行所有组件的初始化方法
            foreach (var support in GetComponentsInChildren<ISongHttpServerSupport>())
            {
                support.Init();
            }
        }
        
        /// <summary>
        /// 处理Http请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void HandleHttpRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            OnRequest?.Invoke(request,response);
            switch (request.Url.AbsolutePath)
            {
                case "/":
#if  UNITY_EDITOR
                    var filePath = Application.dataPath+"/SFramework/Core/Config/index.html";
                    if (File.Exists(filePath))
                    {
                        SendHtmlResponse(response, File.ReadAllText(filePath));
                    }
                    else
                    {
                        SendTextResponse(response, "http服务器已启动！");
                    }
                    break;
#else              
                    SendTextResponse(response, "http服务器已启动！");
                    break;
#endif
            }
        }
        
        /// <summary>
        /// 发送文本响应
        /// </summary>
        public void SendTextResponse(HttpListenerResponse response, string text,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        =>SendResponse(response,"text/plain; charset=utf-8",Encoding.UTF8.GetBytes(text),statusCode);
        
        /// <summary>
        /// 发送Json响应
        /// </summary>
        /// <param name="response"></param>
        /// <param name="json"></param>
        /// <param name="statusCode"></param>
        public void SendJsonResponse(HttpListenerResponse response, string json,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        =>SendResponse(response,"application/json; charset=utf-8",Encoding.UTF8.GetBytes(json),statusCode);
        
        /// <summary>
        /// 发送文件响应
        /// </summary>
        /// <param name="response"></param>
        /// <param name="filePath"></param>
        /// <param name="statusCode"></param>
        public void SendFileResponse(HttpListenerResponse response, string filePath,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        =>SendResponse(response,"application/octet-stream",File.ReadAllBytes(filePath),statusCode);
        
        /// <summary>
        /// 发送Html响应
        /// </summary>
        /// <param name="response"></param>
        /// <param name="html"></param>
        /// <param name="statusCode"></param>
        public void SendHtmlResponse(HttpListenerResponse response, string html,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        =>SendResponse(response,"text/html",Encoding.UTF8.GetBytes(html),statusCode);
        
        /// <summary>
        /// 返回请求
        /// </summary>
        /// <param name="response">响应</param>
        /// <param name="contentType">内容类型</param>
        /// <param name="buffer">数据</param>
        /// <param name="statusCode">状态码</param>
        public void SendResponse(HttpListenerResponse response,string contentType ,byte[] buffer,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }


        /// <summary>
        /// 断开连接
        /// </summary>
        void OnDestroy()
        {
            Debug.Log($"Http服务器已关闭，http://{ip}:{port}");
            httpServer?.Stop();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Reset()
        {
            ip = SongHttpServer.GetMainLocalIP();
        }
    }
}