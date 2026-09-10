using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Tool;

namespace SFramework.SAI.Module.Mcp
{
    /// <summary>把远程 MCP 工具注册进本地 ToolRegistry</summary>
    public sealed class SfAiMcpToolAdapter : ISfAiTool
    {
        readonly SfAiMcpClient _client;
        readonly string _remoteName;

        public SfAiToolSpec Spec { get; }

        public SfAiMcpToolAdapter(SfAiMcpClient client, SfAiMcpRemoteTool remote, string namePrefix = "")
        {
            _client = client;
            _remoteName = remote.Name;
            var localName = string.IsNullOrWhiteSpace(namePrefix)
                ? remote.Name
                : $"{namePrefix}_{remote.Name}";

            Spec = new SfAiToolSpec
            {
                Name = localName,
                Description = $"[MCP:{client.ServerName}] {remote.Description}",
                ParametersJson = string.IsNullOrWhiteSpace(remote.InputSchemaJson)
                    ? "{\"type\":\"object\",\"properties\":{}}"
                    : remote.InputSchemaJson
            };
        }

        public async Task<SfAiToolResult> ExecuteAsync(
            string argumentsJson,
            CancellationToken cancellationToken = default)
        {
            var text = await _client.CallToolAsync(_remoteName, argumentsJson, cancellationToken)
                .ConfigureAwait(false);
            return SfAiToolResult.Success(text);
        }
    }

    public static class SfAiMcpToolBridge
    {
        public static async Task<int> RegisterServerToolsAsync(
            SfAiToolRegistry registry,
            SfAiMcpClient client,
            CancellationToken cancellationToken = default)
        {
            if (registry == null || client == null) return 0;

            var tools = await client.ListToolsAsync(cancellationToken).ConfigureAwait(false);
            var prefix = Sanitize(client.ServerName);
            var count = 0;

            foreach (var tool in tools)
            {
                if (string.IsNullOrWhiteSpace(tool.Name)) continue;
                registry.Register(new SfAiMcpToolAdapter(client, tool, prefix));
                count++;
            }

            return count;
        }

        static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "mcp";
            var chars = name.Trim().ToLowerInvariant().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}
