using SFramework.Core.Support;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.Core.Editor.UIElementEditor
{
    /// <summary>
    /// 输入框
    /// </summary>
    public class SfInputEditor:VisualElement
    {
        // 用于在 UXML 中定义时识别的类名
        public new class UxmlFactory : UxmlFactory<SfInputEditor, UxmlTraits> {}

        // 允许在 UXML 中设置属性，例如 name, tab-index 等
        public new class UxmlTraits : VisualElement.UxmlTraits {}

        /// <summary>
        /// 输入框
        /// </summary>
        public SfInputEditor()
        {
            
        }
        
        /// <summary>
        /// 输入框
        /// </summary>
        public SfInputEditor(string titleValue,object bindValue)
        {
            var inactiveColor = SfColor.HexToColor("#242424"); // 非激活状态颜色 (同背景色)
            var activeColor = SfColor.HexToColor("#3C3C3C");   // 激活状态颜色 (稍亮)
            var inactiveTextColor = SfColor.HexToColor("#6D6D6D"); // 非激活状态文本颜色
            var activeTextColor = Color.white;   // 激活状态文本颜色
            
            //IP地址
            var container = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                },
            };
            // 添加IP地址容器到根元素
            Add(container);
            
            var title = new Label
            {
                style =
                {
                    fontSize = 14,
                    height = 26,
                    color = Color.white,
                    marginLeft = 5,
                    marginTop = 10,
                    alignSelf = Align.Center,
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
                text = "IP地址:",
            };
            // 添加IP地址标签到根元素
            container.Add(title);
            
            var selects = new TextField
            {
                style =
                {
                    backgroundColor = inactiveColor, // 默认非激活
                    color = inactiveTextColor,
                    borderLeftWidth = 0,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                    height = 26,
                    marginLeft = 48,
                    marginTop = 8,
                    minWidth = 118,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                },
                value = bindValue as string,
            };

            selects.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                // // 1. 通知序列化系统开始修改
                // serializedObject.Update(); 
                //
                // // 2. 将新值赋给 SerializedProperty
                // serverMono.serverData.ip = evt.newValue;
                //
                // // 3. 将更改应用到组件并注册撤销
                // serializedObject.ApplyModifiedProperties(); 
            });
            // 添加IP地址输入框到根元素
            container.Add(selects);
        }
    }
}