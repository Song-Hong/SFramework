using System.Collections.Generic;
using System.Linq;
using SFramework.SAI.Editor.Support;
using SFramework.SAI.Module.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Quick.Window
{
    public partial class SfAiQuickWindow
    {
        SfAiSettings _settings;
        DropdownField _providerDropdown;
        DropdownField _modelDropdown;
        TextField _apiKeyField;
        TextField _systemPromptField;

        readonly List<string> _providerIds = new List<string>();
        readonly List<string> _modelIds = new List<string>();
        string _selectedProviderId;

        void InitConnectionSettings()
        {
            _settings = SfAiEditorSettingsUtility.GetOrCreateSettings();
            if (_settings == null || _providerDropdown == null || _modelDropdown == null ||
                _apiKeyField == null || _systemPromptField == null)
                return;

            _settings.EnsureBuiltInProviders();

            _systemPromptField.SetValueWithoutNotify(_settings.SystemPrompt ?? "");
            _systemPromptField.RegisterValueChangedCallback(evt =>
            {
                _settings.SystemPrompt = evt.newValue ?? "";
                MarkSettingsDirty();
            });

            _apiKeyField.isDelayed = true;
            _apiKeyField.RegisterValueChangedCallback(evt => SaveApiKey(evt.newValue));

            _providerDropdown.RegisterValueChangedCallback(_ => OnProviderChanged());
            _modelDropdown.RegisterValueChangedCallback(_ => OnModelChanged());

            RefreshProviderDropdown();
            if (_providerIds.Count > 0)
            {
                var activeModel = _settings.GetActiveModel();
                _selectedProviderId = activeModel != null
                    ? activeModel.ProviderId
                    : _providerIds[0];
            }

            SelectProvider(_selectedProviderId);
            UpdateConnectionStatus();

            // 未配置 Key 时默认展开设置
            var provider = GetSelectedProvider();
            if (provider == null || !provider.HasValidKey())
                OpenSettingsPanel();
        }

        void RefreshProviderDropdown()
        {
            _settings.EnsureBuiltInProviders();
            _providerIds.Clear();

            var labels = new List<string>();
            foreach (var p in _settings.Providers.Where(p => p.Enabled))
            {
                labels.Add(p.Name);
                _providerIds.Add(p.Id);
            }

            if (labels.Count == 0)
            {
                _providerDropdown.choices = new List<string> { "无运营商" };
                _providerDropdown.index = 0;
                return;
            }

            _providerDropdown.choices = labels;
            var idx = _providerIds.IndexOf(_selectedProviderId);
            if (idx < 0) idx = 0;
            _providerDropdown.index = idx;
            _providerDropdown.SetValueWithoutNotify(labels[idx]);
            _selectedProviderId = _providerIds[idx];
        }

        void RefreshModelDropdown()
        {
            _modelIds.Clear();
            var labels = new List<string>();

            if (!string.IsNullOrEmpty(_selectedProviderId))
            {
                foreach (var m in _settings.GetModelsByProvider(_selectedProviderId).Where(m => m.Enabled))
                {
                    labels.Add($"{m.DisplayName}  ({m.ModelId})");
                    _modelIds.Add(m.Id);
                }
            }

            if (labels.Count == 0)
            {
                _modelDropdown.choices = new List<string> { "无模型" };
                _modelDropdown.index = 0;
                return;
            }

            _modelDropdown.choices = labels;
            var activeIdx = _modelIds.IndexOf(_settings.ActiveModelId);
            if (activeIdx < 0) activeIdx = 0;

            _modelDropdown.index = activeIdx;
            _modelDropdown.SetValueWithoutNotify(labels[activeIdx]);
            _settings.ActiveModelId = _modelIds[activeIdx];
        }

        void OnProviderChanged()
        {
            var idx = _providerDropdown.index;
            if (idx < 0 || idx >= _providerIds.Count) return;

            _selectedProviderId = _providerIds[idx];
            LoadApiKeyField();
            RefreshModelDropdown();
            MarkSettingsDirty();
            UpdateConnectionStatus();
        }

        void OnModelChanged()
        {
            var idx = _modelDropdown.index;
            if (idx < 0 || idx >= _modelIds.Count) return;

            _settings.ActiveModelId = _modelIds[idx];
            MarkSettingsDirty();
            UpdateConnectionStatus();
        }

        void SelectProvider(string providerId)
        {
            _selectedProviderId = providerId;
            RefreshProviderDropdown();
            LoadApiKeyField();
            RefreshModelDropdown();
            UpdateConnectionStatus();
        }

        void LoadApiKeyField()
        {
            var key = GetSelectedProvider()?.ApiKey ?? "";
            _apiKeyField.schedule.Execute(() => _apiKeyField.SetValueWithoutNotify(key));
        }

        void SaveApiKey(string raw)
        {
            var provider = GetSelectedProvider();
            if (provider == null) return;

            provider.ApiKey = SfAiProviderProfile.NormalizeApiKey(raw);
            MarkSettingsDirty();
            UpdateConnectionStatus();
        }

        SfAiProviderData GetSelectedProvider() =>
            string.IsNullOrEmpty(_selectedProviderId)
                ? null
                : _settings?.GetProvider(_selectedProviderId);

        void UpdateConnectionStatus()
        {
            if (_statusLabel == null || _settings == null) return;

            var provider = GetSelectedProvider();
            var model = _settings.GetActiveModel();
            if (provider == null || model == null)
            {
                _statusLabel.text = "请配置运营商与模型";
                return;
            }

            var keyOk = provider.HasValidKey() ? "已连接" : "缺少 Key";
            _statusLabel.text = $"{provider.Name} · {model.DisplayName} · {keyOk}";
        }

        void MarkSettingsDirty()
        {
            if (_settings == null) return;
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }

        void OpenSettingsPanel()
        {
            if (_settingsPanel == null) return;
            _settingsPanel.AddToClassList("sfai-quick-settings-open");
        }

        bool EnsureConnectionReady()
        {
            // 发送前把输入框里的 Key 落盘
            if (_apiKeyField != null)
                SaveApiKey(_apiKeyField.value);

            var provider = GetSelectedProvider();
            var model = _settings?.GetActiveModel();

            if (provider == null || model == null)
            {
                OpenSettingsPanel();
                _statusLabel.text = "请先选择运营商和模型";
                AddBubble("错误", "请先在设置中选择运营商与模型", "sfai-quick-bubble-error");
                return false;
            }

            if (!provider.HasValidKey())
            {
                OpenSettingsPanel();
                _statusLabel.text = "请填写 API Key";
                AddBubble("错误", $"请先为 {provider.Name} 填写 API Key", "sfai-quick-bubble-error");
                return false;
            }

            return true;
        }
    }
}
