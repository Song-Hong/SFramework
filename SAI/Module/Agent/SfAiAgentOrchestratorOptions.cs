using System;
using SFramework.SAI.Module.Tool;

namespace SFramework.SAI.Module.Agent
{
    /// <summary>编排模式参数：三阶段提示词、循环检测与工具过滤</summary>
    public class SfAiAgentOrchestratorOptions
    {
        /// <summary>业务侧基础人设（会拼进各阶段提示词）</summary>
        public string BaseSystemPrompt = "";

        /// <summary>核验失败后最多再执行+核验几轮（不含首次）</summary>
        public int MaxVerifyRetries = 1;

        public SfAiAgentOptions ParseOptions = new SfAiAgentOptions
        {
            Temperature = 0.2f,
            MaxTokens = 4096
        };

        public SfAiAgentOptions ExecuteOptions = new SfAiAgentOptions
        {
            Temperature = 0.2f,
            MaxTokens = 4096
        };

        public SfAiAgentOptions VerifyOptions = new SfAiAgentOptions
        {
            Temperature = 0.1f,
            MaxTokens = 4096
        };

        /// <summary>解析阶段工具；默认无业务工具（Runner 会注入 structured_output）</summary>
        public Func<ISfAiTool, bool> ParseToolFilter = _ => false;

        /// <summary>执行阶段工具；默认全部</summary>
        public Func<ISfAiTool, bool> ExecuteToolFilter = _ => true;

        /// <summary>核验阶段工具；默认只读</summary>
        public Func<ISfAiTool, bool> VerifyToolFilter = DefaultVerifyFilter;

        public static bool DefaultVerifyFilter(ISfAiTool tool)
        {
            if (tool?.Spec == null) return false;
            var n = (tool.Spec.Name ?? "").ToLowerInvariant();
            if (n.Contains("create") || n.Contains("delete") || n.Contains("remove") ||
                n.Contains("write") || n.Contains("set_") || n.Contains("modify") ||
                n.Contains("destroy") || n.Contains("rename") || n.Contains("add_component") ||
                n.Contains("addcomponent"))
                return false;
            return true;
        }
    }
}
