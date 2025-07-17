using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Song.Core.Module.Server
{
    /// <summary>
    /// UDP服务 协程
    /// </summary>
    public class SongUDPServerIEnumerator:ISongUDPServer
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
        /// UDP Socket 连接
        /// </summary>
        private Socket _socket;
        /// <summary>
        /// 服务是否开启/连接
        /// volatile 关键字确保多线程之间正确地访问
        /// </summary>
        private volatile bool _isConnected;
        /// <summary>
        /// 接收到消息事件 (将在Unity主线程触发)
        /// 参数: IP, Port, 消息
        /// </summary>
        public event Action<string, int, string> ReceivedIPPort;
        /// <summary>
        /// 用于接收消息的后台线程
        /// </summary>
        private Coroutine _receiveCoroutine;
        /// <summary>
        /// 拥有者
        /// </summary>
        private MonoBehaviour _owner;
        #endregion

        #region 构造与连接
        /// <summary>
        /// 私有构造函数，防止外部直接实例化
        /// </summary>
        private SongUDPServerIEnumerator() { }

        /// <summary>
        /// 创建并启动UDP服务，使用本地主要IP
        /// </summary>
        /// <param name="owner">拥有者</param>
        /// <param name="port">要绑定的端口号</param>
        /// <param name="onReceived">接收到消息时的回调函数</param>
        /// <returns>创建的服务实例</returns>
        public static SongUDPServerIEnumerator Connect(MonoBehaviour owner,int port, Action<string, int, string> onReceived = null)
        {
            var ip = GetMainLocalIP();
            return Connect(owner,ip, port, onReceived);
        }

        /// <summary>
        /// 创建并启动UDP服务
        /// </summary>
        /// <param name="owner">拥有者</param>
        /// <param name="ip">要绑定的IP地址</param>
        /// <param name="port">要绑定的端口号</param>
        /// <param name="onReceived">接收到消息时的回调函数</param>
        /// <returns>创建的服务实例</returns>
        public static SongUDPServerIEnumerator Connect(MonoBehaviour owner,string ip, int port, Action<string, int, string> onReceived = null)
        {
            var server = new SongUDPServerIEnumerator();
            server._owner = owner;
            server.IP = ip;
            server.Port = port;
            try
            {
                // 初始化Socket
                server._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                
                // 设置套接字选项以允许发送广播包
                server._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                
                // 绑定IP和端口
                var ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                server._socket.Bind(ipPoint);

                // 标记为已连接状态
                server._isConnected = true;
                
                // 创建并启动后台线程用于接收消息
                server._receiveCoroutine = owner.StartCoroutine(server.ReceiveLoop());
                            
                // 绑定消息事件
                if (onReceived != null)
                {
                    server.ReceivedIPPort += onReceived;
                }
                
                Debug.Log($"UDP服务开启成功！监听于 {ip}:{port}");
                return server;
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP服务开启失败: {e.Message}");
                server.Disconnect(); // 如果启动失败，确保资源被释放
                return null;
            }
        }
        #endregion

        #region 监听消息
        /// <summary>
        /// 在后台线程中循环接收消息
        /// </summary>
        private IEnumerator ReceiveLoop()
        {
            while (_isConnected)
            {
                try
                {
                    var bytes = new byte[1024]; // 缓冲区可以适当增大
                    // EndPoint 用来接收发送方的地址信息
                    EndPoint receivePoint = new IPEndPoint(IPAddress.Any, 0);
                    
                    // 阻塞接收数据，当Socket被Close时，这里会抛出异常
                    var len = _socket.ReceiveFrom(bytes, ref receivePoint);
                    
                    if (len > 0)
                    {
                        var ipEndPoint = receivePoint as IPEndPoint;
                        var targetIP = ipEndPoint.Address.ToString();
                        var targetPort = ipEndPoint.Port;
                        var msg = Encoding.UTF8.GetString(bytes, 0, len);

                        ReceivedIPPort?.Invoke(targetIP, targetPort, msg);
                    }
                }
                catch (SocketException ex)
                {
                    // 当 _isConnected 为 false 时，是我们主动关闭了Socket，这是预期的异常
                    if (_isConnected)
                    {
                        Debug.LogError($"UDP接收时发生套接字错误: {ex.Message}");
                    }
                }
                catch (Exception e)
                {
                    // 捕获其他可能的异常
                    Debug.LogError($"UDP接收线程发生未知错误: {e.Message}");
                }
                yield return null;
            }
            Debug.Log("UDP接收线程已停止。");
        }
        #endregion
        
        #region 发送消息
        /// <summary>
        /// 向指定IP和端口发送消息
        /// </summary>
        public void Send(string ip, int port, string msg)
        {
            if (!_isConnected) return;
            try
            {
                var receivePoint = new IPEndPoint(IPAddress.Parse(ip), port);
                var buffer = Encoding.UTF8.GetBytes(msg);
                _socket.SendTo(buffer, receivePoint);
            }
            catch(Exception e)
            {
                Debug.LogError($"UDP发送消息失败到 {ip}:{port} -> {e.Message}");
            }
        }
        
        /// <summary>
        /// 向当前网段中特定端口发送消息
        /// </summary>
        public void Send(int port, string msg) => Send(this.IP, port, msg);
        
        /// <summary>
        /// 向指定端口广播消息
        /// </summary>
        public void SendBroadcast(int port, string msg)
        {
            // Android平台由于权限限制，可能不支持标准的IPAddress.Broadcast
            if (Application.platform == RuntimePlatform.Android)
            {
                SendBroadcastWithForeach(port, msg);
            }
            else
            {
                Send(IPAddress.Broadcast.ToString(), port, msg);
            }
        }

        /// <summary>
        /// 向绑定端口广播消息
        /// </summary>
        public void SendBroadcast(string msg) => SendBroadcast(this.Port, msg);
        
        /// <summary>
        /// 通过遍历网段的方式广播消息 (主要用于Android平台)
        /// </summary>
        public void SendBroadcastWithForeach(int port, string msg) => SendBroadcastWithForeach(this.IP, port, msg);

        /// <summary>
        /// 通过遍历指定IP所在网段的方式广播消息
        /// </summary>
        public void SendBroadcastWithForeach(string baseIp, int port, string msg)
        {
            if (!_isConnected) return;
            try
            {
                var lastIndexOf = baseIp.LastIndexOf(".", StringComparison.Ordinal);
                if(lastIndexOf == -1) return;

                var head = baseIp.Substring(0, lastIndexOf + 1);
                var selfPoint = new IPEndPoint(IPAddress.Parse(this.IP), this.Port);
                var buffer = Encoding.UTF8.GetBytes(msg);

                // 遍历网段内的所有主机 (1-254)
                for (var i = 1; i < 255; i++)
                {
                    var targetIpString = head + i;
                    var receivePoint = new IPEndPoint(IPAddress.Parse(targetIpString), port);
                    
                    // 【修复】跳过自己，使用 continue 而不是 return
                    if (Equals(selfPoint.Address, receivePoint.Address))
                    {
                        continue;
                    }

                    _socket.SendTo(buffer, receivePoint);
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"UDP遍历广播失败 -> {e.Message}");
            }
        }
        #endregion
        
        #region 工具
        /// <summary>
        /// 获取本机主要的本地IPv4地址
        /// </summary>
        public static string GetMainLocalIP() 
        {
            List<string> ips = GetLocalIPs();
            // 优先返回 192.168.x.x 网段的地址
            foreach (var ip in ips)
            {
                if (ip.StartsWith("192.168."))
                {
                    return ip;
                }
            }
            // 否则返回第一个找到的地址
            return ips.Count > 0 ? ips[0] : "127.0.0.1";
        }
        
        /// <summary>
        /// 获取本机所有IPv4地址列表
        /// </summary>
        public static List<string> GetLocalIPs()
        {
            var ipv4Addresses = new List<string>();
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
        
        #region 断开连接
        /// <summary>
        /// 断开连接并释放资源
        /// </summary>
        public void Disconnect()
        {
            if (!_isConnected) return;

            // 设置状态为未连接，这将使接收线程的循环条件变为false
            _isConnected = false;

            // 关闭Socket。这会使阻塞在ReceiveFrom上的线程立即抛出SocketException，从而退出循环
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            // 关闭接收协程
            if (_receiveCoroutine != null)
                _owner.StopCoroutine(_receiveCoroutine);
            
            Debug.Log("UDP服务已断开。");
        }
        #endregion
    }
}