using System;
using SFramework.SFNet.Editor.Replace;
using System.Collections.Generic;
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
        /// <summary>
        /// UDP服务器按钮字典
        /// </summary>
        private readonly Dictionary<Button,SfUDPServer> _sfUDPServers
            = new Dictionary<Button, SfUDPServer>();
        
        /// <summary>
        /// 创建UDP网络
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="button">按钮</param>
        /// <returns>是否创建成功</returns>
        public bool CreateUDP(string ip,int port,Button button)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return false;
            }
            if (port is <= 0 or > 65535)
            {
                return false;
            }
            
            // 创建UDP服务器
            var sfUDPServer = SfUDPServer.Connect(ip, port);
            if (sfUDPServer == null)
            {
                return false;
            }
            _sfUDPServers.Add(button,sfUDPServer);
            
            // 选择按钮
            SelectButton(button);
            
            return true;
        }
        
        /// <summary>
        /// 关闭UDP网络
        /// </summary>
        /// <param name="sfUDPServer">UDP服务器</param>
        /// <returns>是否关闭成功</returns>
        public bool CloseUDP(SfUDPServer sfUDPServer)
        {
            if (_sfUDPServers.ContainsValue(sfUDPServer))
            {
                sfUDPServer.Disconnect();
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
                sfUDPServer.Value.Disconnect();
            }
        }
    }
}