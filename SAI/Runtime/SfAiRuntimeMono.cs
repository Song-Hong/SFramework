using System;
using System.Threading;
using SFramework.SAI.Module.Agent;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;
using SFramework.SAI.Module.Tool;
using UnityEngine;

namespace SFramework.SAI.Runtime
{
    /// <summary>
    /// 运行时 AI 开放基类：基于 AI 基座，只提供接口钩子，由项目二次开发。
    /// 继承后实现 RegisterTools，按需覆盖 BuildSystemPrompt / BuildSettings。
    /// </summary>
    public abstract class SfAiRuntimeMono : MonoBehaviour, ISfAiRuntimeAgent, ISfAiRuntimeToolModule
    {
        [Header("AI 连接")]
        public SfAiProviderKind provider = SfAiProviderKind.Kimi;

        public string apiKey = "";
        public string model = "kimi-k2.6";

        [Header("Agent")]
        [TextArea(2, 6)]
        public string systemPrompt = "你是游戏内 AI 助手。通过已注册工具完成任务，并给出简短中文结论。";

        [Tooltip("安全熔断上限（正常靠循环检测退出，一般无需改）")]
        public int absoluteSafetyIterations = 256;

        SynchronizationContext _mainContext;
        SfAiRuntimeHost _host;

        protected virtual void Awake()
        {
            _mainContext = SynchronizationContext.Current;
            SfAiMainThread.SetContext(_mainContext);
        }

        /// <summary>项目在此注册自己的工具</summary>
        public abstract void RegisterTools(SfAiToolRegistry registry);

        protected virtual string BuildSystemPrompt() => systemPrompt;

        protected virtual SfAiAgentOptions BuildOptions() => new SfAiAgentOptions
        {
            SystemPrompt = BuildSystemPrompt(),
            AbsoluteSafetyIterations = Mathf.Max(32, absoluteSafetyIterations)
        };

        protected virtual SfAiSettings BuildSettings()
        {
            var p = new SfAiProviderData
            {
                Name = provider.ToString(),
                Kind = provider,
                ApiKey = SfAiProviderProfile.NormalizeApiKey(apiKey),
                Enabled = true
            };

            var m = new SfAiModelData
            {
                ProviderId = p.Id,
                DisplayName = model,
                ModelId = model,
                Enabled = true
            };

            var settings = ScriptableObject.CreateInstance<SfAiSettings>();
            settings.preserveUserProvidersOnly = true;
            settings.Providers.Add(p);
            settings.Models.Add(m);
            settings.ActiveModelId = m.Id;
            settings.SystemPrompt = BuildSystemPrompt() ?? "";
            return settings;
        }

        /// <summary>可选：挂接 MCP；默认不启用</summary>
        protected virtual ISfAiRuntimeMcpModule CreateMcpModule() => null;

        public void Run(
            string goal,
            Action<SfAiAgentRunResult> onComplete = null,
            Action<SfAiAgentEvent> onEvent = null,
            Action<string> onError = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                PostMain(() => onError?.Invoke("请先填写 API Key"));
                return;
            }

            _host?.Dispose();
            _host = new SfAiRuntimeHost(
                BuildSettings(),
                BuildOptions(),
                this,
                CreateMcpModule());

            _host.Run(
                goal,
                r => PostMain(() => onComplete?.Invoke(r)),
                e => PostMain(() => onEvent?.Invoke(e)),
                err => PostMain(() => onError?.Invoke(err)));
        }

        public void Cancel() => _host?.Cancel();

        protected virtual void OnDestroy()
        {
            _host?.Dispose();
            _host = null;
        }

        protected void PostMain(Action action)
        {
            if (action == null) return;
            if (_mainContext != null)
                _mainContext.Post(_ => action(), null);
            else
                action();
        }
    }
}
