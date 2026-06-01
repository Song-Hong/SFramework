using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFramework.Core.Mono;
using SFramework.SAI.Module;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;
using UnityEngine;

namespace SFramework.SAI.Mono
{
    [Serializable]
    struct SfAiMonoSaveData
    {
        public SfAiProviderKind provider;
        public string apiKey;
        public string model;
        public string systemPrompt;
    }

    /// <summary>
    /// AI 单例：选运营商、填 Key 和模型名即可。
    /// 示例：SfAiMono.Instance.Ask("你好", reply => Debug.Log(reply));
    /// </summary>
    public class SfAiMono : SfMonoSingleton<SfAiMono>
    {
        [Header("连接")]
        public SfAiProviderKind provider = SfAiProviderKind.Kimi;

        public string apiKey = "";

        [Tooltip("API 模型名，如 kimi-k2.6 / qwen3.6-plus / deepseek-v4-pro")]
        public string model = "kimi-k2.6";

        [Header("对话")]
        [TextArea(2, 4)]
        public string systemPrompt = "";

        [Header("存档（可选，运行时可用 SetConfigJson 赋值）")]
        public string configJson = "";

        SfAiSettings _runtimeSettings;
        SynchronizationContext _mainContext;
        CancellationTokenSource _askCts;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            _mainContext = SynchronizationContext.Current;
            ApplyConfigJsonIfAny();
            RefreshRuntimeSettings();
        }

        /// <summary>一行提问，流式结束后回调完整回复</summary>
        public void Ask(string message, Action<string> onReply, Action<string> onError = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                PostMain(() => onError?.Invoke("消息不能为空"));
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                PostMain(() => onError?.Invoke("请先填写 API Key"));
                return;
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                PostMain(() => onError?.Invoke("请先填写模型名"));
                return;
            }

            ApplyConfigJsonIfAny();
            RefreshRuntimeSettings();

            _askCts?.Cancel();
            _askCts = new CancellationTokenSource();
            _ = RunAskAsync(message.Trim(), onReply, onError, _askCts.Token);
        }

        public void Cancel() => _askCts?.Cancel();

        /// <summary>从公开字符串载入（读档 / 服务端下发）</summary>
        public void SetConfigJson(string json)
        {
            configJson = json ?? "";
            ApplyConfigJsonIfAny();
            RefreshRuntimeSettings();
        }

        /// <summary>导出当前配置到 configJson 并返回</summary>
        public string ExportConfigJson()
        {
            configJson = JsonUtility.ToJson(CaptureSaveData());
            return configJson;
        }

        void ApplyConfigJsonIfAny()
        {
            if (string.IsNullOrWhiteSpace(configJson)) return;

            try
            {
                var data = JsonUtility.FromJson<SfAiMonoSaveData>(configJson);
                provider = data.provider;
                apiKey = data.apiKey ?? "";
                model = data.model ?? "";
                systemPrompt = data.systemPrompt ?? "";
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SfAiMono] configJson 解析失败: {e.Message}");
            }
        }

        SfAiMonoSaveData CaptureSaveData() => new SfAiMonoSaveData
        {
            provider = provider,
            apiKey = apiKey ?? "",
            model = model ?? "",
            systemPrompt = systemPrompt ?? ""
        };

        void RefreshRuntimeSettings()
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

            _runtimeSettings = ScriptableObject.CreateInstance<SfAiSettings>();
            _runtimeSettings.preserveUserProvidersOnly = true;
            _runtimeSettings.SystemPrompt = systemPrompt ?? "";
            _runtimeSettings.Providers.Add(p);
            _runtimeSettings.Models.Add(m);
            _runtimeSettings.ActiveModelId = m.Id;

            SfAiClient.SetSettings(_runtimeSettings);
        }

        async Task RunAskAsync(
            string message,
            Action<string> onReply,
            Action<string> onError,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new SfAiChatRequest
                {
                    SystemPrompt = systemPrompt?.Trim()
                };
                request.AddUser(message);

                var sb = new StringBuilder();

                await SfAiClient.ChatStreamAsync(
                    request,
                    chunk =>
                    {
                        if (chunk.IsThinking || string.IsNullOrEmpty(chunk.Text)) return;
                        sb.Append(chunk.Text);
                    },
                    cancellationToken).ConfigureAwait(false);

                var text = sb.ToString();
                PostMain(() => onReply?.Invoke(text));
            }
            catch (OperationCanceledException)
            {
                PostMain(() => onError?.Invoke("已取消"));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfAiMono] {e.Message}");
                PostMain(() => onError?.Invoke(e.Message));
            }
        }

        void PostMain(Action action)
        {
            if (action == null) return;

            if (_mainContext != null)
                _mainContext.Post(_ => action(), null);
            else
                action();
        }
    }
}
