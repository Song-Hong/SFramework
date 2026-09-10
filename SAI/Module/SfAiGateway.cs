using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Provider;
using SFramework.SAI.Module.Support;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SFramework.SAI.Module
{
    /// <summary>
    /// AI Gateway：按当前模型路由请求，支持失败自动 fallback
    /// </summary>
    public static class SfAiGateway
    {
        public static async Task<SfAiChatResponse> ChatAsync(
            SfAiSettings settings,
            SfAiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.EnsureDefaults();

            var candidates = BuildCandidateModels(settings, settings.EnableAutoFallback);
            if (candidates.Count == 0)
                throw new Exception("没有可用的模型，请先添加运营商并配置 API Key");

            Exception lastError = null;

            foreach (var (provider, model) in candidates)
            {
                try
                {
                    var profile = SfAiProviderProfile.From(provider, model);
                    if (!profile.IsConfigured()) continue;

                    var api = SfAiProviderFactory.Create(profile);
                    var response = await api.ChatAsync(profile, request, cancellationToken).ConfigureAwait(false);
                    response.ProviderType = profile.GetResolvedType();

                    provider.Status = SfAiProviderStatus.Ready;
                    provider.StatusMessage = "OK";
                    return response;
                }
                catch (Exception e)
                {
                    lastError = e;
                    provider.Status = SfAiProviderStatus.Error;
                    provider.StatusMessage = e.Message;

                    // 认证失败不应换其它运营商乱试
                    if (!settings.EnableAutoFallback || IsAuthenticationError(e))
                        throw;

                    Debug.LogWarning($"[SfAiGateway] {provider.Name}/{model.DisplayName} 失败，尝试下一个: {e.Message}");
                }
            }

            throw lastError ?? new Exception("所有模型均调用失败");
        }

        /// <summary>流式对话（SSE）</summary>
        public static async Task<SfAiChatResponse> ChatStreamAsync(
            SfAiSettings settings,
            SfAiChatRequest request,
            Action<SfAiStreamChunk> onChunk,
            CancellationToken cancellationToken = default)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.EnsureDefaults();

            // 流式仅请求当前选中模型，避免一次发送触发多次 API
            var candidates = BuildCandidateModels(settings, allowFallback: false);
            if (candidates.Count == 0)
                throw new Exception("没有可用的模型，请先添加运营商并配置 API Key");

            Exception lastError = null;

            foreach (var (provider, model) in candidates)
            {
                try
                {
                    var profile = SfAiProviderProfile.From(provider, model);
                    if (!profile.IsConfigured()) continue;

                    var api = SfAiProviderFactory.Create(profile);
                    var content = await api.ChatStreamAsync(profile, request, onChunk, cancellationToken)
                        .ConfigureAwait(false);

                    provider.Status = SfAiProviderStatus.Ready;
                    provider.StatusMessage = "OK";

                    return new SfAiChatResponse
                    {
                        Content = content,
                        Model = model.ModelId,
                        ProviderType = profile.GetResolvedType()
                    };
                }
                catch (Exception e)
                {
                    lastError = e;
                    provider.Status = SfAiProviderStatus.Error;
                    provider.StatusMessage = e.Message;

                    if (!settings.EnableAutoFallback || IsAuthenticationError(e))
                        throw;

                    Debug.LogWarning($"[SfAiGateway] 流式 {provider.Name}/{model.DisplayName} 失败: {e.Message}");
                }
            }

            throw lastError ?? new Exception("所有模型流式调用均失败");
        }

        static bool IsAuthenticationError(Exception e)
        {
            var msg = e?.Message ?? "";
            return msg.Contains("401") ||
                   msg.IndexOf("invalid_api_key", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   msg.IndexOf("invalid_key", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   msg.IndexOf("invalid api key", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   msg.IndexOf("authentication", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   msg.IndexOf("认证失败", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static async Task<int> TestProviderLatencyAsync(SfAiProviderData provider, CancellationToken cancellationToken = default)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            provider.Status = SfAiProviderStatus.Testing;
            var url = provider.ResolveBaseUrl();
            if (string.IsNullOrWhiteSpace(url))
            {
                provider.Status = SfAiProviderStatus.Error;
                provider.StatusMessage = "BaseUrl 为空";
                return -1;
            }

            var sw = Stopwatch.StartNew();
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                sw.Stop();

                provider.LatencyMs = (int)sw.ElapsedMilliseconds;
                provider.Status = response.IsSuccessStatusCode || (int)response.StatusCode < 500
                    ? SfAiProviderStatus.Ready
                    : SfAiProviderStatus.Error;
                provider.StatusMessage = $"HTTP {(int)response.StatusCode}";
                return provider.LatencyMs;
            }
            catch (Exception e)
            {
                sw.Stop();
                provider.LatencyMs = -1;
                provider.Status = SfAiProviderStatus.Error;
                provider.StatusMessage = e.Message;
                return -1;
            }
        }

        static List<(SfAiProviderData provider, SfAiModelData model)> BuildCandidateModels(
            SfAiSettings settings,
            bool allowFallback)
        {
            var result = new List<(SfAiProviderData provider, SfAiModelData model)>();
            var active = settings.GetActiveModel();

            void TryAdd(SfAiModelData model)
            {
                if (model == null || !model.Enabled) return;
                var provider = settings.GetProvider(model.ProviderId);
                if (provider == null || !provider.Enabled) return;
                if (result.Exists(x => x.model.Id == model.Id)) return;
                result.Add((provider, model));
            }

            TryAdd(active);

            if (allowFallback)
            {
                foreach (var model in settings.GetEnabledModels())
                    TryAdd(model);
            }

            return result;
        }
    }
}
