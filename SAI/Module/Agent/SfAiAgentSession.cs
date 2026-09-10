using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Mcp;
using SFramework.SAI.Module.Tool;
using UnityEngine;

namespace SFramework.SAI.Module.Agent
{
    /// <summary>Agent 会话：单阶段工具循环（持有工具、可选 MCP）。多阶段请用 <see cref="SfAiAgentOrchestrator"/>。</summary>
    public sealed class SfAiAgentSession : IDisposable
    {
        public SfAiToolRegistry Tools { get; }
        public SfAiAgentOptions Options { get; }
        public List<SfAiAgentEvent> Events { get; } = new List<SfAiAgentEvent>();

        readonly List<SfAiMcpClient> _mcpClients = new List<SfAiMcpClient>();

        public SfAiAgentSession(SfAiToolRegistry tools = null, SfAiAgentOptions options = null)
        {
            Tools = tools ?? SfAiToolBootstrap.CreateDefaultRegistry();
            Options = options ?? new SfAiAgentOptions();
        }

        public async Task ConnectMcpAsync(
            SfAiMcpServerConfig config,
            CancellationToken cancellationToken = default)
        {
            if (config == null || !config.Enabled || string.IsNullOrWhiteSpace(config.Command))
                return;

            var client = new SfAiMcpClient(config);
            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
            var count = await SfAiMcpToolBridge.RegisterServerToolsAsync(Tools, client, cancellationToken)
                .ConfigureAwait(false);
            _mcpClients.Add(client);
            Debug.Log($"[SfAiAgent] MCP {config.Name} 注册工具 {count} 个");
        }

        public Task<SfAiAgentRunResult> RunAsync(
            string goal,
            SfAiSettings settings,
            Action<SfAiAgentEvent> onEvent = null,
            CancellationToken cancellationToken = default)
        {
            Events.Clear();
            return SfAiAgentRunner.RunAsync(
                goal,
                settings,
                Tools,
                Options,
                e =>
                {
                    Events.Add(e);
                    onEvent?.Invoke(e);
                },
                cancellationToken);
        }

        public void Dispose()
        {
            foreach (var client in _mcpClients)
                client.Dispose();
            _mcpClients.Clear();
        }
    }
}
