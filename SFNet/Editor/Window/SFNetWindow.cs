using UnityEditor;
using UnityEngine.UIElements;

namespace SFramework.SFNet.Editor.Window
{
    /// <summary>
    /// 网络模块窗口
    /// </summary>
    public partial class SfNetWindow:EditorWindow
    {
        /// <summary>
        /// 左侧栏滚动容器
        /// </summary>
        private ScrollView _sliderContainer;
        /// <summary>
        /// 创建网络项按钮
        /// </summary>
        private Button _createItemButton;
        /// <summary>
        /// 创建网络项面板
        /// </summary>
        private VisualElement _createPanel;
        /// <summary>
        /// 创建UDP网络项按钮
        /// </summary>
        private Button _udpItemButton;
        /// <summary>
        /// 创建TCP网络项按钮
        /// </summary>
        private Button _tcpItemButton;
        /// <summary>
        /// 创建取消按钮
        /// </summary>
        private Button _createCancelButton;
        /// <summary>
        /// 内容区域
        /// </summary>
        private VisualElement _contentArea;
        /// <summary>
        /// 发送区域
        /// </summary>
        private VisualElement _sendArea;
        /// <summary>
        /// 发送输入框
        /// </summary>
        private TextField _sendInput;
        /// <summary>
        /// 发送按钮
        /// </summary>
        private Button _sendButton;
        
        /// <summary>
        /// 打开网络模块窗口
        /// </summary>
        [MenuItem("SFramework/网络调试窗口")]
        public static void OpenNetWindow()
        {
            var window = GetWindow<SfNetWindow>("网络调试窗口");
            window.Show();
        }

        /// <summary>
        /// 创建GUI
        /// </summary>
        private void CreateGUI()
        {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SFramework/SFNet/Editor/Window/SFNetWindow.uxml").
                CloneTree(rootVisualElement);
            
            _sliderContainer = rootVisualElement.Q<ScrollView>("SliderContainer");
            _createItemButton = rootVisualElement.Q<Button>("CreateItem");
            _createPanel = rootVisualElement.Q<VisualElement>("CreatePanel");
            _udpItemButton = rootVisualElement.Q<Button>("UdpItem");
            _tcpItemButton = rootVisualElement.Q<Button>("TcpItem");
            _createCancelButton = rootVisualElement.Q<Button>("CreateCancel");
            _contentArea = rootVisualElement.Q<GroupBox>("ContentArea");
            
            // 创建发送区域
            CreateSendArea();

            InitSlider();
        }
        
        /// <summary>
        /// 创建发送区域
        /// </summary>
        private void CreateSendArea()
        {
            _sendArea = new VisualElement();
            _sendArea.style.flexDirection = FlexDirection.Row;
            _sendArea.style.paddingTop = 8;
            _sendArea.style.paddingBottom = 8;
            _sendArea.style.paddingLeft = 8;
            _sendArea.style.paddingRight = 8;
            _sendArea.style.marginTop = 8;
            _sendArea.style.display = DisplayStyle.None;
            
            _sendInput = new TextField();
            _sendInput.style.flexGrow = 1;
            _sendInput.style.marginRight = 8;
            _sendInput.style.unityTextAlign = TextAnchor.MiddleLeft;
            _sendArea.Add(_sendInput);
            
            _sendButton = new Button();
            _sendButton.text = "发送";
            _sendButton.style.minWidth = 60;
            _sendButton.style.backgroundColor = SfColor.HexToColor("#4CAF50");
            _sendButton.style.color = Color.white;
            _sendButton.style.borderRadius = 6;
            _sendButton.clicked += OnSendClicked;
            _sendArea.Add(_sendButton);
            
            // 将发送区域添加到ContentView中
            var contentView = rootVisualElement.Q<ScrollView>("ContentView");
            if (contentView != null)
            {
                contentView.Add(_sendArea);
            }
        }
        
        /// <summary>
        /// 发送按钮点击事件
        /// </summary>
        private void OnSendClicked()
        {
            HandleSend();
        }
        
        /// <summary>
        /// 窗口销毁时关闭所有网络
        /// </summary>
        private void OnDestroy()
        {
            CloseAllUDP();
            CloseAllTCP();
        }
    }
}
