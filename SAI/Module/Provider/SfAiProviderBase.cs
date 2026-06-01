using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;

namespace SFramework.SAI.Module.Provider
{
    /// <summary>
    /// Provider 默认实现：不支持流式时一次性回调
    /// </summary>
    public abstract class SfAiProviderBase : ISfAiProvider
    {
        public abstract SfAiProviderType ProviderType { get; }

        public abstract Task<SfAiChatResponse> ChatAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            CancellationToken cancellationToken = default);

        public virtual async Task<string> ChatStreamAsync(
            SfAiProviderProfile profile,
            SfAiChatRequest request,
            Action<SfAiStreamChunk> onChunk,
            CancellationToken cancellationToken = default)
        {
            var response = await ChatAsync(profile, request, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(response.Content))
                onChunk?.Invoke(SfAiStreamChunk.Content(response.Content));
            return response.Content ?? "";
        }

        protected static string AppendDelta(StringBuilder sb, Action<SfAiStreamChunk> onChunk, string delta)
        {
            if (string.IsNullOrEmpty(delta)) return sb.ToString();
            sb.Append(delta);
            onChunk?.Invoke(SfAiStreamChunk.Content(delta));
            return sb.ToString();
        }

        protected static void NotifyThinking(Action<SfAiStreamChunk> onChunk) =>
            onChunk?.Invoke(SfAiStreamChunk.Thinking());
    }
}
