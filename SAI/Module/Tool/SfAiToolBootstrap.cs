using SFramework.SAI.Module.Tool.Builtin;

namespace SFramework.SAI.Module.Tool
{
    /// <summary>AI 基座：仅装配通用最小工具；业务工具由 Editor 插件 / Runtime 二次开发注册</summary>
    public static class SfAiToolBootstrap
    {
        public static SfAiToolRegistry CreateCoreRegistry()
        {
            var registry = new SfAiToolRegistry();
            registry.Register(new SfAiEchoTool());
            registry.Register(new SfAiLogTool());
            return registry;
        }

        /// <summary>兼容旧调用：等同 CreateCoreRegistry</summary>
        public static SfAiToolRegistry CreateDefaultRegistry() => CreateCoreRegistry();
    }
}
