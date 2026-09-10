using System.Collections.Generic;

namespace SFramework.SAI.Module.Data
{
    public class SfAiChatResponse
    {
        public string Content;
        public string Model;
        public string RawJson;
        public SfAiProviderType ProviderType;
        public string FinishReason = "";
        public List<SfAiToolCall> ToolCalls = new List<SfAiToolCall>();

        public bool HasToolCalls => ToolCalls != null && ToolCalls.Count > 0;
        public bool Success => !string.IsNullOrEmpty(Content) || HasToolCalls;
    }
}
