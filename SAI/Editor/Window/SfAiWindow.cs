using System.Collections.Generic;
using SFramework.SAI.Editor.Support;
using SFramework.SAI.Module;
using SFramework.SAI.Module.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Window
{
    public partial class SfAiWindow : EditorWindow
    {
        SfAiSettings _settings;
        readonly List<SfAiMessage> _conversation = new List<SfAiMessage>();

        VisualElement _messageList;
        ScrollView _messageScroll;
        Label _statusLabel;

        DropdownField _providerDropdown;
        DropdownField _modelDropdown;
        Button _refreshModelsButton;
        TextField _systemPromptField;
        TextField _apiKeyField;

        bool _isRefreshingModels;

        TextField _inputField;
        Button _sendButton;

        readonly List<string> _providerIds = new List<string>();
        readonly List<string> _modelIds = new List<string>();
        string _selectedProviderId;

        bool _isSending;
        int _conversationEpoch;

        [MenuItem("SFramework/AI 工作台")]
        public static void Open()
        {
            var window = GetWindow<SfAiWindow>("SAI");
            window.minSize = new UnityEngine.Vector2(640, 520);
            window.Show();
        }

        void CreateGUI()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SfAiEditorPaths.WindowUxml);
            if (tree == null)
            {
                rootVisualElement.Add(new Label($"未找到 UXML: {SfAiEditorPaths.WindowUxml}"));
                return;
            }

            tree.CloneTree(rootVisualElement);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(SfAiEditorPaths.WindowUss);
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            _messageList = rootVisualElement.Q<VisualElement>("MessageList");
            _messageScroll = rootVisualElement.Q<ScrollView>("MessageScroll");
            _statusLabel = rootVisualElement.Q<Label>("StatusLabel");

            _providerDropdown = rootVisualElement.Q<DropdownField>("ProviderDropdown");
            _modelDropdown = rootVisualElement.Q<DropdownField>("ModelDropdown");
            _refreshModelsButton = rootVisualElement.Q<Button>("RefreshModelsButton");
            _systemPromptField = rootVisualElement.Q<TextField>("SystemPrompt");
            _apiKeyField = rootVisualElement.Q<TextField>("ApiKey");

            _inputField = rootVisualElement.Q<TextField>("InputField");
            _sendButton = rootVisualElement.Q<Button>("SendButton");

            if (_providerDropdown == null || _modelDropdown == null || _refreshModelsButton == null ||
                _systemPromptField == null || _apiKeyField == null)
            {
                rootVisualElement.Add(new Label("SAI 窗口 UI 绑定失败，请检查 SfAiWindow.uxml"));
                return;
            }

            _settings = SfAiEditorSettingsUtility.GetOrCreateSettings();
            _settings.EnsureBuiltInProviders();
            SfAiClient.SetSettings(_settings);

            InitParamsPanel();
            InitModelsPanel();
            InitChatPanel();
        }

        SfAiProviderData GetSelectedProvider() =>
            string.IsNullOrEmpty(_selectedProviderId)
                ? null
                : _settings.GetProvider(_selectedProviderId);

        void OnDestroy() => UnbindRefreshModelsHandler();
    }
}
