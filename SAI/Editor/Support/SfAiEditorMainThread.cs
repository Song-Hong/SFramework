using System;
using UnityEditor;

namespace SFramework.SAI.Editor.Support
{
    /// <summary>
    /// 将 UI 操作派发到 Unity 编辑器主线程（HTTP 流式回调在后台线程）
    /// </summary>
    static class SfAiEditorMainThread
    {
        public static void Post(Action action)
        {
            if (action == null) return;
            EditorApplication.delayCall += () => action();
        }
    }
}
