﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Song.Core.Module.Server
{
    /// <summary>
    /// TCP服务端
    /// </summary>
    public class SongTCPServer
    {
        #region 变量
        /// <summary>
        /// IP地址
        /// </summary>
        public string IP;
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port;
        /// <summary>
        /// TCP连接
        /// </summary>
        private Socket _socket;
        /// <summary>
        /// 服务是否开启
        /// </summary>
        private bool _isConnected = true;
        /// <summary>
        /// 客户端列表
        /// </summary>
        private List<Socket> _clients = new List<Socket>();
        /// <summary>
        /// 接收到消息
        /// </summary>
        public event Action<Socket,string> OnReceived;
        /// <summary>
        /// 使用到的全部线程
        /// </summary>
        private List<Thread> _threads = new List<Thread>();
        #endregion

        #region 构造函数
        /// <summary>
        /// 创建TCP连接
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="onReceived">接收到消息时</param>
        /// <param name="maxConnect">最大连接数</param>
        /// <returns></returns>
        public static SongTCPServer Connect(int port, Action<Socket,string> onReceived=null,int maxConnect = 10)
        {
            var ip = GetMainLocalIP();
            var songTcpServer = Connect(ip,port,onReceived,maxConnect);
            return songTcpServer;
        }
        
        /// <summary>
        /// 创建TCP连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="onReceived">接收到消息时</param>
        /// <param name="maxConnect">最大连接数</param>
        /// <returns></returns>
        public static SongTCPServer Connect(string ip, int port, Action<Socket,string> onReceived=null,int maxConnect = 10)
        {
            var songTcpServer = Connect(ip, port,maxConnect);
            if(onReceived!=null)
                songTcpServer.OnReceived += onReceived;
            return songTcpServer;
        }
        
        /// <summary>
        /// 创建TCP连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="onReceived">接收到消息时</param>
        /// <param name="maxConnect">最大连接数</param>
        /// <returns></returns>
        public static SongTCPServer Connect(string ip,int port,int maxConnect = 10)
        {
            var server = new SongTCPServer();
            //服务端创建一个负责监听IP和端口号的Socket
            server._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipAddress = IPAddress.Parse(ip);
            var point = new IPEndPoint(ipAddress, port);   //创建对象端口
            //绑定端口号
            server._socket.Bind(point);                        
            //设置监听，最大同时连接10台
            server._socket.Listen(maxConnect);                         
            //创建监听线程
            var thread = new Thread(server.Listen);
            thread.IsBackground = true;
            thread.Start();
            //加入线程列表
            server._threads.Add(thread);
            var thread2 = new Thread(server.ListenClientState);
            thread2.IsBackground = true;
            thread2.Start();
            //加入线程列表
            server._threads.Add(thread2);
            //开启服务
            server._isConnected = true;
            //初始化
            Debug.Log($"TCP服务器开启成功！{ip}:{port}");
            server.IP = ip;
            server.Port = port;
            return server;
        }
        #endregion

        #region 监听客户端连接及消息
        /// <summary>
        /// 等待客户端的连接 并且创建与之通信的Socket
        /// </summary>
        void Listen()
        {
            while (_isConnected)
            {
                try
                {
                    var clientSocket  = _socket.Accept();
                    Debug.Log("客户端连接成功！" + clientSocket.RemoteEndPoint);
                    _clients.Add(clientSocket);
                    var thread = new Thread(() =>
                    {
                        Received(clientSocket);
                    })
                    {
                        IsBackground = true
                    };
                    thread.Start();
                    //加入线程列表
                    _threads.Add(thread);
                }
                catch (Exception e)
                {
                    
                }
            }
        }
        
        /// <summary>
        /// 监听客户端连接状态
        /// </summary>
        void ListenClientState()
        {
            // while (_isConnected)
            // {
            //     for (var index = 0; index < _clients.Count; index++)
            //     {
            //         var client = _clients[index];
            //         if (client == null || !client.Connected)
            //         {
            //             Debug.Log($"客户端断开连接 : {client.RemoteEndPoint}");
            //             _clients.Remove(client);
            //         }
            //     }
            // }
        }
        
        /// <summary>
        /// 服务器端不停的接收客户端发来的消息
        /// </summary>
        void Received(Socket socket)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024 * 6]; //客户端连接服务器成功后，服务器接收客户端发送的消息
                    int len = socket.Receive(buffer); //实际接收到的有效字节数
                    if (len == 0)
                    {
                        break;
                    }

                    var str = Encoding.UTF8.GetString(buffer, 0, len);

                    OnReceived?.Invoke(socket,str);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        #endregion

        #region 发送消息
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void SendAll(string msg)
        {
            var buffer = Encoding.UTF8.GetBytes(msg);
            foreach (var client in _clients)
            {
                client.Send(buffer);
            }
        }
        
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void Send(string msg)
        {
            Send(0, msg);
        }
        
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="index">序号</param>
        /// <param name="msg">消息内容</param>
        public void Send(int index, string msg)
        {
            var buffer = Encoding.UTF8.GetBytes(msg);
            if(index<0||index>=_clients.Count) return;
            var client = _clients[index];
            client.Send(buffer);
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

            if (ips.Count==0)
            {
                return "127.0.0.1";
            }
            else
            {
                return ips[0];
            }
        }
        
        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocalIP()
        {
            List<string> ipv4Addresses = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        // 筛选符合常见 IPv4 范围的地址
                        if (IsValidIPv4(ip.ToString()))
                        {
                            ipv4Addresses.Add(ip.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取本地IP时发生错误: {ex.Message}");
            }
            return ipv4Addresses;
        }

        /// <summary>
        /// 检查是否为有效的 IPv4 地址
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private static bool IsValidIPv4(string ipAddress)
        {
            // 排除 2.0.0.0 等非标准地址
            if (string.IsNullOrWhiteSpace(ipAddress)) return false;

            // 检查常见私有地址范围或非标准地址
            string[] parts = ipAddress.Split('.');
            if (parts.Length != 4) return false;

            if (ipAddress.StartsWith("10.") || ipAddress.StartsWith("192.168.") || ipAddress.StartsWith("172.16.") || ipAddress.StartsWith("172.31."))
            {
                return true; // 私有地址范围
            }

            if (ipAddress.StartsWith("127.")) return false; // 排除回环地址
            if (ipAddress == "2.0.0.1") return false;       // 排除特定的异常地址
            if (ipAddress == "2.0.0.0") return false;       // 排除特定的异常地址

            // 如果需要更多限制，可以添加公网地址范围的校验逻辑

            return true; // 默认返回 true
        }
        #endregion

        #region 断开连接
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            _isConnected = false;
            //关闭监听
            _socket.Close();
            //断开全部客户端
            foreach (var client in _clients)
            {
                client.Close();
            }
            //反转
            _threads.Reverse();
            //关闭全部线程
            foreach (var thread in _threads)
            {
                if(thread!=null&&thread.IsAlive)
                    thread.Abort();
            }
        }
        #endregion
    }
}