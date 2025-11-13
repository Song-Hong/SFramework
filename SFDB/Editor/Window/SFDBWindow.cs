using System;
using SFramework.SFDb.Module;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFDb.Editor.Window
{
    /// <summary>
    /// 数据库窗口
    /// </summary>
    public partial class SfDbWindow:EditorWindow
    {
        #region 内容元素
        /// <summary>
        /// 表格内容元素
        /// </summary>
        private VisualElement _tableSelect;
        /// <summary>
        /// 内容元素
        /// </summary>
        private VisualElement _content;
        /// <summary>
        /// 表格视图
        /// </summary>
        private VisualElement _tableView;
        /// <summary>
        /// 表格选择视图
        /// </summary>
        private VisualElement _tableViewArea;
        /// <summary>
        /// 创建表格视图
        /// </summary>
        private VisualElement _createTableView;
        /// <summary>
        /// 创建表格视图区域
        /// </summary>
        private VisualElement _createTableArea;
        /// <summary>
        /// 创建表格按钮
        /// </summary>
        private Button _createTableBtn;
        /// <summary>
        /// 创建表格字段行按钮
        /// </summary>
        private Button _createTableLineBtn;
        /// <summary>
        /// 创建数据库表格按钮
        /// </summary>
        private Button _createDBTable;
        /// <summary>
        /// 删除数据库表格按钮
        /// </summary>
        private Button _deleteDBTable;
        /// <summary>
        /// 添加新行按钮
        /// </summary>
        private Button _addNewLine;
        #endregion

        #region 可变参数
        /// <summary>
        /// 数据库文件路径
        /// </summary>
        public string dbPath = $"{Application.streamingAssetsPath}/database.db";
        
        /// <summary>
        /// Sqlite数据库实例
        /// </summary>
        public SfSqlite Sqlite;
        #endregion
        
        /// <summary>
        /// 显示数据库窗口
        /// </summary>
        [MenuItem("SFramework/数据库编辑器")]
        public static void ShowSfDbWindow()
        {
            var window = GetWindow<SfDbWindow>("数据库编辑器");
            window.Show();
        }
        
        /// <summary>
        /// 打开数据库窗口
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        public static void OpenSfDbWindow(string dbPath)
        {
            var window = GetWindow<SfDbWindow>("数据库编辑器");
            window.dbPath = dbPath;
            window.Show();
        }

        /// <summary>
        /// 创建GUI
        /// </summary>
        private void CreateGUI()
        {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SFramework/SFDB/Editor/Window/SFDBWindow.uxml").
                CloneTree(rootVisualElement);
            
            _content = rootVisualElement.Q<VisualElement>("ContentContainer"); // 内容容器
            _tableSelect = rootVisualElement.Q<VisualElement>("TableContainer"); // 表格容器
            _tableView = rootVisualElement.Q<VisualElement>("TableView"); // 表格视图
            _tableViewArea = rootVisualElement.Q<VisualElement>("TableViewArea"); // 表格选择视图
            _createTableView = rootVisualElement.Q<VisualElement>("CreateTableView"); // 创建表格视图
            _createTableArea = rootVisualElement.Q<VisualElement>("CreateTableArea"); // 创建表格视图区域
            _createTableBtn = rootVisualElement.Q<Button>("CreateTable"); // 创建表格按钮
            _createTableLineBtn = rootVisualElement.Q<Button>("CreateLine"); // 创建表格字段行按钮
            _createDBTable = rootVisualElement.Q<Button>("CreateDBTable"); // 创建数据库表格按钮
            _deleteDBTable = rootVisualElement.Q<Button>("DeleteDBTable"); // 删除数据库表格按钮
            _addNewLine = rootVisualElement.Q<Button>("AddNewLine");
            
            // 初始化Sqlite数据库实例
            Sqlite = new SfSqlite(dbPath);
            
            InitTableSelect(); // 初始化表格选择视图
            
            InitCreateTable(); // 初始化创建表格视图
        }

        /// <summary>
        /// 切换到表格选择视图
        /// </summary>
        public void ChangeToTableView()
        {
            _tableView.style.display = DisplayStyle.Flex;
            _createTableView.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 切换到创建表格视图
        /// </summary>
        public void ChangeToCreateTable()
        {
            _tableView.style.display = DisplayStyle.None;
            _createTableView.style.display = DisplayStyle.Flex;
        }
        
        /// <summary>
        /// 销毁数据库窗口时调用
        /// </summary>
        private void OnDestroy()
        {
            Sqlite?.CloseConnection();
        }
    }
}