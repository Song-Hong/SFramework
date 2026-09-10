using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Quick.Support
{
    public enum SfAiQuickOpKind
    {
        Think,
        Step,
        Prepare,
        Connect,
        Hierarchy,
        Create,
        Find,
        Select,
        Log,
        Echo,
        ToolCall,
        ToolResult,
        Done,
        Error,
        Generic
    }

    public struct SfAiQuickChainItemParts
    {
        public VisualElement Row;
        public VisualElement Circle;
        public VisualElement Text;
    }

    /// <summary>简约思维链：圆圈线形图标 + 详情</summary>
    public static class SfAiQuickOpIcons
    {
        public static SfAiQuickOpKind FromToolName(string toolName, bool isResult = false)
        {
            if (isResult) return SfAiQuickOpKind.ToolResult;
            var n = (toolName ?? "").ToLowerInvariant();
            if (n.Contains("create_ui") || n.Contains("createui")) return SfAiQuickOpKind.Create;
            if (n.Contains("add_component") || n.Contains("set_ui")) return SfAiQuickOpKind.Create;
            if (n.Contains("hierarchy")) return SfAiQuickOpKind.Hierarchy;
            if (n.Contains("create")) return SfAiQuickOpKind.Create;
            if (n.Contains("find")) return SfAiQuickOpKind.Find;
            if (n.Contains("select")) return SfAiQuickOpKind.Select;
            if (n.Contains("log")) return SfAiQuickOpKind.Log;
            if (n.Contains("echo")) return SfAiQuickOpKind.Echo;
            if (n.Contains("structured_output")) return SfAiQuickOpKind.Done;
            return SfAiQuickOpKind.ToolCall;
        }

        public static SfAiQuickOpKind FromChainTag(string tag)
        {
            var t = (tag ?? "").Trim();
            if (t.Contains("开始") || t.Contains("思考")) return SfAiQuickOpKind.Think;
            if (t.Contains("步骤")) return SfAiQuickOpKind.Step;
            if (t.Contains("准备")) return SfAiQuickOpKind.Prepare;
            if (t.Contains("连接") || t.Contains("MCP")) return SfAiQuickOpKind.Connect;
            if (t.Contains("收尾") || t.Contains("完成")) return SfAiQuickOpKind.Done;
            if (t.Contains("错误") || t.Contains("失败")) return SfAiQuickOpKind.Error;
            return SfAiQuickOpKind.Generic;
        }

        public static (Color accent, string title) Describe(SfAiQuickOpKind kind)
        {
            switch (kind)
            {
                case SfAiQuickOpKind.Think:
                    return (new Color(0.55f, 0.65f, 0.95f), "思考");
                case SfAiQuickOpKind.Step:
                    return (new Color(0.60f, 0.70f, 0.90f), "步骤");
                case SfAiQuickOpKind.Prepare:
                    return (new Color(0.55f, 0.80f, 0.75f), "准备");
                case SfAiQuickOpKind.Connect:
                    return (new Color(0.70f, 0.60f, 0.95f), "连接");
                case SfAiQuickOpKind.Hierarchy:
                    return (new Color(0.55f, 0.85f, 0.65f), "Hierarchy");
                case SfAiQuickOpKind.Create:
                    return (new Color(0.45f, 0.85f, 0.55f), "创建物体");
                case SfAiQuickOpKind.Find:
                    return (new Color(0.90f, 0.78f, 0.45f), "查找物体");
                case SfAiQuickOpKind.Select:
                    return (new Color(0.88f, 0.72f, 0.48f), "选中物体");
                case SfAiQuickOpKind.Log:
                    return (new Color(0.75f, 0.75f, 0.80f), "输出日志");
                case SfAiQuickOpKind.Echo:
                    return (new Color(0.70f, 0.72f, 0.85f), "回显");
                case SfAiQuickOpKind.ToolCall:
                    return (new Color(0.55f, 0.85f, 0.55f), "工具调用");
                case SfAiQuickOpKind.ToolResult:
                    return (new Color(0.50f, 0.85f, 0.65f), "工具结果");
                case SfAiQuickOpKind.Done:
                    return (new Color(0.50f, 0.85f, 0.75f), "完成");
                case SfAiQuickOpKind.Error:
                    return (new Color(0.95f, 0.45f, 0.45f), "错误");
                default:
                    return (new Color(0.70f, 0.70f, 0.75f), "操作");
            }
        }

        public static SfAiQuickChainItemParts CreateItem(
            SfAiQuickOpKind kind,
            string title,
            string detail)
        {
            var (accent, defaultTitle) = Describe(kind);
            var displayTitle = string.IsNullOrWhiteSpace(title) ? defaultTitle : title;

            var row = new VisualElement();
            row.AddToClassList("sfai-quick-chain-item");

            var left = new VisualElement();
            left.AddToClassList("sfai-quick-chain-left");

            var circle = new VisualElement();
            circle.AddToClassList("sfai-quick-chain-circle");
            circle.style.borderTopColor = accent;
            circle.style.borderBottomColor = accent;
            circle.style.borderLeftColor = accent;
            circle.style.borderRightColor = accent;
            circle.style.backgroundColor = new Color(accent.r, accent.g, accent.b, 0.12f);

            var icon = SfAiQuickIconLoader.CreateIconImage(kind);
            icon.tintColor = accent;
            circle.Add(icon);
            left.Add(circle);
            row.Add(left);

            var text = new VisualElement();
            text.AddToClassList("sfai-quick-chain-text");

            var titleLabel = new Label(displayTitle);
            titleLabel.AddToClassList("sfai-quick-chain-title");
            text.Add(titleLabel);

            if (!string.IsNullOrWhiteSpace(detail))
            {
                var detailLabel = new Label(SfAiQuickMarkdown.ToRichText(detail));
                detailLabel.enableRichText = true;
                detailLabel.AddToClassList("sfai-quick-chain-detail");
                detailLabel.style.whiteSpace = WhiteSpace.Normal;
                text.Add(detailLabel);
            }

            row.Add(text);

            return new SfAiQuickChainItemParts
            {
                Row = row,
                Circle = circle,
                Text = text
            };
        }
    }
}
