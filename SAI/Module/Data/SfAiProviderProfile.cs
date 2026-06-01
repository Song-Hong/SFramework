using System;

namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// 运行时请求配置（由 Provider + Model 组合生成，供底层 API 调用）
    /// </summary>
    [Serializable]
    public class SfAiProviderProfile
    {
        public string ProviderName = "";
        public SfAiProviderKind ProviderKind = SfAiProviderKind.OpenAICompatible;
        public SfAiProviderType ProviderType = SfAiProviderType.OpenAICompatible;
        public string ApiKey = "";
        /// <summary>仅用户手动填写的覆盖地址；为空时按 ProviderKind 推断</summary>
        public string BaseUrl = "";
        public string Model = "";
        public int TimeoutSeconds = 120;
        public string Organization = "";

        public static SfAiProviderProfile From(SfAiProviderData provider, SfAiModelData model)
        {
            if (provider == null || model == null)
                return new SfAiProviderProfile();

            return new SfAiProviderProfile
            {
                ProviderName = provider.Name,
                ProviderKind = provider.Kind,
                ProviderType = MapKind(provider.Kind),
                ApiKey = NormalizeApiKey(provider.ApiKey),
                BaseUrl = string.IsNullOrWhiteSpace(provider.BaseUrl) ? "" : provider.BaseUrl.TrimEnd('/'),
                Model = model.ModelId,
                TimeoutSeconds = provider.TimeoutSeconds,
                Organization = provider.Organization
            };
        }

        public static SfAiProviderType MapKind(SfAiProviderKind kind) => kind switch
        {
            SfAiProviderKind.Anthropic => SfAiProviderType.Anthropic,
            SfAiProviderKind.Ollama => SfAiProviderType.Ollama,
            SfAiProviderKind.Kimi => SfAiProviderType.OpenAICompatible,
            SfAiProviderKind.MiMo => SfAiProviderType.OpenAICompatible,
            SfAiProviderKind.Qwen => SfAiProviderType.OpenAICompatible,
            SfAiProviderKind.DeepSeek => SfAiProviderType.OpenAICompatible,
            SfAiProviderKind.GoogleGemini => SfAiProviderType.OpenAICompatible,
            _ => SfAiProviderType.OpenAICompatible
        };

        public SfAiProviderType GetResolvedType() => ProviderType;

        public string ResolveBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(BaseUrl))
                return BaseUrl.TrimEnd('/');

            var temp = new SfAiProviderData { Kind = ProviderKind, Name = ProviderName };
            return temp.ResolveBaseUrl();
        }

        public bool IsKimi() =>
            ProviderKind == SfAiProviderKind.Kimi ||
            (ProviderName != null && ProviderName.IndexOf("kimi", StringComparison.OrdinalIgnoreCase) >= 0);

        public bool IsMiMo() =>
            ProviderKind == SfAiProviderKind.MiMo ||
            (ProviderName != null && (
                ProviderName.IndexOf("mimo", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ProviderName.IndexOf("xiaomi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ProviderName.IndexOf("小米", StringComparison.OrdinalIgnoreCase) >= 0));

        public bool IsConfigured()
        {
            if (string.IsNullOrWhiteSpace(Model)) return false;
            return ProviderType == SfAiProviderType.Ollama || !string.IsNullOrWhiteSpace(ApiKey);
        }

        public static string NormalizeApiKey(string apiKey) =>
            string.IsNullOrWhiteSpace(apiKey) ? "" : apiKey.Trim();
    }
}
