using System;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;

namespace SFramework.SAI.Module.Provider
{
    public interface ISfAiProvider
    {
        SfAiProviderType ProviderType { get; }

        Task<SfAiChatResponse> ChatAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>流式对话；返回完整回复（仅正文，不含思考过程）</summary>
        Task<string> ChatStreamAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            Action<SfAiStreamChunk> onChunk,
            CancellationToken cancellationToken = default);
    }
}
