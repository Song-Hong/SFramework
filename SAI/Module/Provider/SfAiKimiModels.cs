using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;

namespace SFramework.SAI.Module.Provider
{
    /// <summary>
    /// Kimi / Moonshot：GET /v1/models
    /// </summary>
    public static class SfAiKimiModels
    {
        static readonly string[] FallbackBaseUrls =
        {
            "https://api.moonshot.cn/v1",
            "https://api.moonshot.ai/v1"
        };

        public static bool SupportsListModels(SfAiProviderData provider)
        {
            if (provider == null) return false;
            if (provider.Kind == SfAiProviderKind.Kimi) return true;

            var name = provider.Name ?? "";
            return name.IndexOf("kimi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("moonshot", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static Task<List<SfAiRemoteModelInfo>> ListModelsAsync(
            SfAiProviderData provider,
            CancellationToken cancellationToken = default) =>
            SfAiOpenAiModelsApi.ListModelsAsync(
                provider,
                SfAiOpenAiModelsApi.AuthHeaderStyle.Bearer,
                FallbackBaseUrls,
                cancellationToken);
    }
}
