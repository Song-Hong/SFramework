using System;
using System.Threading.Tasks;
using SFramework.SAI.Editor.Support;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Provider;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Window
{
    public partial class SfAiWindow
    {
        Action _refreshModelsClickHandler;

        void InitModelsPanel()
        {
            if (_refreshModelsButton == null) return;
            BindRefreshModelsHandler(GetSelectedProvider());
        }

        /// <summary>按运营商切换刷新按钮可见性与点击处理</summary>
        void BindRefreshModelsHandler(SfAiProviderData provider)
        {
            UnbindRefreshModelsHandler();

            if (_refreshModelsButton == null)
                return;

            if (provider == null)
            {
                SetRefreshModelsButtonVisible(false);
                return;
            }

            switch (provider.Kind)
            {
                case SfAiProviderKind.Kimi:
                    SetRefreshModelsButtonVisible(true);
                    _refreshModelsClickHandler = () => _ = RefreshKimiModelsAsync();
                    break;
                case SfAiProviderKind.Qwen:
                    SetRefreshModelsButtonVisible(true);
                    _refreshModelsClickHandler = () => _ = RefreshQwenModelsAsync();
                    break;
                case SfAiProviderKind.DeepSeek:
                    SetRefreshModelsButtonVisible(true);
                    _refreshModelsClickHandler = () => _ = RefreshDeepSeekModelsAsync();
                    break;
                default:
                    SetRefreshModelsButtonVisible(false);
                    return;
            }

            _refreshModelsButton.clicked += _refreshModelsClickHandler;
            _refreshModelsButton.SetEnabled(true);
        }

        void SetRefreshModelsButtonVisible(bool visible)
        {
            _refreshModelsButton.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void UnbindRefreshModelsHandler()
        {
            if (_refreshModelsClickHandler != null && _refreshModelsButton != null)
                _refreshModelsButton.clicked -= _refreshModelsClickHandler;
            _refreshModelsClickHandler = null;
        }

        async Task RefreshKimiModelsAsync() => await RefreshRemoteModelsAsync(
            GetSelectedProvider(),
            p => SfAiKimiModels.ListModelsAsync(p));

        async Task RefreshQwenModelsAsync() => await RefreshRemoteModelsAsync(
            GetSelectedProvider(),
            p => SfAiQwenModels.ListModelsAsync(p));

        async Task RefreshDeepSeekModelsAsync() => await RefreshRemoteModelsAsync(
            GetSelectedProvider(),
            p => SfAiDeepSeekModels.ListModelsAsync(p));

        async Task RefreshRemoteModelsAsync(
            SfAiProviderData provider,
            Func<SfAiProviderData, Task<System.Collections.Generic.List<SfAiRemoteModelInfo>>> listModels)
        {
            if (_isRefreshingModels || provider == null || listModels == null) return;

            SyncApiKeyFromUi();
            if (!provider.HasValidKey())
            {
                _statusLabel.text = "请先填写 API Key 再刷新模型";
                return;
            }

            _isRefreshingModels = true;
            _refreshModelsButton.SetEnabled(false);
            _statusLabel.text = "正在拉取模型列表…";

            try
            {
                var remote = await listModels(provider);
                var count = _settings.MergeRemoteModels(provider.Id, remote);

                SfAiEditorMainThread.Post(() =>
                {
                    MarkDirty();
                    RefreshModelDropdown();
                    UpdateStatus();
                    _statusLabel.text = $"已从 API 同步 {count} 个模型";
                });
            }
            catch (Exception e)
            {
                var message = e.Message;
                SfAiEditorMainThread.Post(() => _statusLabel.text = $"拉取模型失败: {message}");
            }
            finally
            {
                SfAiEditorMainThread.Post(() =>
                {
                    _isRefreshingModels = false;
                    if (_refreshModelsButton.style.display != DisplayStyle.None)
                        _refreshModelsButton.SetEnabled(true);
                });
            }
        }
    }
}
