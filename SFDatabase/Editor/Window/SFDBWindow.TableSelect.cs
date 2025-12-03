using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFDatabase.Editor.Window
{
    /// <summary>
    /// 数据库窗口-表格选择视图
    /// </summary>
    public partial class SfDbWindow
    {
        /// <summary>
        /// 当前选中的表格选择按钮
        /// </summary>
        private Button _nowSelectTableBtn;
        private List<string> _currentColumnNames = new List<string>();
        private List<long> _currentRowIds = new List<long>();

        /// <summary>
        /// 初始化表格选择视图
        /// </summary>
        public void InitTableSelect()
        {
            // 清空表格选择视图
            if (_tableSelect == null) return;
            _tableSelect.Query<Button>("sfdb-table_item_select")
                .ForEach(btn => { _tableSelect.Remove(btn); });

            // 点击创建表格按钮清空创建表格视图区域
            _createTableBtn.clicked += () =>
            {
                // 移除当前选中的表格选择按钮选中状态
                if (_nowSelectTableBtn != null)
                {
                    _nowSelectTableBtn.RemoveFromClassList("sfdb-table_item_select");
                }
                // 选中创建表格按钮
                _createTableBtn.AddToClassList("sfdb-table_create_btn_select");
                ChangeToCreateTable();
            };
            // 点击删除表格按钮，确认并删除当前选中的表
            _deleteDBTable.clicked += () =>
            {
                var selectedName = _nowSelectTableBtn?.text;
                if (string.IsNullOrEmpty(selectedName))
                {
                    EditorUtility.DisplayDialog("删除表格", "当前未选中任何表格", "确定");
                    return;
                }

                var ok = EditorUtility.DisplayDialog("删除表格",
                    $"确认删除表格 \"{selectedName}\"? 此操作不可撤销。",
                    "删除", "取消");
                if (!ok) return;

                var success = Sqlite.DropTable(selectedName);
                if (!success)
                {
                    EditorUtility.DisplayDialog("删除表格", "删除失败，请查看控制台日志。", "确定");
                    return;
                }

                // UI 清理：移除选中样式、清空视图内容、移除该按钮
                if (_nowSelectTableBtn != null)
                {
                    _nowSelectTableBtn.RemoveFromClassList("sfdb-table_item_select");
                    _tableSelect.Remove(_nowSelectTableBtn);
                    _nowSelectTableBtn = null;
                }
                ClearTableViewContent();
                _tableView.style.display = DisplayStyle.None;
            };
            // 点击添加新行按钮，添加新行
            _addNewLine.clicked += () =>
            {
                var tableName = _nowSelectTableBtn?.text;
                if (string.IsNullOrEmpty(tableName))
                {
                    EditorUtility.DisplayDialog("添加新行", "当前未选中任何表格", "确定");
                    return;
                }
                try
                {
                    var infoReader = Sqlite.ExecuteQuery($"PRAGMA table_info('{tableName}');");
                    var cols = new List<string>();
                    var vals = new List<string>();
                    while (infoReader.Read())
                    {
                        var name = infoReader.GetString(1);
                        var type = infoReader.GetString(2).ToUpperInvariant();
                        var notnull = infoReader.GetInt32(3) == 1;
                        var hasDefault = !infoReader.IsDBNull(4);
                        var dflt = hasDefault ? infoReader.GetValue(4).ToString() : null;
                        var isPk = infoReader.GetInt32(5) == 1;
                        if (isPk) continue;

                        cols.Add($"\"{name}\"");
                        if (hasDefault)
                        {
                            vals.Add(dflt);
                        }
                        else if (!notnull)
                        {
                            vals.Add("NULL");
                        }
                        else
                        {
                            if (type.Contains("INT"))
                                vals.Add("0");
                            else if (type.Contains("REAL") || type.Contains("FLOA") || type.Contains("DOUB"))
                                vals.Add("0");
                            else if (type.Contains("BLOB"))
                                vals.Add("X''");
                            else
                                vals.Add("''");
                        }
                    }
                    infoReader.Close();

                    if (cols.Count == 0)
                    {
                        var effectDefault = Sqlite.ExecuteNonQuery($"INSERT INTO \"{tableName}\" DEFAULT VALUES;");
                        if (effectDefault <= 0)
                        {
                            EditorUtility.DisplayDialog("添加新行", "插入失败，可能存在非空列未提供默认值。", "确定");
                            return;
                        }
                    }
                    else
                    {
                        var insertSql = $"INSERT INTO \"{tableName}\" ({string.Join(", ", cols)}) VALUES ({string.Join(", ", vals)});";
                        var effect = Sqlite.ExecuteNonQuery(insertSql);
                        if (effect <= 0)
                        {
                            EditorUtility.DisplayDialog("添加新行", "插入失败，请查看控制台日志。", "确定");
                            return;
                        }
                    }
                    ClearTableViewContent();
                    SelectTableInfo(tableName);
                    SelectTableData(tableName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    EditorUtility.DisplayDialog("添加新行", "插入失败，请查看控制台日志。", "确定");
                }
            };
            
            // 创建表格选择按钮
            SelectTable();
        }

        #region 表格元素操作
        /// <summary>
        /// 创建表格选择按钮
        /// </summary>
        /// <param name="tableName">表格名称</param>
        public void CreateSelectTableBtn(string tableName)
        {
            // 创建表格选择按钮
            var tableBtn = new Button()
            {
                text = tableName
            };
            tableBtn.AddToClassList("sfdb-table_item");
            tableBtn.clicked += () =>
            {
                SelectBtn(tableBtn);
            };
            // 添加表格选择按钮到表格选择视图
            _tableSelect.Add(tableBtn);
            
            // 第一个表格选择按钮默认选中
            if (_nowSelectTableBtn == null)
            {
                SelectBtn(tableBtn);
            }
            
            // 移到创建表格按钮前面
            _createTableBtn.BringToFront();
        }

        /// <summary>
        /// 选择表格按钮
        /// </summary>
        /// <param name="btn">表格选择按钮</param>
        public void SelectBtn(Button btn)
        {
            if (_nowSelectTableBtn != null)
            {
                _nowSelectTableBtn.RemoveFromClassList("sfdb-table_item_select");
            }

            _nowSelectTableBtn = btn;
            _nowSelectTableBtn.AddToClassList("sfdb-table_item_select");
            
            // 移除创建表格按钮选中状态
            _createTableBtn.RemoveFromClassList("sfdb-table_create_btn_select");

            // 切换到表格选择视图
            ChangeToTableView();
            // 清空表格选择视图内容
            ClearTableViewContent();
            // 查询表格信息
            SelectTableInfo(btn.text);
            // 查询表格数据
            SelectTableData(btn.text);
        }

        /// <summary>
        /// 清空表格选择视图内容
        /// </summary>
        public void ClearTableViewContent()
        {
            if (_tableViewArea == null) return;
            _tableViewArea.Query<VisualElement>().ForEach(ve =>
            {
                if(_tableViewArea != ve && ve.parent == _tableViewArea && ve != _deleteDBTable && ve != _addNewLine)
                    _tableViewArea.Remove(ve);
            });
        }
        
        /// <summary>
        /// 创建表格标题
        /// </summary>
        /// <param name="tableNames">表格名称列表</param>
        public void CreateTableTitle(List<string> tableNames)
        {
            // 创建表格标题容器
            var titleContainer = new VisualElement();
            titleContainer.AddToClassList("sfdb-content_table_view_title");

            // 添加占位按钮
            var button = new Button();
            button.AddToClassList("sfdb-content_table_view_line_item");
            button.style.width = 30;
            button.style.backgroundColor = Color.clear;
            button.text = "";
            titleContainer.Add(button);
            
            // 创建表格标题
            for (var i = 0; i < tableNames.Count; i++)
            {
                var titleItem = new TextField()
                {
                    value = tableNames[i],
                    isDelayed = true
                };
                titleItem.AddToClassList("sfdb-content_table_view_title_item");
                var columnIndex = i;
                var oldName = tableNames[i];
                titleItem.RegisterValueChangedCallback(e =>
                {
                    var tableName = _nowSelectTableBtn?.text;
                    if (string.IsNullOrEmpty(tableName)) return;
                    var newName = e.newValue;
                    if (string.IsNullOrEmpty(newName) || newName == oldName) return;
                    var sql = $"ALTER TABLE \"{tableName}\" RENAME COLUMN \"{oldName}\" TO \"{newName}\";";
                    Sqlite.ExecuteNonQuery(sql);
                    _currentColumnNames[columnIndex] = newName;
                    oldName = newName;
                });
                titleContainer.Add(titleItem);
            }
            // 添加表格标题到表格选择视图
            _tableViewArea.Add(titleContainer);
            // 移动删除表格按钮到最前面
            _deleteDBTable.BringToFront();
            // 移到添加新行按钮前面
            _addNewLine.BringToFront();
        }

        /// <summary>
        /// 创建表格行
        /// </summary>
        /// <param name="lines">表格名称列表</param>
        public void CreateTableLine(List<List<string>> lines)
        {
            for (var rowIndex = 0; rowIndex < lines.Count; rowIndex++)
            {
                var line = lines[rowIndex];
                var lineContainer = new VisualElement();
                lineContainer.AddToClassList("sfdb-content_table_view_line");

                // 添加删除行表格
                var button = new Button();
                button.AddToClassList("sfdb-content_table_view_line_item");
                button.style.width = 30;
                button.text = "-";
                lineContainer.Add(button);
                // 删除行按钮点击
                var rowIndexLocal = rowIndex;
                var rowIdLocalForRow = _currentRowIds.Count > rowIndexLocal ? _currentRowIds[rowIndexLocal] : -1;
                var rowValues = new List<string>(line);
                button.clicked+=()=>
                {
                    var tableName = _nowSelectTableBtn?.text;
                    if (string.IsNullOrEmpty(tableName))
                    {
                        EditorUtility.DisplayDialog("删除行", "当前未选中任何表格", "确定");
                        return;
                    }
                    var rowIdLocalForDelete = rowIdLocalForRow;
                    string whereClause = null;
                    if (rowIdLocalForDelete >= 0)
                    {
                        whereClause = $"rowid = {rowIdLocalForDelete}";
                    }
                    else
                    {
                        var infoReader = Sqlite.ExecuteQuery($"PRAGMA table_info('{tableName}');");
                        var pkConds = new List<string>();
                        var colIndex = 0;
                        while (infoReader.Read())
                        {
                            var name = infoReader.GetString(1);
                            var type = infoReader.GetString(2).ToUpperInvariant();
                            var isPk = infoReader.GetInt32(5) == 1;
                            if (isPk)
                            {
                                var val = colIndex < rowValues.Count ? rowValues[colIndex] : null;
                                if (val == null)
                                {
                                    pkConds.Clear();
                                    break;
                                }
                                if (type.Contains("INT") || type.Contains("REAL") || type.Contains("FLOA") || type.Contains("DOUB"))
                                    pkConds.Add($"\"{name}\" = {val}");
                                else
                                    pkConds.Add($"\"{name}\" = '{val.Replace("'", "''")}'");
                            }
                            colIndex++;
                        }
                        infoReader.Close();
                        if (pkConds.Count > 0)
                            whereClause = string.Join(" AND ", pkConds);
                        else
                        {
                            EditorUtility.DisplayDialog("删除行", "无法确定要删除的行ID", "确定");
                            return;
                        }
                    }
                    var ok = EditorUtility.DisplayDialog("删除行",
                        $"确认删除数据行? 条件: {whereClause}",
                        "删除", "取消");
                    if (!ok) return;
                    var affected = Sqlite.Delete(tableName, whereClause);
                    if (affected <= 0)
                    {
                        EditorUtility.DisplayDialog("删除行", "删除失败，请查看控制台日志。", "确定");
                        return;
                    }
                    ClearTableViewContent();
                    SelectTableInfo(tableName);
                    SelectTableData(tableName);
                };

                // 创建表格行
                for (var colIndex = 0; colIndex < line.Count; colIndex++)
                {
                    var cellValue = line[colIndex];
                    var item = new TextField()
                    {
                        value = cellValue,
                        isDelayed = true
                    };
                    item.AddToClassList("sfdb-content_table_view_line_item");
                    var rowId = _currentRowIds.Count > rowIndex ? _currentRowIds[rowIndex] : -1;
                    var colIndexLocal = colIndex;
                    var rowIdLocal = rowId;
                    item.RegisterValueChangedCallback(e =>
                    {
                        var tableName = _nowSelectTableBtn?.text;
                        if (string.IsNullOrEmpty(tableName)) return;
                        if (_currentColumnNames.Count <= colIndexLocal) return;
                        var columnName = _currentColumnNames[colIndexLocal];
                        if (rowIdLocal < 0) return;
                        var newVal = e.newValue?.Replace("'", "''") ?? string.Empty;
                        Sqlite.Update(tableName, $"\"{columnName}\" = '{newVal}'", $"rowid = {rowIdLocal}");
                    });
                    lineContainer.Add(item);
                }
                _tableViewArea.Add(lineContainer);
            }
        }
        #endregion

        #region 表格数据查询
        /// <summary>
        /// 查询所有表格
        /// </summary>
        public void SelectTable()
        {
            var sqlQuery =
                @"SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'sqlitestudio_%';";
            var sqliteDataReader = Sqlite.ExecuteQuery(sqlQuery);
            while (sqliteDataReader.Read())
            {
                var tableName = sqliteDataReader.GetString(0);
                CreateSelectTableBtn(tableName);
            }
            sqliteDataReader.Close();

            var uQueryBuilder = _tableSelect.Query<VisualElement>().Class("sfdb-table_item");
            if (uQueryBuilder.ToList().Count > 0) return;
            _createTableBtn.AddToClassList("sfdb-table_create_btn_select");
            ChangeToCreateTable();
        }

        /// <summary>
        /// 查询表格信息
        /// </summary>
        /// <param name="tableName">表格名称</param>
        public void SelectTableInfo(string tableName)
        {
            var sqlQuery = $"PRAGMA table_info('{tableName}');";
            var sqliteDataReader = Sqlite.ExecuteQuery(sqlQuery);
            _currentColumnNames = new List<string>();
            while (sqliteDataReader.Read())
            {
                var columnName = sqliteDataReader.GetString(1);
                _currentColumnNames.Add(columnName);
            }
            // 创建表格标题
            CreateTableTitle(_currentColumnNames);
        }
        
        /// <summary>
        /// 查询所有表格数据
        /// </summary>
        /// <param name="tableName">表格名称</param>
        public void SelectTableData(string tableName)
        {
            var sqlQuery = $"SELECT rowid, * FROM {tableName};";
            var sqliteDataReader = Sqlite.ExecuteQuery(sqlQuery);
            // Debug.Log(sqliteDataReader.FieldCount);
            var lines = new List<List<string>>();
            _currentRowIds = new List<long>();
            while (sqliteDataReader.Read())
            {
                var items = new List<string>();
                _currentRowIds.Add(sqliteDataReader.GetInt64(0));
                for (var i = 1; i < sqliteDataReader.FieldCount; i++)
                {
                    var fieldValue = sqliteDataReader.GetValue(i);
                    items.Add(fieldValue.ToString());
                }
                lines.Add(items);
            }
            // 创建表格行
            CreateTableLine(lines);
            sqliteDataReader.Close();
            // 移动删除表格按钮到最前面
            _deleteDBTable.BringToFront();
            // 移到添加新行按钮前面
            _addNewLine.BringToFront();
        }
        #endregion
        
    }
}