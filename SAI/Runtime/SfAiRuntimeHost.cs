using System;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Agent;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Mcp;
using SFramework.SAI.Module.Tool;
using UnityEngine;

namespace SFramework.SAI.Runtime
{
    /// <summary>
    /// 运行时 Host：基于 AI 基座组装编排器（解析 → 执行 → 核验）。
    /// 开发者通过 toolModule / 自定义 registry 二次扩展。
    /// </summary>
    public sealed class SfAiRuntimeHost : ISfAiRuntimeAgent, IDisposable
    {
        readonly SfAiSettings _settings;
        readonly SfAiAgentOptions _options;
        readonly ISfAiRuntimeToolModule _toolModule;
        readonly ISfAiRuntimeMcpModule _mcpModule;

        SfAiAgentOrchestrator _orchestrator;
        CancellationTokenSource _cts;

        public SfAiRuntimeHost(
            SfAiSettings settings,
            SfAiAgentOptions options = null,
            ISfAiRuntimeToolModule toolModule = null,
            ISfAiRuntimeMcpModule mcpModule = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _options = options ?? new SfAiAgentOptions();
            _toolModule = toolModule;
            _mcpModule = mcpModule;
        }

        public void Run(
            string goal,
            Action<SfAiAgentRunResult> onComplete = null,
            Action<SfAiAgentEvent> onEvent = null,
            Action<string> onError = null)
        {
            if (string.IsNullOrWhiteSpace(goal))
            {
                onError?.Invoke("目标不能为空");
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = RunInternalAsync(goal.Trim(), onComplete, onEvent, onError, _cts.Token);
        }

        public void Cancel() => _cts?.Cancel();

        async Task RunInternalAsync(
            string goal,
            Action<SfAiAgentRunResult> onComplete,
            Action<SfAiAgentEvent> onEvent,
            Action<string> onError,
            CancellationToken cancellationToken)
        {
            try
            {
                _orchestrator?.Dispose();

                var registry = SfAiToolBootstrap.CreateCoreRegistry();
                _toolModule?.RegisterTools(registry);

                var orchOptions = new SfAiAgentOrchestratorOptions
                {
                    BaseSystemPrompt = _options.SystemPrompt ?? "",
                    MaxVerifyRetries = 1
                };
                orchOptions.ExecuteOptions.Temperature = _options.Temperature;
                orchOptions.ExecuteOptions.MaxTokens = _options.MaxTokens;
                orchOptions.ExecuteOptions.AbsoluteSafetyIterations = _options.AbsoluteSafetyIterations;
                orchOptions.ExecuteOptions.MaxConsecutiveToolFailures = _options.MaxConsecutiveToolFailures;
                orchOptions.ExecuteOptions.MaxConsecutiveJsonFailures = _options.MaxConsecutiveJsonFailures;
                orchOptions.ExecuteOptions.MaxRepeatedSameOutput = _options.MaxRepeatedSameOutput;

                _orchestrator = new SfAiAgentOrchestrator(registry, orchOptions);

                if (_mcpModule != null)
                {
                    var servers = _mcpModule.GetMcpServers();
                    if (servers != null)
                    {
                        foreach (var server in servers)
                        {
                            if (server == null || !server.Enabled) continue;
                            await _orchestrator.ConnectMcpAsync(server, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                }

                var result = await _orchestrator.RunAsync(goal, _settings, onEvent, cancellationToken)
                    .ConfigureAwait(false);

                if (!result.Ok)
                    onError?.Invoke(result.Error);
                else
                    onComplete?.Invoke(result);
            }
            catch (OperationCanceledException)
            {
                onError?.Invoke("已取消");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfAiRuntimeHost] {e.Message}");
                onError?.Invoke(e.Message);
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _orchestrator?.Dispose();
        }
    }
}
