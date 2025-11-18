using SFramework.SFDB.Mono;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFDb.Editor.Replace
{
    /// <summary>
    /// 数据库模块Mono单例引用
    /// </summary>
    [CustomEditor(typeof(SfSqliteMono))]
    public class SfSqliteMonoEditor:UnityEditor.Editor
    {
        /// <summary>
        /// 根元素
        /// </summary>
        private VisualElement _rootElement;
        
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
            
            // 添加Sqlite模块Mono单例引用
            var sqliteMono = target as SfSqliteMono;
            if (sqliteMono == null) return null;
            
            // 资源路径
            var assetsPath = new TextField("数据库名:")
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
                if (sqliteMono != null)
                    sqliteMono.dbName = x.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            assetsPath.value = sqliteMono.dbName;
            _rootElement.Add(assetsPath);

            return _rootElement;
        }
    }
}