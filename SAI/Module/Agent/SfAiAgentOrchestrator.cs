using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Mcp;
using SFramework.SAI.Module.Tool;
using SimpleJSON;
using UnityEngine;

namespace SFramework.SAI.Module.Agent
{
    /// <summary>
    /// Agent 编排器（对齐 DSH 子 Agent 契约）：
    /// 解析 → 执行 → 核验；阶段间只传递 JSON（各阶段强制 structured_output）。
    /// </summary>
    public sealed class SfAiAgentOrchestrator : IDisposable
    {
        public SfAiToolRegistry Tools { get; }
        public SfAiAgentOrchestratorOptions Options { get; }

        readonly System.Collections.Generic.List<SfAiMcpClient> _mcpClients =
            new System.Collections.Generic.List<SfAiMcpClient>();

        public SfAiAgentOrchestrator(
            SfAiToolRegistry tools = null,
            SfAiAgentOrchestratorOptions options = null)
        {
            Tools = tools ?? SfAiToolBootstrap.CreateDefaultRegistry();
            Options = options ?? new SfAiAgentOrchestratorOptions();
        }

        public async Task ConnectMcpAsync(
            SfAiMcpServerConfig config,
            CancellationToken cancellationToken = default)
        {
            if (config == null || !config.Enabled || string.IsNullOrWhiteSpace(config.Command))
                return;

            var client = new SfAiMcpClient(config);
            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
            var count = await SfAiMcpToolBridge.RegisterServerToolsAsync(Tools, client, cancellationToken)
                .ConfigureAwait(false);
            _mcpClients.Add(client);
            Debug.Log($"[SfAiOrchestrator] MCP {config.Name} 注册工具 {count} 个");
        }

        public async Task<SfAiAgentRunResult> RunAsync(
            string goal,
            SfAiSettings settings,
            Action<SfAiAgentEvent> onEvent = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(goal))
                return Fail("目标不能为空");
            if (settings == null)
                return Fail("未配置 AI Settings");

            goal = goal.Trim();
            var totalSteps = 0;

            EmitPhase(onEvent, SfAiAgentEventKind.Started, SfAiAgentPhase.None, 0, goal);

