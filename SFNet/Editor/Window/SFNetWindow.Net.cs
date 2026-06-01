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
        /// 服务器数据字典
        /// 键：按钮
        /// 值：IP地址、端口号、内容、时间,是否是自己发送的
        /// </summary>
        private Dictionary<Button,List<Tuple<string,int,string,string,bool>>> _sfServerData
            = new Dictionary<Button, List<Tuple<string, int, string, string, bool>>>();
        
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
            var sfUDPServer = SfUDPServer.Start(ip, port);
            if (sfUDPServer == null)
            {
                return false;
            }
            _sfUDPServers.Add(button,sfUDPServer);
            
            // 选择按钮
            SelectButton(button);
            
            // 接收IP端口事件
            sfUDPServer.ReceivedIPPort+= (msgIp, msgPort, content) =>
                SaveServerData(button,msgIp,msgPort,content);

            
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
        }

        /// <summary>
        /// 保存服务器数据
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="content">内容</param>
        public void SaveServerData(Button btn,string ip,int port,string content)
        {
            // 添加服务器数据
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
    }
}