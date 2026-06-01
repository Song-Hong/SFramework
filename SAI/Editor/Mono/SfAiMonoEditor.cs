using SFramework.Core.Editor.UIElementEditor;
using SFramework.SAI.Mono;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Mono
{
    [CustomEditor(typeof(SfAiMono))]
    public class SfAiMonoEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new SfTitleEditor("SFramework AI 模块"));

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
