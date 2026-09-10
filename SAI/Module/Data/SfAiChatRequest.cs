using System;
using System.Collections.Generic;

namespace SFramework.SAI.Module.Data
{
    [Serializable]
    public class SfAiChatRequest
    {
        public List<SfAiMessage> Messages = new List<SfAiMessage>();
        public string SystemPrompt;
        public float Temperature = 0.7f;
        public int MaxTokens = 4096;
        public List<SfAiToolSpec> Tools = new List<SfAiToolSpec>();
        /// <summary>auto / none / required</summary>
        public string ToolChoice = "auto";

        public SfAiChatRequest AddUser(string content)
        {
            Messages.Add(SfAiMessage.User(content));
            return this;
        }

        public SfAiChatRequest AddAssistant(string content)
        {
            Messages.Add(SfAiMessage.Assistant(content));
            return this;
        }

        public SfAiChatRequest AddMessage(SfAiMessage message)
        {
            if (message != null)
                Messages.Add(message);
            return this;
        }

        public bool HasTools => Tools != null && Tools.Count > 0;
    }
}
