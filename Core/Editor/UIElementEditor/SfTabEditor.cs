using SFramework.Core.Support;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.Core.Editor.UIElementEditor
{
    /// <summary>
    /// 切换Tab
    /// </summary>
    public class SfTabEditor:VisualElement
    {
        // 用于在 UXML 中定义时识别的类名
        public new class UxmlFactory : UxmlFactory<SfTabEditor, UxmlTraits> {}

        // 允许在 UXML 中设置属性，例如 name, tab-index 等
        public new class UxmlTraits : VisualElement.UxmlTraits {}
        
        public SfTabEditor()
        {

        }
    }
}