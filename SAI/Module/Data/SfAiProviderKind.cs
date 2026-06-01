namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// 运营商接口类型（决定走哪套 API 协议）
    /// </summary>
    public enum SfAiProviderKind
    {
        /// <summary>OpenAI Compatible（OpenAI / DeepSeek / OpenRouter / 硅基流动 / Ollama 等）</summary>
        OpenAICompatible,
        /// <summary>Kimi（Moonshot AI，OpenAI 兼容 Chat Completions + SSE 流式）</summary>
        Kimi,
        /// <summary>小米 MiMo（OpenAI 兼容，api-key 认证）</summary>
        MiMo,
        /// <summary>阿里通义千问 Qwen（百炼 DashScope OpenAI 兼容模式）</summary>
        Qwen,
        Anthropic,
        /// <summary>Google Gemini（OpenAI 兼容端点）</summary>
        GoogleGemini,
        Ollama,
        /// <summary>DeepSeek（OpenAI 兼容，https://api.deepseek.com）</summary>
        DeepSeek
    }
}