            try
            {
                var parseResult = await RunPhaseAsync(
                        SfAiAgentPhase.Parse,
                        BuildParseGoal(goal),
                        FilterTools(Options.ParseToolFilter),
                        BuildParseOptions(),
                        settings,
                        onEvent,
                        cancellationToken)
                    .ConfigureAwait(false);
                totalSteps += parseResult.Steps;

                if (!parseResult.Ok)
                    return FailPhase(SfAiAgentPhase.Parse, parseResult, totalSteps);

                var planJson = RequireStructured(parseResult, SfAiAgentPhase.Parse);
                if (planJson == null)
                    return FailPhase(SfAiAgentPhase.Parse,
                        Fail("解析阶段未产出合法 JSON"), totalSteps);

                string executeJson = "";
                string verifyJson = "";
                var verified = false;
                var attempts = 0;
                var maxAttempts = 1 + Math.Max(0, Options.MaxVerifyRetries);
                string lastVerifyJson = "";

                while (attempts < maxAttempts)
                {
                    attempts++;
                    cancellationToken.ThrowIfCancellationRequested();

                    var executeResult = await RunPhaseAsync(
                            SfAiAgentPhase.Execute,
                            BuildExecuteGoal(goal, planJson, lastVerifyJson),
                            FilterTools(Options.ExecuteToolFilter),
                            BuildExecuteOptions(),
                            settings,
                            onEvent,
                            cancellationToken)
                        .ConfigureAwait(false);
                    totalSteps += executeResult.Steps;

                    if (!executeResult.Ok)
                        return FailPhase(SfAiAgentPhase.Execute, executeResult, totalSteps,
                            planJson, executeResult.StructuredJson);

                    executeJson = RequireStructured(executeResult, SfAiAgentPhase.Execute);
                    if (executeJson == null)
                        return FailPhase(SfAiAgentPhase.Execute,
                            Fail("执行阶段未产出合法 JSON"), totalSteps, planJson);

                    var verifyResult = await RunPhaseAsync(
                            SfAiAgentPhase.Verify,
                            BuildVerifyGoal(goal, planJson, executeJson),
                            FilterTools(Options.VerifyToolFilter),
                            BuildVerifyOptions(),
                            settings,
                            onEvent,
                            cancellationToken)
                        .ConfigureAwait(false);
                    totalSteps += verifyResult.Steps;

                    if (!verifyResult.Ok)
                        return FailPhase(SfAiAgentPhase.Verify, verifyResult, totalSteps,
                            planJson, executeJson);

                    verifyJson = RequireStructured(verifyResult, SfAiAgentPhase.Verify);
                    if (verifyJson == null)
                        return FailPhase(SfAiAgentPhase.Verify,
                            Fail("核验阶段未产出合法 JSON"), totalSteps, planJson, executeJson);

                    verified = IsVerifyPass(verifyJson);
                    if (verified)
                        break;

                    lastVerifyJson = verifyJson;
                    if (attempts < maxAttempts)
                    {
                        EmitPhase(onEvent, SfAiAgentEventKind.Step, SfAiAgentPhase.Verify, attempts,
                            $"核验 FAIL，准备第 {attempts + 1} 轮执行修复");
                    }
                }

                var finalText = BuildFinalSummary(planJson, executeJson, verifyJson, verified);
                EmitPhase(onEvent, SfAiAgentEventKind.Completed, SfAiAgentPhase.None, totalSteps, finalText);

                return new SfAiAgentRunResult
                {
                    Ok = true,
                    FinalText = finalText,
                    Steps = totalSteps,
                    PlanText = planJson,
                    ExecuteText = executeJson,
                    VerifyText = verifyJson,
                    Verified = verified,
                    StructuredJson = verifyJson
                };
            }
            catch (OperationCanceledException)
            {
                EmitPhase(onEvent, SfAiAgentEventKind.Cancelled, SfAiAgentPhase.None, 0, "已取消");
                return new SfAiAgentRunResult { Ok = false, Error = "已取消" };
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfAiOrchestrator] {e.Message}");
                EmitPhase(onEvent, SfAiAgentEventKind.Failed, SfAiAgentPhase.None, 0, e.Message);
                return Fail(e.Message);
            }
        }

        async Task<SfAiAgentRunResult> RunPhaseAsync(
            SfAiAgentPhase phase,
            string phaseGoal,
            SfAiToolRegistry tools,
            SfAiAgentOptions options,
            SfAiSettings settings,
            Action<SfAiAgentEvent> onEvent,
            CancellationToken cancellationToken)
        {
            EmitPhase(onEvent, SfAiAgentEventKind.PhaseStarted, phase, 0,
                SfAiAgentPhaseNames.Display(phase));

            var result = await SfAiAgentRunner.RunAsync(
                    phaseGoal,
                    settings,
                    tools,
                    options,
                    e =>
                    {
                        e.Phase = phase;
                        if (e.Kind == SfAiAgentEventKind.Started)
                        {
                            e.Kind = SfAiAgentEventKind.Step;
                            e.Message = $"{SfAiAgentPhaseNames.Display(phase)} · 开始";
                        }
                        else if (e.Kind == SfAiAgentEventKind.Completed)
                        {
                            e.Kind = SfAiAgentEventKind.Step;
                            e.Message = $"{SfAiAgentPhaseNames.Display(phase)} · 阶段完成(JSON)";
                        }

                        onEvent?.Invoke(e);
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            EmitPhase(onEvent, SfAiAgentEventKind.PhaseCompleted, phase, result.Steps,
                result.Ok
                    ? Truncate(result.StructuredJson.Length > 0 ? result.StructuredJson : result.FinalText, 400)
                    : "失败: " + (result.Error ?? "未知错误"));

            return result;
        }

        SfAiToolRegistry FilterTools(Func<ISfAiTool, bool> filter) =>
            Tools.Filter(filter ?? (_ => true));

        SfAiAgentOptions BuildParseOptions()
        {
            var o = CloneOptions(Options.ParseOptions);
            o.OutputSchemaJson = SfAiOrchestratorSchemas.Parse;
            o.SystemPrompt = JoinPrompt(
                Options.BaseSystemPrompt,
                "你是「任务解析」Agent。只拆解目标，禁止操作场景。\n" +
                "业务工具不可用；最终必须调用 structured_output 提交计划 JSON。");
            return o;
        }

        SfAiAgentOptions BuildExecuteOptions()
        {
            var o = CloneOptions(Options.ExecuteOptions);
            o.OutputSchemaJson = SfAiOrchestratorSchemas.Execute;
            o.SystemPrompt = JoinPrompt(
                Options.BaseSystemPrompt,
                "你是「任务执行」Agent。输入是上游 JSON 计划，按 steps 调用工具完成。\n" +
                "Unity 是组件式架构：空 GameObject 不够；UI 请用 unity_create_ui，或 create 后 unity_add_component。\n" +
                "常用 UI：canvas / panel / text / button / image / event_system。\n" +
                "完成后必须调用 structured_output 提交执行报告 JSON；纯文本无效。");
            return o;
        }

        SfAiAgentOptions BuildVerifyOptions()
        {
            var o = CloneOptions(Options.VerifyOptions);
            o.OutputSchemaJson = SfAiOrchestratorSchemas.Verify;
            o.SystemPrompt = JoinPrompt(
                Options.BaseSystemPrompt,
                "你是独立「核验」Agent。输入是目标 + 计划 JSON + 执行报告 JSON。\n" +
                "用只读工具检查现场；禁止创建/删除/修改。\n" +
                "最终必须调用 structured_output；verdict 只能是 PASS 或 FAIL。");
            return o;
        }

        static string BuildParseGoal(string goal) =>
            "Parse this user task into structured JSON via structured_output:\n\n" + goal;

        static string BuildExecuteGoal(string goal, string planJson, string verifyJson)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Execute the plan. Upstream data is JSON only.");
            sb.AppendLine();
            sb.AppendLine("user_goal:");
            sb.AppendLine(goal);
            sb.AppendLine();
            sb.AppendLine("plan_json:");
            sb.AppendLine(planJson);
            if (!string.IsNullOrWhiteSpace(verifyJson))
            {
                sb.AppendLine();
                sb.AppendLine("previous_verify_json (fix according to issues/suggestions):");
                sb.AppendLine(verifyJson);
            }

            sb.AppendLine();
            sb.AppendLine("When done, call structured_output with the execute report schema.");
            return sb.ToString();
        }

        static string BuildVerifyGoal(string goal, string planJson, string executeJson)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Verify completion. Upstream data is JSON only.");
            sb.AppendLine();
            sb.AppendLine("user_goal:");
            sb.AppendLine(goal);
            sb.AppendLine();
            sb.AppendLine("plan_json:");
            sb.AppendLine(planJson);
            sb.AppendLine();
            sb.AppendLine("execute_json:");
            sb.AppendLine(executeJson);
            sb.AppendLine();
            sb.AppendLine("Call structured_output with verdict PASS or FAIL.");
            return sb.ToString();
        }

        static string BuildFinalSummary(string plan, string execute, string verify, bool verified)
        {
            var sb = new StringBuilder();
            sb.AppendLine(verified ? "核验通过。" : "核验未通过。");

            var executeSummary = JsonString(execute, "summary");
            var executeStatus = JsonString(execute, "status");
            if (!string.IsNullOrWhiteSpace(executeSummary))
            {
                sb.AppendLine();
                sb.AppendLine(executeSummary.Trim());
            }
            else if (!string.IsNullOrWhiteSpace(executeStatus))
            {
                sb.AppendLine();
                sb.AppendLine("执行状态：" + executeStatus);
            }

            if (!verified)
            {
                var issues = JsonStringArray(verify, "issues");
                var suggestions = JsonStringArray(verify, "suggestions");
                if (!string.IsNullOrWhiteSpace(issues))
                {
                    sb.AppendLine();
                    sb.AppendLine("问题：");
                    sb.AppendLine(issues);
                }

                if (!string.IsNullOrWhiteSpace(suggestions))
                {
                    sb.AppendLine();
                    sb.AppendLine("建议：");
                    sb.AppendLine(suggestions);
                }
            }

            return sb.ToString().Trim();
        }

        static string JsonString(string json, string key)
        {
            try
            {
                var node = JSON.Parse(json ?? "{}");
                return node?[key]?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        static string JsonStringArray(string json, string key)
        {
            try
            {
                var node = JSON.Parse(json ?? "{}");
                var arr = node?[key];
                if (arr == null || !arr.IsArray || arr.Count == 0) return "";
                var sb = new StringBuilder();
                for (var i = 0; i < arr.Count; i++)
                {
                    var v = arr[i]?.Value;
                    if (string.IsNullOrWhiteSpace(v)) continue;
                    sb.Append("- ");
                    sb.AppendLine(v);
                }

                return sb.ToString().TrimEnd();
            }
            catch
            {
                return "";
            }
        }

        static string RequireStructured(SfAiAgentRunResult result, SfAiAgentPhase phase)
        {
            var json = result?.StructuredJson;
            if (string.IsNullOrWhiteSpace(json))
                json = result?.FinalText;
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                var node = JSON.Parse(json);
                if (node == null || node.Tag != JSONNodeType.Object)
                {
                    Debug.LogWarning($"[SfAiOrchestrator] {phase} 结果不是 JSON object");
                    return null;
                }

                return node.ToString();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SfAiOrchestrator] {phase} JSON 解析失败: {e.Message}");
                return null;
            }
        }

        static bool IsVerifyPass(string verifyJson)
        {
            try
            {
                var node = JSON.Parse(verifyJson);
                var verdict = node?["verdict"]?.Value ?? "";
                return string.Equals(verdict, "PASS", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        static SfAiAgentOptions CloneOptions(SfAiAgentOptions src)
        {
            src ??= new SfAiAgentOptions();
            return new SfAiAgentOptions
            {
                SystemPrompt = src.SystemPrompt,
                Temperature = src.Temperature,
                MaxTokens = src.MaxTokens,
                OutputSchemaJson = src.OutputSchemaJson,
                MaxConsecutiveToolFailures = src.MaxConsecutiveToolFailures,
                MaxConsecutiveJsonFailures = src.MaxConsecutiveJsonFailures,
                MaxRepeatedSameOutput = src.MaxRepeatedSameOutput,
                AbsoluteSafetyIterations = src.AbsoluteSafetyIterations
            };
        }

        static string JoinPrompt(string basePrompt, string phasePrompt)
        {
            if (string.IsNullOrWhiteSpace(basePrompt))
                return phasePrompt;
            return basePrompt.Trim() + "\n\n" + phasePrompt;
        }

        static SfAiAgentRunResult Fail(string error) =>
            new SfAiAgentRunResult { Ok = false, Error = error ?? "失败" };

        static SfAiAgentRunResult FailPhase(
            SfAiAgentPhase phase,
            SfAiAgentRunResult phaseResult,
            int totalSteps,
            string planText = "",
            string executeText = "")
        {
            return new SfAiAgentRunResult
            {
                Ok = false,
                Error = $"[{SfAiAgentPhaseNames.Display(phase)}] {phaseResult.Error}",
                Steps = totalSteps,
                PlanText = planText ?? "",
                ExecuteText = executeText ?? "",
                FailedPhase = phase
            };
        }

        static void EmitPhase(
            Action<SfAiAgentEvent> onEvent,
            SfAiAgentEventKind kind,
            SfAiAgentPhase phase,
            int step,
            string message)
        {
            onEvent?.Invoke(new SfAiAgentEvent
            {
                Kind = kind,
                Phase = phase,
                Step = step,
                Message = message ?? "",
                Detail = message ?? ""
            });
        }

        static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s ?? "";
            return s.Substring(0, max) + "…";
        }

        public void Dispose()
        {
            foreach (var client in _mcpClients)
                client.Dispose();
            _mcpClients.Clear();
        }
    }
}
