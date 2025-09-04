using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SFramework.Core.Module.Server.Http
{
    /// <summary>
    /// HTTP服务器
    /// 基于.Net的HttpListener实现，用于在Unity中快速搭建一个轻量级HTTP服务
    /// </summary>
    public class SongHttpServer
    {
        #region 变量

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// .NET内置的HTTP监听器
        /// </summary>
        private HttpListener _listener;

        /// <summary>
        /// 监听请求的线程
        /// </summary>
        private Thread _listenThread;

        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        private bool _isRunning = false;

        /// <summary>
        /// 接收到HTTP请求时的回调事件
        /// 参数1: HttpListenerRequest - 客户端的请求对象
        /// 参数2: HttpListenerResponse - 用于向客户端发送响应的对象
        /// </summary>
        public event Action<HttpListenerRequest, HttpListenerResponse> OnRequest;

        #endregion

        #region 构造函数与启动

        /// <summary>
        /// 启动HTTP服务器
        /// </summary>
        /// <param name="port">要监听的端口号</param>
        /// <param name="onRequest">收到请求时的回调</param>
        /// <returns>服务器实例</returns>
        public static SongHttpServer Start(int port, Action<HttpListenerRequest, HttpListenerResponse> onRequest = null)
        {
            var ip = GetMainLocalIP();
            var httpServer = Start(ip, port, onRequest);
            return httpServer;
        }

        /// <summary>
        /// 启动HTTP服务器
        /// </summary>
        /// <param name="ip">要监听的IP地址</param>
        /// <param name="port">要监听的端口号</param>
        /// <param name="onRequest">收到请求时的回调</param>
        /// <returns>服务器实例</returns>
        public static SongHttpServer Start(string ip, int port,
            Action<HttpListenerRequest, HttpListenerResponse> onRequest = null)
        {
            var server = new SongHttpServer();
            server.IP = ip;
            server.Port = port;
            if (onRequest != null)
            {
                server.OnRequest += onRequest;
            }

            try
            {
                server._listener = new HttpListener();
                // 添加监听地址前缀，例如 "http://192.168.1.5:8080/"
                // 使用 "+" 代替IP地址可以监听所有网络接口上的请求
                // server._listener.Prefixes.Add($"http://+:{port}/"); 
                server._listener.Prefixes.Add($"http://{ip}:{port}/");

                // 启动监听
                server._listener.Start();
                server._isRunning = true;

                // 创建并启动处理请求的后台线程
                server._listenThread = new Thread(server.HandleRequests);
                server._listenThread.IsBackground = true;
                server._listenThread.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"HTTP服务器启动失败: {e.Message}");
                return null;
            }

            return server;
        }

        #endregion

        #region 核心处理

        /// <summary>
        /// 在后台线程中循环处理进入的HTTP请求
        /// </summary>
        private void HandleRequests()
        {
            while (_isRunning)
            {
                try
                {
                    // 阻塞线程，直到接收到一个新的HTTP请求
                    var context = _listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    try
                    {
                        // 触发外部事件，让用户代码来处理请求
                        if (OnRequest != null)
                        {
                            OnRequest.Invoke(request, response);
                        }
                        else
                        {
                            // 如果没有订阅者，则提供一个默认的响应
                            DefaultHandle(request, response);
                        }
                    }
                    catch (Exception e)
                    {
                        // 处理请求时发生内部错误
                        Debug.LogError($"处理HTTP请求时发生错误: {e.Message}\n{e.StackTrace}");
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        byte[] buffer = Encoding.UTF8.GetBytes($"500 Internal Server Error: {e.Message}");
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    finally
                    {
                        // 必须关闭响应流以将响应发送回客户端
                        response.OutputStream.Close();
                    }
                }
                catch (HttpListenerException ex)
                {
                    // 当调用 _listener.Stop() 时，GetContext() 会抛出异常，这是正常行为
                    if (ex.ErrorCode == 995) // IO operation aborted.
                    {
                        // 服务器已停止
                    }
                    else
                    {
                        Debug.LogError($"HttpListener 监听时发生异常: {ex.Message}");
                    }
                }
                catch (Exception e)
                {
                    // 捕获其他可能的异常
                    if (_isRunning)
                        Debug.LogError($"HTTP服务器线程发生未知错误: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 默认的请求处理器
        /// </summary>
        private void DefaultHandle(HttpListenerRequest request, HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/html; charset=utf-8";

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<!DOCTYPE html>");
            htmlBuilder.Append("<html><head><title>SongHttpServer</title></head>");
            htmlBuilder.Append("<body style='font-family: Arial, sans-serif; text-align: center; margin-top: 50px;'>");
            htmlBuilder.Append("<h1>Hello from SongHttpServer!</h1>");
            htmlBuilder.Append($"<p>服务器时间: {DateTime.Now}</p>");
            htmlBuilder.Append($"<p>请求的路径: {request.Url.AbsolutePath}</p>");
            htmlBuilder.Append(
                "<p><em>(This is a default response. Please subscribe to the OnRequest event to handle requests.)</em></p>");
            htmlBuilder.Append("</body></html>");

            byte[] buffer = Encoding.UTF8.GetBytes(htmlBuilder.ToString());

            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        #endregion

        #region 工具

        /// <summary>
        /// 获取主要本地地址
        /// </summary>
        /// <returns>本地IP地址</returns>
        public static string GetMainLocalIP()
        {
            List<string> ips = GetLocalIP();

            if (ips.Count == 0)
            {
                // 如果没有找到合适的IP，回退到环回地址
                return "127.0.0.1";
            }
            else
            {
                return ips[0];
            }
        }

        /// <summary>
        /// 获取本地IP列表
        /// </summary>
        /// <returns>所有有效的IPv4地址</returns>
        public static List<string> GetLocalIP()
        {
            List<string> ipv4Addresses = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4Addresses.Add(ip.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"获取本地IP时发生错误: {ex.Message}");
            }

            return ipv4Addresses;
        }

        #endregion

        #region 停止服务

        /// <summary>
        /// 停止HTTP服务器
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;

            try
            {
                // 停止监听器，这将导致在GetContext()上的阻塞调用抛出异常并退出循环
                _listener.Stop();
                _listener.Close();

                // 终止线程
                if (_listenThread != null && _listenThread.IsAlive)
                {
                    _listenThread.Abort();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"停止HTTP服务器时发生错误: {e.Message}");
            }
            finally
            {
                _listenThread = null;
                _listener = null;
            }
        }

        #endregion
    }
}