using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;
using SimpleJSON;

namespace SFramework.SAI.Module.Provider
{
    /// <summary>
    /// OpenAI Chat Completions 兼容（支持 SSE 流式）
    /// </summary>
    public class SfAiOpenAiCompatibleProvider : SfAiProviderBase
    {
        static readonly string[] KimiFallbackBaseUrls =
        {
            "https://api.moonshot.cn/v1",
            "https://api.moonshot.ai/v1"
        };

        public override SfAiProviderType ProviderType => SfAiProviderType.OpenAICompatible;

        public override Task<SfAiChatResponse> ChatAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteWithEndpointFallback(profile, p => ChatAsyncCore(p, request, cancellationToken));

        public override Task<string> ChatStreamAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            Action<SfAiStreamChunk> onChunk,
            CancellationToken cancellationToken = default) =>
            ExecuteWithEndpointFallback(profile, p => ChatStreamAsyncCore(p, request, onChunk, cancellationToken));

        static async Task<SfAiChatResponse> ChatAsyncCore(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            CancellationToken cancellationToken)
        {
            var url = $"{ResolveRequestBaseUrl(profile)}/chat/completions";
            var body = BuildRequestBody(profile, request, stream: false);
            var headers = BuildHeaders(profile);

            var raw = await SfAiHttp.PostJsonAsync(url, body, headers, profile.TimeoutSeconds, cancellationToken)
                .ConfigureAwait(false);

            return ParseResponse(raw, profile.Model);
        }

        static async Task<string> ChatStreamAsyncCore(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            Action<SfAiStreamChunk> onChunk,
            CancellationToken cancellationToken)
        {
            var url = $"{ResolveRequestBaseUrl(profile)}/chat/completions";
            var body = BuildRequestBody(profile, request, stream: true);
            var headers = BuildHeaders(profile);
            var sb = new StringBuilder();

            await SfAiHttp.PostJsonStreamLinesAsync(
                url,
                body,
                headers,
                profile.TimeoutSeconds,
                line =>
                {
                    if (!SfAiStreamParser.TryParseOpenAiSseLine(line, out var chunk)) return;

                    if (chunk.IsThinking)
                        NotifyThinking(onChunk);
                    else
                        AppendDelta(sb, onChunk, chunk.Text);
                },
                cancellationToken).ConfigureAwait(false);

            return sb.ToString();
        }

        static async Task<T> ExecuteWithEndpointFallback<T>(
            SfAiProviderProfile profile,
            Func<SfAiProviderProfile, Task<T>> action)
        {
            if (!IsKimiEndpoint(profile))
                return await action(profile).ConfigureAwait(false);

            Exception lastError = null;
            foreach (var baseUrl in EnumerateRequestBaseUrls(profile))
            {
                try
                {
                    return await action(CloneProfile(profile, baseUrl)).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    lastError = e;
                    if (!IsAuthenticationError(e))
                        throw;
                }
            }

            throw new Exception(
                "Kimi 认证失败：已尝试 api.moonshot.cn 与 api.moonshot.ai。请确认 Key 在 platform.kimi.com 创建且未过期。",
                lastError);
        }

        static IEnumerable<string> EnumerateRequestBaseUrls(SfAiProviderProfile profile)
        {
            if (IsKimiEndpoint(profile))
            {
                if (!string.IsNullOrWhiteSpace(profile.BaseUrl))
                {
                    yield return profile.BaseUrl.TrimEnd('/');
                    yield break;
                }

                foreach (var fallbackUrl in KimiFallbackBaseUrls)
                    yield return fallbackUrl;
                yield break;
            }

            var resolved = profile.ResolveBaseUrl();
            if (string.IsNullOrWhiteSpace(resolved))
                throw new Exception("请配置 BaseUrl");
            yield return resolved.TrimEnd('/');
        }

        static string ResolveRequestBaseUrl(SfAiProviderProfile profile) =>
            profile.ResolveBaseUrl().TrimEnd('/');

        static SfAiProviderProfile CloneProfile(SfAiProviderProfile source, string baseUrl) =>
            new SfAiProviderProfile
            {
                ProviderName = source.ProviderName,
                ProviderKind = source.ProviderKind,
                ProviderType = source.ProviderType,
                ApiKey = SfAiProviderProfile.NormalizeApiKey(source.ApiKey),
                BaseUrl = baseUrl,
                Model = source.Model,
                TimeoutSeconds = source.TimeoutSeconds,
                Organization = source.Organization
            };

