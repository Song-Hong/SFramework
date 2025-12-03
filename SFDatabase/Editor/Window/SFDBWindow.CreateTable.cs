using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace SFramework.SFDatabase.Editor.Window
{
    /// <summary>
    /// 数据库窗口-创建表格界面
    /// </summary>
    public partial class SfDbWindow
    {
        /// <summary>
        /// 创建表格
        /// </summary>
        public void InitCreateTable()
        {
            // 创建表格字段行按钮点击事件
            _createTableLineBtn.clicked += () =>
            {
                CreateCreateFiled();
            };
            // 
            _createDBTable.clicked += () =>
            {
                var tableNameInput = rootVisualElement.Q<TextField>("TableNameInput");
                var tableName = tableNameInput?.value?.Trim();
                if (string.IsNullOrEmpty(tableName))
                {
                    EditorUtility.DisplayDialog("创建表格", "表名不能为空", "确定");
                    return;
                }

                var lines = new List<VisualElement>();
                _createTableArea.Query<VisualElement>(null, "sfdb-content_table_create_line").ForEach(l => lines.Add(l));
                var columnDefs = new List<string>();
                var pkColumns = new List<int>();
                var names = new List<string>();
                var types = new List<string>();
                var notNullFlags = new List<bool>();
                var autoIncFlags = new List<bool>();

                if (lines.Count == 0)
                {
                    columnDefs.Add("\"id\" INTEGER PRIMARY KEY AUTOINCREMENT");
                }

                foreach (var line in lines)
                {
                    var nameField = line.Q<TextField>();
                    var dropdowns = new List<DropdownField>();
                    line.Query<DropdownField>().ForEach(dd => dropdowns.Add(dd));
                    if (nameField == null || dropdowns.Count < 4) continue;

                    var colName = nameField.value?.Trim();
                    if (string.IsNullOrEmpty(colName)) continue;
                    var colType = dropdowns[0].value;
                    var nullableVal = dropdowns[1].value;
                    var pkVal = dropdowns[2].value;
                    var autoIncVal = dropdowns[3].value;

                    names.Add(colName);
                    types.Add(colType);
                    notNullFlags.Add(nullableVal == "不为空");
                    autoIncFlags.Add(autoIncVal == "不自增" ? false : true);
                    if (pkVal == "主键") pkColumns.Add(names.Count - 1);

                    columnDefs.Add($"\"{colName}\" {colType}{(nullableVal == "不为空" ? " NOT NULL" : string.Empty)}");
                }

                if (columnDefs.Count == 0)
                {
                    EditorUtility.DisplayDialog("创建表格", "至少需要一个字段", "确定");
                    return;
                }

                if (pkColumns.Count == 1)
                {
                    var idx = pkColumns[0];
                    var autoInc = autoIncFlags[idx] && types[idx] == "INTEGER";
                    columnDefs[idx] = $"\"{names[idx]}\" {types[idx]} PRIMARY KEY{(autoInc ? " AUTOINCREMENT" : string.Empty)}{(notNullFlags[idx] ? " NOT NULL" : string.Empty)}";
                }
                else if (pkColumns.Count > 1)
                {
                    var pkCols = new List<string>();
                    foreach (var i in pkColumns) pkCols.Add($"\"{names[i]}\"");
                    columnDefs.Add($"PRIMARY KEY({string.Join(", ", pkCols)})");
                }

                var defs = string.Join(", ", columnDefs);
                var created = Sqlite.CreateTable(tableName, defs);
                if (!created) return;

                CreateSelectTableBtn(tableName);
                Button target = null;
                _tableSelect.Query<Button>().ForEach(b => { if (b.text == tableName) target = b; });
                if (target != null)
                {
                    SelectBtn(target);
                }
                else
                {
                    EditorUtility.DisplayDialog("创建表格", "表格创建成功，但未能选中。", "确定");
                }
            };
        }

        /// <summary>
        /// 清除创建表格视图区域
        /// </summary>
        public void ClearCreateTableArea()
        {
            _createTableArea.Query<VisualElement>(null, "sfdb-content_table_create_line")
                .ForEach(line => line.RemoveFromHierarchy());
        }
        
        /// <summary>
        /// 创建表格字段行
        /// </summary>
        public void CreateCreateFiled()
        {
            // 创建表格字段行
            var line = new VisualElement();
            line.AddToClassList("sfdb-content_table_create_line");

            // 字段名
            var fieldInput = new TextField();
            fieldInput.AddToClassList("sfdb-content_table_create_line_item");
            line.Add(fieldInput);
            
            // 字段类型
            var dropdownField = new DropdownField();
            dropdownField.AddToClassList("sfdb-content_table_create_line_item");
            dropdownField.choices.Add("INTEGER");
            dropdownField.choices.Add("REAL");
            dropdownField.choices.Add("TEXT");
            dropdownField.choices.Add("BLOB");
            dropdownField.value = "INTEGER";
            line.Add(dropdownField);
            
            // 是否可为空
            var nullableToggle = new DropdownField();
            nullableToggle.AddToClassList("sfdb-content_table_create_line_item");
            nullableToggle.choices.Add("可为空");
            nullableToggle.choices.Add("不为空");
            nullableToggle.value = "可为空";
            line.Add(nullableToggle);
            
            // 是否为主键
            var primaryKeyToggle = new DropdownField();
            primaryKeyToggle.AddToClassList("sfdb-content_table_create_line_item");
            primaryKeyToggle.choices.Add("主键");
            primaryKeyToggle.choices.Add("非主键");
            primaryKeyToggle.value = "非主键";
            line.Add(primaryKeyToggle);
            
            // 是否自增
            var autoIncrementToggle = new DropdownField();
            autoIncrementToggle.AddToClassList("sfdb-content_table_create_line_item");
            autoIncrementToggle.choices.Add("自增");
            autoIncrementToggle.choices.Add("不自增");
            autoIncrementToggle.value = "不自增";
            line.Add(autoIncrementToggle);

            // 添加到创建表格视图区域
            _createTableArea.Add(line);
            //  BringToFront() 方法将按钮 bring 到最前面
            _createTableLineBtn.BringToFront();
        }
    }
}