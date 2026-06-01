namespace SFramework.SAI.Module.Support
{
    /// <summary>
    /// 流式输出片段（Qwen 等会在思考阶段发送 content=null + reasoning_content）
    /// </summary>
    public readonly struct SfAiStreamChunk
    {
        public bool IsThinking { get; }
        public string Text { get; }

        public SfAiStreamChunk(bool isThinking, string text)
        {
            IsThinking = isThinking;
            Text = text;
        }

        public static SfAiStreamChunk Thinking() => new SfAiStreamChunk(true, null);

        public static SfAiStreamChunk Content(string text) => new SfAiStreamChunk(false, text);
    }
}
