using System;
using UnityEngine;

namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// 运营商（平台）：OpenAI / Anthropic / DeepSeek / OpenRouter 等
    /// </summary>
    [Serializable]
    public class SfAiProviderData
    {
        public string Id = Guid.NewGuid().ToString("N");

        [Tooltip("显示名称，如 OpenAI、DeepSeek")]
        public string Name = "OpenAI";

        public SfAiProviderKind Kind = SfAiProviderKind.OpenAICompatible;

        [Tooltip("API Base URL，留空则按平台推断")]
        public string BaseUrl = "";

        [Tooltip("API Key（本地 Ollama 可留空）")]
        public string ApiKey = "";

        [Tooltip("Organization（OpenAI 可选）")]
        public string Organization = "";

        public bool Enabled = true;
        public int TimeoutSeconds = 120;

        public SfAiProviderStatus Status = SfAiProviderStatus.Unknown;
        public int LatencyMs = -1;
        public string StatusMessage = "";

        public string ResolveBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(BaseUrl))
                return BaseUrl.TrimEnd('/');

            var name = (Name ?? string.Empty).Trim().ToLowerInvariant();
            return Kind switch
            {
                // platform.kimi.com / moonshot.cn 发放的 Key 走国内节点；国际 Key 可在 BaseUrl 填 https://api.moonshot.ai/v1
                SfAiProviderKind.Kimi => "https://api.moonshot.cn/v1",
                SfAiProviderKind.MiMo => "https://api.xiaomimimo.com/v1",
                SfAiProviderKind.Qwen => "https://dashscope.aliyuncs.com/compatible-mode/v1",
                SfAiProviderKind.DeepSeek => "https://api.deepseek.com",
                SfAiProviderKind.Anthropic => "https://api.anthropic.com",
                SfAiProviderKind.Ollama => "http://127.0.0.1:11434",
                SfAiProviderKind.GoogleGemini => "https://generativelanguage.googleapis.com/v1beta/openai",
                SfAiProviderKind.OpenAICompatible when name.Contains("deepseek") => "https://api.deepseek.com/v1",
                SfAiProviderKind.OpenAICompatible when name.Contains("openrouter") => "https://openrouter.ai/api/v1",
                SfAiProviderKind.OpenAICompatible when name.Contains("qwen") || name.Contains("dashscope") =>
                    "https://dashscope.aliyuncs.com/compatible-mode/v1",
                SfAiProviderKind.OpenAICompatible when name.Contains("moonshot") || name.Contains("kimi") =>
                    "https://api.moonshot.cn/v1",
                SfAiProviderKind.OpenAICompatible when name.Contains("silicon") =>
                    "https://api.siliconflow.cn/v1",
                SfAiProviderKind.OpenAICompatible => "https://api.openai.com/v1",
                _ => ""
            };
        }

        public bool HasValidKey() =>
            Kind == SfAiProviderKind.Ollama || !string.IsNullOrWhiteSpace(ApiKey);
    }
}
