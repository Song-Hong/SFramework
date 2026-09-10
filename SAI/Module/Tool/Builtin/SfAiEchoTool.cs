using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;

namespace SFramework.SAI.Module.Tool.Builtin
{
    /// <summary>回显工具，便于联调 Agent Loop</summary>
    public sealed class SfAiEchoTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "echo",
            Description = "回显输入文本，用于测试 Agent 工具调用。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\",\"description\":\"要回显的文本\"}},\"required\":[\"text\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            var text = ExtractString(argumentsJson, "text");
            return Task.FromResult(SfAiToolResult.Success(text));
        }

        static string ExtractString(string json, string key)
        {
            if (string.IsNullOrWhiteSpace(json)) return "";
            try
            {
                var node = SimpleJSON.JSON.Parse(json);
                return node?[key]?.Value ?? json;
            }
            catch
            {
                return json;
            }
        }
    }
}
