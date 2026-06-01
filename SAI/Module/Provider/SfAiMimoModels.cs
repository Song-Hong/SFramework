using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;

namespace SFramework.SAI.Module.Provider
{
    /// <summary>
    /// 小米 MiMo（OpenAI 兼容对话；平台未提供模型列表接口，工作台不展示刷新按钮）
    /// </summary>
    public static class SfAiMimoModels
    {
        public static bool SupportsListModels(SfAiProviderData provider)
        {
            if (provider == null) return false;
            if (provider.Kind == SfAiProviderKind.MiMo) return true;

            var name = provider.Name ?? "";
            return name.IndexOf("mimo", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("xiaomi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("小米", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static Task<List<SfAiRemoteModelInfo>> ListModelsAsync(
            SfAiProviderData provider,
            CancellationToken cancellationToken = default) =>
            SfAiOpenAiModelsApi.ListModelsAsync(
                provider,
                SfAiOpenAiModelsApi.AuthHeaderStyle.ApiKey,
                null,
                cancellationToken);
    }
}
