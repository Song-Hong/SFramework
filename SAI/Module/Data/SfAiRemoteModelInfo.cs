using System;
using System.Collections.Generic;

namespace SFramework.SAI.Module.Data
{
    /// <summary>
    /// GET /v1/models 返回的模型条目（OpenAI 兼容格式）
    /// </summary>
    [Serializable]
    public class SfAiRemoteModelInfo
    {
        public string Id = "";
        public long Created;
        public string OwnedBy = "";
        public int ContextLength;
        public bool SupportsImageIn;
        public bool SupportsVideoIn;
        public bool SupportsReasoning;

        public string BuildDisplayName()
        {
            var tags = new List<string>();
            if (SupportsReasoning) tags.Add("思考");
            if (SupportsImageIn) tags.Add("图");
            if (SupportsVideoIn) tags.Add("视频");
            if (ContextLength > 0) tags.Add($"{ContextLength / 1024}K");

            return tags.Count == 0 ? Id : $"{Id} ({string.Join("·", tags)})";
        }

        public string BuildTags()
        {
            var tags = new List<string>();
            if (SupportsReasoning) tags.Add("思考");
            if (SupportsImageIn) tags.Add("图");
            if (SupportsVideoIn) tags.Add("视频");
            return string.Join(",", tags);
        }
    }
}
