using System;
using System.Text;
using System.Threading.Tasks;
using SFramework.SAI.Editor.Support;
using SFramework.SAI.Module;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Window
{
    public partial class SfAiWindow
    {
        Label _streamBodyLabel;
        string _streamPendingText;
        bool _streamUiDirty;
        bool _streamIsThinking;

        void InitChatPanel()
        {
            _sendButton.clicked += () => _ = SendMessageAsync();
            rootVisualElement.Q<Button>("ClearButton").clicked += ClearConversation;

            _inputField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return && !evt.shiftKey)
                {
                    evt.PreventDefault();
                    if (!_isSending) _ = SendMessageAsync();
                }
            });
        }

        async Task SendMessageAsync()
        {
            if (_isSending) return;

            var text = _inputField.value?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            SyncApiKeyFromUi();

            var active = _settings.GetActiveModel();
            var provider = _settings.GetProviderForModel(active);
            if (active == null || provider == null || !SfAiProviderProfile.From(provider, active).IsConfigured())
            {
                AppendSystemMessage("请选择运营商、模型并填写 API Key。");
                return;
            }

            _isSending = true;
            _sendButton.SetEnabled(false);
            ClearInputField();

            var sendEpoch = _conversationEpoch;

            AppendMessage(SfAiRole.User, text);
            _conversation.Add(SfAiMessage.User(text));

            var assistantBubble = AppendStreamingAssistant(out var bodyLabel);
            var fullText = new StringBuilder();
            BeginStreamUi(bodyLabel);

            try
            {
                var request = new SfAiChatRequest
                {
                    SystemPrompt = _systemPromptField.value?.Trim()
                };
                request.Messages.AddRange(_conversation);

                var response = await SfAiClient.ChatStreamAsync(request, chunk =>
                {
                    if (chunk.IsThinking)
                    {
                        QueueStreamThinking();
                        return;
                    }

                    fullText.Append(chunk.Text);
                    QueueStreamText(fullText.ToString());
                });

                var finalContent = string.IsNullOrEmpty(response.Content)
                    ? fullText.ToString()
                    : response.Content;

                var bubble = assistantBubble;
                SfAiEditorMainThread.Post(() => FinishStreamSuccess(bubble, bodyLabel, finalContent, sendEpoch));
            }
            catch (Exception e)
            {
                var bubble = assistantBubble;
                var message = e.Message;
                SfAiEditorMainThread.Post(() => FinishStreamError(bubble, message, sendEpoch));
            }
        }

        void FinishStreamSuccess(VisualElement assistantBubble, Label bodyLabel, string finalContent, int sendEpoch)
        {
            if (sendEpoch != _conversationEpoch)
            {
                EndSend();
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(finalContent))
                {
                    RemoveElement(assistantBubble);
                    AppendSystemMessage("AI 返回为空，请检查模型或配额。");
                    return;
                }

                bodyLabel.text = finalContent;
                _conversation.Add(SfAiMessage.Assistant(finalContent));
            }
            finally
            {
                EndSend();
            }
        }

        void FinishStreamError(VisualElement assistantBubble, string message, int sendEpoch)
        {
            if (sendEpoch != _conversationEpoch)
            {
                EndSend();
                return;
            }

            try
            {
                RemoveElement(assistantBubble);
                AppendSystemMessage($"请求失败: {message}");
            }
            finally
            {
                EndSend();
            }
        }

        void EndSend()
        {
            EndStreamUi();
            _isSending = false;
            _sendButton.SetEnabled(true);
            ScrollToBottom();
            UpdateStatus();
        }

        void BeginStreamUi(Label bodyLabel)
        {
            _streamBodyLabel = bodyLabel;
            _streamUiDirty = false;
            _streamIsThinking = true;
            _streamPendingText = "";
            QueueStreamThinking();
            EditorApplication.update -= OnStreamUiUpdate;
            EditorApplication.update += OnStreamUiUpdate;
        }

        void EndStreamUi()
        {
            EditorApplication.update -= OnStreamUiUpdate;
            _streamBodyLabel = null;
            _streamUiDirty = false;
            _streamIsThinking = false;
        }

        void QueueStreamThinking()
        {
            _streamIsThinking = true;
            _streamUiDirty = true;
        }

        void QueueStreamText(string text)
        {
            _streamIsThinking = false;
            _streamPendingText = text ?? "";
            _streamUiDirty = true;
        }

        void OnStreamUiUpdate()
        {
            if (!_streamUiDirty || _streamBodyLabel == null) return;

            _streamUiDirty = false;
            _streamBodyLabel.text = _streamIsThinking
                ? "思考中…"
                : _streamPendingText + "▌";
            if (_messageScroll != null)
                _messageScroll.scrollOffset = new Vector2(0, float.MaxValue);
        }

        VisualElement AppendStreamingAssistant(out Label bodyLabel)
        {
            var bubble = new VisualElement();
            bubble.AddToClassList("sfai-bubble");
            bubble.AddToClassList("sfai-bubble-assistant");

            var title = new Label("AI");
            title.AddToClassList("sfai-bubble-title");
            bubble.Add(title);

            bodyLabel = new Label("▌");
            bodyLabel.style.whiteSpace = WhiteSpace.Normal;
            bubble.Add(bodyLabel);

            _messageList.Add(bubble);
            ScrollToBottom();
            return bubble;
        }

        void ClearConversation()
        {
            _conversation.Clear();
            _messageList?.Clear();
        }

        void OnProviderSwitched()
        {
            _conversationEpoch++;
            ClearConversation();
            if (!_isSending) return;

            EndStreamUi();
            _isSending = false;
            _sendButton?.SetEnabled(true);
        }

        void AppendMessage(SfAiRole role, string content)
        {
            var bubble = new VisualElement();
            bubble.AddToClassList("sfai-bubble");
            bubble.AddToClassList(role switch
            {
                SfAiRole.User => "sfai-bubble-user",
                SfAiRole.Assistant => "sfai-bubble-assistant",
                _ => "sfai-bubble-system"
            });

            var title = new Label(role switch
            {
                SfAiRole.User => "你",
                SfAiRole.Assistant => "AI",
                _ => "系统"
            });
            title.AddToClassList("sfai-bubble-title");
            bubble.Add(title);

            var body = new Label(content ?? "");
            body.style.whiteSpace = WhiteSpace.Normal;
            bubble.Add(body);

            _messageList.Add(bubble);
            ScrollToBottom();
        }

        void AppendSystemMessage(string content) => AppendMessage(SfAiRole.System, content);

        void RemoveElement(VisualElement element)
        {
            if (element?.parent != null)
                element.parent.Remove(element);
        }

        void ClearInputField()
        {
            if (_inputField.focusController?.focusedElement == _inputField)
                _inputField.Blur();
            _inputField.schedule.Execute(() => _inputField.SetValueWithoutNotify(""));
        }

        void ScrollToBottom()
        {
            _messageScroll.schedule.Execute(() =>
            {
                _messageScroll.scrollOffset = new Vector2(0, float.MaxValue);
            });
        }
    }
}
