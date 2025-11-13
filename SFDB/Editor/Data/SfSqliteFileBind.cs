using System.IO;
using SFramework.SFDb.Editor.Window;
using SFramework.SFTask.Editor.Window;
using UnityEditor;
using UnityEngine;

namespace SFramework.SFDb.Editor.Data
{
    /// <summary>
    /// 数据库文件绑定图标
    /// </summary>
    [InitializeOnLoad]
    public class SfSqliteFileBind
    {
        private static Texture2D s_Icon;

        // 图标路径保持不变
        private static string IconPath => "Assets/SFramework/SFDB/Editor/Data/DatabaseData.png";
        
        // 静态构造函数，在 Unity 加载时运行一次
        static SfSqliteFileBind()
        {
            s_Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);

            // 如果找不到图标，就打印一个提示，避免后续出错
            if (s_Icon == null)
            {
                Debug.LogWarning($"[SfDatabaseFileBind] 无法在路径 '{IconPath}' 找到 .db 文件图标。请检查路径和图标的导入设置。");
                return;
            }

            // 注册一个回调：每当 Project 窗口绘制一个条目时，都会调用 DrawIcon 方法
            EditorApplication.projectWindowItemOnGUI += DrawIcon;
        }

        /// <summary>
        /// 在 Project 窗口绘制自定义图标
        /// </summary>
        private static void DrawIcon(string guid, Rect selectionRect)
        {
            // 如果我们没有加载图标，就退出
            if (s_Icon == null)
                return;

            // 通过 GUID 获取资产路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 检查文件后缀是否是 .db
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".db", System.StringComparison.OrdinalIgnoreCase))
            {
                return; // 不是 .db 文件，不处理
            }

            // --- 绘制图标逻辑保持不变 ---
            if (selectionRect.height <= 20) 
            {
                // List View (列表视图)
                Rect iconRect = selectionRect;
                iconRect.width = 16;  // 强制为 16x16
                iconRect.height = 16;
            
                // 在 Unity 默认图标的位置，绘制我们的自定义图标
                GUI.DrawTexture(iconRect, s_Icon);
            }
            else
            {
                // Grid View (网格视图)
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
        
        // 监听双击文件事件 (优先级 0)
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        public static bool OnOpenDbFile(int instanceID, int line)
        {
            // 1. 获取被双击的资产的路径
            var path = AssetDatabase.GetAssetPath(instanceID);
            
            // 2. 检查文件扩展名是否为 .db
            if (Path.GetExtension(path)?.ToLower() == ".db")
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                SfDbWindow.OpenSfDbWindow(fileName);
                return true; 
            }

            // 返回 false 表示文件不是 .db，继续让 Unity 处理
            return false;
        }
    }
}