using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;
using SimpleJSON;

namespace SFramework.SAI.Module.Provider
{
    /// <summary>
    /// OpenAI 兼容 GET /v1/models
    /// </summary>
    public static class SfAiOpenAiModelsApi
    {
        public enum AuthHeaderStyle
        {
            Bearer,
            ApiKey
        }

        public static async Task<List<SfAiRemoteModelInfo>> ListModelsAsync(
            SfAiProviderData provider,
            AuthHeaderStyle authStyle,
            string[] fallbackBaseUrls,
            CancellationToken cancellationToken = default)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (!provider.HasValidKey())
                throw new Exception("请先填写 API Key");

            var headers = BuildAuthHeaders(provider, authStyle);
            Exception lastError = null;

            foreach (var baseUrl in EnumerateBaseUrls(provider, fallbackBaseUrls))
            {
                try
                {
                    var url = $"{baseUrl.TrimEnd('/')}/models";
                    var json = await SfAiHttp
                        .GetJsonAsync(url, headers, provider.TimeoutSeconds, cancellationToken)
                        .ConfigureAwait(false);
                    return ParseModelList(json);
                }
                catch (Exception e)
                {
                    lastError = e;
                    if (fallbackBaseUrls == null || fallbackBaseUrls.Length == 0 || !IsAuthenticationError(e))
                        throw;
                }
            }

            throw new Exception("拉取模型列表认证失败，请检查 API Key。", lastError);
        }

        public static Dictionary<string, string> BuildAuthHeaders(
            SfAiProviderData provider,
            AuthHeaderStyle authStyle)
        {
            var apiKey = SfAiProviderProfile.NormalizeApiKey(provider.ApiKey);
            return authStyle switch
            {
                AuthHeaderStyle.ApiKey => new Dictionary<string, string> { { "api-key", apiKey } },
                _ => new Dictionary<string, string> { { "Authorization", $"Bearer {apiKey}" } }
            };
        }

        static IEnumerable<string> EnumerateBaseUrls(SfAiProviderData provider, string[] fallbackBaseUrls)
        {
            if (!string.IsNullOrWhiteSpace(provider.BaseUrl))
            {
                yield return provider.BaseUrl.TrimEnd('/');
                yield break;
            }

            var resolved = provider.ResolveBaseUrl();
            if (!string.IsNullOrWhiteSpace(resolved))
                yield return resolved.TrimEnd('/');

            if (fallbackBaseUrls == null) yield break;

            foreach (var url in fallbackBaseUrls)
            {
                if (url != resolved)
                    yield return url;
            }
        }

        static bool IsAuthenticationError(Exception e)
        {
            var msg = e.Message ?? "";
            return msg.Contains("401") ||
                   msg.IndexOf("invalid_authentication", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static List<SfAiRemoteModelInfo> ParseModelList(string json)
        {
            var result = new List<SfAiRemoteModelInfo>();
            var root = JSON.Parse(json);
            var data = root["data"]?.AsArray;
            if (data == null) return result;

            foreach (JSONNode item in data)
            {
                var id = item["id"]?.Value;
                if (string.IsNullOrWhiteSpace(id)) continue;

                result.Add(new SfAiRemoteModelInfo
                {
                    Id = id,
                    Created = item["created"].AsLong,
                    OwnedBy = item["owned_by"]?.Value ?? "",
                    ContextLength = item["context_length"].AsInt,
                    SupportsImageIn = item["supports_image_in"].AsBool,
                    SupportsVideoIn = item["supports_video_in"].AsBool,
                    SupportsReasoning = item["supports_reasoning"].AsBool
                });
            }

            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase));
            return result;
        }
    }
}
