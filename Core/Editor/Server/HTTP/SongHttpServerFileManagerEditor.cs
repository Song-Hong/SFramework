using System.Collections.Generic;
using Song.Core.Support.HTTP;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace SFramework.Core.Editor.Server.HTTP
{
    /// <summary>
    /// HTTP 接口文件管理器 编辑器样式
    /// </summary>
    [CustomEditor(typeof(SongHttpServerFileManager))]
    public class SongHttpServerFileManagerEditor:UnityEditor.Editor
    {
        /// <summary>
        /// 创建Inspector视图
        /// </summary>
        /// <returns></returns>
        public override VisualElement CreateInspectorGUI()
        {
            //获取目标对象
            var songHttpServerFileManager = target as SongHttpServerFileManager;

            //根节点
            var root = new VisualElement();
            
            //标题
            var label = new Label()
            {
                text = "HTTP文件管理服务器",
                style =
                {
                    fontSize = 14,
                    alignSelf = Align.Center,
                }
            };
            root.Add(label);
            
            //间隔
            Space(root);
            
            //下拉选择框
            var dropdownField = new DropdownField()
            {
                label = "路径类型",
                choices = new List<string>()
                {
                    "Assets",
                    "StreamingAssets",
                    "PersistentDataPath",
                    "CustomDataPath",
                },
                value = songHttpServerFileManager.fileType.ToString(),
            };
            root.Add(dropdownField);
            //添加监听事件
            dropdownField.RegisterValueChangedCallback((evt) =>
            {
                songHttpServerFileManager.fileType = (SongHttpServerFileManager.FileType)dropdownField.index;
                
                // 创建自定义路径输入框
                CreateCustomDataPath(root,songHttpServerFileManager);
            });
            
            // 创建自定义路径输入框
            CreateCustomDataPath(root,songHttpServerFileManager);
            
            return root;
        }
        
        /// <summary>
        /// 添加间隔
        /// </summary>
        /// <param name="root">根节点</param>
        public void Space(VisualElement root)
        {
            root.Add(new VisualElement()
            {
                style =
                {
                    height = 10,
                }
            });
        }
        
        /// <summary>
        /// 创建自定义文件路径
        /// </summary>
        /// <param name="root">根节点</param>
        /// <param name="songHttpServerFileManager">文件管理器</param>
        public void CreateCustomDataPath(VisualElement root,SongHttpServerFileManager songHttpServerFileManager)
        {
            //间隔
            var space = new VisualElement()
            {
                name = "CustomDataPathSpace",
                style =
                {
                    height = 5,
                }
            };
            root.Add(space);
            
            //文件路径
            if (songHttpServerFileManager.fileType == SongHttpServerFileManager.FileType.CustomDataPath)
            {
                var textField = new TextField()
                {
                    name = "CustomDataPath",
                    label = "文件路径",
                    value = songHttpServerFileManager.filePath,
                };
                root.Add(textField);
                //添加监听事件
                textField.RegisterValueChangedCallback((evt) =>
                {
                    songHttpServerFileManager.filePath = evt.newValue;
                });
            }
            else
            {
                var visualElement = root.Q<VisualElement>("CustomDataPath");
                if (visualElement != null)
                {
                    root.Remove(visualElement);
                }
                var visualElementSpace = root.Q<VisualElement>("CustomDataPathSpace");
                if (visualElementSpace != null)
                {
                    root.Remove(visualElementSpace);
                }
            }
        }
    }
}