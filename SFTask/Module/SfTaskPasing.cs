using System;
using System.Collections.Generic;
using SFramework.SFTask.Data;
using UnityEngine;
using System.Reflection;
using System.Linq; // 用于简化 LINQ 操作
using UnityEngine.SceneManagement;

namespace SFramework.SFTask.Module
{
    /// <summary>
    /// 任务解析模块 (运行时)
    /// </summary>
    public static class SfTaskParsing
    {
        /// <summary>
        /// 解析任务字符串
        /// </summary>
        /// <param name="taskString">任务字符串</param>
        /// <returns>返回解析后的第一个任务点</returns>
        public static List<SfTaskPoint> ParseTask(string taskString)
        {
            // 解析任务字符串为任务列表数据
            List<SfTaskPoint> tasks;
            // 解析任务字符串为任务列表数据
            SfTaskListData taskListData;
            try
            {
                taskListData = JsonUtility.FromJson<SfTaskListData>(taskString);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SFTaskParsing] 任务 JSON 反序列化失败: {e.Message}");
                return null;
            }

            if (taskListData == null || taskListData.tasks == null || taskListData.tasks.Count == 0)
            {
                Debug.LogWarning("[SFTaskParsing] 任务数据为空或无效。");
                return null;
            }

            // 使用 LINQ 筛选掉 StartNode 并转换为 List<SfTaskPoint>
            tasks = taskListData.tasks
                .Where(data => data.type != "StartNode") // 忽略编辑器起始节点
                .Select(sfTaskPointData => CreateTaskPoint(sfTaskPointData))
                .ToList();

            // 返回列表中的第一个有效任务点
            return tasks;
        }

        /// <summary>
        /// 辅助函数：根据数据创建并初始化 SfTaskPoint 及其内部的 SfTaskNode 列表
        /// </summary>
        private static SfTaskPoint CreateTaskPoint(SfTaskPointData sfTaskPointData)
        {
            // --- 1. 创建运行时任务点 (SfTaskPoint) ---
            var currentTaskPoint = new SfTaskPoint
            {
                title = sfTaskPointData.title,
                Type = sfTaskPointData.type switch
                {
                    "Sequential" => SfTaskPointType.Sequential,
                    "Parallel" => SfTaskPointType.Parallel,
                    _ => SfTaskPointType.Sequential
                },
                Tasks = new List<SfTaskNode>() // 确保初始化列表
            };

            // --- 2. 解析该任务点下的所有具体任务 (SfTaskNode) ---
            foreach (var sfTaskNodeData in sfTaskPointData.tasks)
            {
                var type = GetTypeFromAllAssemblies(sfTaskNodeData.taskType);
                if (type == null)
                {
                    Debug.LogError($"[SFTaskParsing] 无法在程序集中找到类型: {sfTaskNodeData.taskType}");
                    continue;
                }

                if (!typeof(SfTaskNode).IsAssignableFrom(type))
                {
                    Debug.LogError($"[SFTaskParsing] 类型 {sfTaskNodeData.taskType} 不是 SfTaskNode 的子类");
                    continue;
                }

                var sfTaskNode = ScriptableObject.CreateInstance(type) as SfTaskNode;
                if (sfTaskNode == null)
                {
                    Debug.LogError($"[SFTaskParsing] 创建实例失败: {sfTaskNodeData.taskType}");
                    continue;
                }
                
                // --- 3. 【核心】通过反射为 sfTaskNode 赋值 ---
                sfTaskNodeData.fields.ForEach(fieldData => { AssignField(sfTaskNode, fieldData); });

                currentTaskPoint.Tasks.Add(sfTaskNode);
            }

            return currentTaskPoint;
        }

