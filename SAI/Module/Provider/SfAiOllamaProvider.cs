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
    public class SfAiOllamaProvider : SfAiProviderBase
    {
        public override SfAiProviderType ProviderType => SfAiProviderType.Ollama;

        public override async Task<SfAiChatResponse> ChatAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            var baseUrl = profile.ResolveBaseUrl();
            var url = $"{baseUrl.TrimEnd('/')}/api/chat";
            var body = BuildRequestBody(profile, request, stream: false);

            var raw = await SfAiHttp.PostJsonAsync(url, body, null, profile.TimeoutSeconds, cancellationToken)
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
            var url = $"{baseUrl.TrimEnd('/')}/api/chat";
            var body = BuildRequestBody(profile, request, stream: true);
            var sb = new StringBuilder();

            await SfAiHttp.PostJsonStreamLinesAsync(
                url,
                body,
                null,
                profile.TimeoutSeconds,
                line =>
                {
                    if (SfAiStreamParser.TryParseOllamaLine(line, out var delta))
                        AppendDelta(sb, onChunk, delta);
                },
                cancellationToken).ConfigureAwait(false);

            return sb.ToString();
        }

        static string BuildRequestBody(SfAiProviderProfile profile, SfAiChatRequest request, bool stream)
        {
            var root = new JSONObject();
            root["model"] = profile.Model;
            root["stream"] = stream;

            var messages = new JSONArray();
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
                messages.Add(OllamaMessage("system", request.SystemPrompt));

            foreach (var msg in request.Messages)
            {
                var role = msg.Role switch
                {
                    SfAiRole.System => "system",
                    SfAiRole.Assistant => "assistant",
                    _ => "user"
                };
                messages.Add(OllamaMessage(role, msg.Content));
            }

            root["messages"] = messages;
            return root.ToString();
        }

        static JSONObject OllamaMessage(string role, string content)
        {
            var obj = new JSONObject();
            obj["role"] = role;
            obj["content"] = content ?? "";
            return obj;
        }

        static SfAiChatResponse ParseResponse(string raw, string model)
        {
            var json = JSON.Parse(raw);
            var content = json["message"]?["content"]?.Value ?? "";
            return new SfAiChatResponse
            {
                Content = content,
                Model = model,
                RawJson = raw
            };
        }
    }
}
