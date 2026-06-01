using System;
using System.Text;
using SFramework.SFOffice.Module;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFOffice.Editor.Window
{
    /// <summary>
    /// Office模块视图窗口 Excel模块
    /// </summary>
    public partial class SfOfficeViewWindow:EditorWindow
    {
        private VisualElement _addColumnButton;
        private VisualElement _addRowButton;
        
        /// <summary>
        /// 初始化Excel模块
        /// </summary>
        public void InitExcel()
        {
            // 读取Excel文件
            var myExcelData = SfExcel.ReadToExcelData(_filePath);
            if (myExcelData == null) return;
            // 遍历Excel文件中的所有工作表
            foreach (var sheet in myExcelData.Sheets())
            {
                // 创建标题
                var titleLine = CreateLine();
                CreateLineTitle("", titleLine);
                for (var j = 0; j < sheet.ColCount; j++)
                {
                    CreateLineTitle(NumberToExcelColumn(j+1),titleLine);
                }
                // 创建添加列按钮
                _addColumnButton = CreateLineTitle("+", titleLine);
                
                // 创建行
                for (var i = 0; i < sheet.RowCount; i++)
                {
                    // 创建行
                    var line = CreateLine();
                    CreateLineTitle(i+1+"", line);
                    for (var k = 0; k < sheet.ColCount; k++)
                    {
                        CreateLineItem(sheet.Get(i, k), line);
                    }
                }
                
                break;
            }
            
            // 添加创建按钮
            var rowLine = CreateLine();
            _addRowButton = CreateLineTitle("+", rowLine);
        }

        /// <summary>
        /// 创建Excel行
        /// </summary>
        public VisualElement CreateLine()
        {
            // 创建行元素
            var line = new VisualElement();
            line.AddToClassList("excel_line");
            _content.Add(line);
            return line;
        }
        
        /// <summary>
        /// 创建Excel行标题
        /// </summary>
        /// <param name="content">行标题内容</param>
        /// <param name="line">行元素</param>
        /// <returns>行标题元素</returns>
        public VisualElement CreateLineTitle(string content,VisualElement line)
        {
            // 创建行标题元素
            var lineTitle = new Button()
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    alignSelf = Align.Center,
                },
                text = content,
            };
            lineTitle.AddToClassList("excel_title");
            line.Add(lineTitle);
            return lineTitle;
        }
        
        /// <summary>
        /// 创建Excel行项
        /// </summary>
        /// <param name="content">行项内容</param>
        /// <param name="line">行元素</param>
        /// <returns>行项元素</returns>
        public VisualElement CreateLineItem(string content,VisualElement line)
        {
            // 创建行项元素
            var lineItem = new TextField()
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    alignSelf = Align.Center,
                },
                value = content,
            };
            lineItem.Q<VisualElement>(className:"unity-text-element").style.unityTextAlign = TextAnchor.MiddleCenter;
            lineItem.AddToClassList("excel_item");
            line.Add(lineItem);
            return lineItem;
        }
        
        /// <summary>
        /// 将正整数转换为 Excel 列名（A, B, C, ..., Z, AA, ...）
        /// </summary>
        /// <param name="n">要转换的正整数 (从 1 开始)</param>
        /// <returns>对应的列名字母串</returns>
        public string NumberToExcelColumn(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "输入数字必须是大于零的正整数。");
            }

            // 使用 StringBuilder 提高字符串拼接效率
            var columnName = new StringBuilder();

            while (n > 0)
            {
                // 关键步骤：先将数字 n 减 1
                // 1. 将 1-based 索引转换为 0-based 索引 (1 -> 0, 26 -> 25)
                n--; 
        
                // 2. 求余数 (0-25)，得到当前字符的索引
                int remainder = n % 26; 
        
                // 3. 将索引转换为字符 ('A' + remainder)
                char currentCharacter = (char)('A' + remainder);
        
                // 4. 将字符添加到结果的开头
                columnName.Insert(0, currentCharacter);
        
                // 5. 将 n 更新为除以 26 的结果，用于下一次循环
                n = n / 26;
            }

            return columnName.ToString();
        }
    }
}