        /// <summary>
        /// 辅助方法：使用反射为 SfTaskNode 的字段/属性赋值
        /// </summary>
        private static void AssignField(SfTaskNode node, SfTaskFieldData fieldData)
        {
            Type type = node.GetType();
            try
            {
                // 1. 尝试获取字段 (Field)
                FieldInfo fieldInfo = type.GetField(fieldData.fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    object value = ParseFieldValue(fieldData.fieldValue, fieldInfo.FieldType);
                    if (value != null) fieldInfo.SetValue(node, value);
                    return;
                }

                // 2. 尝试获取属性 (Property)
                PropertyInfo propInfo =
                    type.GetProperty(fieldData.fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null && propInfo.CanWrite)
                {
                    object value = ParseFieldValue(fieldData.fieldValue, propInfo.PropertyType);
                    if (value != null) propInfo.SetValue(node, value);
                    return;
                }

                Debug.LogWarning($"[SFTaskParsing] 在 {type.Name} 中未找到可写的公共字段或属性: {fieldData.fieldName}");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"[SFTaskParsing] 为 {type.Name}.{fieldData.fieldName} 赋值失败 (值: '{fieldData.fieldValue}'): {e.Message}");
            }
        }


        /// <summary>
        /// 【运行时安全】将序列化(string)的值转换回其原始的 C# object 类型
        /// (假设 UnityEngine.Object 使用 Resources 路径序列化)
        /// </summary>
        private static object ParseFieldValue(string stringValue, Type targetType)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                // 对于值类型 (int, float, bool) 应该返回 default(T)，但反射会处理 null
                return null;
            }

            try
            {
                // --- 简单类型 ---
                if (targetType == typeof(string)) return stringValue;
                if (targetType == typeof(int)) return int.Parse(stringValue);
                if (targetType == typeof(float))
                    return float.Parse(stringValue, System.Globalization.CultureInfo.InvariantCulture);
                if (targetType == typeof(bool)) return bool.Parse(stringValue);

                // --- 枚举 ---
                if (targetType.IsEnum) return Enum.Parse(targetType, stringValue, true);

                // --- JSON 序列化类型 (Vector3, Vector2, Color) ---
                if (targetType == typeof(Vector3) || targetType == typeof(Vector2) || targetType == typeof(Color))
                {
                    return JsonUtility.FromJson(stringValue, targetType);
                }

                // --- 关键的运行时对象加载 (Resources.Load) ---
                if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
                {
                    if (stringValue.StartsWith("scene-id://", StringComparison.OrdinalIgnoreCase))
                    {
                        var id = stringValue.Substring("scene-id://".Length);
                        var all = UnityEngine.Object.FindObjectsOfType<SFramework.SFTask.Module.SfUniqueId>(true);
                        var hit = all.FirstOrDefault(x => x.Id == id);
                        if (hit == null) return null;
                        var go = hit.gameObject;
                        if (targetType == typeof(GameObject)) return go;
                        return go.GetComponent(targetType);
                    }
                    if (stringValue.StartsWith("scene://", StringComparison.OrdinalIgnoreCase))
                    {
                        var rel = stringValue.Substring("scene://".Length);
                        var go = GameObject.Find(rel);
                        if (go == null)
                        {
                            go = FindGameObjectByPathIncludingInactive(rel);
                        }
                        if (go == null) return null;
                        if (targetType == typeof(GameObject)) return go;
                        return go.GetComponent(targetType);
                    }
                    var loadedAsset = Resources.Load(stringValue, targetType);
                    if (loadedAsset == null)
                    {
                        Debug.LogWarning($"[SFTaskParsing] 无法从 Resources 加载: '{stringValue}' (目标类型: {targetType.Name})");
                    }
                    return loadedAsset;
                }

                // --- 未知类型回退 ---
                // 尝试用 JsonUtility 解析自定义对象或结构体
                return JsonUtility.FromJson(stringValue, targetType);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"[SFTaskParsing] ParseFieldValue 失败 (值: '{stringValue}', 目标类型: {targetType.Name}): {e.Message}");
                return null;
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

        /// <summary>
        /// 在所有已加载的程序集中安全地查找类型 (运行时必备)
        /// </summary>
        private static Type GetTypeFromAllAssemblies(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            // 1. 快速尝试 (适用于简单情况)
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            // 2. 遍历所有程序集 (适用于跨 .asmdef 的情况)
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (Exception)
                {
                    // 忽略加载程序集或类型时的错误
                }
            }

            return null;
        }
    }
}
