using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Quick.Support
{
    /// <summary>加载 Quick PNG 图标</summary>
    public static class SfAiQuickIconLoader
    {
        public const string IconsFolder = "Assets/SFramework/SAI/Editor/Quick/Icons";

        static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        public static string FileName(SfAiQuickOpKind kind)
        {
            switch (kind)
            {
                case SfAiQuickOpKind.Think: return "think.png";
                case SfAiQuickOpKind.Step: return "step.png";
                case SfAiQuickOpKind.Prepare: return "prepare.png";
                case SfAiQuickOpKind.Connect: return "connect.png";
                case SfAiQuickOpKind.Hierarchy: return "hierarchy.png";
                case SfAiQuickOpKind.Create: return "create.png";
                case SfAiQuickOpKind.Find: return "find.png";
                case SfAiQuickOpKind.Select: return "select.png";
                case SfAiQuickOpKind.Log: return "log.png";
                case SfAiQuickOpKind.Echo: return "echo.png";
                case SfAiQuickOpKind.ToolCall: return "tool.png";
                case SfAiQuickOpKind.ToolResult: return "result.png";
                case SfAiQuickOpKind.Done: return "done.png";
                case SfAiQuickOpKind.Error: return "error.png";
                default: return "generic.png";
            }
        }

        public static string AssetPath(SfAiQuickOpKind kind) =>
            $"{IconsFolder}/{FileName(kind)}";

        public static Image CreateIconImage(SfAiQuickOpKind kind)
        {
            var image = new Image();
            image.AddToClassList("sfai-quick-op-png");
            image.scaleMode = ScaleMode.ScaleToFit;

            var tex = LoadTexture(kind);
            if (tex != null)
                image.image = tex;

            return image;
        }

        public static Texture2D LoadTexture(SfAiQuickOpKind kind)
        {
            var path = AssetPath(kind);
            if (Cache.TryGetValue(path, out var cached) && cached != null)
                return cached;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
                Cache[path] = tex;
            else
                Debug.LogWarning($"[SfAiQuick] 未找到图标: {path}");

            return tex;
        }
    }
}