        static bool IsAuthenticationError(Exception e)
        {
            var msg = e.Message ?? "";
            return msg.Contains("401") ||
                   msg.IndexOf("invalid_authentication", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static Dictionary<string, string> BuildHeaders(SfAiProviderProfile profile)
        {
            var apiKey = SfAiProviderProfile.NormalizeApiKey(profile.ApiKey);
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API Key 为空，请在 AI 工作台填写");

            var provider = new SfAiProviderData
            {
                Kind = profile.ProviderKind,
                Name = profile.ProviderName,
                ApiKey = apiKey,
                BaseUrl = profile.BaseUrl
            };

            var headers = IsMiMoEndpoint(profile)
                ? SfAiOpenAiModelsApi.BuildAuthHeaders(provider, SfAiOpenAiModelsApi.AuthHeaderStyle.ApiKey)
                : SfAiOpenAiModelsApi.BuildAuthHeaders(provider, SfAiOpenAiModelsApi.AuthHeaderStyle.Bearer);

            if (!string.IsNullOrWhiteSpace(profile.Organization))
                headers["OpenAI-Organization"] = profile.Organization;

            return headers;
        }

        static string BuildRequestBody(SfAiProviderProfile profile, SfAiChatRequest request, bool stream)
        {
            var root = new JSONObject();
            root["model"] = profile.Model;

            // kimi-k2.6 / k2.5 等固定采样参数，传 temperature 会 400
            if (!UsesKimiFixedSampling(profile.Model))
                root["temperature"] = request.Temperature;

            if (IsKimiEndpoint(profile) || IsMiMoEndpoint(profile))
            {
                root["max_completion_tokens"] = request.MaxTokens;
                if (UsesKimiFixedSampling(profile.Model) || ShouldDisableMimoThinking(profile))
                {
                    var thinking = new JSONObject();
                    thinking["type"] = "disabled";
                    root["thinking"] = thinking;
                }
            }
            else
            {
                root["max_tokens"] = request.MaxTokens;
            }

            root["stream"] = stream;

            var messages = new JSONArray();
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
                messages.Add(MessageObject(SfAiRole.System, request.SystemPrompt));

            foreach (var msg in request.Messages)
                messages.Add(MessageObject(msg.Role, msg.Content));

            root["messages"] = messages;
            return root.ToString();
        }

        /// <summary>K2.6/K2.5 等模型由服务端固定 temperature/top_p，请求中不得携带</summary>
        static bool UsesKimiFixedSampling(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId)) return false;
            var m = modelId.Trim().ToLowerInvariant();
            return m.StartsWith("kimi-k2.6") || m.StartsWith("kimi-k2.5") || m.StartsWith("kimi-k2-");
        }

        /// <summary>工作台对话默认关闭深度思考，与官方基础示例一致</summary>
        static bool ShouldDisableMimoThinking(SfAiProviderProfile profile)
        {
            if (!IsMiMoEndpoint(profile)) return false;
            var model = profile.Model?.ToLowerInvariant() ?? "";
            return !model.Contains("tts");
        }

        static bool IsMiMoEndpoint(SfAiProviderProfile profile)
        {
            if (profile.IsMiMo()) return true;
            var url = profile.ResolveBaseUrl() ?? "";
            return url.IndexOf("xiaomimimo", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Kimi 使用 OpenAI 兼容端点，流式为 SSE（data: ... / data: [DONE]）</summary>
        static bool IsKimiEndpoint(SfAiProviderProfile profile)
        {
            if (profile.IsKimi()) return true;

            var url = profile.ResolveBaseUrl() ?? "";
            return url.IndexOf("moonshot", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static JSONObject MessageObject(SfAiRole role, string content)
        {
            var obj = new JSONObject();
            obj["role"] = role switch
            {
                SfAiRole.System => "system",
                SfAiRole.Assistant => "assistant",
                _ => "user"
            };
            obj["content"] = content ?? "";
            return obj;
        }

        static SfAiChatResponse ParseResponse(string raw, string model)
        {
            var json = JSON.Parse(raw);
            var content = json["choices"]?[0]?["message"]?["content"]?.Value ?? "";
            return new SfAiChatResponse
            {
                Content = content,
                Model = model,
                RawJson = raw
            };
        }
    }
}
