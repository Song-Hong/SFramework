namespace SFramework.SAI.Module.Data
{
    public class SfAiChatResponse
    {
        public string Content;
        public string Model;
        public string RawJson;
        public SfAiProviderType ProviderType;

        public bool Success => !string.IsNullOrEmpty(Content);
    }
}
