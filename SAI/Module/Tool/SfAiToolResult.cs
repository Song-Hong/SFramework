namespace SFramework.SAI.Module.Tool
{
    public class SfAiToolResult
    {
        public bool Ok;
        public string Content = "";
        public string Error = "";

        public static SfAiToolResult Success(string content) =>
            new SfAiToolResult { Ok = true, Content = content ?? "" };

        public static SfAiToolResult Fail(string error) =>
            new SfAiToolResult { Ok = false, Error = error ?? "工具执行失败", Content = error ?? "工具执行失败" };
    }
}
