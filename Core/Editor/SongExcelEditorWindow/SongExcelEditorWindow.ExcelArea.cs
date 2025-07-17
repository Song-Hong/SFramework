using System.IO;
using Song.Core.Extends.Excel;
using Song.Core.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.Core.Editor.SongExcelEditorWindow
{
    /// <summary>
    /// Excel界面
    /// </summary>
    public partial class SongExcelEditorWindow
    {
        /// <summary>
        /// 全部Itmes
        /// </summary>
        public GroupBox Items;

        /// <summary>
        /// 底部区域
        /// </summary>
        public GroupBox Bottom;

        /// <summary>
        /// 当前选中的子表
        /// </summary>
        public Button NowSelectItem;

        /// <summary>
        /// Excel数据
        /// </summary>
        public SongExcelData MyExcelData;
        
        /// <summary>
        /// 当前选中的Sheet
        /// </summary>
        public SheetData NowSelectSheet;
        
        /// <summary>
        /// 创建子表按钮
        /// </summary>
        public Button createSheetButton;
        
        /// <summary>
        /// 初始化Excel区域
        /// </summary>
        public void InitExcelArea()
        {
            var fileName = Path.GetFileNameWithoutExtension(excelFilePath);
            var songEditorWindow = GetWindow<SongExcelEditorWindow>();
            songEditorWindow.titleContent = new GUIContent(fileName);

            Items = rootVisualElement.Q<GroupBox>("Items");
            Bottom = rootVisualElement.Q<GroupBox>("Bottom");

            rootVisualElement.Q<ScrollView>().style.height = songEditorWindow.minSize.y;
            
            //打开文件
            rootVisualElement.Q<Button>("Open").clickable.clicked += () =>
            {
                excelFilePath = EditorUtility.OpenFilePanel("选择Excel文件", "", "xlsx");
                if (string.IsNullOrWhiteSpace(excelFilePath)) return;
                InitExcelArea();
                LoadExcel();
            };
            
            //存储按钮点击
            rootVisualElement.Q<Button>("Save").clickable.clicked += () =>
            {
                if (MyExcelData == null) return;
                Debug.Log("文件存储成功 "+excelFilePath);
                SongExcelSupport.Save(excelFilePath, MyExcelData);
            };
        }

        /// <summary>
        /// 加载Excel文件
        /// </summary>
        public void LoadExcel()
        {
            MyExcelData = SongExcelSupport.ReadToExcelData(excelFilePath);
            if (MyExcelData == null) return;
            //创建全部表格按钮
            Bottom.Clear();
            var sheetDatas = MyExcelData.Sheets();
            for (var i = 0; i < sheetDatas.Count; i++)
            {
                var sheetData = sheetDatas[i];
                var sheetButton = CreateSheetButton(sheetData.name,i);

                if (i == 0)
                {
                    sheetButton.AddToClassList("SheetItemSelect");
                    NowSelectItem = sheetButton;
                }
            }

            //加载表格数据
            LoadExcel(0);
        }
        
        /// <summary>
        /// 加载子表格
        /// </summary>
        /// <param name="count"></param>
        public void LoadExcel(int count)
        {
            if (MyExcelData == null) return;
            if (count >= MyExcelData.Sheets().Count) return;
            Items.Clear();
            var sheet = MyExcelData.Sheets()[count];
            NowSelectSheet = sheet;

            if (sheet.RowCount == 0 && sheet.ColCount == 0)
            {
                sheet.Set(0,0,"");
            }
            
            for (var i = 0; i < sheet.RowCount; i++)
            {
                if (i == 0)
                {
                    var rowFirst = CreateCellLine("0");
                    CreateRowButton("",rowFirst);
                    for (var k = 0; k < sheet.ColCount; k++)
                    {
                        CreateRowButton(Index2ABC(k+1),rowFirst);
                    }
                }
                    
                var rowRoot = CreateCellLine((i+1).ToString());
                for (var k = 0; k < sheet.ColCount; k++)
                {
                    if(k==0)
                        CreateColButton((i+1).ToString(),rowRoot);
                    CreateCellButton(i+","+k,rowRoot,sheet.Get(i,k));
                }
            }
            
            //创建新增列按钮
            CreateAddRowButton();
            
            //创建新增行按钮
            CreateAddColButton();

            //绑定新增子表按钮
            CreateAddSheetButton();
        }

        /// <summary>
        /// 创建Sheet按钮
        /// </summary>
        public void CreateAddSheetButton()
        {
            if(createSheetButton!=null)return;
            createSheetButton = new Button();
            createSheetButton.AddToClassList("SheetItem");
            createSheetButton.name = "addSheet";
            createSheetButton.text = "+";
            Bottom.Add(createSheetButton);
            createSheetButton.clickable.clicked += () =>
            {
                var sheetData = new SheetData(new string[1,1])
                {
                    name = "Sheet"+MyExcelData.Sheets().Count+1
                };
                MyExcelData.Add(sheetData);
                CreateSheetButton("Sheet"+MyExcelData.Sheets().Count,MyExcelData.Sheets().Count);
                createSheetButton.BringToFront();
            };
        }
        
        /// <summary>
        /// 创建Sheet按钮
        /// </summary>
        /// <param name="sheetName">表格名称</param>
        /// <param name="index"></param>
        public Button CreateSheetButton(string sheetName,int index)
        {
            var button = new Button();
            button.AddToClassList("SheetItem");
            button.text = sheetName;
            button.name = sheetName;
            Bottom.Add(button);
            button.clickable.clicked += () => { SelectSheetButton(sheetName, index);};
            return button;
        }

        /// <summary>
        /// 选择Sheet按钮
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="index"></param>
        public void SelectSheetButton(string sheetName, int index)
        {
            NowSelectItem?.RemoveFromClassList("SheetItemSelect");
            var button = Bottom.Q<Button>(sheetName);
            button.AddToClassList("SheetItemSelect");
            NowSelectItem = button;
            LoadExcel(index);
        }

        /// <summary>
        /// 创建行按钮
        /// </summary>
        /// <param name="rowName"></param>
        /// <param name="parent"></param>
        public void CreateRowButton(string rowName,VisualElement parent)
        {
            var button = new Button();
            button.AddToClassList("TitleCell");
            button.name = rowName;
            button.text = rowName;
            parent.Add(button);
        }

        /// <summary>
        /// 添加行按钮
        /// </summary>
        /// <param name="parent"></param>
        public void CreateAddRowButton()
        {
            var button = new Button();
            button.AddToClassList("TitleCell");
            button.name = "addRow";
            button.text = "+";
            var parent = CreateCellLine((Items.childCount+1).ToString());
            parent.Add(button);
            button.clickable.clicked += () =>
            {
                var newLine = CreateCellLine((Items.childCount+1).ToString());
                parent.BringToFront();
                var i = NowSelectSheet.RowCount;
                var colCount = NowSelectSheet.ColCount;
                for (var k = 0; k < colCount; k++)
                {
                    if(k==0)
                        CreateColButton((i+1).ToString(),newLine);
                    CreateCellButton(i+","+k,newLine,"");
                }
                for (var k = 0; k < colCount; k++)
                {
                    NowSelectSheet.Set(i,k,"");
                }
            };
            button.BringToFront();
        }

        /// <summary>
        /// 创建列按钮
        /// </summary>
        /// <param name="colName"></param>
        /// <param name="parent"></param>
        public void CreateColButton(string colName,VisualElement parent)
        {
            var button = new Button();
            button.AddToClassList("TitleCell");
            button.name = colName;
            button.text = colName;
            parent.Add(button);
        }

        /// <summary>
        /// 添加列按钮
        /// </summary>
        /// <param name="parent"></param>
        public void CreateAddColButton()
        {
            var button = new Button();
            button.AddToClassList("TitleCell");
            button.name = "addRow";
            button.text = "+";
            var parent = Items.Q<VisualElement>("0") ?? CreateCellLine("0");
            parent.Add(button);
            button.clickable.clicked += () =>
            {
                var colCount = NowSelectSheet.ColCount;
                var rowCount = NowSelectSheet.RowCount;
                var index = 0;
                foreach (var visualElement in Items.Children())
                {
                    if (index == 0)
                    {
                        CreateRowButton(Index2ABC(colCount+1),visualElement);
                        button.BringToFront();
                        index++;
                        continue;
                    }
                    if(index == colCount) continue;
                    CreateCellButton((index-1)+","+visualElement.childCount,visualElement,"");
                }
                
                for (var k = 0; k < rowCount; k++)
                {
                    NowSelectSheet.SetCol(colCount,"");
                }
                
            };
            button.BringToFront();
        }
        
        
        /// <summary>
        /// 创建一行
        /// </summary>
        /// <returns></returns>
        public VisualElement CreateCellLine(string index)
        {
            var visualElement = new VisualElement();
            visualElement.AddToClassList("CellLine");
            visualElement.name = index;
            Items.Add(visualElement);
            return visualElement;
        }
        
        /// <summary>
        /// 创建单元格按钮
        /// </summary>
        public void CreateCellButton(string cellContent,VisualElement parent,string content ="")
        {
            var textField = new TextField
            {
                label = "",
                value = content
            };
            textField.AddToClassList("Cell");
            textField.name = cellContent;
            parent.Add(textField);
            
            textField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                var po = cellContent.Split(",");
                int.TryParse(po[0], out var x);
                int.TryParse(po[1], out var y);
                NowSelectSheet.Set(x,y,evt.newValue);
            });
        }
        
        public string Index2ABC(int i)
        {
            var s = "";
            while (i > 0)
            {
                i--;
                s = (char)('A' + i % 26) + s;
                i /= 26;
            }
            return s;
        }
    }
}