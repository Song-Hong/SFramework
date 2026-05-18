using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.Core.Editor.UIElementEditor
{
    public class SfTitleEditor: VisualElement
    {
        // 用于在 UXML 中定义时识别的类名
        public new class UxmlFactory : UxmlFactory<SfTitleEditor, UxmlTraits> {}

        // 允许在 UXML 中设置属性，例如 name, tab-index 等
        public new class UxmlTraits : VisualElement.UxmlTraits {}
        
        #region 构造函数
        public SfTitleEditor() : this("SFramework") { }
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SfTitleEditor(string tex)
        {
            // 添加标题
            var title = new Label(tex)
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            Add(title);
        }
        #endregion
    }
}