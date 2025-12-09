using System;
using System.Collections.Generic;
using System.Linq;
using SFramework.Core.Editor.Support;
using SFramework.SFTask.Data;
using SFramework.SFTask.Editor.NodeStyle;
using SFramework.SFTask.Module;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFramework.SFTask.Editor.View
{
    /// <summary>
    /// 任务图视图数据库
    /// </summary>
    public partial class SfTaskGraphView:GraphView
    {
        /// <summary>
        /// 节点数据库
        /// </summary>
        public static List<Tuple<string,string>> Nodes = new List<Tuple<string,string>> ();
        
        [Serializable]
        public class SingleTaskPointDataWrapper
        {
            public SfTaskPointData taskPointData;
        }
        
        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void InitNodes()
        {
            // 初始化节点数据库
            foreach (var runtimeSubclass in SfReflection.GetRuntimeSubclasses<SfTaskNode>())
            {
                var createdInstance = ScriptableObject.CreateInstance(runtimeSubclass) as SfTaskNode;
                if (createdInstance == null) continue;
                Nodes.Add(new Tuple<string,string>(
                    createdInstance.GetTaskNodeName(),
                    createdInstance.GetType().FullName));
            }
        }
        
        /// <summary>
        /// 将指定节点的数据序列化并存储到系统剪贴板。
        /// </summary>
        /// <param name="targetNode">要复制的 SfTaskNodePointEditor 节点</param>
        public void CopyToClipboard(SfTaskNodePointEditor targetNode)
        {
            if (targetNode == null) return;

            try
            {
                // 1. 将节点数据转换为 SfTaskPointData
                var sfTaskPointData = new SfTaskPointData()
                {
                    x = targetNode.GetPosition().x + 20, // 复制时略微偏移，防止重叠
                    y = targetNode.GetPosition().y + 20,
                    title = targetNode.title + " (Copy)", // 标记为副本
                    type = targetNode.GetTaskType(),
                };
                
                // 2. 遍历并收集内部任务组件数据
                var sfTaskNodeTaskViews = targetNode.GetTaskComponents();
                foreach (var sfTaskNodeTaskView in sfTaskNodeTaskViews)
                {
                    List<SfTaskFieldData> fields = new List<SfTaskFieldData>();
                    if (sfTaskNodeTaskView.SerializedTarget != null)
                    {
                        var it = sfTaskNodeTaskView.SerializedTarget.GetIterator();
                        bool enterChildren = true;
                        while (it.NextVisible(enterChildren))
                        {
                            if (it.propertyPath == "m_Script") { enterChildren = false; continue; }
                            fields.Add(new SfTaskFieldData
                            {
                                fieldName = it.propertyPath,
                                fieldType = it.propertyType.ToString(),
                                fieldValue = SerializePropertyValue(it)
                            });
                            enterChildren = false;
                        }
                    }
                    

                    sfTaskPointData.tasks.Add(new SfTaskData()
                    {
                        taskName = sfTaskNodeTaskView.TitleLabel.text,
                        fields = fields,
                        taskType = sfTaskNodeTaskView.TaskType,
                    });
                }
                
                // 3. 包装并序列化为 JSON
                var wrapper = new SingleTaskPointDataWrapper { taskPointData = sfTaskPointData };
                string json = JsonUtility.ToJson(wrapper, true);

                // 4. 存储到系统剪贴板
                EditorGUIUtility.systemCopyBuffer = json;
            }
            catch (Exception e)
            {
                Debug.LogError($"复制节点失败: {e.Message}");
            }
        }

        /// <summary>
        /// 从系统剪贴板读取数据并创建新节点。
        /// </summary>
        /// <param name="mousePosition">右键菜单触发时的鼠标位置，用于放置新节点</param>
        public void PasteFromClipboard(Vector2 mousePosition)
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("剪贴板中没有可粘贴的任务节点数据。");
                return;
            }

            SingleTaskPointDataWrapper wrapper;
            try
            {
                // 1. 反序列化 JSON
                wrapper = JsonUtility.FromJson<SingleTaskPointDataWrapper>(json);
            }
            catch
            {
                Debug.LogError("剪贴板中的数据无法解析为任务节点格式。");
                return;
            }

            if (wrapper?.taskPointData == null) return;
            
            // 2. 获取数据并根据鼠标位置设置新位置
            var pointData = wrapper.taskPointData;
            
            // 将节点位置设置为鼠标点击位置（视图坐标）
            pointData.x = mousePosition.x;
            pointData.y = mousePosition.y;
            
            // 创建 SfTaskNodePointEditor
            var newNode = new SfTaskNodePointEditor(pointData.title, new Vector2(pointData.x, pointData.y));
            foreach (var taskData in pointData.tasks)
            {
                var type = ResolveType(taskData.taskType);
                if (type == null) continue;
                var so = ScriptableObject.CreateInstance(type) as SFramework.SFTask.Module.SfTaskNode;
                if (so == null) continue;
                var serialized = new SerializedObject(so);
                ApplyFieldsToSerializedObject(serialized, taskData.fields);
                serialized.ApplyModifiedProperties();

                var sfTaskNodeTaskView = new SfTaskNodeTaskView();
                sfTaskNodeTaskView.TitleLabel.text = taskData.taskName;
                sfTaskNodeTaskView.TaskType = taskData.taskType;
                sfTaskNodeTaskView.Init(serialized);

                newNode.TaskContainerAdd(sfTaskNodeTaskView);
            }
            
            // 5. 将新节点添加到 GraphView
            AddElement(newNode);
        }
        
        /// <summary>
        /// 从系统剪贴板读取任务数据，并覆盖/追加到指定的任务点节点上。
        /// </summary>
        /// <param name="targetNode">要接收数据的 SfTaskNodePointEditor 节点</param>
        public void PasteDataToTargetNode(SfTaskNodePointEditor targetNode)
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("剪贴板中没有可粘贴的任务节点数据。");
                return;
            }

            SingleTaskPointDataWrapper wrapper;
            try
            {
                // 1. 反序列化 JSON
                wrapper = JsonUtility.FromJson<SingleTaskPointDataWrapper>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"剪贴板中的数据无法解析为任务节点格式: {e.Message}");
                return;
            }

            if (wrapper?.taskPointData == null) return;

            // 3. 遍历并创建新的任务组件 (SfTaskNodeTaskView)
            foreach (var taskData in wrapper.taskPointData.tasks)
            {
                var type = ResolveType(taskData.taskType);
                if (type == null) continue;
                var so = ScriptableObject.CreateInstance(type) as SFramework.SFTask.Module.SfTaskNode;
                if (so == null) continue;
                var serialized = new SerializedObject(so);
                ApplyFieldsToSerializedObject(serialized, taskData.fields);
                serialized.ApplyModifiedProperties();

                var sfTaskNodeTaskView = new SfTaskNodeTaskView();
                sfTaskNodeTaskView.TitleLabel.text = taskData.taskName;
                sfTaskNodeTaskView.TaskType = taskData.taskType;
                sfTaskNodeTaskView.Init(serialized);

                targetNode.TaskContainerAdd(sfTaskNodeTaskView);
            }
        }
        
        /// <summary>
        /// 检查系统剪贴板中是否有有效的任务节点数据 JSON 字符串。
        /// </summary>
        /// <returns>如果数据存在且格式正确，返回 true；否则返回 false。</returns>
        public bool IsTaskDataInClipboard()
        {
            string json = EditorGUIUtility.systemCopyBuffer;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }
    
            // 尝试进行快速解析，验证数据是否为我们的 SingleTaskPointDataWrapper 格式
            try
            {
                // 尝试反序列化
                var wrapper = JsonUtility.FromJson<SingleTaskPointDataWrapper>(json);
        
                // 检查是否成功解析且包含数据
                return wrapper != null && wrapper.taskPointData != null;
            }
            catch
            {
                // 解析失败，不是我们想要的数据
                return false;
            }
        }

        private void ApplyFieldsToSerializedObject(SerializedObject so, List<SfTaskFieldData> fields)
        {
            if (fields == null) return;
            foreach (var f in fields)
            {
                var p = so.FindProperty(f.fieldName);
                if (p == null) continue;
                try
                {
                    switch (p.propertyType)
                    {
                        case SerializedPropertyType.Integer:
                            if (int.TryParse(f.fieldValue, out var iv)) p.intValue = iv; break;
                        case SerializedPropertyType.Float:
                            if (float.TryParse(f.fieldValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fv)) p.floatValue = fv; break;
                        case SerializedPropertyType.Boolean:
                            if (bool.TryParse(f.fieldValue, out var bv)) p.boolValue = bv; break;
                        case SerializedPropertyType.String:
                            p.stringValue = f.fieldValue ?? string.Empty; break;
                        case SerializedPropertyType.Enum:
                            if (p.enumDisplayNames != null)
                            {
                                var idx = Array.IndexOf(p.enumDisplayNames, f.fieldValue);
                                p.enumValueIndex = idx >= 0 ? idx : p.enumValueIndex;
                            }
                            break;
                        case SerializedPropertyType.Vector2:
                            p.vector2Value = JsonUtility.FromJson<Vector2>(f.fieldValue);
                            break;
                        case SerializedPropertyType.Vector3:
                            p.vector3Value = JsonUtility.FromJson<Vector3>(f.fieldValue);
                            break;
                        case SerializedPropertyType.Color:
                            p.colorValue = JsonUtility.FromJson<Color>(f.fieldValue);
                            break;
                        case SerializedPropertyType.ObjectReference:
                            if (string.IsNullOrEmpty(f.fieldValue)) break;
                            if (f.fieldValue.StartsWith("scene-id://", StringComparison.OrdinalIgnoreCase))
                            {
                                var id = f.fieldValue.Substring("scene-id://".Length);
                                var all = UnityEngine.Object.FindObjectsOfType<SFramework.SFTask.Module.SfUniqueId>(true);
                                var hit = all.FirstOrDefault(x => x.Id == id);
                                if (hit != null)
                                {
                                    var typeName = p.type;
                                    var go = hit.gameObject;
                                    var t = System.Type.GetType(typeName);
                                    if (t == typeof(UnityEngine.GameObject) || t == null)
                                        p.objectReferenceValue = go;
                                    else
                                        p.objectReferenceValue = go.GetComponent(t);
                                }
                                break;
                            }
                            if (f.fieldValue.StartsWith("scene://", StringComparison.OrdinalIgnoreCase))
                            {
                                var rel = f.fieldValue.Substring("scene://".Length);
                                var go = UnityEngine.GameObject.Find(rel);
                                if (go == null)
                                {
                                    go = FindGameObjectByPathIncludingInactive(rel);
                                }
                                if (go != null)
                                {
                                    var typeName = p.type;
                                    var t = System.Type.GetType(typeName);
                                    if (t == typeof(UnityEngine.GameObject) || t == null)
                                        p.objectReferenceValue = go;
                                    else
                                        p.objectReferenceValue = go.GetComponent(t);
                                }
                                break;
                            }
                            {
                                var path = AssetDatabase.GUIDToAssetPath(f.fieldValue);
                                if (string.IsNullOrEmpty(path))
                                {
                                    var res = UnityEngine.Resources.Load(f.fieldValue);
                                    if (res != null) { p.objectReferenceValue = res; break; }
                                }
                                if (!string.IsNullOrEmpty(path))
                                {
                                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                                    p.objectReferenceValue = obj;
                                }
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static GameObject FindGameObjectByPathIncludingInactive(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var parts = path.Split('/');
            for (int si = 0; si < SceneManager.sceneCount; si++)
            {
                var scene = SceneManager.GetSceneAt(si);
                if (!scene.isLoaded) continue;
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    if (!string.Equals(root.name, parts[0], StringComparison.Ordinal)) continue;
                    var current = root.transform;
                    bool matched = true;
                    for (int i = 1; i < parts.Length; i++)
                    {
                        Transform next = null;
                        for (int c = 0; c < current.childCount; c++)
                        {
                            var child = current.GetChild(c);
                            if (string.Equals(child.name, parts[i], StringComparison.Ordinal))
                            {
                                next = child;
                                break;
                            }
                        }
                        if (next == null)
                        {
                            matched = false;
                            break;
                        }
                        current = next;
                    }
                    if (matched) return current.gameObject;
                }
            }
            return null;
        }

        private static Type ResolveType(string fullName)
        {
            var t = Type.GetType(fullName);
            if (t != null) return t;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetType(fullName, false, true);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }
    }
}
