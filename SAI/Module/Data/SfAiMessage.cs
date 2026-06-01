using System;

namespace SFramework.SAI.Module.Data
{
    [Serializable]
    public class SfAiMessage
    {
        public SfAiRole Role = SfAiRole.User;
        public string Content = "";

        public SfAiMessage() { }

        public SfAiMessage(SfAiRole role, string content)
        {
            Role = role;
            Content = content ?? "";
        }

        public static SfAiMessage System(string content) => new(SfAiRole.System, content);
        public static SfAiMessage User(string content) => new(SfAiRole.User, content);
        public static SfAiMessage Assistant(string content) => new(SfAiRole.Assistant, content);
    }
}
