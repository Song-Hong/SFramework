namespace SFramework.SAI.Module.Agent
{
    /// <summary>AI 基座：Agent 运行参数（业务无关）</summary>
    public class SfAiAgentOptions
    {
        public string SystemPrompt = "你是一个可调用工具的助手。需要时使用工具，完成后给出简短结论。";
        public float Temperature = 0.2f;
        public int MaxTokens = 4096;

        /// <summary>
        /// DSH 式 object-rooted JSON Schema。非空时强制注册 <c>structured_output</c>，
        /// 只有该工具成功收录的 JSON 才算阶段结果；纯文本收尾无效。
        /// </summary>
        public string OutputSchemaJson = "";

        /// <summary>工具连续失败达到此次数则打断（默认 5）</summary>
        public int MaxConsecutiveToolFailures = 5;

        /// <summary>连续 JSON / structured_output 失败达到此次数则打断（默认 5）</summary>
        public int MaxConsecutiveJsonFailures = 5;

        /// <summary>模型连续重复同一句话（或同一工具调用签名）达到此次数则打断（默认 3）</summary>
        public int MaxRepeatedSameOutput = 3;

        /// <summary>极端情况安全熔断（防止无限烧 Token），正常靠循环检测退出</summary>
        public int AbsoluteSafetyIterations = 256;
    }
}
