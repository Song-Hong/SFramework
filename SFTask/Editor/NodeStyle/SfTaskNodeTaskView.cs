using System;
using System.Collections.Generic;
using SFramework.Core.Support;
using SFramework.SFTask.Editor.View;
using SFramework.SFTask.Editor.Window;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.NodeStyle
{
    /// <summary>
    /// 任务节点任务视图
    /// </summary>
    public class SfTaskNodeTaskView : VisualElement
    {
        // 用于在 UXML 中定义时识别的类名
        public new class UxmlFactory : UxmlFactory<SfTaskNodeTaskView, UxmlTraits>
        {
        }

        // 允许在 UXML 中设置属性，例如 name, tab-index 等
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            
        }

        /// <summary>
        /// 标题
        /// </summary>
        public Label TitleLabel;

        /// <summary>
        /// 任务类型
        /// </summary>
        public string TaskType;

        public SerializedObject SerializedTarget { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SfTaskNodeTaskView()
        {
            //初始化样式
            name = "fields-container";
            style.paddingTop = 5;
            style.paddingBottom = 5;
            style.paddingLeft = 5;
            style.paddingRight = 5;
            style.marginTop = 2;
            style.marginBottom = 2;
            style.marginLeft = 5;
            style.marginRight = 5;
            style.flexDirection = FlexDirection.Column;
            style.flexGrow = 1;
            style.flexBasis = StyleKeyword.Auto;
            style.backgroundColor = SfColor.HexToColor("#3A3A3A");
            style.borderBottomLeftRadius = 5;
            style.borderBottomRightRadius = 5;
            style.borderTopLeftRadius = 5;
            style.borderTopRightRadius = 5;

            // 标题样式
            TitleLabel = new Label
            {
                text = "新任务",
                style =
                {
                    fontSize = 12,
                    color = Color.white,
                }
            };
            Add(TitleLabel);
        }

        public void Init(SerializedObject taskSerializedObject)
        {
            SerializedTarget = taskSerializedObject;
            taskSerializedObject.Update();
            var iterator = taskSerializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.propertyPath == "m_Script") continue;
                    var field = new PropertyField(iterator.Copy(), iterator.displayName);
                    field.style.width = new StyleLength(Length.Percent(100));
                    field.style.flexGrow = 1;
                    field.style.marginBottom = 4;
                    
                    // 定义字段样式
                    field.RegisterCallback<GeometryChangedEvent>(_ =>
                    {
                        // 字段容器
                        var fieldContainer = field.Q<VisualElement>(name: null, className: "unity-property-field");
                        if (fieldContainer != null)
                        {
                            fieldContainer.style.marginTop = 2;
                            fieldContainer.style.marginBottom = 2;
                        }
                        // 字段注释容器
                        var labelContainer = field.Q<VisualElement>(name: null, className: "unity-decorator-drawers-container");
                        if (labelContainer != null)
                        {
                            labelContainer.style.visibility = Visibility.Hidden;
                            labelContainer.style.height = 16;
                            labelContainer.style.height = 0;
                        }
                        // 字段注释（Header），用于将其文本赋给字段标签
                        var commentEl = field.Q<Label>(name: null, className: "unity-header-drawer__label");
                        if (commentEl != null)
                        {
                            commentEl.style.visibility = Visibility.Hidden;
                        }
                        // 字段名
                        var labelEl = field.Q<Label>(name: null, className: "unity-property-field__label");
                        if (labelEl != null)
                        {
                            labelEl.style.color = Color.white;
                            labelEl.style.minWidth = 0;
                            labelEl.style.marginRight = 8;
                            labelEl.style.marginTop = 1;
                            // 若存在 Header 文本，则赋值给字段标签
                            if (commentEl != null && !string.IsNullOrEmpty(commentEl.text))
                            {
                                labelEl.text = commentEl.text;
                            }
                        }
                        //字段值
                        var inputEl = field.Q<VisualElement>(name: null, className: "unity-base-field__input");
                        if (inputEl != null)
                        {
                            inputEl.style.borderBottomWidth = 1;
                            inputEl.style.borderTopWidth = 1;
                            inputEl.style.borderLeftWidth = 1;
                            inputEl.style.borderRightWidth = 1;
                            inputEl.style.borderBottomLeftRadius = 3;
                            inputEl.style.borderBottomRightRadius = 3;
                            inputEl.style.borderTopLeftRadius = 3;
                            inputEl.style.borderTopRightRadius = 3;
                            inputEl.style.marginTop = 2;
                        }
                    });
                    Add(field);
                } while (iterator.NextVisible(false));
            }
            this.Bind(taskSerializedObject);
            
            CreateRemoveBtn();
        }

        /// <summary>
        /// 创建删除按钮
        /// </summary>
        private void CreateRemoveBtn()
        {
            var removeBtn = new Button
            {
                text = "",
                style =
                {
                    fontSize = 12,
                    color = Color.white,
                    position = Position.Absolute,
                    top = 2,
                    right = 2,
                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    marginTop = 2,
                    marginRight = 2,
                    marginBottom = 0,
                    marginLeft = 0,
                    paddingTop = 0,
                    paddingRight = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    backgroundImage = SfTaskWindow.CloseIcon
                }
            };
            removeBtn.style.width = 10;
            removeBtn.style.height = 10;
            removeBtn.clicked += RemoveTaskNode;
            Add(removeBtn);
        }
        
        /// <summary>
        /// 删除任务节点
        /// </summary>
        private void RemoveTaskNode()
        {
            var sfTaskNodePointEditor = GetFirstAncestorOfType<SfTaskNodePointEditor>();
            if (sfTaskNodePointEditor != null)
                sfTaskNodePointEditor.RemoveTaskNode(this);
        }

        private static Type ResolveType(string fullName)
        {
            var t = Type.GetType(fullName);
            if (t != null) return t;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetType(fullName, false, true);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }
    }
}
