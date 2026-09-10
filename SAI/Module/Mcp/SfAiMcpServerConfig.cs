using System;

namespace SFramework.SAI.Module.Mcp
{
    [Serializable]
    public class SfAiMcpServerConfig
    {
        public string Name = "unity-mcp";
        public string Command = "";
        public string[] Args = Array.Empty<string>();
        public bool Enabled = true;
    }
}
