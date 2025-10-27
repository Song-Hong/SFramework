using System;
using SFramework.Core.Support;
using SFramework.SFNet.Mono;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFNet.Editor
{
    /// <summary>
    /// UDP服务器编辑器
    /// </summary>
    [CustomEditor(typeof(SfUdpServerMono))]
    public class SongUdpServerMonoEditor : UnityEditor.Editor
    {
        /// <summary>
        /// 根元素
        /// </summary>
        private VisualElement _rootElement;
        
        /// <summary>
        /// 创建Inspector GUI
        /// </summary>
        /// <returns>根元素</returns>
        public override VisualElement CreateInspectorGUI()
        {
            // 创建根元素
            _rootElement = new VisualElement();
            
            // 添加标题
            var title = new Label("SFrameworkUDP 服务器")
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            _rootElement.Add(title);
            
            // 
            var serverMono = target as SfUdpServerMono;
            
            var inactiveColor = SfColor.HexToColor("#242424"); // 非激活状态颜色 (同背景色)
            var activeColor = SfColor.HexToColor("#3C3C3C");   // 激活状态颜色 (稍亮)
            var inactiveTextColor = SfColor.HexToColor("#6D6D6D"); // 非激活状态文本颜色
            var activeTextColor = Color.white;   // 激活状态文本颜色
            
            //IP地址
            var ipAddressContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            // 添加IP地址容器到根元素
            _rootElement.Add(ipAddressContainer);
            
            var ipAddressLabel = new Label
            {
                style =
                {
                    fontSize = 14,
                    height = 26,
                    color = Color.white,
                    marginLeft = 5,
                    marginTop = 10,
                    alignSelf = Align.Center,
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
                text = "IP地址:",
            };
            // 添加IP地址标签到根元素
            ipAddressContainer.Add(ipAddressLabel);

            if (serverMono == null) return _rootElement;
            var ipAddressInput = new TextField
            {
                style =
                {
                    backgroundColor = inactiveColor, // 默认非激活
                    color = inactiveTextColor,
                    borderLeftWidth = 0,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                    height = 26,
                    marginLeft = 48,
                    marginTop = 8,
                    minWidth = 118,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                },
                value = serverMono.ip
            };

            ipAddressInput.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                // 1. 通知序列化系统开始修改
                serializedObject.Update(); 
                
                // 2. 将新值赋给 SerializedProperty
                serverMono.ip = evt.newValue;
        
                // 3. 将更改应用到组件并注册撤销
                serializedObject.ApplyModifiedProperties(); 
            });
            // 添加IP地址输入框到根元素
            ipAddressContainer.Add(ipAddressInput);

            // 端口
            var portContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            // 添加端口容器到根元素
            _rootElement.Add(portContainer);
            var portLabel = new Label
            {
                style =
                {
                    fontSize = 14,
                    height = 26,
                    color = Color.white,
                    marginLeft = 5,
                    marginTop = 10,
                    alignSelf = Align.Center,
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
                text = "端口:",
            };
            // 添加端口标签到根元素
            portContainer.Add(portLabel);
            
            var portInput = new TextField
            {
                style =
                {
                    backgroundColor = inactiveColor, // 默认非激活
                    color = inactiveTextColor,
                    borderLeftWidth = 0,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                    height = 26,
                    marginLeft = 61,
                    marginTop = 8,
                    minWidth = 118,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                },
                value = serverMono.port.ToString(),
            };
            
            portInput.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                // 1. 通知序列化系统开始修改
                serializedObject.Update(); 
                
                // 尝试解析输入值。如果输入为空，使用 0 作为默认值
                if (int.TryParse(evt.newValue, out var newPortValue))
                {
                    serverMono.port = newPortValue;
                }
                else
                {
                    portInput.value = serverMono.port.ToString();
                }
                
                // 2. 将新值赋给 SerializedProperty
                
                serializedObject.ApplyModifiedProperties();
            });
            // 添加端口输入框到根元素
            portContainer.Add(portInput);
            
            // 是否打印消息
             var logState = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                }
            };
            // 添加服务器状态标签
            var logStateLabel = new Label("打印消息:")
            {
                style =
                {
                    fontSize = 14,
                    height = 26,
                    color = Color.white,
                    marginLeft = 5,
                    marginTop = 10,
                    alignSelf = Align.Center,
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            // 服务器/客户端选择背景
            var logStateBg = new VisualElement
            {
                style =
                {
                    backgroundColor = SfColor.HexToColor("#242424"),
                    marginLeft = 32,
                    marginTop = 10,
                    paddingLeft = 2,
                    paddingRight = 2,
                    paddingTop = 2,
                    paddingBottom = 2,
                    flexDirection = FlexDirection.Row,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                    height = 26,
                }
            };

            //服务器按钮
            var logStateEnable = new Button
            {
                style =
                {
                    // backgroundColor = SfColor.HexToColor("#242424"), // 改为使用变量
                    backgroundColor = activeColor, // 默认激活服务器
                    color = activeTextColor,
                    borderLeftWidth = 0,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                },
                text = "打印"
            };
            //客户端按钮
            var logStateDisable = new Button
            {
                style =
                {
                    // backgroundColor = SfColor.HexToColor("#242424"), // 改为使用变量
                    backgroundColor = inactiveColor, // 默认非激活
                    color = inactiveTextColor,
                    borderLeftWidth = 0,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                },
                text = "不打印"
            };
            
            // --- 新增：注册点击事件 ---
            logStateEnable.clicked += () =>
            {
                // 设置是否打印消息为 "激活"
                logStateEnable.style.backgroundColor = activeColor;
                // 设置是否打印消息为 "非激活"
                logStateDisable.style.backgroundColor = inactiveColor;
                // 设置是否打印消息按钮文本颜色为白色
                logStateEnable.style.color = activeTextColor;
                // 设置是否打印消息按钮文本颜色为灰色
                logStateDisable.style.color = inactiveTextColor;

                if (serverMono != null) serverMono.printLog = true;
                serializedObject.ApplyModifiedProperties();
            };
            
            logStateDisable.clicked += () =>
            {
                // 设置是否打印消息为 "非激活"
                logStateEnable.style.backgroundColor = inactiveColor;
                // 设置是否打印消息为 "激活"
                logStateDisable.style.backgroundColor = activeColor;
                // 设置是否打印消息按钮文本颜色为灰色
                logStateEnable.style.color = inactiveTextColor;
                // 设置是否打印消息按钮文本颜色为白色
                logStateDisable.style.color = activeTextColor;
                
                if (serverMono != null) serverMono.printLog = false;
                serializedObject.ApplyModifiedProperties();
            };
            // -------------------------

            // 添加服务器/客户端选择按钮到背景
            logStateBg.Add(logStateEnable);
            logStateBg.Add(logStateDisable);
            // 添加是否打印消息标签到根元素
            logState.Add(logStateLabel);
            // 添加是否打印消息选择背景到根元素
            logState.Add(logStateBg);
            // 添加是否打印消息到根元素
            _rootElement.Add(logState);
            
            return _rootElement;
        }
    }
}