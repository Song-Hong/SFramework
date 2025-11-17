using SFramework.Core.SfUIElementExtends;
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
        /// 创建网络模块窗口 网络内容
        /// </summary>
        public void ShowContent(Button btn)
        {
            // 移除所有内容
            _contentArea.Query<VisualElement>().ForEach(item =>
            {
                if(item.parent == _contentArea) 
                    _contentArea.Remove(item);
            });
            // 添加UDP服务器内容
            if (_sfUDPServers.ContainsKey(btn))
            {
                _sfUDPServers[btn].ReceivedIPPort += BindEvents;
            }
        }
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="content">内容</param>
        public void BindEvents(string ip,int port,string content)
        {
            CreateMessage(ip,port,content);
        }
        
        /// <summary>
        /// 解绑事件
        /// </summary>
        public void DisBindEvents(Button btn)
        {
            if (btn == null)
            {
                return;
            }
            // 原来的代码继续
            if (_sfUDPServers.Count <= 0) return;
            // 现在可以安全地使用 btn 作为字典的键了
            if (!_sfUDPServers.TryGetValue(btn, out var server)) return; 
            if (server == null) return;
            server.ReceivedIPPort -= BindEvents;
        }

        /// <summary>
        /// 创建消息
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="content">内容</param>
        public void CreateMessage(string ip,int port,string content)
        {
            // 创建消息面板
            var panel = new VisualElement();
            panel.AddToClassList("sfnet-net_panel_target");
            
            // 创建标题
            var label = new Label();
            label.AddToClassList("sfnet-net_panel_title");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.text = $"{ip}:{port}";
            panel.Add(label);
            
            // 创建标签页
            var sfTab = new SfTab();
            sfTab.AddChoice("原文","JSON");
            sfTab.SetTitle("");
            sfTab.style.alignItems = Align.FlexEnd;
            sfTab.style.alignSelf = Align.FlexEnd;
            sfTab.style.marginTop = -5;
            sfTab.ChooseBackground.style.backgroundColor = Color.clear;
            // sfTab.style.
            label.Add(sfTab);
            
            // 创建内容
            var field = new Label();
            field.AddToClassList("sfnet-net_panel_field");
            field.text = content;
            panel.Add(field);
            
            // 添加到内容区域
            _contentArea.Add(panel);
        }

        /// <summary>
        /// 创建JSON消息
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="content">内容</param>
        public void CreateJsonMessage(string ip,int port,string content)
        {
            
        }
    }
}