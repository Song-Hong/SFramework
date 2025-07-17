using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using Song.Core.Extends.Excel;
using UnityEngine.UIElements;

namespace Song.Core.Editor.TaskManager
{
    /// <summary>
    /// 任务管理器窗口
    /// </summary>
    public partial class TaskManagerWindow:EditorWindow
    {
        /// <summary>
        /// 文件位置
        /// </summary>
        private static string _filePath = Application.streamingAssetsPath+"/SongTaskManager.xlsx";
        
        /// <summary>
        /// 滚动条位置
        /// </summary>
        private Vector2 _scrollPos;
        
        /// <summary>
        /// 内容区域
        /// </summary>
        private VisualElement _contentArea;
        
        /// <summary>
        /// 底部区域
        /// </summary>
        private VisualElement _bottomArea;
        
        /// <summary>
        /// Excel数据
        /// </summary>
        private SongExcelData _excelData;
        
        // [MenuItem("Song/任务管理器")]
        public static void ShowTaskManager()
        {
            //显示窗口
            var taskManager = GetWindow<TaskManagerWindow>();
            taskManager.titleContent = new GUIContent("任务管理器");
            taskManager.Show();
        }

        private void OnEnable()
        {
            //加载数据
            if (!File.Exists(_filePath))
            {
                SongExcelSupport.CreateExcel(_filePath);
            }
            _excelData = SongExcelSupport.ReadToExcelData(_filePath);
            if (_excelData != null)
            {
                foreach (var sheet in _excelData.Sheets())
                {
                    // for (var i = 1; i < sheet.RowCount; i++)
                    // {
                    //     for (var k = 0; k < sheet.ColCount; k++)
                    //     {
                    //         
                    //     }
                    // }
                }
            }
        }

        private void CreateGUI()
        {
            //加载UI
            var root = rootVisualElement;
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Song/Core/Editor/TaskManager/TaskManager.uxml")
                .CloneTree(root);
            
            // 内容区域
            _contentArea = root.Q<VisualElement>("ContentArea");
            
            root.Q<Button>("Save").clicked += () =>
            {
                foreach (var sheetData in _excelData.Sheets())
                {
                    Debug.Log(sheetData.name);
                }

                SongExcelSupport.Save(_filePath, _excelData);
            };
            
            var scrollView = root.Q<ScrollView>("Content");
            var addContentPanel = root.Q<Button>("AddContentPanel");
            addContentPanel.clicked += () =>
            {
                var contentPanel = CreateContentPanel();
                contentPanel.PlaceBehind(addContentPanel.parent);
            };

            //底部区域
            _bottomArea = root.Q<VisualElement>("Bottom");
            
            foreach (var sheetData in _excelData.Sheets())
            {
                var bottomButtonItem = CreateBottomButtonItem(sheetData.name);
                bottomButtonItem.clicked += () =>
                {
                    BottomButtonClick(bottomButtonItem);
                };
            }
            //底部添加按钮
            var bottomAddButton = root.Q<Button>("AddItem");
            bottomAddButton.clicked += () =>
            {
                var sheetData = new SheetData("Sheet" + (_excelData.Sheets().Count + 1));
                _excelData.Add(sheetData);
                var newBtn = CreateBottomButtonItem(sheetData.name);
                newBtn.clicked += () =>
                {
                    BottomButtonClick(newBtn);
                };
                newBtn.PlaceBehind(bottomAddButton);
            };
            bottomAddButton.BringToFront();
        }

        /// <summary>
        /// 创建内容面板
        /// </summary>
        public ScrollView CreateContentPanel(string title = "newTitle")
        {
            var contentPanel = new VisualElement();
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Song/Core/Editor/TaskManager/ContentPanel.uxml").CloneTree(contentPanel);
            var scrollView = contentPanel.Q<ScrollView>("ContentPanel");
            var groupBox = contentPanel.Q<GroupBox>("ContentPanelArea");
            scrollView.AddToClassList("ContentPanel");
            var textField = groupBox.Q<TextField>("Title");
            textField.AddToClassList("ContentPanelTitle");
            textField.value = title;
            _contentArea.Add(scrollView);
            return scrollView;
        }

        private Button currentClickedButton;
        
        /// <summary>
        /// 底部按钮点击事件
        /// </summary>
        private void BottomButtonClick(Button btn)
        {
            if(currentClickedButton!=null)
            {
                currentClickedButton.RemoveFromClassList("BottomItemSelected");
            }
            btn.AddToClassList("BottomItemSelected");
            currentClickedButton = btn;
        }

        /// <summary>
        /// 创建底部按钮Item
        /// </summary>
        /// <param name="itemName">按钮名</param>
        /// <returns></returns>
        private Button CreateBottomButtonItem(string itemName)
        {
            var button = new Button
            {
                text = itemName
            };
            button.AddToClassList("BottomItem");
            _bottomArea.Add(button);
            return button;
        }
    }
}