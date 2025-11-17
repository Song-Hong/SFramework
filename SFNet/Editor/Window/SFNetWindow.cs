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
            // 加载UXML文件
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SFramework/SFNet/Editor/Window/SFNetWindow.uxml").
                CloneTree(rootVisualElement);
            
            _sliderContainer = rootVisualElement.Q<ScrollView>("SliderContainer");
            _createItemButton = rootVisualElement.Q<Button>("CreateItem");
            _createPanel = rootVisualElement.Q<VisualElement>("CreatePanel");
            _udpItemButton = rootVisualElement.Q<Button>("UdpItem");
            _tcpItemButton = rootVisualElement.Q<Button>("TcpItem");
            _createCancelButton = rootVisualElement.Q<Button>("CreateCancel");
            _contentArea = rootVisualElement.Q<GroupBox>("ContentArea");

            // 初始化左侧栏
            InitSlider();
        }
        
        /// <summary>
        /// 窗口销毁时关闭所有UDP网络
        /// </summary>
        private void OnDestroy()
        {
            CloseAllUDP();
        }
    }
}