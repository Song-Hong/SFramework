using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;

namespace SFramework.SAI.Module.Provider
{
    /// <summary>
    /// DeepSeek：GET /models（OpenAI 兼容）
    /// </summary>
    public static class SfAiDeepSeekModels
    {
        public static bool SupportsListModels(SfAiProviderData provider)
        {
            if (provider == null) return false;
            if (provider.Kind == SfAiProviderKind.DeepSeek) return true;

            var name = provider.Name ?? "";
            return name.IndexOf("deepseek", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static Task<List<SfAiRemoteModelInfo>> ListModelsAsync(
            SfAiProviderData provider,
            CancellationToken cancellationToken = default) =>
            SfAiOpenAiModelsApi.ListModelsAsync(
                provider,
                SfAiOpenAiModelsApi.AuthHeaderStyle.Bearer,
                null,
                cancellationToken);
    }
}
