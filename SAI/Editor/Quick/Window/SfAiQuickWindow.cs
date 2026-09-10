using System;
using System.Threading;
using SFramework.SAI.Editor.Quick.Support;
using SFramework.SAI.Editor.Quick.Tool;
using SFramework.SAI.Editor.Support;
using SFramework.SAI.Module.Agent;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Mcp;
using SFramework.SAI.Module.Tool;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Quick.Window
{
    /// <summary>
    /// SAI Quick：编排式对话 —— 解析任务 → 执行任务 → 检查任务。
    /// 上方消息 / 工作链，下方输入；基于 AI 基座，进程内 unity_* 操作 Unity。
    /// </summary>
    public partial class SfAiQuickWindow : EditorWindow
    {
        VisualElement _root;
        VisualElement _feedInner;
        ScrollView _feedScroll;
        TextField _inputField;
        Button _settingsButton;
        Button _stopButton;
        Label _statusLabel;
        VisualElement _settingsPanel;
        Toggle _mcpToggle;
        TextField _mcpCommandField;

        CancellationTokenSource _cts;
        bool _running;
        VisualElement _currentChainBlock;
        readonly SfAiQuickChainAnimator _chainAnimator = new SfAiQuickChainAnimator();
        bool _chainHasItem;

        [MenuItem("SFramework/AI Quick", false, 1)]
        public static void Open()
        {
            var w = GetWindow<SfAiQuickWindow>();
            w.titleContent = new GUIContent("SAI Quick");
            w.minSize = new Vector2(420, 520);
            w.Show();
        }

        void CreateGUI()
        {
            _root = rootVisualElement;
            _root.Clear();
            _root.AddToClassList("sfai-quick-root");

            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(SfAiEditorPaths.QuickUss);
            if (uss != null)
                _root.styleSheets.Add(uss);

            BuildTopBar();
            BuildSettingsPanel();
            BuildFeed();
            BuildComposer();
            InitConnectionSettings();

            if (uss == null)
                ApplyFallbackLayoutStyles();
        }

        void BuildTopBar()
        {
            var top = new VisualElement();
            top.AddToClassList("sfai-quick-topbar");

            var title = new Label("SAI Quick");
            title.AddToClassList("sfai-quick-title");
            top.Add(title);

            _statusLabel = new Label("就绪");
            _statusLabel.AddToClassList("sfai-quick-status");
            top.Add(_statusLabel);

            _settingsButton = new Button(ToggleSettings) { text = "设置" };
            _settingsButton.AddToClassList("sfai-quick-topbar-btn");
            top.Add(_settingsButton);

            _stopButton = new Button(OnCancelClicked) { text = "停止" };
            _stopButton.AddToClassList("sfai-quick-topbar-btn");
            _stopButton.style.display = DisplayStyle.None;
            top.Add(_stopButton);

            var clearBtn = new Button(ClearFeed) { text = "清空" };
            clearBtn.AddToClassList("sfai-quick-topbar-btn");
            top.Add(clearBtn);

            _root.Add(top);
        }

        void BuildSettingsPanel()
        {
            _settingsPanel = new VisualElement();
            _settingsPanel.AddToClassList("sfai-quick-settings");

            var aiTitle = new Label("AI 连接");
            aiTitle.AddToClassList("sfai-quick-settings-section");
            _settingsPanel.Add(aiTitle);

            _providerDropdown = new DropdownField("运营商");
            _providerDropdown.AddToClassList("sfai-quick-settings-field");
            _settingsPanel.Add(_providerDropdown);

            _apiKeyField = new TextField("API Key") { isPasswordField = true };
            _apiKeyField.AddToClassList("sfai-quick-settings-field");
            _settingsPanel.Add(_apiKeyField);

            _modelDropdown = new DropdownField("模型");
            _modelDropdown.AddToClassList("sfai-quick-settings-field");
            _settingsPanel.Add(_modelDropdown);

            _systemPromptField = new TextField("系统提示词") { multiline = true };
            _systemPromptField.AddToClassList("sfai-quick-settings-field");
            _systemPromptField.AddToClassList("sfai-quick-settings-prompt");
            _settingsPanel.Add(_systemPromptField);

            var openWorkbench = new Button(SFramework.SAI.Editor.Window.SfAiWindow.Open)
            {
                text = "打开完整 AI 工作台"
            };
            openWorkbench.AddToClassList("sfai-quick-topbar-btn");
            openWorkbench.style.marginTop = 4;
            openWorkbench.style.marginBottom = 8;
            openWorkbench.style.alignSelf = Align.FlexStart;
            _settingsPanel.Add(openWorkbench);

            var mcpTitle = new Label("Unity MCP（可选）");
            mcpTitle.AddToClassList("sfai-quick-settings-section");
            _settingsPanel.Add(mcpTitle);

            _mcpToggle = new Toggle("启用 MCP 连接") { value = false };
            _settingsPanel.Add(_mcpToggle);

            _mcpCommandField = new TextField("MCP Command") { value = GuessUnityRelayPath() };
            _mcpCommandField.AddToClassList("sfai-quick-settings-field");
            _settingsPanel.Add(_mcpCommandField);

            _root.Add(_settingsPanel);
        }

        void BuildFeed()
        {
            _feedScroll = new ScrollView(ScrollViewMode.Vertical);
            _feedScroll.AddToClassList("sfai-quick-feed");
            _feedScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            _feedInner = new VisualElement();
            _feedInner.AddToClassList("sfai-quick-feed-inner");
            _feedScroll.Add(_feedInner);
            _root.Add(_feedScroll);

            AddSystemHint("描述你想对 Unity 做的事。思维链会逐项展开。");
        }

        void BuildComposer()
        {
            var composer = new VisualElement();
            composer.AddToClassList("sfai-quick-composer");

            _inputField = new TextField
            {
                multiline = true,
                value = ""
            };
            _inputField.AddToClassList("sfai-quick-input");
            _inputField.RegisterCallback<KeyDownEvent>(OnInputKeyDown);

            var label = _inputField.Q<Label>();
            if (label != null) label.style.display = DisplayStyle.None;

            composer.Add(_inputField);

            _root.Add(composer);
        }

        void OnInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                return;

            if (evt.shiftKey) return;

            evt.StopPropagation();
            evt.PreventDefault();
            OnSendClicked();
        }

        void ToggleSettings()
        {
            var open = _settingsPanel.ClassListContains("sfai-quick-settings-open");
            if (open)
                _settingsPanel.RemoveFromClassList("sfai-quick-settings-open");
            else
                _settingsPanel.AddToClassList("sfai-quick-settings-open");
        }

        void ClearFeed()
        {
            _chainAnimator.Clear();
            _feedInner.Clear();
            _currentChainBlock = null;
            _chainHasItem = false;
            AddSystemHint("会话已清空。");
            UpdateConnectionStatus();
        }

        void OnSendClicked()
        {
            if (_running) return;
            var goal = _inputField?.value?.Trim();
            if (string.IsNullOrWhiteSpace(goal))
            {
                _statusLabel.text = "请输入内容";
                return;
            }

            if (_settings == null)
                _settings = SfAiEditorSettingsUtility.GetOrCreateSettings();

            if (_settings == null)
            {
                _statusLabel.text = "请先配置 AI";
                AddBubble("错误", "未找到 SfAiSettings", "sfai-quick-bubble-error");
                OpenSettingsPanel();
                return;
            }

            if (!EnsureConnectionReady())
                return;

            _inputField.value = "";
            AddBubble("你", goal, "sfai-quick-bubble-user");
            _currentChainBlock = null;
            _chainHasItem = false;
            _chainAnimator.Clear();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            SetRunning(true);
            _ = RunAsync(goal, _settings, _cts.Token);
        }

        void OnCancelClicked() => _cts?.Cancel();

        async System.Threading.Tasks.Task RunAsync(
            string goal,
            SfAiSettings settings,
            CancellationToken cancellationToken)
        {
            SfAiAgentOrchestrator orchestrator = null;
            try
            {
                var registry = SfAiToolBootstrap.CreateCoreRegistry();
                SfAiUnityBuiltinTools.RegisterAll(registry);

                var basePrompt = string.IsNullOrWhiteSpace(settings.SystemPrompt)
                    ? "你是 SFramework Quick 助手，在 Unity Editor 中通过工具完成用户目标。" +
                      "Unity 基于组件：搭建 UI 请用 unity_create_ui（canvas/panel/text/button/image 等），" +
                      "或 unity_create_gameobject 后 unity_add_component；不要只建空物体。"
                    : settings.SystemPrompt;

                orchestrator = new SfAiAgentOrchestrator(
                    registry,
                    new SfAiAgentOrchestratorOptions
                    {
                        BaseSystemPrompt = basePrompt,
                        MaxVerifyRetries = 1
                    });

                if (_mcpToggle != null && _mcpToggle.value &&
                    !string.IsNullOrWhiteSpace(_mcpCommandField?.value))
                {
                    SfAiEditorMainThread.Post(() =>
                        EnqueueChainItem(SfAiQuickOpKind.Connect, "连接 MCP", _mcpCommandField.value.Trim()));
                    await orchestrator.ConnectMcpAsync(
                        new SfAiMcpServerConfig
                        {
                            Name = "unity-mcp",
                            Command = _mcpCommandField.value.Trim(),
                            Args = new[] { "--mcp" },
                            Enabled = true
                        },
                        cancellationToken);
                }

                var toolCount = orchestrator.Tools.Count;
                SfAiEditorMainThread.Post(() =>
                    EnqueueChainItem(SfAiQuickOpKind.Prepare, "准备编排",
                        $"解析 → 执行 → 核验 · {toolCount} 个工具"));

                var result = await orchestrator.RunAsync(
                    goal,
                    settings,
                    e => SfAiEditorMainThread.Post(() => AppendEvent(e)),
                    cancellationToken);

                SfAiEditorMainThread.Post(() =>
                {
                    if (result.Ok)
                    {
                        if (!string.IsNullOrWhiteSpace(result.FinalText))
                            AddBubble("助手", result.FinalText, "sfai-quick-bubble-assistant");
                        _statusLabel.text = result.Verified
                            ? $"完成 · 核验通过 · {result.Steps} 步"
                            : $"完成 · 核验未通过 · {result.Steps} 步";
                    }
                    else
                    {
                        AddBubble("错误", result.Error, "sfai-quick-bubble-error");
                        _statusLabel.text = "失败";
                    }

                    SetRunning(false);
                });
            }
            catch (Exception e)
            {
                SfAiEditorMainThread.Post(() =>
                {
                    AddBubble("错误", e.Message, "sfai-quick-bubble-error");
                    _statusLabel.text = "异常";
                    SetRunning(false);
                });
            }
            finally
            {
                orchestrator?.Dispose();
            }
        }

        void AppendEvent(SfAiAgentEvent e)
        {
            switch (e.Kind)
            {
                case SfAiAgentEventKind.Started:
                    EnqueueChainItem(SfAiQuickOpKind.Think, "开始编排", Truncate(e.Message, 200));
                    _statusLabel.text = "编排中…";
                    break;
                case SfAiAgentEventKind.PhaseStarted:
                    // 新阶段另起一条链
                    _currentChainBlock = null;
                    _chainHasItem = false;
                    EnqueueChainItem(
                        PhaseOpKind(e.Phase),
                        SfAiAgentPhaseNames.Display(e.Phase),
                        "阶段开始");
                    _statusLabel.text = SfAiAgentPhaseNames.Display(e.Phase);
                    break;
                case SfAiAgentEventKind.PhaseCompleted:
                {
                    var failed = !string.IsNullOrEmpty(e.Message) &&
                                 (e.Message.StartsWith("失败", StringComparison.Ordinal) ||
                                  e.Message.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase));
                    EnqueueChainItem(
                        failed ? SfAiQuickOpKind.Error : SfAiQuickOpKind.Done,
                        SfAiAgentPhaseNames.Display(e.Phase) + " · 结束",
                        Truncate(e.Message, 400));
                    break;
                }
                case SfAiAgentEventKind.Step:
                    EnqueueChainItem(SfAiQuickOpKind.Step, e.Message, PhaseTag(e.Phase));
                    _statusLabel.text = e.Message;
                    break;
                case SfAiAgentEventKind.ToolCall:
                    EnqueueChainItem(
                        SfAiQuickOpIcons.FromToolName(e.ToolName),
                        FriendlyToolTitle(e.ToolName),
                        Truncate(e.Message, 400));
                    _statusLabel.text = $"工具: {e.ToolName}";
                    break;
                case SfAiAgentEventKind.ToolResult:
                    EnqueueChainItem(
                        SfAiQuickOpKind.ToolResult,
                        FriendlyToolTitle(e.ToolName) + " · 结果",
                        Truncate(e.Message, 600));
                    break;
                case SfAiAgentEventKind.ModelText:
                    break;
                case SfAiAgentEventKind.Completed:
                    EnqueueChainItem(SfAiQuickOpKind.Done, "编排完成", "三阶段流程结束");
                    _chainAnimator.StopBreathing();
                    break;
                case SfAiAgentEventKind.Failed:
                case SfAiAgentEventKind.Cancelled:
                    AddBubble(e.Kind == SfAiAgentEventKind.Cancelled ? "已取消" : "错误",
                        e.Message, "sfai-quick-bubble-error");
                    break;
            }
        }

        static SfAiQuickOpKind PhaseOpKind(SfAiAgentPhase phase)
        {
            switch (phase)
            {
                case SfAiAgentPhase.Parse: return SfAiQuickOpKind.Think;
                case SfAiAgentPhase.Execute: return SfAiQuickOpKind.Prepare;
                case SfAiAgentPhase.Verify: return SfAiQuickOpKind.Find;
                default: return SfAiQuickOpKind.Step;
            }
        }

        static string PhaseTag(SfAiAgentPhase phase) =>
            phase == SfAiAgentPhase.None ? null : SfAiAgentPhaseNames.Display(phase);

        void EnsureChainBlock()
        {
            if (_currentChainBlock != null) return;

            _currentChainBlock = new VisualElement();
            _currentChainBlock.AddToClassList("sfai-quick-bubble");
            _currentChainBlock.AddToClassList("sfai-quick-bubble-chain");
            _feedInner.Add(_currentChainBlock);
            _chainHasItem = false;
        }

        void AppendChain(SfAiQuickOpKind kind, string title, string detail) =>
            EnqueueChainItem(kind, title, detail);

        void EnqueueChainItem(SfAiQuickOpKind kind, string title, string detail)
        {
            EnsureChainBlock();

            var parts = SfAiQuickOpIcons.CreateItem(kind, title, detail);
            _chainHasItem = true;

            // 结束类节点不再呼吸；操作中节点持续呼吸
            var breath = kind != SfAiQuickOpKind.Done && kind != SfAiQuickOpKind.Error;

            _chainAnimator.Enqueue(
                _currentChainBlock,
                parts.Row,
                parts.Circle,
                parts.Text,
                onShown: ScrollToBottom,
                breathAfterShow: breath);

            ScrollToBottom();
        }

        static string FriendlyToolTitle(string toolName)
        {
            var n = (toolName ?? "").ToLowerInvariant();
            if (n.Contains("create_ui") || n.Contains("createui")) return "创建 UI";
            if (n.Contains("add_component") || n.Contains("addcomponent")) return "添加组件";
            if (n.Contains("set_ui") || n.Contains("setui")) return "设置 UI";
            if (n.Contains("hierarchy")) return "查看 Hierarchy";
            if (n.Contains("create")) return "创建 GameObject";
            if (n.Contains("find")) return "查找 GameObject";
            if (n.Contains("select")) return "选中 GameObject";
            if (n.Contains("structured_output")) return "提交 JSON";
            if (n.Contains("log")) return "输出日志";
            if (n.Contains("echo")) return "回显";
            if (string.IsNullOrWhiteSpace(toolName)) return "工具";
            return toolName;
        }

        void AddBubble(string roleText, string body, string bubbleClass)
        {
            _currentChainBlock = null;
            _chainHasItem = false;

            var card = new VisualElement();
            card.AddToClassList("sfai-quick-bubble");
            if (!string.IsNullOrEmpty(bubbleClass))
                card.AddToClassList(bubbleClass);

            var role = new Label(roleText);
            role.AddToClassList("sfai-quick-role");
            card.Add(role);

            var content = new VisualElement();
            content.AddToClassList("sfai-quick-md-content");
            SfAiQuickMarkdown.BuildInto(content, body ?? "");
            card.Add(content);

            _feedInner.Add(card);
            ScrollToBottom();
        }

        void AddSystemHint(string text)
        {
            var card = new VisualElement();
            card.AddToClassList("sfai-quick-bubble");
            card.AddToClassList("sfai-quick-bubble-chain");

            var content = new VisualElement();
            content.AddToClassList("sfai-quick-md-content");
            content.AddToClassList("sfai-quick-body-dim");
            SfAiQuickMarkdown.BuildInto(content, text);
            card.Add(content);

            _feedInner.Add(card);
        }

        void ScrollToBottom()
        {
            _feedScroll.schedule.Execute(() =>
            {
                _feedScroll.scrollOffset = new Vector2(0, float.MaxValue);
            }).ExecuteLater(1);
        }

        void SetRunning(bool running)
        {
            _running = running;
            _inputField?.SetEnabled(!running);
            if (_stopButton != null)
                _stopButton.style.display = running ? DisplayStyle.Flex : DisplayStyle.None;
            if (running)
            {
                _statusLabel.text = "运行中…";
                _chainAnimator.SetBreathAllowed(true);
            }
            else
            {
                _chainAnimator.StopBreathing();
            }
        }

        static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s ?? "";
            return s.Substring(0, max) + "…";
        }

        static string GuessUnityRelayPath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#if UNITY_EDITOR_OSX
            var arm = System.IO.Path.Combine(home,
                ".unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64");
            if (System.IO.File.Exists(arm)) return arm;
            var intel = System.IO.Path.Combine(home,
                ".unity/relay/relay_mac_x64.app/Contents/MacOS/relay_mac_x64");
            if (System.IO.File.Exists(intel)) return intel;
#elif UNITY_EDITOR_WIN
            var win = System.IO.Path.Combine(home, ".unity", "relay", "relay_win.exe");
            if (System.IO.File.Exists(win)) return win;
#else
            var linux = System.IO.Path.Combine(home, ".unity/relay/relay_linux");
            if (System.IO.File.Exists(linux)) return linux;
#endif
            return "";
        }

        void ApplyFallbackLayoutStyles()
        {
            _root.style.flexGrow = 1;
            _root.style.flexDirection = FlexDirection.Column;
            _feedScroll.style.flexGrow = 1;
        }

        void OnDestroy()
        {
            _cts?.Cancel();
            _chainAnimator.Clear();
        }
    }
}
