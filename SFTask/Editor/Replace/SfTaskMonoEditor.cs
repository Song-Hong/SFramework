using System.IO;
using SFramework.Core.SfUIElementExtends;
using SFramework.SFTask.Editor.Window;
using SFramework.SFTask.Mono;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.Replace
{
    /// <summary>
    /// 任务模块Mono单例编辑器
    /// </summary>
    [CustomEditor(typeof(SfTaskMono))]
    public class SfTaskMonoEditor:UnityEditor.Editor
    {
        /// <summary>
        /// 根元素
        /// </summary>
        private VisualElement _rootElement;
        
        /// <summary>
        /// 任务模块Mono单例图标路径
        /// </summary>
        private static string IconPath => "Assets/SFramework/SFTask/Editor/Data/TaskFile.png";
        
        /// <summary>
        /// 创建Inspector GUI
        /// </summary>
        /// <returns>Inspector GUI</returns>
        public override VisualElement CreateInspectorGUI()
        {
            // 创建根元素
            _rootElement = new VisualElement();
            
            // 添加标题
            var title = new Label("SFramework 任务模块")
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            _rootElement.Add(title);
            
            // 添加任务模块Mono单例引用
            var taskMono = target as SfTaskMono;
            if (taskMono == null) return null;
            
            // 自定义资源加载类型
            var assetsPathType = new SfTab();
            assetsPathType.SetTitle("资源加载:");
            assetsPathType.AddChoice("StrAssets","PerData","Url");
            assetsPathType.OnChoiceChanged += choice =>
            {
                taskMono.assetsPathType = choice switch
                {
                    "StrAssets" => taskMono.assetsPathType = SfTaskMono.AssetsPathType.StreamingAssets,
                    "PerData" => taskMono.assetsPathType = SfTaskMono.AssetsPathType.PersistentData,
                    _ => taskMono.assetsPathType = SfTaskMono.AssetsPathType.Url,
                };
                serializedObject.ApplyModifiedProperties();
            };
            assetsPathType.Select(taskMono.assetsPathType switch
            {
                SfTaskMono.AssetsPathType.StreamingAssets => "StrAssets",
                SfTaskMono.AssetsPathType.PersistentData => "PerData",
                _ => "Url",
            });
            _rootElement.Add(assetsPathType);
            
            // 添加任务启动类型
            var taskStartType = new SfTab();
            taskStartType.SetTitle("是否自动启动任务:");
            taskStartType.AddChoice("自动","手动");
            taskStartType.OnChoiceChanged += choice =>
            {
                if (taskMono != null)
                    taskMono.taskStartType = choice switch
                    {
                        "自动" => taskMono.taskStartType = SfTaskMono.TaskStartType.Auto,
                        _ => taskMono.taskStartType = SfTaskMono.TaskStartType.Manual,
                    };
                serializedObject.ApplyModifiedProperties();
            };
            taskStartType.Select(taskMono.taskStartType switch
            {
                SfTaskMono.TaskStartType.Auto => "自动",
                _ => "手动",
            });
            _rootElement.Add(taskStartType);
            
            // assetsPath
            var assetsPath = new TextField("资源路径:")
            {
                style =
                {
                    color = Color.white,
                    marginTop = 8
                }
            };
            var label = assetsPath.Q<Label>();
            label.style.color = Color.white;
            label.style.fontSize = 14;
            assetsPath.RegisterValueChangedCallback(x =>
            {
                if (taskMono != null)
                    taskMono.assetsPath = x.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            assetsPath.value = taskMono.assetsPath;
            _rootElement.Add(assetsPath);
            
            // 编辑任务
            var button = new Button
            {
                text = "编辑任务",
                style =
                {
                    marginTop = 8,
                    fontSize = 14,
                    color = Color.white,
                }
            };
            button.clicked += () =>
            {
                // 检查文件名是否为空
                var path = taskMono.assetsPath;
                if (string.IsNullOrWhiteSpace(path))
                {
                    Debug.LogError("文件名为空");
                    return;
                }
                // 检查资源路径类型是否为Url
                if (taskMono.assetsPathType == SfTaskMono.AssetsPathType.Url)
                {
                    Debug.LogError("网络资源不支持编辑");
                    return;
                }
                //拼接字符串
                var dirPath = taskMono.assetsPathType switch
                {
                    SfTaskMono.AssetsPathType.StreamingAssets => $"{Application.streamingAssetsPath}/",
                    SfTaskMono.AssetsPathType.PersistentData => $"{Application.persistentDataPath}/",
                    _ => ""
                };
                if(!path.EndsWith(".sftask"))
                    path += ".sftask";
                path = dirPath + path;
                // 打开编辑期
                var jsonText = File.ReadAllText(path);
                var window = EditorWindow.GetWindow<SfTaskWindow>(false, "任务图编辑器");
                window.titleContent = new GUIContent(
                    Path.GetFileNameWithoutExtension(path), 
                    AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath));
                if (window.GetGraphView() != null)
                {
                    window.GetGraphView().ImportTaskFile(jsonText,path);
                }
            };
            _rootElement.Add(button);

            return _rootElement;
        }

        /// <summary>
        /// 显示任务
        /// </summary>
        public void ViewTasks()
        {
            
        }
    }
}