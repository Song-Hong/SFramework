using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.View
{
    /// <summary>
    /// 任务图视图 右键菜单
    /// </summary>
    public partial class SfTaskGraphView
    {
        /// <summary>
        /// 重写此方法来构建上下文菜单
        /// </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 转换鼠标位置到图视图坐标
            var position = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);

            if (evt.target is not GraphView) return;
            //创建右键选择菜单
            foreach (var node in _nodes)
            {
                evt.menu.AppendAction(node.Item1, _ =>
                {
                    CreateNewTaskNode(node.Item1, position, node.Item2);
                });
            }
            // 添加分隔符
            // evt.menu.AppendSeparator();
        }

        /// <summary>
        /// 创建任务节点
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="position">节点位置</param>
        /// <param name="publicFields">公开字段 item1 名称 item2 类型</param>
        private void CreateNewTaskNode(string nodeName, Vector2 position, List<Tuple<string, string>> publicFields)
        {
            // 创建节点
            var node = new Node
            {
                title = nodeName,
                style =
                {
                    width = 180,
                }
            };
            node.SetPosition(new Rect(position, new Vector2(160, 100)));
            
            // 创建一个容器来放置所有字段控件
            var fieldsContainer = new VisualElement()
            {
                name = "fields-container",
                style =
                {
                    // 简单的边距样式，让 UI 看起来更整洁
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5,
                    flexDirection = FlexDirection.Row
                }
            };

            // 核心部分：遍历字段并创建输入控件
            foreach (var publicField in publicFields)
            {
                var fieldName = publicField.Item1;
                var fieldTypeName = publicField.Item2;

                // 尝试获取字段的实际 Type
                var fieldType = GetTypeFromTypeName(fieldTypeName);

                // 如果获取类型失败，或者我们不支持该类型，则跳过
                if (fieldType == null)
                {
                    continue;
                }

                // 创建控件
                var control = CreateControlForType(fieldName, fieldType);

                if (control != null)
                {
                    fieldsContainer.Add(control);
                }
            }

            //创建端口
            var controlFlowType = typeof(object); // 使用 object 允许连接任何类型，或用一个特定的标记类/结构体

            // 输入端口：用于控制流的进入点
            var entryPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, controlFlowType);
            entryPort.portName = "进入";
            node.inputContainer.Add(entryPort);

            // 输出端口：用于控制流的流出点
            var exitPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, controlFlowType);
            exitPort.portName = "退出";
            node.outputContainer.Add(exitPort);
            
            // 刷新节点,并加入节点扩展区域
            node.extensionContainer.Add(fieldsContainer);
            node.RefreshExpandedState();
            node.RefreshPorts();
            AddElement(node);
        }
        
        /// <summary>
        /// 创建根据类型创建对应的输入控件
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldType">字段类型</param>
        /// <returns>返回创建的输入控件</returns>
        private VisualElement CreateControlForType(string fieldName, Type fieldType)
        {
            // 创建一个 Label 来显示字段名称
            var label = new Label(fieldName + ":");
            VisualElement inputField = null;
            
            //筛选类型进行创建控件
            if (fieldType == typeof(int))
            {
                var intField = new IntegerField
                {
                    value = 0
                };
                inputField = intField;
            }
            else if (fieldType == typeof(float) || fieldType == typeof(double))
            {
                var floatField = new FloatField
                {
                    value = 0f
                };
                inputField = floatField;
            }
            else if (fieldType == typeof(string))
            {
                var textField = new TextField
                {
                    value = "",
                };
                inputField = textField;
            }
            else if (fieldType == typeof(bool))
            {
                var toggle = new Toggle
                {
                    value = false,
                    text = fieldName
                };
                return toggle;
            }
            // TODO: 可以在这里添加对 Vector3Field, ObjectField 等的支持


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
    }
}