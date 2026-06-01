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
    public class SfAiAnthropicProvider : SfAiProviderBase
    {
        public override SfAiProviderType ProviderType => SfAiProviderType.Anthropic;

        public override async Task<SfAiChatResponse> ChatAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            var baseUrl = profile.ResolveBaseUrl();
            var url = $"{baseUrl.TrimEnd('/')}/v1/messages";
            var body = BuildRequestBody(profile, request, stream: false);
            var headers = BuildHeaders(profile);

            var raw = await SfAiHttp.PostJsonAsync(url, body, headers, profile.TimeoutSeconds, cancellationToken)
                .ConfigureAwait(false);

            return ParseResponse(raw, profile.Model);
        }

        public override async Task<string> ChatStreamAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            Action<SfAiStreamChunk> onChunk,
            CancellationToken cancellationToken = default)
        {
            var baseUrl = profile.ResolveBaseUrl();
            var url = $"{baseUrl.TrimEnd('/')}/v1/messages";
            var body = BuildRequestBody(profile, request, stream: true);
            var headers = BuildHeaders(profile);
            var sb = new StringBuilder();

            string currentEvent = null;
            string pendingDataLine = null;

            await SfAiHttp.PostJsonStreamLinesAsync(
                url,
                body,
                headers,
                profile.TimeoutSeconds,
                line =>
                {
                    if (line.StartsWith("event:"))
                    {
                        currentEvent = line.Substring(6).Trim();
                        return;
                    }

                    if (line.StartsWith("data:"))
                    {
                        pendingDataLine = line;
                        if (SfAiStreamParser.TryParseAnthropicSse(currentEvent, pendingDataLine, out var delta))
                            AppendDelta(sb, onChunk, delta);
                    }
                },
                cancellationToken).ConfigureAwait(false);

            return sb.ToString();
        }

        static Dictionary<string, string> BuildHeaders(SfAiProviderProfile profile) => new Dictionary<string, string>
        {
            { "x-api-key", profile.ApiKey },
            { "anthropic-version", "2023-06-01" }
        };

        static string BuildRequestBody(SfAiProviderProfile profile, SfAiChatRequest request, bool stream)
        {
            var root = new JSONObject();
            root["model"] = profile.Model;
            root["max_tokens"] = request.MaxTokens;
            root["stream"] = stream;

            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
                root["system"] = request.SystemPrompt;

            var messages = new JSONArray();
            foreach (var msg in request.Messages)
            {
                if (msg.Role == SfAiRole.System) continue;
                var obj = new JSONObject();
                obj["role"] = msg.Role == SfAiRole.Assistant ? "assistant" : "user";
                obj["content"] = msg.Content ?? "";
                messages.Add(obj);
            }

            root["messages"] = messages;
            return root.ToString();
        }

        static SfAiChatResponse ParseResponse(string raw, string model)
        {
            var json = JSON.Parse(raw);
            var content = json["content"]?[0]?["text"]?.Value ?? "";
            return new SfAiChatResponse
            {
                Content = content,
                Model = model,
                RawJson = raw
            };
        }
    }
}
