using System;
using SFramework.Core.Extends.UIElement;
using SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFNet.Editor.Window
{
    /// <summary>
    /// 网络模块窗口 内容
    /// </summary>
    public partial class SfNetWindow:EditorWindow
    {
        /// <summary>
        /// 显示网络内容
        /// </summary>
        public void ShowContent(Button btn)
        {
            _contentArea.Query<VisualElement>().ForEach(item =>
            {
                if(item.parent == _contentArea) 
                    _contentArea.Remove(item);
            });
            
            // 绑定UDP事件
            if (_sfUDPServers.ContainsKey(btn))
            {
                _sfUDPServers[btn].ReceivedIPPort += BindEvents;
            }
        }
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        public void BindEvents(string ip,int port,string content)
        {
            CreateMessage(ip,port,content);
        }
        
        /// <summary>
        /// 解绑事件
        /// </summary>
        public void DisBindEvents(Button btn)
        {
            if (btn == null) return;
            if (_sfUDPServers.Count <= 0) return;
            if (!_sfUDPServers.TryGetValue(btn, out var server)) return; 
            if (server == null) return;
            server.ReceivedIPPort -= BindEvents;
        }

        /// <summary>
        /// 创建消息
        /// </summary>
        public void CreateMessage(string ip,int port,string content,string time = "",bool isSelf = false)
        {
            var panel = new VisualElement();
            panel.AddToClassList(isSelf? "sfnet-net_panel_self" : "sfnet-net_panel_target");
            
            var label = new Label();
            label.AddToClassList("sfnet-net_panel_title");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.text = $"{ip}:{port}  " +
                         $"{(string.IsNullOrEmpty(time)?DateTime.Now.ToString("HH:mm:ss"):time)}";
            panel.Add(label);
            
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
            
            var jsonNode = JSON.Parse(content);
            if (jsonNode == null || jsonNode.Count <= 0)
            {
                sfTab.Select("原文");
                sfTab.style.display = DisplayStyle.None;
            }
            else
            {
                sfTab.Select("JSON");
                sfTab.style.display = DisplayStyle.Flex;
            }
            
            _contentArea.Add(panel);
        }

        /// <summary>
        /// 创建消息内容
        /// </summary>
        public void CreateMessageContent(string content,VisualElement panel,SfTab sfTab)
        {
            try
            {
                var jsonNode = JSON.Parse(content);
                if (jsonNode == null || jsonNode.Count < 0)
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
                Debug.LogError(e.Message);
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
        public void LoadServerData(Button btn)
        {
            if (!_sfServerData.ContainsKey(btn))
                return;
            foreach (var data in _sfServerData[btn])
            {
                CreateMessage(data.Item1,data.Item2,data.Item3,data.Item4,data.Item5);
            }
        }
    }
}
