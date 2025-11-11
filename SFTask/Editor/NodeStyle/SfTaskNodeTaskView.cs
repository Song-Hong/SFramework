using System;
using System.Collections.Generic;
using SFramework.Core.Support;
using SFramework.SFTask.Editor.View;
using SFramework.SFTask.Editor.Window;
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

        /// <summary>
        /// 任务节点的公共字段
        /// </summary>
        public List<Tuple<string, string, string>> PublicFields = new List<Tuple<string, string, string>>();
        
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

        /// <summary>
        /// 初始化任务节点任务视图
        /// </summary>
        /// <param name="title">任务节点标题</param>
        /// <param name="taskType">任务类型</param>
        /// <param name="publicFields">任务节点的公共字段</param>
        public void Init(string title, string taskType, List<Tuple<string, string, string>> publicFields)
        {
            // 设置标题
            TitleLabel.text = title;
            // 设置任务类型
            TaskType = taskType;
            // 保存公共字段
            PublicFields = publicFields;
            // 核心部分：遍历字段并创建输入控件
            
            foreach (var publicField in publicFields)
            {
                var fieldName = publicField.Item1;
                var fieldTypeName = publicField.Item2;
                var fieldValue = publicField.Item3; // 💥 获取字段值

                // 尝试获取字段的实际 Type
                var fieldType = GetTypeFromTypeName(fieldTypeName);

                // 如果获取类型失败，或者我们不支持该类型，则跳过
                if (fieldType == null)
                {
                    continue;
                }

                // 💥 传入字段值
                var control = CreateControlForType(fieldName, fieldType, fieldValue);

                if (control != null)
                {
                    Add(control);
                }
            }

            // 创建删除按钮
            CreateRemoveBtn();
        }

        /// <summary>
        /// 创建根据类型创建对应的输入控件
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldType">字段类型</param>
        /// <param name="fieldValue">字段值</param>
        /// <returns>返回创建的输入控件</returns>
        private VisualElement CreateControlForType(string fieldName, Type fieldType, string fieldValue)
        {
            // 创建一个 Label 来显示字段名称
            var label = new Label(fieldName + ":");
            VisualElement inputField = null;

            //筛选类型进行创建控件
            if (fieldType == typeof(int))
            {
                var intField = new IntegerField
                {
                    // 尝试从字符串解析值
                    value = int.TryParse(fieldValue, out int result) ? result : 0
                };
                inputField = intField;
            }
            else if (fieldType == typeof(float) || fieldType == typeof(double))
            {
                var floatField = new FloatField
                {
                    value = float.TryParse(fieldValue, out float result) ? result : 0f
                };
                inputField = floatField;
            }
            else if (fieldType == typeof(string))
            {
                var textField = new TextField
                {
                    value = fieldValue ?? "", // 使用值
                };
                inputField = textField;
            }
            else if (fieldType == typeof(bool))
            {
                var toggle = new Toggle
                {
                    value = bool.TryParse(fieldValue, out bool result) && result,
                };
                inputField = toggle;
                label.text = fieldName + ":"; // 保持标签
            }
            else if (fieldType == typeof(Vector3))
            {
                // 对于 Vector3，您之前使用了 JsonUtility.ToJson 序列化，这里需要反序列化
                var vector3Value = JsonUtility.FromJson<Vector3>(fieldValue);
                var vector3Field = new Vector3Field
                {
                    value = vector3Value
                };
                inputField = vector3Field;
            }
            else if (fieldType == typeof(Vector2))
            {
                var vector2Value = JsonUtility.FromJson<Vector2>(fieldValue);
                var vector2Field = new Vector2Field
                {
                    value = vector2Value
                };
                inputField = vector2Field;
            }
            else if (fieldType == typeof(Color))
            {
                var colorValue = JsonUtility.FromJson<Color>(fieldValue);
                var colorField = new ColorField
                {
                    value = colorValue
                };
                inputField = colorField;
            }
            else if (fieldType.IsEnum) // 处理所有枚举类型
            {
                var defaultEnumValue = (Enum)Activator.CreateInstance(fieldType);
                var enumField = new EnumField(defaultEnumValue);

                // 尝试从字符串设置枚举值
                if (!string.IsNullOrEmpty(fieldValue))
                {
                    try
                    {
                        var parsedEnum = Enum.Parse(fieldType, fieldValue, true);
                        enumField.value = (Enum)parsedEnum;
                    }
                    catch (ArgumentException)
                    {
                        // 解析失败，使用默认值
                    }
                }

                inputField = enumField;
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                UnityEngine.Object initialValue = null;

                // 尝试将 fieldValue (GUID) 解析为资产
                if (!string.IsNullOrEmpty(fieldValue))
                {
                    // 1. 通过 GUID 获取资产路径
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(fieldValue);
                    if (!string.IsNullOrEmpty(path))
                    {
                        // 2. 从路径加载资产
                        initialValue = UnityEditor.AssetDatabase.LoadAssetAtPath(path, fieldType);
                    }
                }

                var objectField = new ObjectField
                {
                    objectType = fieldType,
                    allowSceneObjects = false, // ‼️【重要】序列化不支持场景对象
                    value = initialValue // ⬅️ 设置加载到的值
                };
                inputField = objectField;
            }

            if (inputField == null) return null;
            // 将 Label 放在输入框前面，形成常见的属性面板布局
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row, // 水平排列
                    alignItems = Align.Center
                }
            };

            label.style.minWidth = 50; // 确保 Label 有足够的空间
            inputField.style.flexGrow = 1; // 确保输入框占据剩余空间
            inputField.style.flexShrink = 1;
            inputField.name = fieldName;

            row.Add(label);
            row.Add(inputField);
            return row;
        }

        /// <summary>
        /// 将字段的字符串类型名称转换为 System.Type
        /// </summary>
        /// <param name="typeName"> 字段的字符串类型名称 </param>
        /// <returns> 返回对应的 System.Type 类型 </returns>
        private Type GetTypeFromTypeName(string typeName)
        {
            var type = Type.GetType($"System.{typeName}", false, true);
            if (type != null) return type;
            type = Type.GetType($"UnityEngine.{typeName}, UnityEngine", false, true);
            return type ?? Type.GetType(typeName, false, true);
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
    }
}