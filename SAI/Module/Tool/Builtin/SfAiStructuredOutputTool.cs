using System;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SimpleJSON;

namespace SFramework.SAI.Module.Tool.Builtin
{
    /// <summary>
    /// DSH 式结构化收尾工具：参数必须匹配 outputSchema；成功一次后本轮结束。
    /// </summary>
    public sealed class SfAiStructuredOutputTool : ISfAiTool
    {
        public const string ToolName = "structured_output";

        readonly string _schemaJson;
        readonly string[] _requiredKeys;

        public bool Recorded { get; private set; }
        public string RecordedJson { get; private set; } = "";

        public SfAiToolSpec Spec { get; }

        public SfAiStructuredOutputTool(string outputSchemaJson)
        {
            if (string.IsNullOrWhiteSpace(outputSchemaJson))
                throw new ArgumentException("outputSchema 不能为空", nameof(outputSchemaJson));

            _schemaJson = outputSchemaJson.Trim();
            _requiredKeys = ExtractRequiredKeys(_schemaJson);

            Spec = new SfAiToolSpec
            {
                Name = ToolName,
                Description =
                    "Report your final structured result. Call this exactly once, when your answer is complete; " +
                    "the arguments must match this tool's parameter schema exactly.",
                ParametersJson = _schemaJson
            };
        }

        public Task<SfAiToolResult> ExecuteAsync(
            string argumentsJson,
            CancellationToken cancellationToken = default)
        {
            if (Recorded)
                return Task.FromResult(SfAiToolResult.Fail(
                    "structured output already recorded: the run is complete"));

            JSONNode node;
            try
            {
                node = JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            }
            catch (Exception e)
            {
                return Task.FromResult(SfAiToolResult.Fail("invalid JSON: " + e.Message));
            }

            if (node == null || node.Tag != JSONNodeType.Object)
                return Task.FromResult(SfAiToolResult.Fail("structured_output 参数必须是 JSON object"));

            foreach (var key in _requiredKeys)
            {
                if (node[key] == null || node[key].Tag == JSONNodeType.NullValue)
                    return Task.FromResult(SfAiToolResult.Fail($"missing required field: {key}"));
            }

            Recorded = true;
            RecordedJson = node.ToString();
            // DSH 式确认：{ recorded: true }
            return Task.FromResult(SfAiToolResult.Success("{\"recorded\":true}"));
        }

        static string[] ExtractRequiredKeys(string schemaJson)
        {
            try
            {
                var root = JSON.Parse(schemaJson);
                var req = root?["required"];
                if (req == null || !req.IsArray || req.Count == 0)
                    return Array.Empty<string>();

                var keys = new string[req.Count];
                for (var i = 0; i < req.Count; i++)
                    keys[i] = req[i]?.Value ?? "";
                return keys;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
