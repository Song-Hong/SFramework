using System;

namespace SFramework.SAI.Module.Agent
{
    /// <summary>编排阶段：解析 → 执行 → 核验</summary>
    public enum SfAiAgentPhase
    {
        None = 0,
        /// <summary>解析任务：拆解目标与计划，通常不调写工具</summary>
        Parse = 1,
        /// <summary>执行任务：按计划调用工具完成工作</summary>
        Execute = 2,
        /// <summary>检查任务：独立核验是否达成目标</summary>
        Verify = 3
    }

    public static class SfAiAgentPhaseNames
    {
        public static string Display(SfAiAgentPhase phase)
        {
            switch (phase)
            {
                case SfAiAgentPhase.Parse: return "解析任务";
                case SfAiAgentPhase.Execute: return "执行任务";
                case SfAiAgentPhase.Verify: return "检查任务";
                default: return "编排";
            }
        }
    }
}
