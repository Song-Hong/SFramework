using System;
using UnityEngine;

namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// 模型条目：绑定到某个运营商，用于顶部一键切换
    /// </summary>
    [Serializable]
    public class SfAiModelData
    {
        public string Id = Guid.NewGuid().ToString("N");

        [Tooltip("所属运营商 Id")]
        public string ProviderId = "";

        [Tooltip("UI 显示名，如 GPT-4o mini")]
        public string DisplayName = "GPT-4o mini";

        [Tooltip("API 模型 ID，如 gpt-4o-mini")]
        public string ModelId = "gpt-4o-mini";

        public bool Enabled = true;

        [Tooltip("能力标签（可选），如 代码,推理,长上下文")]
        public string Tags = "";

        public string GetSwitcherLabel(string providerName) =>
            string.IsNullOrWhiteSpace(providerName)
                ? DisplayName
                : $"{providerName} · {DisplayName}";
    }
}
