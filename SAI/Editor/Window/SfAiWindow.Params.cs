using System.Collections.Generic;
using System.Linq;
using SFramework.SAI.Module.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Window
{
    public partial class SfAiWindow
    {
        void InitParamsPanel()
        {
            if (_settings == null || _providerDropdown == null || _modelDropdown == null ||
                _systemPromptField == null || _apiKeyField == null)
                return;

            _settings.EnsureBuiltInProviders();

            _systemPromptField.SetValueWithoutNotify(_settings.SystemPrompt ?? "");
            _systemPromptField.RegisterValueChangedCallback(evt =>
            {
                _settings.SystemPrompt = evt.newValue;
                MarkDirty();
            });

            _apiKeyField.isDelayed = true;
            _apiKeyField.RegisterValueChangedCallback(evt => SaveApiKeyFromField(evt.newValue));

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
                return;
            }

            _providerDropdown.choices = labels;
            var idx = _providerIds.IndexOf(_selectedProviderId);
            if (idx < 0) idx = 0;
            _providerDropdown.index = idx;
            _providerDropdown.value = labels[idx];
            _selectedProviderId = _providerIds[idx];
        }

        void RefreshModelDropdown()
        {
            if (_modelDropdown == null) return;

            _modelIds.Clear();
            var labels = new List<string>();

            if (!string.IsNullOrEmpty(_selectedProviderId))
            {
                foreach (var m in _settings.GetModelsByProvider(_selectedProviderId).Where(m => m.Enabled))
                {
                    labels.Add(m.DisplayName);
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
            if (activeIdx < 0 || _settings.GetProvider(_selectedProviderId)?.Id !=
                _settings.GetProviderForModel(_settings.GetActiveModel())?.Id)
                activeIdx = 0;

            _modelDropdown.index = activeIdx;
            _modelDropdown.value = labels[activeIdx];
            _settings.ActiveModelId = _modelIds[activeIdx];
        }

        void OnProviderChanged()
        {
            var idx = _providerDropdown.index;
            if (idx < 0 || idx >= _providerIds.Count) return;

            var newProviderId = _providerIds[idx];
            var providerChanged = newProviderId != _selectedProviderId;
            _selectedProviderId = newProviderId;

            if (providerChanged)
                OnProviderSwitched();

            LoadApiKeyField();
            RefreshModelDropdown();
            BindRefreshModelsHandler(GetSelectedProvider());
            UpdateStatus();
        }

        void OnModelChanged()
        {
            var idx = _modelDropdown.index;
            if (idx < 0 || idx >= _modelIds.Count) return;

            _settings.ActiveModelId = _modelIds[idx];
            MarkDirty();
            UpdateStatus();
        }

        void SelectProvider(string providerId)
        {
            _selectedProviderId = providerId;
            RefreshProviderDropdown();
            LoadApiKeyField();
            RefreshModelDropdown();
            BindRefreshModelsHandler(GetSelectedProvider());
            UpdateStatus();
        }

        void LoadApiKeyField()
        {
            var provider = GetSelectedProvider();
            var key = provider?.ApiKey ?? "";

            if (_apiKeyField.focusController?.focusedElement == _apiKeyField)
                _apiKeyField.Blur();

            _apiKeyField.schedule.Execute(() =>
                _apiKeyField.SetValueWithoutNotify(key));
        }

        void SaveApiKeyFromField(string raw)
        {
            var provider = GetSelectedProvider();
            if (provider == null) return;

            provider.ApiKey = SfAiProviderProfile.NormalizeApiKey(raw);
            MarkDirty();
            UpdateStatus();
        }

        public void SyncApiKeyFromUi()
        {
            if (_apiKeyField == null) return;
            SaveApiKeyFromField(_apiKeyField.value);
        }

        void UpdateStatus()
        {
            var provider = GetSelectedProvider();
            var model = _settings.GetActiveModel();
            if (provider == null || model == null)
            {
                _statusLabel.text = "请选择运营商、模型并填写 API Key";
                return;
            }

            var keyOk = provider.HasValidKey() ? "Key 已配置" : "缺少 Key";
            _statusLabel.text = $"{provider.Name} · {model.DisplayName} ({model.ModelId}) · {keyOk}";
        }

        void MarkDirty()
        {
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }
    }
}
