using System;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;
using UnityEngine;

namespace SFramework.SAI.Module
{
    /// <summary>
    /// AI 统一调用入口（经 Gateway 路由到当前模型 / 运营商）
    /// </summary>
    public static class SfAiClient
    {
        const string DefaultSettingsPath = "Assets/SFramework/SAI/Editor/Config/SfAiSettings.asset";

        static SfAiSettings _cachedSettings;

        public static SfAiSettings LoadSettings()
        {
            if (_cachedSettings != null) return _cachedSettings;

#if UNITY_EDITOR
            _cachedSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SfAiSettings>(DefaultSettingsPath);
            if (_cachedSettings != null)
            {
                _cachedSettings.EnsureDefaults();
                return _cachedSettings;
            }
#endif
            _cachedSettings = Resources.Load<SfAiSettings>("SfAiSettings");
            _cachedSettings?.EnsureDefaults();
            return _cachedSettings;
        }

        public static void SetSettings(SfAiSettings settings)
        {
            _cachedSettings = settings;
            _cachedSettings?.EnsureDefaults();
        }

        public static async Task<SfAiChatResponse> ChatAsync(
            SfAiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            var settings = LoadSettings();
            if (settings == null)
                throw new Exception("未找到 SfAiSettings，请打开 SAI 工作台完成配置");

            return await SfAiGateway.ChatAsync(settings, request, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<SfAiChatResponse> ChatWithModelAsync(
            string modelId,
            SfAiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            var settings = LoadSettings();
            var previous = settings.ActiveModelId;
            settings.ActiveModelId = modelId;
            try
            {
                return await SfAiGateway.ChatAsync(settings, request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                settings.ActiveModelId = previous;
            }
        }

        /// <summary>流式对话</summary>
        public static async Task<SfAiChatResponse> ChatStreamAsync(
            SfAiChatRequest request,
            Action<SfAiStreamChunk> onChunk,
            CancellationToken cancellationToken = default)
        {
            var settings = LoadSettings();
            if (settings == null)
                throw new Exception("未找到 SfAiSettings，请打开 SAI 工作台完成配置");

            return await SfAiGateway.ChatStreamAsync(settings, request, onChunk, cancellationToken)
                .ConfigureAwait(false);
        }

        public static async Task<string> AskAsync(string userMessage, string systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            var request = new SfAiChatRequest { SystemPrompt = systemPrompt };
            request.AddUser(userMessage);
            var response = await ChatAsync(request, cancellationToken).ConfigureAwait(false);
            return response.Content;
        }

        public static async Task<string> AskStreamAsync(
            string userMessage,
            Action<string> onDelta,
            string systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            var request = new SfAiChatRequest { SystemPrompt = systemPrompt };
            request.AddUser(userMessage);
            var response = await ChatStreamAsync(
                request,
                chunk =>
                {
                    if (!chunk.IsThinking)
                        onDelta?.Invoke(chunk.Text);
                },
                cancellationToken).ConfigureAwait(false);
            return response.Content;
        }

        public static void Chat(
            SfAiChatRequest request,
            Action<SfAiChatResponse> onSuccess,
            Action<string> onError)
        {
            _ = ChatCoroutine(request, onSuccess, onError);
        }

        static async Task ChatCoroutine(
            SfAiChatRequest request,
            Action<SfAiChatResponse> onSuccess,
            Action<string> onError)
        {
            try
            {
                var response = await ChatAsync(request).ConfigureAwait(true);
                onSuccess?.Invoke(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfAiClient] {e.Message}");
                onError?.Invoke(e.Message);
            }
        }
    }
}
