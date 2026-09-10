namespace SFramework.SAI.Module.Agent
{
    public enum SfAiAgentEventKind
    {
        Started,
        ModelText,
        ToolCall,
        ToolResult,
        Step,
        /// <summary>编排：某阶段开始</summary>
        PhaseStarted,
        /// <summary>编排：某阶段结束</summary>
        PhaseCompleted,
        Completed,
        Failed,
        Cancelled
    }

    public class SfAiAgentEvent
    {
        public SfAiAgentEventKind Kind;
        public SfAiAgentPhase Phase = SfAiAgentPhase.None;
        public int Step;
        public string Message = "";
        public string ToolName = "";
        public string Detail = "";
    }

    public class SfAiAgentRunResult
    {
        public bool Ok;
        public string FinalText = "";
        public int Steps;
        public string Error = "";

        /// <summary>DSH 式 structured_output 收录的 JSON（若本轮强制了 outputSchema）</summary>
        public string StructuredJson = "";

        /// <summary>解析阶段产出的计划（JSON 文本）</summary>
        public string PlanText = "";
        /// <summary>执行阶段报告（JSON 文本）</summary>
        public string ExecuteText = "";
        /// <summary>核验阶段结论（JSON 文本）</summary>
        public string VerifyText = "";
        /// <summary>核验是否判定通过</summary>
        public bool Verified;
        /// <summary>失败时落在哪一阶段</summary>
        public SfAiAgentPhase FailedPhase = SfAiAgentPhase.None;
    }
}
