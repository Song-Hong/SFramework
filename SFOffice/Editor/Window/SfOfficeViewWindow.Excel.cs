using System;
using System.Collections.Generic;
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
        #region 变量
        /// <summary>
        /// 当前编辑的Excel数据
        /// </summary>
        private SfExcelData _excelData;
        
        /// <summary>
        /// 当前编辑的工作表
        /// </summary>
        private SheetData _currentSheet;
        
        /// <summary>
        /// 单元格输入框列表 (行索引, 列索引) -> TextField
        /// </summary>
        private Dictionary<Tuple<int, int>, TextField> _cellFields = new Dictionary<Tuple<int, int>, TextField>();
        
        /// <summary>
        /// 添加列按钮
        /// </summary>
        private Button _addColumnButton;
        
        /// <summary>
        /// 添加行按钮
        /// </summary>
        private Button _addRowButton;
        
        /// <summary>
        /// 删除列按钮列表
        /// </summary>
        private List<Button> _deleteColButtons = new List<Button>();
        
        /// <summary>
        /// 删除行按钮列表
        /// </summary>
        private List<Button> _deleteRowButtons = new List<Button>();
        
        /// <summary>
        /// 保存按钮
        /// </summary>
        private Button _saveButton;
        #endregion
        
        #region 初始化
        /// <summary>
        /// 初始化Excel模块
        /// </summary>
        public void InitExcel()
        {
            // 读取Excel文件
            _excelData = SfExcel.ReadToExcelData(_filePath);
            if (_excelData == null) return;
            
            // 遍历Excel文件中的所有工作表 (目前只处理第一个)
            foreach (var sheet in _excelData.Sheets())
            {
                _currentSheet = sheet;
                _cellFields.Clear();
                _deleteColButtons.Clear();
                _deleteRowButtons.Clear();
                
                // 创建标题行
                var titleLine = CreateLine();
                CreateLineTitle("", titleLine);
                for (var j = 0; j < sheet.ColCount; j++)
                {
                    CreateColHeader(j, titleLine);
                }
                // 创建添加列按钮
                _addColumnButton = CreateLineTitle("+", titleLine);
                _addColumnButton.clicked += OnAddColumnClicked;
                
                // 创建数据行
                for (var i = 0; i < sheet.RowCount; i++)
                {
                    CreateDataRow(i, sheet);
                }
                
                break;
            }
            
            // 添加创建行按钮
            var rowLine = CreateLine();
            _addRowButton = CreateLineTitle("+", rowLine);
            _addRowButton.clicked += OnAddRowClicked;
            
            // 创建工具栏
            CreateToolbar();
        }
        
        /// <summary>
        /// 创建工具栏
        /// </summary>
        private void CreateToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.marginTop = 10;
            toolbar.style.marginBottom = 10;
            toolbar.style.paddingLeft = 10;
            
            _saveButton = new Button();
            _saveButton.text = "保存文件";
            _saveButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
            _saveButton.style.color = Color.white;
            _saveButton.style.borderRadius = 8;
            _saveButton.style.paddingLeft = 16;
            _saveButton.style.paddingRight = 16;
            _saveButton.style.paddingTop = 6;
            _saveButton.style.paddingBottom = 6;
            _saveButton.clicked += OnSaveClicked;
            toolbar.Add(_saveButton);
            
            _content.Add(toolbar);
        }
        #endregion
        
        #region 行创建
        /// <summary>
        /// 创建Excel行
        /// </summary>
        public VisualElement CreateLine()
        {
            var line = new VisualElement();
            line.AddToClassList("excel_line");
            _content.Add(line);
            return line;
        }
        
        /// <summary>
        /// 创建列标题
        /// </summary>
        public Button CreateColHeader(int colIndex, VisualElement line)
        {
            var colName = NumberToExcelColumn(colIndex + 1);
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Column;
            headerContainer.style.alignItems = Align.Center;
            
            var lineTitle = new Button()
            {
                style =
                {
                    color = Color.white,
                    fontSize = 14,
                    alignSelf = Align.Center,
                },
                text = colName,
            };
            lineTitle.AddToClassList("excel_title");
            headerContainer.Add(lineTitle);
            
            // 删除列按钮
            var deleteBtn = new Button()
            {
                text = "×",
                style =
                {
                    color = new Color(0.8f, 0.3f, 0.3f),
                    fontSize = 10,
                    minHeight = 14,
                    minWidth = 14,
                    paddingBottom = 0,
                    paddingTop = 0,
                    paddingLeft = 2,
                    paddingRight = 2,
                    marginBottom = 2,
                }
            };
            deleteBtn.clicked += () => OnDeleteColumnClicked(colIndex);
            _deleteColButtons.Add(deleteBtn);
            headerContainer.Add(deleteBtn);
            
            line.Add(headerContainer);
            return lineTitle;
        }
        
        /// <summary>
        /// 创建数据行
        /// </summary>
        private void CreateDataRow(int rowIndex, SheetData sheet)
        {
            var line = CreateLine();
            
            // 行号容器
            var rowHeaderContainer = new VisualElement();
            rowHeaderContainer.style.flexDirection = FlexDirection.Column;
            rowHeaderContainer.style.alignItems = Align.Center;
            
            var rowTitle = CreateLineTitle(rowIndex + 1 + "", line);
            rowHeaderContainer.Add(rowTitle);
            
            // 删除行按钮
            var deleteBtn = new Button()
            {
                text = "×",
                style =
                {
                    color = new Color(0.8f, 0.3f, 0.3f),
                    fontSize = 10,
                    minHeight = 14,
                    minWidth = 14,
                    paddingBottom = 0,
                    paddingTop = 0,
                    paddingLeft = 2,
                    paddingRight = 2,
                    marginBottom = 2,
                }
            };
            deleteBtn.clicked += () => OnDeleteRowClicked(rowIndex);
            _deleteRowButtons.Add(deleteBtn);
            rowHeaderContainer.Add(deleteBtn);
            
            // 替换原来的行标题
            line.Remove(rowTitle);
            line.Insert(0, rowHeaderContainer);
            
            // 创建单元格
            for (var k = 0; k < sheet.ColCount; k++)
            {
                CreateCellItem(rowIndex, k, sheet.Get(rowIndex, k), line);
            }
        }
        
        /// <summary>
        /// 创建Excel行标题
        /// </summary>
        public Button CreateLineTitle(string content, VisualElement line)
        {
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
        /// 创建Excel单元格
        /// </summary>
        public TextField CreateCellItem(int rowIndex, int colIndex, string content, VisualElement line)
        {
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
            
            // 记录单元格引用
            _cellFields[new Tuple<int, int>(rowIndex, colIndex)] = lineItem;
            
            return lineItem;
        }
        #endregion
        
        #region 按钮事件
        /// <summary>
        /// 添加列按钮点击
        /// </summary>
        private void OnAddColumnClicked()
        {
            if (_currentSheet == null) return;
            
            var newColIndex = _currentSheet.ColCount;
            
            // 在标题行添加新列头
            var titleLine = _content.Q<VisualElement>(className: "excel_line");
            if (titleLine != null)
            {
                // 移除旧的添加列按钮
                if (_addColumnButton != null && _addColumnButton.parent != null)
                {
                    _addColumnButton.parent.Remove(_addColumnButton);
                }
                
                // 添加新列头
                CreateColHeader(newColIndex, titleLine);
                
                // 重新添加添加列按钮
                _addColumnButton = CreateLineTitle("+", titleLine);
                _addColumnButton.clicked += OnAddColumnClicked;
            }
            
            // 在每行数据中添加新单元格
            for (var i = 0; i < _currentSheet.RowCount; i++)
            {
                var lines = _content.Query<VisualElement>(className: "excel_line").ToList();
                if (i + 1 < lines.Count)
                {
                    var dataLine = lines[i + 1];
                    CreateCellItem(i, newColIndex, "", dataLine);
                }
            }
            
            // 扩展数据
            _currentSheet.CheckArray(_currentSheet.RowCount - 1, newColIndex);
            
            Debug.Log($"[Excel] 添加列成功，当前列数: {_currentSheet.ColCount}");
        }
        
        /// <summary>
        /// 添加行按钮点击
        /// </summary>
        private void OnAddRowClicked()
        {
            if (_currentSheet == null) return;
            
            var newRowIndex = _currentSheet.RowCount;
            _currentSheet.CheckArray(newRowIndex, _currentSheet.ColCount - 1);
            
            // 在添加行按钮之前插入新行
            var addRowLine = _addRowButton?.parent;
            if (addRowLine != null)
            {
                var line = CreateLine();
                addRowLine.parent.Insert(addRowLine.parent.IndexOf(addRowLine), line);
                
                // 行号
                var rowHeaderContainer = new VisualElement();
                rowHeaderContainer.style.flexDirection = FlexDirection.Column;
                rowHeaderContainer.style.alignItems = Align.Center;
                
                var rowTitle = CreateLineTitle(newRowIndex + 1 + "", line);
                rowHeaderContainer.Add(rowTitle);
                
                var deleteBtn = new Button()
                {
                    text = "×",
                    style =
                    {
                        color = new Color(0.8f, 0.3f, 0.3f),
                        fontSize = 10,
                        minHeight = 14,
                        minWidth = 14,
                        paddingBottom = 0,
                        paddingTop = 0,
                        paddingLeft = 2,
                        paddingRight = 2,
                        marginBottom = 2,
                    }
                };
                int capturedRowIndex = newRowIndex;
                deleteBtn.clicked += () => OnDeleteRowClicked(capturedRowIndex);
                _deleteRowButtons.Add(deleteBtn);
                rowHeaderContainer.Add(deleteBtn);
                
                line.Remove(rowTitle);
                line.Insert(0, rowHeaderContainer);
                
                // 单元格
                for (var k = 0; k < _currentSheet.ColCount; k++)
                {
                    CreateCellItem(newRowIndex, k, "", line);
                }
            }
            
            Debug.Log($"[Excel] 添加行成功，当前行数: {_currentSheet.RowCount}");
        }
        
        /// <summary>
        /// 删除列按钮点击
        /// </summary>
        private void OnDeleteColumnClicked(int colIndex)
        {
            if (_currentSheet == null || _currentSheet.ColCount <= 1)
            {
                Debug.LogWarning("[Excel] 至少需要保留一列");
                return;
            }
            
            // 从UI中移除该列的所有单元格
            var lines = _content.Query<VisualElement>(className: "excel_line").ToList();
            for (var i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                var fieldKey = new Tuple<int, int>(i - 1, colIndex);
                if (_cellFields.TryGetValue(fieldKey, out var field) && field.parent != null)
                {
                    field.parent.Remove(field);
                    _cellFields.Remove(fieldKey);
                }
            }
            
            // 移除列标题
            var titleLine = lines[0];
            if (colIndex < _deleteColButtons.Count)
            {
                var delBtn = _deleteColButtons[colIndex];
                if (delBtn.parent != null)
                {
                    delBtn.parent.parent?.Remove(delBtn.parent);
                }
                _deleteColButtons.RemoveAt(colIndex);
            }
            
            // 更新剩余列的删除按钮索引
            for (var i = colIndex; i < _deleteColButtons.Count; i++)
            {
                var capturedIndex = i;
                _deleteColButtons[i].clicked -= () => OnDeleteColumnClicked(capturedIndex + 1);
                _deleteColButtons[i].clicked += () => OnDeleteColumnClicked(capturedIndex);
            }
            
            Debug.Log($"[Excel] 删除列成功，当前列数: {_currentSheet.ColCount - 1}");
        }
        
        /// <summary>
        /// 删除行按钮点击
        /// </summary>
        private void OnDeleteRowClicked(int rowIndex)
        {
            if (_currentSheet == null || _currentSheet.RowCount <= 1)
            {
                Debug.LogWarning("[Excel] 至少需要保留一行");
                return;
            }
            
            // 从UI中移除该行
            var lines = _content.Query<VisualElement>(className: "excel_line").ToList();
            if (rowIndex + 1 < lines.Count)
            {
                var line = lines[rowIndex + 1];
                if (line.parent != null)
                {
                    line.parent.Remove(line);
                }
            }
            
            // 清理单元格引用
            var keysToRemove = new List<Tuple<int, int>>();
            foreach (var kvp in _cellFields)
            {
                if (kvp.Key.Item1 == rowIndex)
                {
                    keysToRemove.Add(kvp.Key);
                }
                else if (kvp.Key.Item1 > rowIndex)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                _cellFields.Remove(key);
            }
            
            Debug.Log($"[Excel] 删除行成功，当前行数: {_currentSheet.RowCount - 1}");
        }
        
        /// <summary>
        /// 保存按钮点击
        /// </summary>
        private void OnSaveClicked()
        {
            if (_currentSheet == null || string.IsNullOrEmpty(_filePath))
            {
                Debug.LogWarning("[Excel] 没有可保存的数据");
                return;
            }
            
            // 从UI收集数据更新到SheetData
            foreach (var kvp in _cellFields)
            {
                var rowIndex = kvp.Key.Item1;
                var colIndex = kvp.Key.Item2;
                var value = kvp.Value.value ?? "";
                _currentSheet.Set(rowIndex, colIndex, value);
            }
            
            // 保存到文件
            var success = SfExcel.Save(_filePath, _excelData, isCover: true, isCreate: true);
            if (success)
            {
                Debug.Log($"[Excel] 保存成功: {_filePath}");
                EditorUtility.DisplayDialog("保存成功", $"文件已保存到:\n{_filePath}", "确定");
            }
            else
            {
                Debug.LogError("[Excel] 保存失败，请检查文件是否被其他程序占用");
                EditorUtility.DisplayDialog("保存失败", "文件可能被其他程序占用，请关闭后重试", "确定");
            }
        }
        #endregion
        
        #region 工具
        /// <summary>
        /// 将正整数转换为 Excel 列名（A, B, C, ..., Z, AA, ...）
        /// </summary>
        public string NumberToExcelColumn(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "输入数字必须是大于零的正整数。");
            }

            var columnName = new StringBuilder();

            while (n > 0)
            {
                n--; 
                int remainder = n % 26; 
                char currentCharacter = (char)('A' + remainder);
                columnName.Insert(0, currentCharacter);
                n = n / 26;
            }

            return columnName.ToString();
        }
        #endregion
    }
}
