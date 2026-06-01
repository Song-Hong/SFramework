using System;
using UnityEngine;

namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// 旧版单条配置（仅用于 asset 迁移）
    /// </summary>
    [Serializable]
    public class SfAiLegacyProfile
    {
        public string ProfileName = "默认配置";
        public string ProviderName = "OpenAI";
        public SfAiProviderType ProviderType = SfAiProviderType.OpenAICompatible;
        public string CustomProviderType = "";
        public string ApiKey = "";
        public string BaseUrl = "";
        public string Model = "gpt-4o-mini";
        public int TimeoutSeconds = 120;

        public SfAiProviderType GetResolvedType()
        {
            var custom = (CustomProviderType ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(custom))
            {
                if (custom.Contains("anthropic") || custom.Contains("claude")) return SfAiProviderType.Anthropic;
                if (custom.Contains("ollama")) return SfAiProviderType.Ollama;
                return SfAiProviderType.OpenAICompatible;
            }
            return ProviderType;
        }
    }
}
