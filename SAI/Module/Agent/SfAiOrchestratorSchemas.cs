namespace SFramework.SAI.Module.Agent
{
    /// <summary>编排各阶段强制 JSON Schema（object-rooted，对齐 DSH outputSchema）</summary>
    public static class SfAiOrchestratorSchemas
    {
        public const string Parse = @"{
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""required"": [""goal"", ""steps"", ""success_criteria""],
  ""properties"": {
    ""goal"": { ""type"": ""string"", ""description"": ""归一化后的任务目标"" },
    ""steps"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object"",
        ""required"": [""id"", ""action"", ""done_when""],
        ""properties"": {
          ""id"": { ""type"": ""string"" },
          ""action"": { ""type"": ""string"" },
          ""done_when"": { ""type"": ""string"" }
        }
      }
    },
    ""success_criteria"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" }
    },
    ""risks"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" }
    }
  }
}";

        public const string Execute = @"{
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""required"": [""status"", ""summary"", ""completed_steps"", ""remaining""],
  ""properties"": {
    ""status"": {
      ""type"": ""string"",
      ""enum"": [""completed"", ""partial"", ""failed""]
    },
    ""summary"": { ""type"": ""string"" },
    ""completed_steps"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" }
    },
    ""remaining"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" }
    },
    ""notes"": { ""type"": ""string"" }
  }
}";

        public const string Verify = @"{
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""required"": [""verdict"", ""evidence"", ""issues""],
  ""properties"": {
    ""verdict"": {
      ""type"": ""string"",
      ""enum"": [""PASS"", ""FAIL""]
    },
    ""evidence"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" }
    },
    ""issues"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" }
    },
    ""suggestions"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" }
    }
  }
}";

        public const string StructuredOutputInstruction =
            "When you have your final answer, you MUST report it by calling the `structured_output` tool " +
            "with arguments matching its parameter schema exactly. Do not finish with a plain text answer: " +
            "only the tool call counts as your result. Call it exactly once.";
    }
}
