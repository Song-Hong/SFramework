using SimpleJSON;

namespace SFramework.SAI.Module.Support
{
    /// <summary>
    /// 各平台流式响应解析
    /// </summary>
    public static class SfAiStreamParser
    {
        /// <summary>OpenAI Compatible SSE（含 Qwen reasoning_content）</summary>
        public static bool TryParseOpenAiSseLine(string line, out SfAiStreamChunk chunk)
        {
            chunk = default;
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:"))
                return false;

            var data = line.Substring(5).Trim();
            if (data == "[DONE]") return false;

            var json = JSON.Parse(data);
            var delta = json["choices"]?[0]?["delta"];
            if (delta == null) return false;

            var reasoning = GetDeltaString(delta, "reasoning_content");
            if (!string.IsNullOrEmpty(reasoning))
            {
                chunk = SfAiStreamChunk.Thinking();
                return true;
            }

            var content = GetDeltaString(delta, "content");
            if (!string.IsNullOrEmpty(content))
            {
                chunk = SfAiStreamChunk.Content(content);
                return true;
            }

            return false;
        }

        /// <summary>Ollama NDJSON: {"message":{"content":"..."},"done":false}</summary>
        public static bool TryParseOllamaLine(string line, out string delta)
        {
            delta = null;
            if (string.IsNullOrWhiteSpace(line)) return false;

            var json = JSON.Parse(line);
            delta = GetDeltaString(json["message"], "content");
            return !string.IsNullOrEmpty(delta);
        }

        /// <summary>Anthropic SSE: event: content_block_delta / data: {"delta":{"text":"..."}}</summary>
        public static bool TryParseAnthropicSse(string eventName, string dataLine, out string delta)
        {
            delta = null;
            if (eventName != "content_block_delta" || string.IsNullOrWhiteSpace(dataLine))
                return false;

            if (!dataLine.StartsWith("data:")) return false;
            var data = dataLine.Substring(5).Trim();
            var json = JSON.Parse(data);
            delta = GetDeltaString(json["delta"], "text");
            return !string.IsNullOrEmpty(delta);
        }

        static string GetDeltaString(JSONNode parent, string key)
        {
            if (parent == null) return null;

            var node = parent[key];
            if (node == null || node.Tag == JSONNodeType.NullValue)
                return null;

            if (node.Tag != JSONNodeType.String)
                return null;

            var value = node.Value;
            if (string.IsNullOrEmpty(value) ||
                string.Equals(value, "null", System.StringComparison.OrdinalIgnoreCase))
                return null;

            return value;
        }
    }
}
