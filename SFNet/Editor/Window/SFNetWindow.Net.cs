using System;
using System.Collections.Generic;
using SFramework.SFNet.Module.Tcp;
using SFramework.SFNet.Module.Udp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFNet.Editor.Window
{
    /// <summary>
    /// 网络模块窗口 网络
    /// </summary>
    public partial class SfNetWindow:EditorWindow
    {
        #region UDP
        /// <summary>
        /// UDP服务器按钮字典
        /// </summary>
        private readonly Dictionary<Button,SfUDPServer> _sfUDPServers
            = new Dictionary<Button, SfUDPServer>();
        #endregion
        
        #region TCP
        /// <summary>
        /// TCP服务器按钮字典
        /// </summary>
        private readonly Dictionary<Button,SfTcpServer> _sfTCPServers
            = new Dictionary<Button, SfTcpServer>();
        #endregion
        
        #region 数据
        /// <summary>
        /// 服务器数据字典
        /// 键：按钮
        /// 值：IP地址、端口号、内容、时间,是否是自己发送的
        /// </summary>
        private Dictionary<Button,List<Tuple<string,int,string,string,bool>>> _sfServerData
            = new Dictionary<Button, List<Tuple<string, int, string, string, bool>>>();
        #endregion
        
        #region UDP方法
        /// <summary>
        /// 创建UDP网络
        /// </summary>
        public bool CreateUDP(string ip,int port,Button button)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            if (port is <= 0 or > 65535) return false;
            
            var sfUDPServer = SfUDPServer.Start(ip, port);
            if (sfUDPServer == null) return false;
            _sfUDPServers.Add(button,sfUDPServer);
            
            SelectButton(button);
            
            sfUDPServer.ReceivedIPPort+= (msgIp, msgPort, content) =>
                SaveServerData(button,msgIp,msgPort,content);

            return true;
        }
        
        /// <summary>
        /// 关闭UDP网络
        /// </summary>
        public bool CloseUDP(SfUDPServer sfUDPServer)
        {
            if (_sfUDPServers.ContainsValue(sfUDPServer))
            {
                sfUDPServer.Stop();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 关闭所有UDP网络
        /// </summary>
        public void CloseAllUDP()
        {
            foreach (var sfUDPServer in _sfUDPServers)
            {
                sfUDPServer.Value.Stop();
            }
            _sfUDPServers.Clear();
        }
        #endregion
        
        #region TCP方法
        /// <summary>
        /// 创建TCP网络
        /// </summary>
        public bool CreateTCP(string ip,int port,Button button)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            if (port is <= 0 or > 65535) return false;
            
            try
            {
                var sfTcpServer = SfTcpServer.Start(ip, port, (socket, content) =>
                {
                    var remoteEndPoint = socket.RemoteEndPoint;
                    var targetIp = remoteEndPoint?.ToString() ?? "Unknown";
                    var targetPort = 0;
                    if (remoteEndPoint is System.Net.IPEndPoint ep)
                    {
                        targetPort = ep.Port;
                    }
                    SaveServerData(button, targetIp, targetPort, content);
                });
                
                if (sfTcpServer == null) return false;
                _sfTCPServers.Add(button, sfTcpServer);
                
                SelectButton(button);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"TCP服务器创建失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 关闭TCP网络
        /// </summary>
        public bool CloseTCP(Button button)
        {
            if (_sfTCPServers.TryGetValue(button, out var server))
            {
                server.Stop();
                _sfTCPServers.Remove(button);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 关闭所有TCP网络
        /// </summary>
        public void CloseAllTCP()
        {
            foreach (var sfTcpServer in _sfTCPServers)
            {
                sfTcpServer.Value.Stop();
            }
            _sfTCPServers.Clear();
        }
        #endregion
        
        #region 数据处理
        /// <summary>
        /// 保存服务器数据
        /// </summary>
        public void SaveServerData(Button btn,string ip,int port,string content)
        {
            if (!_sfServerData.ContainsKey(btn))
            {
                _sfServerData.Add(btn,new List<Tuple<string, int, string, string, bool>>());
            }
            _sfServerData[btn].Add(new Tuple<string, int, string, string, bool>(
                ip,
                port,
                content,
                DateTime.Now.ToString("HH:mm:ss"),
                false));
        }
        
        /// <summary>
        /// 处理发送消息
        /// </summary>
        public void HandleSend()
        {
            if (_nowSelectItem == null) return;
            if (string.IsNullOrEmpty(_sendInput?.value)) return;
            
            var msg = _sendInput.value;
            var isSent = false;
            
            // 尝试UDP发送
            if (_sfUDPServers.TryGetValue(_nowSelectItem, out var udpServer))
            {
                udpServer.SendBroadcast(msg);
                isSent = true;
            }
            // 尝试TCP发送
            else if (_sfTCPServers.TryGetValue(_nowSelectItem, out var tcpServer))
            {
                tcpServer.SendAll(msg);
                isSent = true;
            }
            
            if (isSent)
            {
                SaveServerData(_nowSelectItem, "本地", 0, msg);
                CreateMessage("本地", 0, msg, DateTime.Now.ToString("HH:mm:ss"), true);
                _sendInput.value = "";
            }
        }
        #endregion
    }
}
