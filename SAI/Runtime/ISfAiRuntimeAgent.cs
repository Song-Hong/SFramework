using System;
using SFramework.SAI.Module.Agent;
using SFramework.SAI.Module.Mcp;
using SFramework.SAI.Module.Tool;

namespace SFramework.SAI.Runtime
{
    /// <summary>运行时 AI：开放 Agent 接口，由项目二次开发实现工具与提示词</summary>
    public interface ISfAiRuntimeAgent
    {
        void Run(
            string goal,
            Action<SfAiAgentRunResult> onComplete = null,
            Action<SfAiAgentEvent> onEvent = null,
            Action<string> onError = null);

        void Cancel();
    }

    /// <summary>运行时工具模块：向基座 Registry 注册项目工具</summary>
    public interface ISfAiRuntimeToolModule
    {
        void RegisterTools(SfAiToolRegistry registry);
    }

    /// <summary>可选：运行时挂接 MCP Server</summary>
    public interface ISfAiRuntimeMcpModule
    {
        SfAiMcpServerConfig[] GetMcpServers();
    }
}
