using SFramework.Core.SfUIElementExtends;
using SFramework.Core.Support;
using SFramework.SFNet.Mono;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFNet.Editor
{
    /// <summary>
    /// TCP服务器编辑器
    /// </summary>
    [CustomEditor(typeof(SfTcpServerMono))]
    public class SongTcpServerMonoEditor:UnityEditor.Editor
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
            var title = new Label("SFrameworkTCP 服务器")
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            _rootElement.Add(title);
            
            var serverMono = target as SfTcpServerMono;
            
            // 添加服务器状态 服务器/客户端
            var serverStateChoices = new SfTab();
            serverStateChoices.SetTitle("服务器状态:");
            serverStateChoices.AddChoice("服务器","客户端");
            serverStateChoices.ChooseBackground.style.marginLeft = 19;
            _rootElement.Add(serverStateChoices);
            serverStateChoices.OnChoiceChanged += choice => //添加变化事件
            {
                if (choice == "服务器")
                {
                    if (serverMono != null) serverMono.serverState = SfTcpServerMono.ServerState.Server;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (choice == "客户端")
                {
                    if (serverMono != null) serverMono.serverState = SfTcpServerMono.ServerState.Client;
                    serializedObject.ApplyModifiedProperties();
                }
            };
            switch (serverMono.serverState)//设置默认值
            {
                case SfTcpServerMono.ServerState.Server:
                    serverStateChoices.Select("服务器");
                    break;
                case SfTcpServerMono.ServerState.Client:
                    serverStateChoices.Select("客户端");
                    break;
            }

            
            // 添加服务器状态 服务器/客户端 选项切换背景颜色
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
            var logStateChoice = new SfTab();
            logStateChoice.SetTitle("打印消息:");
            logStateChoice.AddChoice("打印","不打印");
            logStateChoice.ChooseBackground.style.marginLeft =32;
            _rootElement.Add(logStateChoice);
            logStateChoice.OnChoiceChanged += choice => //添加变化事件
            {
                if (choice == "打印")
                {
                    if (serverMono != null) serverMono.printLog = true;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (choice == "不打印")
                {
                    if (serverMono != null) serverMono.printLog = false;
                    serializedObject.ApplyModifiedProperties();
                }
            };
            if (serverMono.printLog)
            {
                logStateChoice.Select("打印");
            }
            else
            {
                logStateChoice.Select("不打印");
            }
            
            return _rootElement;
        }
    }
}