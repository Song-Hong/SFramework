using System;
using System.Collections.Generic;

namespace SFramework.SAI.Module.Data
{
    [Serializable]
    public class SfAiMessage
    {
        public SfAiRole Role = SfAiRole.User;
        public string Content = "";
        public string Name = "";
        public string ToolCallId = "";
        public List<SfAiToolCall> ToolCalls = new List<SfAiToolCall>();

        public SfAiMessage() { }

        public SfAiMessage(SfAiRole role, string content)
        {
            Role = role;
            Content = content ?? "";
        }

        public static SfAiMessage System(string content) => new(SfAiRole.System, content);
        public static SfAiMessage User(string content) => new(SfAiRole.User, content);
        public static SfAiMessage Assistant(string content) => new(SfAiRole.Assistant, content);

        public static SfAiMessage AssistantWithTools(string content, List<SfAiToolCall> toolCalls) =>
            new SfAiMessage(SfAiRole.Assistant, content ?? "")
            {
                ToolCalls = toolCalls ?? new List<SfAiToolCall>()
            };

        public static SfAiMessage ToolResult(string toolCallId, string content, string name = "") =>
            new SfAiMessage(SfAiRole.Tool, content ?? "")
            {
                ToolCallId = toolCallId ?? "",
                Name = name ?? ""
            };
    }
}
