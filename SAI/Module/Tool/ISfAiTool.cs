using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;

namespace SFramework.SAI.Module.Tool
{
    /// <summary>Agent 可调用工具（进程内或 MCP 桥接）</summary>
    public interface ISfAiTool
    {
        SfAiToolSpec Spec { get; }

        Task<SfAiToolResult> ExecuteAsync(
            string argumentsJson,
            CancellationToken cancellationToken = default);
    }
}
