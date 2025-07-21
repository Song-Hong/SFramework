using System;
using System.IO;
using System.Net;
using System.Text;
using SFramework.Core.Module.Server.Http;
using Song.Core.Module;
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
                    SendTextResponse(response, "欢迎来到首页！");
                    break;

                case "/api/getplayerinfo":
                    string json = "{\"playerName\": \"Song\", \"level\": 99, \"health\": 100}";
                    SendJsonResponse(response, json);
                    break;

                case "/posttest":
                    if (request.HttpMethod == "POST")
                    {
                        string postBody = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
                        Debug.Log($"收到POST数据: {postBody}");
                        SendTextResponse(response, $"服务器已收到您的POST数据: {postBody}");
                    }
                    else
                    {
                        SendTextResponse(response, "请使用POST方法访问此路径。", HttpStatusCode.MethodNotAllowed);
                    }

                    break;

                default:
                    SendTextResponse(response, "404 - 未找到页面", HttpStatusCode.NotFound);
                    break;
            }
        }
        
        /// <summary>
        /// 发送文本响应
        /// </summary>
        private void SendTextResponse(HttpListenerResponse response, string text,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = "text/plain; charset=utf-8";
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        
        /// <summary>
        /// 发送Json响应
        /// </summary>
        /// <param name="response"></param>
        /// <param name="json"></param>
        /// <param name="statusCode"></param>
        private void SendJsonResponse(HttpListenerResponse response, string json,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = "application/json; charset=utf-8";
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        void OnDestroy()
        {
            Debug.Log($"Http服务器已关闭，http://{ip}:{port}");
            httpServer?.Stop();
        }
    }
}