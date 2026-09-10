using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using UnityEngine;

namespace SFramework.SAI.Module.Tool.Builtin
{
    /// <summary>写 Unity 日志</summary>
    public sealed class SfAiLogTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_log",
            Description = "向 Unity Console 输出一条日志。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{\"message\":{\"type\":\"string\"},\"level\":{\"type\":\"string\",\"enum\":[\"info\",\"warning\",\"error\"],\"default\":\"info\"}},\"required\":[\"message\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            var node = SimpleJSON.JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var message = node?["message"]?.Value ?? "";
            var level = (node?["level"]?.Value ?? "info").ToLowerInvariant();

            switch (level)
            {
                case "warning":
                    Debug.LogWarning(message);
                    break;
                case "error":
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }

            return Task.FromResult(SfAiToolResult.Success($"已输出 {level}: {message}"));
        }
    }
}
