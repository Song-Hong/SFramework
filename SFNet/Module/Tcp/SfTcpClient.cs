using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SFramework.SFNet.Module.Tcp
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public class SfTcpClient
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
        /// 接收到消息
        /// </summary>
        public event Action<string> OnReceived;
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
        public static SfTcpClient Connect(int port, Action<string> onReceived=null)
        {
            var ip = GetMainLocalIP();
            var sfTcpClient = Connect(ip,port,onReceived);
            return sfTcpClient;
        }
        
        /// <summary>
        /// 创建TCP连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="onReceived">接收到消息时</param>
        public static SfTcpClient Connect(string ip, int port, Action<string> onReceived=null)
        {
            var sfTcpClient = Connect(ip,port);
            if(onReceived!=null)
                sfTcpClient.OnReceived += onReceived;
            return sfTcpClient;
        }
        
        /// <summary>
        /// 创建TCP连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public static SfTcpClient Connect(string ip, int port)
        {
            //创建一个负责监听IP和端口号的Socket
            var songTcpClient = new SfTcpClient();
            songTcpClient._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            //建立连接请求
            var iPAddress = IPAddress.Parse(ip);
            EndPoint endPoint = new IPEndPoint(iPAddress, port);
            songTcpClient._socket.Connect(endPoint);
            
            //创建一个线程，用于接收数据
            var thread = new Thread(songTcpClient.Received);
            thread.IsBackground = true;
            thread.Start();
            
            //加入线程列表
            songTcpClient._threads.Add(thread);
            
            //初始化
            songTcpClient.IP = ip;
            songTcpClient.Port = port;
            Debug.Log($"TCP服务器连接成功！{ip}:{port}");
            return songTcpClient;
        }
        #endregion

        #region 监听消息
        /// <summary>
        /// 服务器端不停的接收客户端发来的消息
        /// </summary>
        private void Received()
        {
            try
            {
                while (true)
                {
                    var buffer = new byte[1024 * 6]; //客户端连接服务器成功后，服务器接收客户端发送的消息
                    var len = _socket.Receive(buffer);   //实际接收到的有效字节数
                    if (len == 0)
                    {
                        break;
                    }

                    var str = Encoding.UTF8.GetString(buffer, 0, len);

                    OnReceived?.Invoke(str);
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
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void Send(string msg)
        {
            var buffer = Encoding.UTF8.GetBytes(msg);
            _socket.Send(buffer);
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
            var ipv4Addresses = new List<string>();
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) continue;
                    // 筛选符合常见 IPv4 范围的地址
                    if (IsValidIPv4(ip.ToString()))
                    {
                        ipv4Addresses.Add(ip.ToString());
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
            //关闭连接
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(true);
            //关闭监听
            _socket.Close();
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