﻿using System;
using System.Collections.Generic;
using System.Threading;
using SFramework.Core.Module.Config;
using SFramework.Core.Support.UDP;
using Song.Core.Module;
using Song.Core.Module.Server;
using UnityEngine;

namespace SFramework.Core.Mono
{
    /// <summary>
    /// UDP 开箱即用
    /// </summary>
    public class SongUDPServerMono:MonoSingleton<SongUDPServerMono>
    {
        [Header("IP")] public string ip;
        [Header("端口号")] public int port = 8888;
        [Header("打印消息日志")]public bool printLog = true;
        
        /// <summary>
        /// UDP服务器
        /// </summary>
        private ISongUDPServer server;
        
        /// <summary>
        /// 接收到消息
        /// </summary>
        public event Action<string> Received;
        /// <summary>
        /// 接收到消息 IP Port 消息
        /// </summary>
        public event Action<string,int,string> ReceivedIPPort;
        
        public void Start()
        {
            //当 IP 为空时 自动获取本地IP
            if(string.IsNullOrWhiteSpace(ip))
            {
                ip = SongUDPServer.GetMainLocalIP();
            }
            
            var mainThread = SynchronizationContext.Current;
            try
            {
                server = SongUDPServer.Connect(ip, port, (ip,port,msg) =>
                {
                    if(printLog)
                        Debug.Log($"接收到{ip}:{port}消息: {msg}");
                    mainThread.Post(_ =>
                    {
                        Received?.Invoke(msg);
                    }, null);
                    mainThread.Post(_ =>
                    {
                        ReceivedIPPort?.Invoke(ip,port,msg);
                    }, null);
                });

                //获取并执行所有组件的初始化方法
                foreach (var support in GetComponentsInChildren<ISongUDPSupport>())
                {
                    support.Init();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void Reset()
        {
            ip = SongUDPServer.GetMainLocalIP();
        }

        /// <summary>
        /// 关闭客户端断开连接
        /// </summary>
        private void OnDestroy()
        {
            server?.Disconnect();
        }
        
        /// <summary>
        /// 发送消息至指定客户端
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="msg">消息</param>
        public void Send(string ip, int port, string msg)
        {
            server.Send(ip,port,msg);
        }
        
        /// <summary>
        /// 向同网段广播消息
        /// </summary>
        /// <param name="msg">消息</param>
        public void SendBroadcast(int port,string msg)
        {
            server.SendBroadcast(port,msg);
        }
        
        /// <summary>
        /// 发送广播消息使用遍历的方式
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="msg"></param>
        public void SendBroadcastWithForeach(string ip,int port,string msg)
        {
            server.SendBroadcastWithForeach(ip,port, msg);
        }
    }
}