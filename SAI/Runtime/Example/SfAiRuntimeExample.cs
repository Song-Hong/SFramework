using SFramework.SAI.Module.Tool;
using UnityEngine;

namespace SFramework.SAI.Runtime.Example
{
    /// <summary>
    /// 运行时二次开发示例：继承 SfAiRuntimeMono，只注册项目自己的工具。
    /// 挂到场景后：((SfAiRuntimeMono)this).Run("...");
    /// </summary>
    public class SfAiRuntimeExample : SfAiRuntimeMono
    {
        public override void RegisterTools(SfAiToolRegistry registry)
        {
            // 在此注册游戏业务工具，例如：
            // registry.Register(new MyQuestTool());
            // registry.Register(new MyInventoryTool());
            Debug.Log("[SfAiRuntimeExample] 未注册业务工具，仅使用基座 echo / unity_log");
        }
    }
}
