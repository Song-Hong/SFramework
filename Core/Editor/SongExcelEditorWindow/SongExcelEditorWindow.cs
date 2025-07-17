using System.IO;
using Song.Core.Extends.Excel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.Core.Editor.SongExcelEditorWindow
{
    /// <summary>
    /// Excel编辑器
    /// </summary>
    public partial class SongExcelEditorWindow:EditorWindow
    {
        [MenuItem("Song/Excel编辑器")]
        public static void ShowExcelEditor()
        {
            var songEditorWindow = GetWindow<SongExcelEditorWindow>();
            songEditorWindow.titleContent = new GUIContent("Excel编辑器");
            songEditorWindow.Show();
        }

        /// <summary>
        /// Excel文件位置
        /// </summary>
        public string excelFilePath = "";
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void CreateGUI()
        {
            rootVisualElement.Clear();
            //加载UI文件
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SFramework/Core/Editor/SongExcelEditorWindow/SongEditorWindow.uxml");
            root.CloneTree(rootVisualElement);
            
            //设置是否打开文件
            if (string.IsNullOrWhiteSpace(excelFilePath))
            {
                rootVisualElement.Remove(rootVisualElement.Q<VisualElement>("ExcelArea"));
                
                //打开文件
                rootVisualElement.Q<Button>("OpneFile").clickable.clicked += () =>
                {
                    excelFilePath = EditorUtility.OpenFilePanel("选择Excel文件", "", "xlsx");
                    if (string.IsNullOrWhiteSpace(excelFilePath)) return;
                    CreateGUI();
                };
                
                rootVisualElement.Q<VisualElement>("OpenFile").RegisterCallback<DragUpdatedEvent>(evt =>
                {
                    // 检查是否有文件被拖拽
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic; // 可以根据需要修改反馈模式
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.None;
                    }
                });
                
                //拖拽结束事件
                rootVisualElement.Q<VisualElement>("OpenFile").RegisterCallback<DragPerformEvent>(evt =>
                {
                    // 确保有文件被拖拽
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        foreach (string path in DragAndDrop.paths)
                        {
                            if (!path.Contains(".xlsx")) continue;
                            excelFilePath = path;
                            CreateGUI();
                            break;
                        }
                    }
                    DragAndDrop.AcceptDrag(); // 接受拖拽操作
                });
            }
            else
            {
                rootVisualElement.Remove(rootVisualElement.Q<VisualElement>("OpenFile"));
                
                //初始化
                InitExcelArea();
                //加载文件
                LoadExcel();
            }
        }
    }
}