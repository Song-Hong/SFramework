using System.IO;
using SFramework.SFTask.Editor.Window;
using UnityEditor;
using UnityEngine;

// [InitializeOnLoad] 属性让这个类在 Unity 编辑器启动时自动运行
namespace SFramework.SFTask.Editor.Data
{
    [InitializeOnLoad]
    public class SfTaskIconHandler
    {
        private static Texture2D s_Icon;

        private static string IconPath => "Assets/SFramework/SFTask/Editor/Data/TaskFile.png";
        
        // 静态构造函数，在 Unity 加载时运行一次
        static SfTaskIconHandler()
        {
            s_Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);

            // 如果找不到图标，就打印一个提示，避免后续出错
            if (s_Icon == null)
            {
                Debug.LogWarning($"[SFTaskIconHandler] 无法在路径 '{IconPath}' 找到 .sftask 图标。请检查路径和图标的导入设置。");
                return;
            }

            // 注册一个回调：每当 Project 窗口绘制一个条目时，都会调用 DrawIcon 方法
            EditorApplication.projectWindowItemOnGUI += DrawIcon;
        }

        /// <summary>
        /// 在 Project 窗口绘制图标
        /// </summary>
        private static void DrawIcon(string guid, Rect selectionRect)
        {
            // 如果我们没有加载图标，就退出
            if (s_Icon == null)
                return;

            // 通过 GUID 获取资产路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 检查文件后缀是否是 .sftask
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".sftask", System.StringComparison.OrdinalIgnoreCase))
            {
                return; // 不是 .sftask 文件，不处理
            }

            // --- 绘制图标 ---
            if (selectionRect.height <= 20) 
            {
                // List View
                Rect iconRect = selectionRect;
                iconRect.width = 16;  // 强制为 16x16
                iconRect.height = 16;
            
                // 在 Unity 默认图标的位置，绘制我们的自定义图标
                GUI.DrawTexture(iconRect, s_Icon);
            }
            else
            {
                // Grid View (双列, 大图标)
                // 在 Grid 视图中, selectionRect 是整个图标+标签的矩形
                // 我们需要计算出图标的实际绘制区域
            
                // 图标区域通常是位于顶部的一个方形
                Rect iconRect = new Rect(
                    selectionRect.x, 
                    selectionRect.y, 
                    selectionRect.width, 
                    selectionRect.width // 图标区域是方形的 (宽度 = 高度)
                ); 
            
                // 在计算出的图标区域绘制我们的图标，并让它自动缩放
                GUI.DrawTexture(iconRect, s_Icon, ScaleMode.ScaleToFit);
            }
        }
        
        // 监听双击文件事件 (保持不变)
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        public static bool OnOpenTaskFile(int instanceID, int line)
        {
            // 1. 获取被双击的资产的路径
            var path = AssetDatabase.GetAssetPath(instanceID);
            
            // 2. 检查文件扩展名是否为 .sftask
            if (Path.GetExtension(path)?.ToLower() == ".sftask")
            {
                // 3. 读取文件内容 (JSON 字符串)
                var jsonText = File.ReadAllText(path);

                // 4. 打开自定义编辑器窗口
                // 假设您的任务图编辑器窗口类名为 SfTaskWindow
                var window = EditorWindow.GetWindow<SfTaskWindow>(false, "任务图编辑器");
                window.titleContent = new GUIContent(Path.GetFileNameWithoutExtension(path), AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath));

                // 5. 调用 ImportTaskFile 方法进行加载
                // 假设 SfTaskWindow 有一个 GetGraphView() 方法
                if (window.GetGraphView() != null)
                {
                    window.GetGraphView().ImportTaskFile(jsonText,path);
                }
                
                // 6. 返回 true 表示事件已处理，Unity 不会执行默认操作
                return true; 
            }

            // 返回 false 表示文件不是 .sftask，继续让 Unity 处理
            return false;
        }
    }
}