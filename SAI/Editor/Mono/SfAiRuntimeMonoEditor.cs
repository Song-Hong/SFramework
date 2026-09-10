using SFramework.Core.Editor.UIElementEditor;
using SFramework.SAI.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Mono
{
    [CustomEditor(typeof(SfAiRuntimeMono), true)]
    public class SfAiRuntimeMonoEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new SfTitleEditor("SFramework 运行时 AI（开放接口）"));

            root.Add(new HelpBox(
                "继承 SfAiRuntimeMono，在 RegisterTools 中注册项目工具后二次开发。",
                HelpBoxMessageType.Info));

            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    var field = new PropertyField(iterator.Copy());
                    field.Bind(serializedObject);
                    root.Add(field);
                } while (iterator.NextVisible(false));
            }

            return root;
        }
    }
}
