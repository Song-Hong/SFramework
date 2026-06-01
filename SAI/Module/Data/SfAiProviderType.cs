namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// AI 接口类型（可选项）
    /// </summary>
    public enum SfAiProviderType
    {
        /// <summary>OpenAI 兼容 Chat Completions</summary>
        OpenAICompatible,
        /// <summary>Anthropic Messages API</summary>
        Anthropic,
        /// <summary>Ollama 本地 API</summary>
        Ollama,
        /// <summary>自定义类型（由 CustomProviderType 字段决定）</summary>
        Custom
    }
}
