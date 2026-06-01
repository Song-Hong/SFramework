using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;

namespace SFramework.SAI.Module.Provider
{
    /// <summary>
    /// 阿里通义千问 / 百炼 DashScope：GET /compatible-mode/v1/models
    /// </summary>
    public static class SfAiQwenModels
    {
        public static bool SupportsListModels(SfAiProviderData provider)
        {
            if (provider == null) return false;
            if (provider.Kind == SfAiProviderKind.Qwen) return true;

            var name = provider.Name ?? "";
            return name.IndexOf("qwen", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("通义", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("千问", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("dashscope", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("百炼", StringComparison.OrdinalIgnoreCase) >= 0;
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
