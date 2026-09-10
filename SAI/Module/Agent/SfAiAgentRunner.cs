using System;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Tool;
using SFramework.SAI.Module.Tool.Builtin;
using UnityEngine;

namespace SFramework.SAI.Module.Agent
{
    /// <summary>
    /// Agent Loop（对齐 DSH）：Model → Tool → 回填；
    /// 无固定最大步数，靠循环检测打断（连续工具失败 / 重复输出 / JSON 失败）。
    /// </summary>
    public static class SfAiAgentRunner
    {
        public static async Task<SfAiAgentRunResult> RunAsync(
            string goal,
            SfAiSettings settings,
            SfAiToolRegistry tools,
            SfAiAgentOptions options = null,
            Action<SfAiAgentEvent> onEvent = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new SfAiAgentOptions();
            tools ??= SfAiToolBootstrap.CreateDefaultRegistry();

            if (string.IsNullOrWhiteSpace(goal))
                return Fail("目标不能为空");

            if (settings == null)
                return Fail("未配置 AI Settings");

            SfAiStructuredOutputTool structured = null;
            var workTools = tools;
            var requireStructured = !string.IsNullOrWhiteSpace(options.OutputSchemaJson);
            if (requireStructured)
            {
                workTools = tools.Filter(_ => true);
                structured = new SfAiStructuredOutputTool(options.OutputSchemaJson);
                workTools.Register(structured);
            }

            var hasTools = workTools.Count > 0;
            if (requireStructured && !hasTools)
                return Fail("outputSchema 已配置但无法注册 structured_output");

            var previousFallback = settings.EnableAutoFallback;
            settings.EnableAutoFallback = false;
            SfAiClient.SetSettings(settings);

            var systemPrompt = options.SystemPrompt ?? "";
            if (requireStructured)
            {
                if (!string.IsNullOrWhiteSpace(systemPrompt))
                    systemPrompt = systemPrompt.TrimEnd() + "\n\n";
                systemPrompt += SfAiOrchestratorSchemas.StructuredOutputInstruction;
            }

            var request = new SfAiChatRequest
            {
                SystemPrompt = systemPrompt,
                Temperature = options.Temperature,
                MaxTokens = options.MaxTokens,
                Tools = hasTools ? workTools.ToSpecs() : null,
                ToolChoice = hasTools ? "auto" : null
            };
            request.AddUser(goal.Trim());

            Emit(onEvent, SfAiAgentEventKind.Started, 0, goal);

            var maxToolFail = Math.Max(1, options.MaxConsecutiveToolFailures);
            var maxJsonFail = Math.Max(1, options.MaxConsecutiveJsonFailures);
            var maxRepeat = Math.Max(2, options.MaxRepeatedSameOutput);
            var safetyCap = Math.Max(32, options.AbsoluteSafetyIterations);

            var consecutiveToolFails = 0;
            var consecutiveJsonFails = 0;
            var consecutiveRepeats = 0;
            string lastFingerprint = null;

            try
            {
                for (var step = 1; step <= safetyCap; step++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Emit(onEvent, SfAiAgentEventKind.Step, step, $"第 {step} 步");

                    var response = await SfAiClient.ChatAsync(request, cancellationToken)
                        .ConfigureAwait(false);

                    var fingerprint = BuildFingerprint(response);
                    if (!string.IsNullOrEmpty(fingerprint) &&
                        string.Equals(fingerprint, lastFingerprint, StringComparison.Ordinal))
                    {
                        consecutiveRepeats++;
                        if (consecutiveRepeats >= maxRepeat)
                            return FailStuck(step, onEvent,
                                $"检测到模型连续 {consecutiveRepeats} 次重复同一输出，已打断");
                    }
                    else
                    {
                        consecutiveRepeats = 1;
                        lastFingerprint = fingerprint;
                    }

                    if (response.HasToolCalls)
                    {
                        request.AddMessage(SfAiMessage.AssistantWithTools(response.Content, response.ToolCalls));

                        foreach (var call in response.ToolCalls)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            Emit(onEvent, SfAiAgentEventKind.ToolCall, step, call.ArgumentsJson, call.Name);

                            var result = await workTools.InvokeAsync(call.Name, call.ArgumentsJson, cancellationToken)
                                .ConfigureAwait(false);

                            var isStructured = string.Equals(
                                call.Name, SfAiStructuredOutputTool.ToolName, StringComparison.OrdinalIgnoreCase);

                            if (!result.Ok)
                            {
                                consecutiveToolFails++;
                                if (isStructured)
                                    consecutiveJsonFails++;

                                var content = $"ERROR: {result.Error}";
                                Emit(onEvent, SfAiAgentEventKind.ToolResult, step, content, call.Name);
                                request.AddMessage(SfAiMessage.ToolResult(call.Id, content, call.Name));

                                if (consecutiveToolFails >= maxToolFail)
                                    return FailStuck(step, onEvent,
                                        $"工具连续失败 {consecutiveToolFails} 次，已打断（最近: {call.Name}）");

                                if (consecutiveJsonFails >= maxJsonFail)
                                    return FailStuck(step, onEvent,
                                        $"structured_output / JSON 连续失败 {consecutiveJsonFails} 次，已打断");

                                continue;
                            }

                            consecutiveToolFails = 0;
                            if (isStructured)
                                consecutiveJsonFails = 0;

                            Emit(onEvent, SfAiAgentEventKind.ToolResult, step, result.Content, call.Name);
                            request.AddMessage(SfAiMessage.ToolResult(call.Id, result.Content, call.Name));
                        }

                        if (structured != null && structured.Recorded)
                        {
                            var json = structured.RecordedJson;
                            Emit(onEvent, SfAiAgentEventKind.ModelText, step, json);
                            Emit(onEvent, SfAiAgentEventKind.Completed, step, json);
                            return new SfAiAgentRunResult
                            {
                                Ok = true,
                                FinalText = json,
                                StructuredJson = json,
                                Steps = step
                            };
                        }

                        continue;
                    }

                    // 强制 JSON 时：纯文本不算完成
                    if (requireStructured)
                    {
                        consecutiveJsonFails++;
                        var hint = response.Content ?? "";
                        if (!string.IsNullOrWhiteSpace(hint))
                            Emit(onEvent, SfAiAgentEventKind.ModelText, step, hint);

                        request.AddAssistant(hint);
                        request.AddUser(
                            "纯文本无效。请立即调用 structured_output，参数必须是符合 schema 的 JSON object。");

                        if (consecutiveJsonFails >= maxJsonFail)
                            return FailStuck(step, onEvent,
                                $"连续 {consecutiveJsonFails} 次未提交合法 JSON（structured_output），已打断");

                        continue;
                    }

                    var finalText = response.Content ?? "";
                    Emit(onEvent, SfAiAgentEventKind.ModelText, step, finalText);
                    Emit(onEvent, SfAiAgentEventKind.Completed, step, finalText);

                    return new SfAiAgentRunResult
                    {
                        Ok = true,
                        FinalText = finalText,
                        Steps = step
                    };
                }

                return FailStuck(safetyCap, onEvent, $"达到安全熔断上限 {safetyCap} 步，已打断");
            }
            catch (OperationCanceledException)
            {
                Emit(onEvent, SfAiAgentEventKind.Cancelled, 0, "已取消");
                return new SfAiAgentRunResult { Ok = false, Error = "已取消" };
            }
            catch (Exception e)
            {
                Debug.LogError($"[SfAiAgent] {e.Message}");
                Emit(onEvent, SfAiAgentEventKind.Failed, 0, e.Message);
                return Fail(e.Message);
            }
            finally
            {
                settings.EnableAutoFallback = previousFallback;
            }
        }

