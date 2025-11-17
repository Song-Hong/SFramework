using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFNet.Editor.Window
{
    /// <summary>
    /// 网络模块窗口 左侧窗口
    /// </summary>
    public partial class SfNetWindow:EditorWindow
    {
        /// <summary>
        /// 当前选中的网络项按钮
        /// </summary>
        private Button _nowSelectItem;
        
        /// <summary>
        /// 初始化左侧栏
        /// </summary>
        public void InitSlider()
        {
            // 移除所有子项
            _sliderContainer.Query<VisualElement>().Class("sfnet-net_item").ForEach(item =>
            {
                _sliderContainer.Remove(item);
            });
            
            // 创建网络项按钮点击事件
            _createItemButton.clicked+=()=>
            {
                _createPanel.style.display = DisplayStyle.Flex;
                _createItemButton.style.display = DisplayStyle.None;
            };
            // 创建UDP网络项按钮点击事件
            _udpItemButton.clicked+=()=>
            {
                _createPanel.style.display = DisplayStyle.None;
                _createItemButton.style.display = DisplayStyle.Flex;
                
                CreateNetItem("UDP");
            };
            // 创建TCP网络项按钮点击事件
            _tcpItemButton.clicked+=()=>
            {
                _createPanel.style.display = DisplayStyle.None;
                _createItemButton.style.display = DisplayStyle.Flex;
                
                CreateNetItem("TCP");
            };
            // 创建取消按钮点击事件
            _createCancelButton.clicked+=()=>
            {
                // 隐藏创建网络项面板
                _createPanel.style.display = DisplayStyle.None;
                _createItemButton.style.display = DisplayStyle.Flex;
            };
            // 隐藏创建网络项面板
            _createPanel.style.display = DisplayStyle.None;
        }
        
        /// <summary>
        /// 创建网络项
        /// </summary>
        /// <param name="netType">网络类型</param>
        public void CreateNetItem(string netType)
        {
            var netPanel = new Button();
            netPanel.AddToClassList("sfnet-net_item");
            netPanel.name = netType;
            netPanel.clicked+=()=>
            {
                SelectButton(netPanel);
            };

            // 创建标题
            var title = new Label();
            title.AddToClassList("sfnet-net_item_title");
            title.text = netType;
            netPanel.Add(title);
            
            // 创建IP输入框
            var ip = new TextField();
            ip.AddToClassList("sfnet-net_item_input");
            ip.value = GetMainLocalIP();
            netPanel.Add(ip);
            
            // 创建端口输入框
            var port = new IntegerField();
            port.AddToClassList("sfnet-net_item_input");
            netPanel.Add(port);

            var state = new Button();
            state.AddToClassList("sfnet-net_item_btn");
            netPanel.Add(state);
            state.text = "连接";
            state.clicked+=()=>
            {
                if (state.text == "连接")
                {
                    if (netType == "UDP")
                    {
                        if (CreateUDP(ip.value, port.value, netPanel))
                        {
                            state.text = "断开";
                        }
                    }
                    else if (netType == "TCP")
                    {
                        // CreateTCP(ip.value, port.value, netPanel);
                    }
                }
                else
                {
                    if (netType == "UDP")
                    {
                        if (CloseUDP(_sfUDPServers[netPanel]))
                        {
                            state.text = "连接";
                        }
                    }
                    else if (netType == "TCP")
                    {
                        // CloseTCP(netPanel);
                    }
                }
            };
            
            // 创建关闭按钮
            var closeBtn = new Button();
            closeBtn.AddToClassList("sfnet-net_item_close");
            netPanel.Add(closeBtn);
            closeBtn.clicked+=()=>
            {
                _sliderContainer.Remove(netPanel);
            };
            
            _sliderContainer.Add(netPanel);
            _createItemButton.BringToFront();
            _createPanel.BringToFront();
        }

        /// <summary>
        /// 选择按钮
        /// </summary>
        /// <param name="button">按钮</param>
        public void SelectButton(Button button)
        {
            DisBindEvents(_nowSelectItem);
            _nowSelectItem?.RemoveFromClassList("sfnet-net_item_select");
            button.AddToClassList("sfnet-net_item_select");
            _nowSelectItem = button;
            ShowContent(button);
        }
        
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
    }
}