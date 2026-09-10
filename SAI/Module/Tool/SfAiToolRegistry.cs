using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using UnityEngine;

namespace SFramework.SAI.Module.Tool
{
    /// <summary>工具注册表（DSH tools.register 的轻量对应）</summary>
    public class SfAiToolRegistry
    {
        readonly Dictionary<string, ISfAiTool> _tools =
            new Dictionary<string, ISfAiTool>(StringComparer.OrdinalIgnoreCase);

        public int Count => _tools.Count;

        public void Register(ISfAiTool tool)
        {
            if (tool?.Spec == null || string.IsNullOrWhiteSpace(tool.Spec.Name))
                throw new ArgumentException("工具 Name 不能为空");

            _tools[tool.Spec.Name.Trim()] = tool;
        }

        public bool Unregister(string name) =>
            !string.IsNullOrWhiteSpace(name) && _tools.Remove(name.Trim());

        public bool TryGet(string name, out ISfAiTool tool)
        {
            tool = null;
            return !string.IsNullOrWhiteSpace(name) && _tools.TryGetValue(name.Trim(), out tool);
        }

        public IReadOnlyCollection<ISfAiTool> All => _tools.Values;

        /// <summary>按谓词复制一份子集注册表（编排各阶段隔离工具用）</summary>
        public SfAiToolRegistry Filter(Func<ISfAiTool, bool> predicate)
        {
            var clone = new SfAiToolRegistry();
            foreach (var tool in _tools.Values)
            {
                if (predicate == null || predicate(tool))
                    clone.Register(tool);
            }

            return clone;
        }

        public List<SfAiToolSpec> ToSpecs()
        {
            var list = new List<SfAiToolSpec>(_tools.Count);
            foreach (var tool in _tools.Values)
                list.Add(tool.Spec);
            return list;
        }

        public async Task<SfAiToolResult> InvokeAsync(
            string name,
            string argumentsJson,
            CancellationToken cancellationToken = default)
        {
            if (!TryGet(name, out var tool))
                return SfAiToolResult.Fail($"未注册工具: {name}");

            try
            {
                return await tool.ExecuteAsync(argumentsJson ?? "{}", cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfAiToolRegistry] {name}: {e.Message}");
                return SfAiToolResult.Fail(e.Message);
            }
        }
    }
}
