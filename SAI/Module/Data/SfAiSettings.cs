using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// AI 工作台全局配置（Kimi + MiMo + Qwen + DeepSeek）
    /// </summary>
    [CreateAssetMenu(fileName = "SfAiSettings", menuName = "SFramework/SAI/AI Settings")]
    public class SfAiSettings : ScriptableObject
    {
        [Header("运营商")]
        public List<SfAiProviderData> Providers = new List<SfAiProviderData>();

        [Header("模型")]
        public List<SfAiModelData> Models = new List<SfAiModelData>();

        [Header("当前选中模型 Id")]
        public string ActiveModelId = "";

        [Header("失败时自动尝试其他已启用模型")]
        public bool EnableAutoFallback = false;

        [Header("系统提示词")]
        [TextArea(2, 6)]
        public string SystemPrompt = "";

        [Header("旧版配置（自动迁移）")]
        [SerializeField] List<SfAiLegacyProfile> Profiles = new List<SfAiLegacyProfile>();

        /// <summary>为 true 时不合并内置运营商（SfAiMono 运行时实例）</summary>
        [NonSerialized] public bool preserveUserProvidersOnly;

        public SfAiProviderData GetProviderByKind(SfAiProviderKind kind) =>
            Providers?.FirstOrDefault(p => p.Enabled && p.Kind == kind);

        public SfAiModelData GetActiveModel()
        {
            if (!preserveUserProvidersOnly)
                EnsureBuiltInProviders();

            if (Models == null || Models.Count == 0) return null;

            var model = Models.FirstOrDefault(m => m.Id == ActiveModelId && m.Enabled);
            if (model != null) return model;

            model = Models.FirstOrDefault(m => m.Enabled);
            if (model != null) ActiveModelId = model.Id;
            return model;
        }

        public SfAiProviderData GetProvider(string providerId) =>
            Providers?.FirstOrDefault(p => p.Id == providerId);

        public SfAiProviderData GetProviderForModel(SfAiModelData model) =>
            model == null ? null : GetProvider(model.ProviderId);

        public List<SfAiModelData> GetEnabledModels() =>
            Models?.Where(m => m.Enabled).ToList() ?? new List<SfAiModelData>();

        public List<SfAiModelData> GetModelsByProvider(string providerId) =>
            Models?.Where(m => m.ProviderId == providerId).ToList() ?? new List<SfAiModelData>();

        public void EnsureDefaults()
        {
            if (!preserveUserProvidersOnly)
                EnsureBuiltInProviders();
        }

        public void EnsureBuiltInProviders()
        {
            MigrateLegacyProfilesIfNeeded();

            if (Providers == null) Providers = new List<SfAiProviderData>();
            if (Models == null) Models = new List<SfAiModelData>();

            if (Providers.Count == 0)
            {
                SeedBuiltInProviders();
                return;
            }

            RemoveUnsupportedProviders();

            var kimi = EnsureProviderEntry(SfAiProviderKind.Kimi, "Kimi");
            if (!Models.Exists(m => m.ProviderId == kimi.Id))
                SeedKimiModelsFor(kimi.Id);

            var mimo = EnsureProviderEntry(SfAiProviderKind.MiMo, "MiMo");
            if (!Models.Exists(m => m.ProviderId == mimo.Id))
                SeedMimoModelsFor(mimo.Id);

            var qwen = EnsureProviderEntry(SfAiProviderKind.Qwen, "Qwen");
            EnsureQwenModelsFor(qwen.Id);

            var deepSeek = EnsureProviderEntry(SfAiProviderKind.DeepSeek, "DeepSeek");
            EnsureDeepSeekModelsFor(deepSeek.Id);

            SanitizeDeprecatedKimiModels();

            if (string.IsNullOrEmpty(ActiveModelId) ||
                Models.All(m => m.Id != ActiveModelId || !m.Enabled))
            {
                var first = Models.FirstOrDefault(m => m.Enabled);
                if (first != null) ActiveModelId = first.Id;
            }
        }

        SfAiProviderData EnsureProviderEntry(SfAiProviderKind kind, string name)
        {
            var provider = Providers.FirstOrDefault(p => p.Kind == kind);
            if (provider == null)
                provider = AddProvider(name, kind);

            provider.Name = name;
            provider.Kind = kind;
            provider.Enabled = true;
            return provider;
        }

        void RemoveUnsupportedProviders()
        {
            Providers.RemoveAll(p =>
                p.Kind != SfAiProviderKind.Kimi &&
                p.Kind != SfAiProviderKind.MiMo &&
                p.Kind != SfAiProviderKind.Qwen &&
                p.Kind != SfAiProviderKind.DeepSeek);

            foreach (var kind in new[]
                     {
                         SfAiProviderKind.Kimi,
                         SfAiProviderKind.MiMo,
                         SfAiProviderKind.Qwen,
                         SfAiProviderKind.DeepSeek
                     })
            {
                var sameKind = Providers.Where(p => p.Kind == kind).ToList();
                for (var i = 1; i < sameKind.Count; i++)
                {
                    var duplicate = sameKind[i];
                    Models.RemoveAll(m => m.ProviderId == duplicate.Id);
                    Providers.Remove(duplicate);
                }
            }

            var keepIds = Providers.Select(p => p.Id).ToHashSet();
            Models.RemoveAll(m => !keepIds.Contains(m.ProviderId));
        }

        void SeedKimiModelsFor(string providerId)
        {
            AddModel(providerId, "Kimi K2.6", "kimi-k2.6");
            AddModel(providerId, "Kimi K2.5", "kimi-k2.5");
        }

        void SeedMimoModelsFor(string providerId)
        {
            AddModel(providerId, "MiMo V2.5 Pro", "mimo-v2.5-pro");
            AddModel(providerId, "MiMo V2.5", "mimo-v2.5");
            AddModel(providerId, "MiMo V2 Flash", "mimo-v2-flash");
        }

        /// <summary>
        /// 百炼文本生成模型列表（推荐 + 常用）：https://help.aliyun.com/zh/model-studio/text-generation-model
        /// </summary>
        void EnsureQwenModelsFor(string providerId)
        {
            EnsureQwenModel(providerId, "Qwen3.7 Max", "qwen3.7-max");
            EnsureQwenModel(providerId, "Qwen3.6 Plus", "qwen3.6-plus");
            EnsureQwenModel(providerId, "Qwen3.6 Flash", "qwen3.6-flash");
            EnsureQwenModel(providerId, "Qwen3.5 Plus", "qwen3.5-plus");
            EnsureQwenModel(providerId, "Qwen3.5 Flash", "qwen3.5-flash");
            EnsureQwenModel(providerId, "Qwen3 Max", "qwen3-max");
            EnsureQwenModel(providerId, "Qwen Long", "qwen-long");
        }

        void EnsureQwenModel(string providerId, string displayName, string modelId)
        {
            if (Models.Exists(m =>
                    m.ProviderId == providerId &&
                    string.Equals(m.ModelId, modelId, StringComparison.OrdinalIgnoreCase)))
                return;

            AddModel(providerId, displayName, modelId);
        }

        /// <summary>
        /// DeepSeek 模型：https://api-docs.deepseek.com/zh-cn/quick_start/pricing
        /// </summary>
        void EnsureDeepSeekModelsFor(string providerId)
        {
            EnsureDeepSeekModel(providerId, "DeepSeek V4 Pro", "deepseek-v4-pro");
            EnsureDeepSeekModel(providerId, "DeepSeek V4 Flash", "deepseek-v4-flash");
            EnsureDeepSeekModel(providerId, "DeepSeek Chat (兼容)", "deepseek-chat");
            EnsureDeepSeekModel(providerId, "DeepSeek Reasoner (兼容)", "deepseek-reasoner");
        }

        void EnsureDeepSeekModel(string providerId, string displayName, string modelId)
        {
            if (Models.Exists(m =>
                    m.ProviderId == providerId &&
                    string.Equals(m.ModelId, modelId, StringComparison.OrdinalIgnoreCase)))
                return;

            AddModel(providerId, displayName, modelId);
        }

        void SanitizeDeprecatedKimiModels()
        {
            var deprecated = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "kimi-k2-turbo-preview",
                "kimi-k2-0905-preview",
                "kimi-k2-0711-preview",
                "kimi-k2-thinking",
                "kimi-k2-thinking-turbo"
            };

            foreach (var model in Models)
            {
                if (model == null || !deprecated.Contains(model.ModelId)) continue;
                model.Enabled = false;
                if (ActiveModelId == model.Id)
                    ActiveModelId = Models.Find(m => m.Enabled && m.ModelId == "kimi-k2.6")?.Id ?? "";
            }
        }

        void MigrateLegacyProfilesIfNeeded()
        {
            if (Profiles == null || Profiles.Count == 0) return;
            if (Providers != null && Providers.Count > 0) return;

            Providers = new List<SfAiProviderData>();
            Models = new List<SfAiModelData>();

            var kimi = AddProvider("Kimi", SfAiProviderKind.Kimi);
            foreach (var legacy in Profiles)
            {
                if (legacy == null) continue;
                kimi.ApiKey = string.IsNullOrWhiteSpace(kimi.ApiKey) ? legacy.ApiKey : kimi.ApiKey;
                if (!string.IsNullOrWhiteSpace(legacy.BaseUrl))
                    kimi.BaseUrl = legacy.BaseUrl;
                if (!string.IsNullOrWhiteSpace(legacy.Model))
                    AddModel(kimi.Id, legacy.Model, legacy.Model);
            }

            if (!Models.Exists(m => m.ProviderId == kimi.Id))
                SeedKimiModelsFor(kimi.Id);

            var mimo = AddProvider("MiMo", SfAiProviderKind.MiMo);
            SeedMimoModelsFor(mimo.Id);

            var qwen = AddProvider("Qwen", SfAiProviderKind.Qwen);
            EnsureQwenModelsFor(qwen.Id);

            var deepSeek = AddProvider("DeepSeek", SfAiProviderKind.DeepSeek);
            EnsureDeepSeekModelsFor(deepSeek.Id);

            if (Models.Count > 0)
                ActiveModelId = Models[0].Id;

            Profiles.Clear();
        }

        void SeedBuiltInProviders()
        {
            Providers.Clear();
            Models.Clear();

            var kimi = AddProvider("Kimi", SfAiProviderKind.Kimi);
            SeedKimiModelsFor(kimi.Id);

            var mimo = AddProvider("MiMo", SfAiProviderKind.MiMo);
            SeedMimoModelsFor(mimo.Id);

            var qwen = AddProvider("Qwen", SfAiProviderKind.Qwen);
            EnsureQwenModelsFor(qwen.Id);

            var deepSeek = AddProvider("DeepSeek", SfAiProviderKind.DeepSeek);
            EnsureDeepSeekModelsFor(deepSeek.Id);

            ActiveModelId = Models[0].Id;
        }

        public SfAiProviderData AddProvider(string name, SfAiProviderKind kind)
        {
            var p = new SfAiProviderData { Name = name, Kind = kind };
            Providers.Add(p);
            return p;
        }

        public SfAiModelData AddModel(string providerId, string displayName, string modelId)
        {
            var m = new SfAiModelData
            {
                ProviderId = providerId,
                DisplayName = displayName,
                ModelId = modelId
            };
            Models.Add(m);
            return m;
        }

        public int MergeRemoteModels(string providerId, IList<SfAiRemoteModelInfo> remoteModels)
        {
            if (string.IsNullOrEmpty(providerId) || remoteModels == null) return 0;

            var remoteIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var merged = 0;

            foreach (var remote in remoteModels)
            {
                if (remote == null || string.IsNullOrWhiteSpace(remote.Id)) continue;

                remoteIds.Add(remote.Id);
                var existing = Models.Find(m =>
                    m.ProviderId == providerId &&
                    string.Equals(m.ModelId, remote.Id, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.DisplayName = remote.BuildDisplayName();
                    existing.Tags = remote.BuildTags();
                    existing.Enabled = true;
                }
                else
                {
                    Models.Add(new SfAiModelData
                    {
                        ProviderId = providerId,
                        DisplayName = remote.BuildDisplayName(),
                        ModelId = remote.Id,
                        Tags = remote.BuildTags()
                    });
                }

                merged++;
            }

            foreach (var local in Models)
            {
                if (local.ProviderId != providerId) continue;
                if (!remoteIds.Contains(local.ModelId))
                    local.Enabled = false;
            }

            if (merged > 0)
            {
                var active = GetActiveModel();
                if (active == null || !active.Enabled || active.ProviderId != providerId)
                {
                    var first = Models.Find(m => m.ProviderId == providerId && m.Enabled);
                    if (first != null)
                        ActiveModelId = first.Id;
                }
            }

            return merged;
        }
    }
}
