using System;
using System.Collections.Generic;
using SFramework.Core.Support;
using SFramework.SFNet.Mono;
// 假设 SFramework.Core.Support 和 SfColor 存在于您的项目中
// using SFramework.Core.Support; 
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.Core.SfUIElementExtends
{
    /// <summary>
    /// Tab切换UI
    /// </summary>
    public class SfTab : VisualElement
    {
        // 用于在 UXML 中定义时识别的类名
        public new class UxmlFactory : UxmlFactory<SfTab, UxmlTraits> {}

        // 允许在 UXML 中设置属性，例如 name, tab-index 等
        public new class UxmlTraits : VisualElement.UxmlTraits {}
        
        /// <summary>
        /// 选项列表
        /// </summary>
        private readonly List<string> ChoiceList = new List<string>();

        /// <summary>
        /// 选项切换事件
        /// </summary>
        public event Action<string> OnChoiceChanged;

        /// <summary>
        /// 标题标签
        /// </summary>
        public Label TitleLabel;

        /// <summary>
        /// 选项切换背景
        /// </summary>
        public VisualElement ChooseBackground;
        
        /// <summary>
        /// 非激活状态颜色 (同背景色)
        /// </summary>
        public Color InactiveColor = SfColor.HexToColor("#242424"); // 非激活状态颜色 (同背景色)
        /// <summary>
        /// 激活状态颜色 (稍亮)
        /// </summary>
        public Color ActiveColor = SfColor.HexToColor("#3C3C3C");   // 激活状态颜色 (稍亮)
        /// <summary>
        /// 非激活状态文本颜色
        /// </summary>
        public Color InactiveTextColor = SfColor.HexToColor("#6D6D6D"); // 非激活状态文本颜色
        /// <summary>
        /// 激活状态文本颜色
        /// </summary>
        public Color ActiveTextColor = Color.white;   // 激活状态文本颜色
        
        /// <summary>
        /// 当前选中的选项按钮
        /// </summary>
        public Button nowChooseBtn;
        
        #region 构造函数
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SfTab()
        {
            //设置默认样式
            style.height = 30;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            
            // 添加标题
            TitleLabel = new Label("标题")
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
                }
            };
            Add(TitleLabel);
            
            
            // 选择背景
            ChooseBackground = new VisualElement
            {
                style =
                {
                    backgroundColor = SfColor.HexToColor("#242424"),
                    marginLeft = 20,
                    marginTop = 10,
                    paddingLeft = 2,
                    paddingRight = 2,
                    paddingTop = 2,
                    paddingBottom = 2,
                    flexDirection = FlexDirection.Row,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                    height = 26,
                }
            };
            Add(ChooseBackground);
            
            //添加选项按钮
            foreach (var se in ChoiceList)
            {
                ChooseBackground.Add(CreateChoiceBtn(se));
            }
        }
        #endregion

        #region 元素
        /// <summary>
        /// 创建选择按钮
        /// </summary>
        private Button CreateChoiceBtn(string btnText)
        {
            var btn =new Button
            {
                style =
                {
                    // backgroundColor = SfColor.HexToColor("#242424"), // 改为使用变量
                    backgroundColor = InactiveColor, // 默认激活服务器
                    color = InactiveTextColor,
                    borderLeftWidth = 0,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                },
                text = btnText
            };
            btn.clicked += () =>
            {
                // 先将当前选中按钮的颜色重置
                if (nowChooseBtn != null)
                {
                    nowChooseBtn.style.backgroundColor = InactiveColor;
                    nowChooseBtn.style.color = InactiveTextColor;
                }
                
                // 设置当前选中按钮为点击按钮
                nowChooseBtn = btn;
                // 设置按钮为 "激活"
                btn.style.backgroundColor = ActiveColor;
                btn.style.color = ActiveTextColor;
                OnChoiceChanged?.Invoke(btnText);
            };
            return btn;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 添加选项
        /// </summary>
        /// <param name="choices">选项文本数组</param>
        public void AddChoice(params string[] choices)
        {
            foreach (var choice in choices)
            {
                AddChoice(choice);
            }
        }

        /// <summary>
        /// 添加选项
        /// </summary>
        /// <param name="choice"></param>
        public void AddChoice(string choice)
        {
            ChoiceList.Add(choice);
            var newChoiceBtn = CreateChoiceBtn(choice);
            ChooseBackground.Add(newChoiceBtn);
            if(nowChooseBtn == null)
            {
                // 设置当前选中按钮为点击按钮
                nowChooseBtn = newChoiceBtn;
                // 设置按钮为 "激活"
                newChoiceBtn.style.backgroundColor = ActiveColor;
                newChoiceBtn.style.color = ActiveTextColor;
                
                OnChoiceChanged?.Invoke(choice);
            }
        }
        
        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="title">标题文本</param>
        public void SetTitle(string title)
        {
            TitleLabel.text = title;
        }

        /// <summary>
        /// 选择选项
        /// </summary>
        /// <param name="btnValue">选项按钮文本</param>
        public void Select(string btnValue)
        {
            foreach (var btn in ChooseBackground.Children())
            {
                if (btn is Button choiceBtn && choiceBtn.text == btnValue)
                {
                    // 先将当前选中按钮的颜色重置
                    if (nowChooseBtn != null)
                    {
                        nowChooseBtn.style.backgroundColor = InactiveColor;
                        nowChooseBtn.style.color = InactiveTextColor;
                    }
                    
                    // 设置当前选中按钮为点击按钮
                    nowChooseBtn = choiceBtn;
                    // 设置按钮为 "激活"
                    choiceBtn.style.backgroundColor = ActiveColor;
                    choiceBtn.style.color = ActiveTextColor;
                    
                    OnChoiceChanged?.Invoke(btnValue);
                    break;
                }
            }
        }
        #endregion
    }
}