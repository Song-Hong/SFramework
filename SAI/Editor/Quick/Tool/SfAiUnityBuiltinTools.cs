using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFramework.SAI.Module.Data;
using SFramework.SAI.Module.Support;
using SFramework.SAI.Module.Tool;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SFramework.SAI.Editor.Quick.Tool
{
    /// <summary>读取当前场景 Hierarchy（含组件摘要）</summary>
    public sealed class SfAiUnityGetHierarchyTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_get_hierarchy",
            Description = "读取当前打开场景的 Hierarchy；每行附带主要组件类型。",
            ParametersJson = "{\"type\":\"object\",\"properties\":{}}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            return SfAiMainThread.RunAsync(() =>
            {
                var scene = SceneManager.GetActiveScene();
                var sb = new StringBuilder();
                sb.AppendLine($"scene={scene.name} path={scene.path}");

                foreach (var root in scene.GetRootGameObjects())
                    AppendNode(sb, root.transform, 0);

                return SfAiToolResult.Success(sb.ToString());
            });
        }

        static void AppendNode(StringBuilder sb, Transform t, int depth)
        {
            sb.Append(' ', depth * 2);
            sb.Append(t.name);
            sb.Append(" [");
            sb.Append(t.gameObject.GetInstanceID());
            sb.Append("] active=");
            sb.Append(t.gameObject.activeSelf);
            sb.Append(" comps=");
            sb.Append(SfAiUnityComponentUtil.ListComponentNames(t.gameObject));
            sb.AppendLine();
            for (var i = 0; i < t.childCount; i++)
                AppendNode(sb, t.GetChild(i), depth + 1);
        }
    }

    /// <summary>创建空 GameObject（可顺带挂组件）</summary>
    public sealed class SfAiUnityCreateGameObjectTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_create_gameobject",
            Description =
                "创建 GameObject。仅空物体不够用；UI 请优先 unity_create_ui。" +
                "可用 components 数组一次挂上组件（如 [\"Canvas\",\"Image\"]）。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{" +
                "\"name\":{\"type\":\"string\"}," +
                "\"parent\":{\"type\":\"string\",\"description\":\"可选父物体名称\"}," +
                "\"components\":{\"type\":\"array\",\"items\":{\"type\":\"string\"},\"description\":\"要挂载的组件类型名\"}" +
                "},\"required\":[\"name\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            return SfAiMainThread.RunAsync(() =>
            {
                var node = SimpleJSON.JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var name = node?["name"]?.Value;
                if (string.IsNullOrWhiteSpace(name))
                    return SfAiToolResult.Fail("缺少 name");

                var go = new GameObject(name.Trim());
                var parentName = node?["parent"]?.Value;
                if (!string.IsNullOrWhiteSpace(parentName))
                {
                    var parent = GameObject.Find(parentName);
                    if (parent != null)
                        go.transform.SetParent(parent.transform, false);
                }

                var added = new List<string>();
                var comps = node?["components"];
                if (comps != null && comps.IsArray)
                {
                    for (var i = 0; i < comps.Count; i++)
                    {
                        var typeName = comps[i]?.Value;
                        if (string.IsNullOrWhiteSpace(typeName)) continue;
                        var result = SfAiUnityComponentUtil.AddComponent(go, typeName);
                        if (!result.ok)
                            return SfAiToolResult.Fail(result.error);
                        added.Add(result.typeName);
                    }
                }

                Undo.RegisterCreatedObjectUndo(go, "SfAi Create GameObject");
                Selection.activeGameObject = go;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                var msg = $"已创建 GameObject name={go.name} id={go.GetInstanceID()}";
                if (added.Count > 0)
                    msg += " components=" + string.Join(",", added);
                else
                    msg += "（仅 Transform；需要 UI/功能请 unity_add_component 或 unity_create_ui）";
                return SfAiToolResult.Success(msg);
            });
        }
    }

    /// <summary>按预设创建带组件的 UI 元素</summary>
    public sealed class SfAiUnityCreateUiTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_create_ui",
            Description =
                "创建带完整组件的 UI。" +
                "kind: canvas|panel|text|button|image|rawimage|toggle|slider|inputfield|scrollview|event_system。" +
                "UI 子节点会自动补 RectTransform；canvas 会带 CanvasScaler+GraphicRaycaster。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{" +
                "\"kind\":{\"type\":\"string\"}," +
                "\"name\":{\"type\":\"string\"}," +
                "\"parent\":{\"type\":\"string\",\"description\":\"父物体名；非 canvas/event_system 建议挂在 Canvas 下\"}," +
                "\"text\":{\"type\":\"string\",\"description\":\"Text/Button 初始文案\"}" +
                "},\"required\":[\"kind\",\"name\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            return SfAiMainThread.RunAsync(() =>
            {
                var node = SimpleJSON.JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var kind = (node?["kind"]?.Value ?? "").Trim().ToLowerInvariant();
                var name = node?["name"]?.Value;
                if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(name))
                    return SfAiToolResult.Fail("缺少 kind 或 name");

                var parentName = node?["parent"]?.Value;
                var text = node?["text"]?.Value ?? "";

                GameObject go;
                string detail;
                try
                {
                    (go, detail) = SfAiUnityUiFactory.Create(kind, name.Trim(), parentName, text);
                }
                catch (Exception e)
                {
                    return SfAiToolResult.Fail(e.Message);
                }

                Undo.RegisterCreatedObjectUndo(go, "SfAi Create UI");
                Selection.activeGameObject = go;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                return SfAiToolResult.Success(
                    $"已创建 UI kind={kind} name={go.name} id={go.GetInstanceID()} comps={SfAiUnityComponentUtil.ListComponentNames(go)} {detail}");
            });
        }
    }

    /// <summary>给已有物体挂组件</summary>
    public sealed class SfAiUnityAddComponentTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_add_component",
            Description =
                "给指定 GameObject 添加组件。component 支持短名：" +
                "Image, Text, TextMeshProUGUI, Button, Canvas, CanvasScaler, GraphicRaycaster, " +
                "RectTransform, LayoutElement, ContentSizeFitter, HorizontalLayoutGroup, " +
                "VerticalLayoutGroup, GridLayoutGroup, ScrollRect, Toggle, Slider, InputField, " +
                "EventSystem, StandaloneInputModule 等。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{" +
                "\"target\":{\"type\":\"string\",\"description\":\"物体名称\"}," +
                "\"component\":{\"type\":\"string\",\"description\":\"组件类型名\"}" +
                "},\"required\":[\"target\",\"component\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            return SfAiMainThread.RunAsync(() =>
            {
                var node = SimpleJSON.JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var target = node?["target"]?.Value;
                var component = node?["component"]?.Value;
                if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(component))
                    return SfAiToolResult.Fail("缺少 target 或 component");

                var go = GameObject.Find(target);
                if (go == null)
                    return SfAiToolResult.Fail($"未找到: {target}");

                var result = SfAiUnityComponentUtil.AddComponent(go, component);
                if (!result.ok)
                    return SfAiToolResult.Fail(result.error);

                Undo.RegisterCompleteObjectUndo(go, "SfAi Add Component");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Selection.activeGameObject = go;

                return SfAiToolResult.Success(
                    $"已添加 {result.typeName} → {go.name}；当前 comps={SfAiUnityComponentUtil.ListComponentNames(go)}");
            });
        }
    }

    /// <summary>设置常用 UI / RectTransform 属性</summary>
    public sealed class SfAiUnitySetUiPropertyTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_set_ui",
            Description =
                "设置 UI 常用属性。property: text|color|raycastTarget|anchorMin|anchorMax|pivot|anchoredPosition|sizeDelta|fontSize。" +
                "value 为字符串；向量用 \"x,y\" 或 \"x,y,z,w\"；颜色用 \"#RRGGBB\" 或 \"r,g,b,a\"。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{" +
                "\"target\":{\"type\":\"string\"}," +
                "\"property\":{\"type\":\"string\"}," +
                "\"value\":{\"type\":\"string\"}" +
                "},\"required\":[\"target\",\"property\",\"value\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            return SfAiMainThread.RunAsync(() =>
            {
                var node = SimpleJSON.JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var target = node?["target"]?.Value;
                var property = (node?["property"]?.Value ?? "").Trim();
                var value = node?["value"]?.Value ?? "";
                if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(property))
                    return SfAiToolResult.Fail("缺少 target 或 property");

                var go = GameObject.Find(target);
                if (go == null)
                    return SfAiToolResult.Fail($"未找到: {target}");

                Undo.RegisterCompleteObjectUndo(go, "SfAi Set UI");
                var err = SfAiUnityUiFactory.SetProperty(go, property, value);
                if (!string.IsNullOrEmpty(err))
                    return SfAiToolResult.Fail(err);

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                return SfAiToolResult.Success($"已设置 {go.name}.{property} = {value}");
            });
        }
    }

    public sealed class SfAiUnityFindGameObjectTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_find_gameobject",
            Description = "按名称查找 GameObject，返回路径与组件列表。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            return SfAiMainThread.RunAsync(() =>
            {
                var node = SimpleJSON.JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var name = node?["name"]?.Value;
                if (string.IsNullOrWhiteSpace(name))
                    return SfAiToolResult.Fail("缺少 name");

                var go = GameObject.Find(name);
                if (go == null)
                    return SfAiToolResult.Fail($"未找到: {name}");

                var sb = new StringBuilder();
                sb.AppendLine($"name={go.name}");
                sb.AppendLine($"id={go.GetInstanceID()}");
                sb.AppendLine($"active={go.activeSelf}");
                sb.AppendLine($"path={SfAiUnityComponentUtil.GetPath(go.transform)}");
                sb.AppendLine($"components={SfAiUnityComponentUtil.ListComponentNames(go)}");
                return SfAiToolResult.Success(sb.ToString());
            });
        }
    }

    public sealed class SfAiUnitySelectGameObjectTool : ISfAiTool
    {
        public SfAiToolSpec Spec { get; } = new SfAiToolSpec
        {
            Name = "unity_select_gameobject",
            Description = "在 Hierarchy 中选中指定名称的 GameObject。",
            ParametersJson =
                "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}"
        };

        public Task<SfAiToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            return SfAiMainThread.RunAsync(() =>
            {
                var node = SimpleJSON.JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
                var name = node?["name"]?.Value;
                if (string.IsNullOrWhiteSpace(name))
                    return SfAiToolResult.Fail("缺少 name");

                var go = GameObject.Find(name);
                if (go == null)
                    return SfAiToolResult.Fail($"未找到: {name}");

                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
                return SfAiToolResult.Success($"已选中 {go.name}");
            });
        }
    }

    public static class SfAiUnityBuiltinTools
    {
        public static void RegisterAll(SfAiToolRegistry registry)
        {
            if (registry == null) return;
            registry.Register(new SfAiUnityGetHierarchyTool());
            registry.Register(new SfAiUnityCreateGameObjectTool());
            registry.Register(new SfAiUnityCreateUiTool());
            registry.Register(new SfAiUnityAddComponentTool());
            registry.Register(new SfAiUnitySetUiPropertyTool());
            registry.Register(new SfAiUnityFindGameObjectTool());
            registry.Register(new SfAiUnitySelectGameObjectTool());
        }
    }

    internal static class SfAiUnityComponentUtil
    {
        static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "recttransform", "UnityEngine.RectTransform" },
            { "transform", "UnityEngine.Transform" },
            { "canvas", "UnityEngine.Canvas" },
            { "canvasscaler", "UnityEngine.UI.CanvasScaler" },
            { "graphicraycaster", "UnityEngine.UI.GraphicRaycaster" },
            { "image", "UnityEngine.UI.Image" },
            { "rawimage", "UnityEngine.UI.RawImage" },
            { "text", "UnityEngine.UI.Text" },
            { "button", "UnityEngine.UI.Button" },
            { "toggle", "UnityEngine.UI.Toggle" },
            { "slider", "UnityEngine.UI.Slider" },
            { "scrollbar", "UnityEngine.UI.Scrollbar" },
            { "scrollrect", "UnityEngine.UI.ScrollRect" },
            { "inputfield", "UnityEngine.UI.InputField" },
            { "dropdown", "UnityEngine.UI.Dropdown" },
            { "mask", "UnityEngine.UI.Mask" },
            { "rectmask2d", "UnityEngine.UI.RectMask2D" },
            { "layoutelement", "UnityEngine.UI.LayoutElement" },
            { "contentsizefitter", "UnityEngine.UI.ContentSizeFitter" },
            { "aspectratiofitter", "UnityEngine.UI.AspectRatioFitter" },
            { "horizontallayoutgroup", "UnityEngine.UI.HorizontalLayoutGroup" },
            { "verticallayoutgroup", "UnityEngine.UI.VerticalLayoutGroup" },
            { "gridlayoutgroup", "UnityEngine.UI.GridLayoutGroup" },
            { "outline", "UnityEngine.UI.Outline" },
            { "shadow", "UnityEngine.UI.Shadow" },
            { "canvasgroup", "UnityEngine.CanvasGroup" },
            { "canvasrenderer", "UnityEngine.CanvasRenderer" },
            { "eventsyste", "UnityEngine.EventSystems.EventSystem" },
            { "eventsystem", "UnityEngine.EventSystems.EventSystem" },
            { "standaloneinputmodule", "UnityEngine.EventSystems.StandaloneInputModule" },
            { "cameramodule", "UnityEngine.EventSystems.PhysicsRaycaster" },
            { "audiolistener", "UnityEngine.AudioListener" },
            { "audiosource", "UnityEngine.AudioSource" },
            { "camera", "UnityEngine.Camera" },
            { "light", "UnityEngine.Light" },
            { "meshfilter", "UnityEngine.MeshFilter" },
            { "meshrenderer", "UnityEngine.MeshRenderer" },
            { "spriterenderer", "UnityEngine.SpriteRenderer" },
            { "boxcollider", "UnityEngine.BoxCollider" },
            { "boxcollider2d", "UnityEngine.BoxCollider2D" },
            { "rigidbody", "UnityEngine.Rigidbody" },
            { "rigidbody2d", "UnityEngine.Rigidbody2D" },
            { "textmeshprougui", "TMPro.TextMeshProUGUI" },
            { "tmp", "TMPro.TextMeshProUGUI" },
            { "tmp_text", "TMPro.TextMeshProUGUI" },
        };

        public static string ListComponentNames(GameObject go)
        {
            if (go == null) return "";
            var comps = go.GetComponents<Component>();
            var sb = new StringBuilder();
            for (var i = 0; i < comps.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(comps[i] != null ? comps[i].GetType().Name : "Missing");
            }

            return sb.ToString();
        }

        public static string GetPath(Transform t)
        {
            if (t.parent == null) return t.name;
            return GetPath(t.parent) + "/" + t.name;
        }

        public static (bool ok, string typeName, string error) AddComponent(GameObject go, string typeName)
        {
            if (go == null) return (false, "", "GameObject 为空");
            var type = ResolveType(typeName);
            if (type == null)
                return (false, "", $"未识别组件类型: {typeName}");

            if (!typeof(Component).IsAssignableFrom(type))
                return (false, "", $"{type.FullName} 不是 Component");

            if (type == typeof(Transform) || type == typeof(RectTransform))
            {
                // RectTransform：若当前是普通 Transform，需升级
                if (type == typeof(RectTransform) && !(go.transform is RectTransform))
                {
                    var rt = go.AddComponent<RectTransform>();
                    return (true, nameof(RectTransform), "");
                }

                return (true, type.Name, "");
            }

            if (go.GetComponent(type) != null)
                return (true, type.Name, "");

            try
            {
                var c = Undo.AddComponent(go, type);
                if (c == null)
                    return (false, "", $"AddComponent 失败: {type.Name}");
                return (true, type.Name, "");
            }
            catch (Exception e)
            {
                return (false, "", e.Message);
            }
        }

        public static Type ResolveType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return null;
            var key = typeName.Trim();

            if (Aliases.TryGetValue(key, out var full))
            {
                var aliased = FindType(full) ?? FindType(key);
                if (aliased != null) return aliased;
            }

            return FindType(key)
                   ?? FindType("UnityEngine." + key)
                   ?? FindType("UnityEngine.UI." + key)
                   ?? FindType("UnityEngine.EventSystems." + key)
                   ?? FindType("TMPro." + key);
        }

        static Type FindType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return null;
            var direct = Type.GetType(fullName);
            if (direct != null) return direct;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(fullName, false);
                    if (t != null) return t;
                    // 短名扫描（仅限常见程序集，避免太慢）
                    var an = asm.GetName().Name ?? "";
                    if (an.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                        an.StartsWith("Unity.", StringComparison.Ordinal) ||
                        an.IndexOf("TextMeshPro", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foreach (var type in asm.GetTypes())
                        {
                            if (type.Name.Equals(fullName, StringComparison.OrdinalIgnoreCase) &&
                                typeof(Component).IsAssignableFrom(type))
                                return type;
                        }
                    }
                }
                catch
                {
                    // dynamic assemblies
                }
            }

            return null;
        }
    }

    internal static class SfAiUnityUiFactory
    {
        public static (GameObject go, string detail) Create(string kind, string name, string parentName, string text)
        {
            switch (kind)
            {
                case "canvas":
                {
                    var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                        typeof(GraphicRaycaster));
                    var canvas = go.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    var scaler = go.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    return (go, "renderMode=ScreenSpaceOverlay");
                }
                case "event_system":
                {
                    var existingEs = UnityEngine.Object.FindObjectOfType<EventSystem>();
                    if (existingEs != null)
                        return (existingEs.gameObject, "已存在 EventSystem，复用");

                    var go = new GameObject(name, typeof(EventSystem), typeof(StandaloneInputModule));
                    return (go, "已创建 EventSystem");
                }
                case "panel":
                {
                    var go = CreateUiChild(name, parentName, typeof(Image));
                    var img = go.GetComponent<Image>();
                    img.color = new Color(1f, 1f, 1f, 0.4f);
                    StretchFull(go.GetComponent<RectTransform>());
                    return (go, "Image panel");
                }
                case "image":
                {
                    var go = CreateUiChild(name, parentName, typeof(Image));
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                    return (go, "Image");
                }
                case "rawimage":
                {
                    var go = CreateUiChild(name, parentName, typeof(RawImage));
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                    return (go, "RawImage");
                }
                case "text":
                {
                    var go = CreateUiChild(name, parentName, typeof(Text));
                    var t = go.GetComponent<Text>();
                    t.text = string.IsNullOrEmpty(text) ? "Text" : text;
                    t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    t.fontSize = 24;
                    t.color = Color.black;
                    t.alignment = TextAnchor.MiddleCenter;
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
                    return (go, "Text");
                }
                case "button":
                {
                    var go = CreateUiChild(name, parentName, typeof(Image), typeof(Button));
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
                    var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
                    label.transform.SetParent(go.transform, false);
                    StretchFull(label.GetComponent<RectTransform>());
                    var t = label.GetComponent<Text>();
                    t.text = string.IsNullOrEmpty(text) ? "Button" : text;
                    t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    t.fontSize = 20;
                    t.color = Color.black;
                    t.alignment = TextAnchor.MiddleCenter;
                    return (go, "Button+Label");
                }
                case "toggle":
                {
                    var go = CreateUiChild(name, parentName, typeof(Toggle));
                    return (go, "Toggle（可再补 Background/Checkmark 子节点）");
                }
                case "slider":
                {
                    var go = CreateUiChild(name, parentName, typeof(Slider));
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 20);
                    return (go, "Slider");
                }
                case "inputfield":
                {
                    var go = CreateUiChild(name, parentName, typeof(Image), typeof(InputField));
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
                    return (go, "InputField");
                }
                case "scrollview":
                {
                    var go = CreateUiChild(name, parentName, typeof(Image), typeof(ScrollRect));
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 200);
                    return (go, "ScrollRect");
                }
                default:
                    throw new Exception("未知 UI kind: " + kind +
                                        "（支持 canvas/panel/text/button/image/rawimage/toggle/slider/inputfield/scrollview/event_system）");
            }
        }

        public static string SetProperty(GameObject go, string property, string value)
        {
            var p = property.Trim().ToLowerInvariant();
            switch (p)
            {
                case "text":
                {
                    var uiText = go.GetComponent<Text>();
                    if (uiText != null)
                    {
                        uiText.text = value;
                        return "";
                    }

                    var input = go.GetComponent<InputField>();
                    if (input != null)
                    {
                        input.text = value;
                        return "";
                    }

                    return "目标没有 Text/InputField";
                }
                case "fontsize":
                {
                    var uiText = go.GetComponent<Text>();
                    if (uiText == null) return "目标没有 Text";
                    if (!int.TryParse(value, out var size)) return "fontSize 需要整数";
                    uiText.fontSize = size;
                    return "";
                }
                case "color":
                {
                    if (!TryParseColor(value, out var color)) return "无法解析颜色";
                    var img = go.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = color;
                        return "";
                    }

                    var uiText = go.GetComponent<Text>();
                    if (uiText != null)
                    {
                        uiText.color = color;
                        return "";
                    }

                    var raw = go.GetComponent<RawImage>();
                    if (raw != null)
                    {
                        raw.color = color;
                        return "";
                    }

                    return "目标没有 Image/Text/RawImage";
                }
                case "raycasttarget":
                {
                    var g = go.GetComponent<Graphic>();
                    if (g == null) return "目标没有 Graphic";
                    g.raycastTarget = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
                    return "";
                }
                case "anchormin":
                case "anchormax":
                case "pivot":
                case "anchoredposition":
                case "sizedelta":
                {
                    var rt = go.transform as RectTransform;
                    if (rt == null) return "目标没有 RectTransform（请先挂到 Canvas 下或 Add RectTransform）";
                    if (!TryParseVector2(value, out var v)) return "向量格式应为 x,y";
                    if (p == "anchormin") rt.anchorMin = v;
                    else if (p == "anchormax") rt.anchorMax = v;
                    else if (p == "pivot") rt.pivot = v;
                    else if (p == "anchoredposition") rt.anchoredPosition = v;
                    else rt.sizeDelta = v;
                    return "";
                }
                default:
                    return "不支持的 property: " + property;
            }
        }

        static GameObject CreateUiChild(string name, string parentName, params Type[] components)
        {
            var types = new List<Type> { typeof(RectTransform) };
            foreach (var c in components)
                if (c != null && !types.Contains(c))
                    types.Add(c);

            var go = new GameObject(name, types.ToArray());
            if (!string.IsNullOrWhiteSpace(parentName))
            {
                var parent = GameObject.Find(parentName);
                if (parent != null)
                    go.transform.SetParent(parent.transform, false);
            }

            return go;
        }

        static void StretchFull(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static bool TryParseVector2(string value, out Vector2 v)
        {
            v = Vector2.zero;
            if (string.IsNullOrWhiteSpace(value)) return false;
            var parts = value.Split(',');
            if (parts.Length < 2) return false;
            if (!float.TryParse(parts[0].Trim(), out var x)) return false;
            if (!float.TryParse(parts[1].Trim(), out var y)) return false;
            v = new Vector2(x, y);
            return true;
        }

        static bool TryParseColor(string value, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();
            if (value.StartsWith("#", StringComparison.Ordinal) && ColorUtility.TryParseHtmlString(value, out color))
                return true;

            var parts = value.Split(',');
            if (parts.Length < 3) return false;
            if (!float.TryParse(parts[0].Trim(), out var r)) return false;
            if (!float.TryParse(parts[1].Trim(), out var g)) return false;
            if (!float.TryParse(parts[2].Trim(), out var b)) return false;
            var a = 1f;
            if (parts.Length >= 4)
                float.TryParse(parts[3].Trim(), out a);
            // 支持 0-255
            if (r > 1f || g > 1f || b > 1f || a > 1f)
            {
                r /= 255f;
                g /= 255f;
                b /= 255f;
                if (a > 1f) a /= 255f;
            }

            color = new Color(r, g, b, a);
            return true;
        }
    }
}
