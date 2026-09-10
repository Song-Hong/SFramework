using System;
using SFramework.SAI.Module.Support;
using UnityEditor;

namespace SFramework.SAI.Editor.Quick.Tool
{
    [InitializeOnLoad]
    static class SfAiEditorMainThreadInstaller
    {
        static SfAiEditorMainThreadInstaller()
        {
            SfAiMainThread.SetEditorPoster(action =>
            {
                EditorApplication.delayCall += () => action?.Invoke();
                return true;
            });
        }
    }
}
