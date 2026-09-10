using System;

namespace SFramework.SAI.Module.Data
{
    [Serializable]
    public class SfAiToolCall
    {
        public string Id = "";
        public string Name = "";
        public string ArgumentsJson = "{}";
    }

    [Serializable]
    public class SfAiToolSpec
    {
        public string Name = "";
        public string Description = "";
        /// <summary>JSON Schema object，描述 parameters</summary>
        public string ParametersJson = "{\"type\":\"object\",\"properties\":{}}";
    }
}