        static string BuildFingerprint(SfAiChatResponse response)
        {
            if (response == null) return "";

            if (response.HasToolCalls)
            {
                var sb = new System.Text.StringBuilder("tools:");
                foreach (var call in response.ToolCalls)
                {
                    sb.Append(call.Name ?? "");
                    sb.Append('|');
                    sb.Append(NormalizeText(call.ArgumentsJson));
                    sb.Append(';');
                }

                return sb.ToString();
            }

            return "text:" + NormalizeText(response.Content);
        }

        static string NormalizeText(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var chars = s.Trim().ToCharArray();
            var sb = new System.Text.StringBuilder(chars.Length);
            var prevSpace = false;
            foreach (var c in chars)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!prevSpace) sb.Append(' ');
                    prevSpace = true;
                }
                else
                {
                    sb.Append(c);
                    prevSpace = false;
                }
            }

            return sb.ToString();
        }

        static SfAiAgentRunResult FailStuck(int step, Action<SfAiAgentEvent> onEvent, string message)
        {
            Emit(onEvent, SfAiAgentEventKind.Failed, step, message);
            return new SfAiAgentRunResult { Ok = false, Error = message, Steps = step };
        }

        static SfAiAgentRunResult Fail(string error) =>
            new SfAiAgentRunResult { Ok = false, Error = error ?? "失败" };

        static void Emit(
            Action<SfAiAgentEvent> onEvent,
            SfAiAgentEventKind kind,
            int step,
            string message,
            string toolName = "")
        {
            onEvent?.Invoke(new SfAiAgentEvent
            {
                Kind = kind,
                Step = step,
                Message = message ?? "",
                ToolName = toolName ?? "",
                Detail = message ?? ""
            });
        }
    }
}
