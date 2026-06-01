using System;
using SFramework.Core.Extends.UIElement;
using SimpleJSON;
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
        /// <param name="time">时间</param>
        /// <param name="isSelf">是否是自己发送的</param>
        public void CreateMessage(string ip,int port,string content,string time = "",bool isSelf = false)
        {
            // 创建消息面板
            var panel = new VisualElement();
            panel.AddToClassList(isSelf? "sfnet-net_panel_self" : "sfnet-net_panel_target");
            
            // 创建标题
            var label = new Label();
            label.AddToClassList("sfnet-net_panel_title");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.text = $"{ip}:{port}  " +
                         $"{(string.IsNullOrEmpty(time)?DateTime.Now.ToString("hh:mm:ss"):time)}";
            panel.Add(label);
            
            // 创建标签页
            var sfTab = new SfTab();
            sfTab.AddChoice("原文","JSON");
            sfTab.SetTitle("");
            sfTab.style.alignItems = Align.FlexEnd;
            sfTab.style.alignSelf = Align.FlexEnd;
            sfTab.style.marginTop = -5;
            sfTab.ChooseBackground.style.backgroundColor = Color.clear;
            sfTab.OnChoiceChanged+= value =>
            {
                panel.Query<Label>().Class("sfnet-net_panel_field").ForEach(item =>
                {
                    panel.Remove(item);
                }); 
                switch (value)
                {
                    case "原文":
                        var field = new Label();
                        field.AddToClassList("sfnet-net_panel_field");
                        field.text = content;
                        panel.Add(field);
                        break;
                    case "JSON":
                        CreateMessageContent(content,panel,sfTab);
                        break;
                }
            };
            label.Add(sfTab);
            
            // 设置Json数据
            var jsonNode = JSON.Parse(content);
            if (jsonNode.Count <= 0)
            {
                sfTab.Select("原文");
                sfTab.style.display = DisplayStyle.None;
            }
            else
            {
                sfTab.Select("JSON");
                sfTab.style.display = DisplayStyle.Flex;
            }
            
            // 添加到内容区域
            _contentArea.Add(panel);
        }

        /// <summary>
        /// 创建消息内容
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="panel">面板</param>
        /// <param name="sfTab">标签页</param>
        public void CreateMessageContent(string content,VisualElement panel,SfTab sfTab)
        {
            try
            {
                var jsonNode = JSON.Parse(content);
                if (jsonNode.Count < 0)
                {
                    var field = new Label();
                    field.AddToClassList("sfnet-net_panel_field");
                    field.text = content;
                    panel.Add(field);

                }
                else
                {
                    sfTab.style.display = DisplayStyle.Flex;
                    foreach (var keyValuePair in jsonNode)
                    {
                        var field = new Label();
                        field.AddToClassList("sfnet-net_panel_field");
                        field.text = $"{keyValuePair.Key} : " +
                                     $"{keyValuePair.Value.ToString().Replace("\"","")}";
                        panel.Add(field);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var field = new Label();
                field.AddToClassList("sfnet-net_panel_field");
                field.text = content;
                panel.Add(field);
                sfTab.style.display = DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// 加载服务器数据
        /// </summary>
        /// <param name="btn">按钮</param>
        public void LoadServerData(Button btn)
        {
            // 添加服务器数据
            if (!_sfServerData.ContainsKey(btn))
                return;
            // 加载服务器数据
            foreach (var data in _sfServerData[btn])
            {
                CreateMessage(data.Item1,data.Item2,data.Item3,data.Item4,data.Item5);
            }
        }
    }
}