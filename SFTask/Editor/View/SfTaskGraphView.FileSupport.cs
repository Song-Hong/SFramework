using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFramework.SFTask.Data;
using SFramework.SFTask.Editor.NodeStyle;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.View
{
    /// <summary>
    /// 任务图视图导出文件功能
    /// </summary>
    public partial class SfTaskGraphView : GraphView
    {
        // 任务图文件路径
        public string FilePath = Application.streamingAssetsPath + "/SFTask/新任务.sftask";

        /// <summary>
        /// 导出任务图视图为任务文件
        /// </summary>
        private void ExportTaskFile(string message = "已导出任务文件到")
        {
            //创建任务列表数据
            var data = new SfTaskListData();

            //添加初始任务节点
            var startNode = graphElements.OfType<Node>().FirstOrDefault(node => node.name == "StartNode");
            if (startNode == null) return;
            data.tasks.Add(new SfTaskPointData()
            {
                x = startNode.GetPosition().x,
                y = startNode.GetPosition().y,
                title = startNode.title,
                type = "StartNode",
            });
            //获取第一个任务节点
            var outputPort = startNode.outputContainer.Children()
                .OfType<Port>()
                .FirstOrDefault(p => p.portName == "任务开始");
            try
            {
                var taskNode = GetConnectedNodes(outputPort).First();

                while (taskNode != null)
                {
                    // 添加任务节点数据
                    if (taskNode is SfTaskNodePointEditor sfTaskPointNode)
                    {
                        var sfTaskPointData = new SfTaskPointData()
                        {
                            x = sfTaskPointNode.GetPosition().x,
                            y = sfTaskPointNode.GetPosition().y,
                            title = sfTaskPointNode.title,
                            type = sfTaskPointNode.GetTaskType(),
                        };
                        {
                            var sfTaskNodeTaskViews = sfTaskPointNode.GetTaskComponents();
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
                        }

                        //添加任务节点
                        data.tasks.Add(sfTaskPointData);
                    }

                    //获取下一个任务节点
                    var taskOutputPort = taskNode.Query<Port>() // 1. 查询所有 Port 元素
                        .Where(p => p.direction == Direction.Output) // 筛选出口端口
                        .Where(p => p.portName == "任务完成") // 筛选端口名称 (假设名称是这个，而不是“任务结束”)
                        .ToList() // 2. 【关键修复】将查询结果转换为 List<Port>
                        .FirstOrDefault(); // 3. 安全地获取第一个匹配项
                    //获取下一个任务节点
                    taskNode = GetConnectedNodes(taskOutputPort).FirstOrDefault();
                    if (taskNode == null)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            finally
            {
                // 导出任务数据为 JSON 字符串
                var taskData = JsonUtility.ToJson(data, true);
                var directoryName = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(FilePath, taskData);
                Debug.Log($"{message}: {FilePath}");
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 获取一个端口（Port）连接的所有目标节点
        /// </summary>
        /// <param name="startPort">起始端口（通常是出口端口 Direction.Output）</param>
        /// <returns>连接到该端口的所有节点列表</returns>
        private List<Node> GetConnectedNodes(Port startPort)
        {
            var connectedNodes = startPort.connections
                .SelectMany(edge =>
                {
                    var targetPort = startPort.direction == Direction.Output ? edge.input : edge.output;
                    return targetPort.node?.Yield();
                })
                .Where(node => node != null)
                .ToList();

            return connectedNodes;
        }

        [Serializable]
        private class UnityEventArgData
        {
            public string objectRef;
            public string assemblyTypeName;
            public int intValue;
            public float floatValue;
            public string stringValue;
            public bool boolValue;
        }

        [Serializable]
        private class UnityEventCallData
        {
            public string targetRef;
            public string targetType;
            public string methodName;
            public int mode;
            public UnityEventArgData arguments;
            public int callState;
        }

        [Serializable]
        private class UnityEventData
        {
            public List<UnityEventCallData> calls = new List<UnityEventCallData>();
        }

        private string SerializeObjectReference(UnityEngine.Object obj)
        {
            if (obj == null) return null;
            if (AssetDatabase.Contains(obj))
            {
                string fullPath = AssetDatabase.GetAssetPath(obj);
                int idx = fullPath.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string rel = fullPath.Substring(idx + "/Resources/".Length);
                    string noExt = Path.ChangeExtension(rel, null);
                    return noExt;
                }
                return null;
            }
            GameObject go = null;
            if (obj is GameObject g) go = g;
            else if (obj is Component c) go = c.gameObject;
            if (go == null) return null;
            var uid = go.GetComponent<SFramework.SFTask.Module.SfUniqueId>();
            if (uid != null && !string.IsNullOrEmpty(uid.Id)) return "scene-id://" + uid.Id;
            List<string> names = new List<string>();
            var t = go.transform;
            while (t != null)
            {
                names.Insert(0, t.name);
                t = t.parent;
            }
            var path = string.Join("/", names);
            return "scene://" + path;
        }

        private string SerializePropertyValue(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return p.intValue.ToString();
                case SerializedPropertyType.Float:
                    return p.floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case SerializedPropertyType.Boolean:
                    return p.boolValue.ToString();
                case SerializedPropertyType.String:
                    return p.stringValue;
                case SerializedPropertyType.Enum:
                    return p.enumDisplayNames != null && p.enumDisplayNames.Length > 0 ? p.enumDisplayNames[p.enumValueIndex] : p.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2:
                    return JsonUtility.ToJson(p.vector2Value);
                case SerializedPropertyType.Vector3:
                    return JsonUtility.ToJson(p.vector3Value);
                case SerializedPropertyType.Color:
                    return JsonUtility.ToJson(p.colorValue);
                case SerializedPropertyType.ObjectReference:
                    return SerializeObjectReference(p.objectReferenceValue);
                case SerializedPropertyType.Generic:
                    if (p.type == "UnityEvent")
                    {
                        var callsProp = p.FindPropertyRelative("m_PersistentCalls.m_Calls");
                        var data = new UnityEventData();
                        if (callsProp != null)
                        {
                            for (int i = 0; i < callsProp.arraySize; i++)
                            {
                                var call = callsProp.GetArrayElementAtIndex(i);
                                var targetProp = call.FindPropertyRelative("m_Target");
                                var methodProp = call.FindPropertyRelative("m_MethodName");
                                var modeProp = call.FindPropertyRelative("m_Mode");
                                var argsProp = call.FindPropertyRelative("m_Arguments");
                                var callStateProp = call.FindPropertyRelative("m_CallState");

                                var arg = new UnityEventArgData();
                                if (argsProp != null)
                                {
                                    var objArgProp = argsProp.FindPropertyRelative("m_ObjectArgument");
                                    var objAsmProp = argsProp.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                                    var intArgProp = argsProp.FindPropertyRelative("m_IntArgument");
                                    var floatArgProp = argsProp.FindPropertyRelative("m_FloatArgument");
                                    var stringArgProp = argsProp.FindPropertyRelative("m_StringArgument");
                                    var boolArgProp = argsProp.FindPropertyRelative("m_BoolArgument");
                                    arg.objectRef = objArgProp != null ? SerializeObjectReference(objArgProp.objectReferenceValue) : null;
                                    arg.assemblyTypeName = objAsmProp != null ? objAsmProp.stringValue : null;
                                    arg.intValue = intArgProp != null ? intArgProp.intValue : 0;
                                    arg.floatValue = floatArgProp != null ? floatArgProp.floatValue : 0f;
                                    arg.stringValue = stringArgProp != null ? stringArgProp.stringValue : null;
                                    arg.boolValue = boolArgProp != null && boolArgProp.boolValue;
                                }

                                var callData = new UnityEventCallData
                                {
                                    targetRef = targetProp != null ? SerializeObjectReference(targetProp.objectReferenceValue) : null,
                                    targetType = targetProp != null && targetProp.objectReferenceValue != null ? targetProp.objectReferenceValue.GetType().AssemblyQualifiedName : null,
                                    methodName = methodProp != null ? methodProp.stringValue : null,
                                    mode = modeProp != null ? modeProp.intValue : 0,
                                    arguments = arg,
                                    callState = callStateProp != null ? callStateProp.intValue : 0
                                };
                                data.calls.Add(callData);
                            }
                        }
                        return JsonUtility.ToJson(data);
                    }
                    break;
                default:
                    return null;
            }
            return null;
        }

        /// <summary>
        /// 从 JSON 文件导入任务图视图并还原页面
        /// </summary>
        /// <param name="jsonText">包含任务数据的 JSON 字符串</param>
        /// <param name="filePath">任务文件路径</param>
        public void ImportTaskFile(string jsonText, string filePath = "")
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                FilePath = filePath;
            }

            // 1. 反序列化 JSON 字符串
            SfTaskListData data;
            try
            {
                data = JsonUtility.FromJson<SfTaskListData>(jsonText);
            }
            catch (Exception e)
            {
                Debug.LogError($"导入任务文件失败：JSON 反序列化错误。{e.Message}");
                return;
            }

            if (data == null || data.tasks == null || data.tasks.Count == 0)
            {
                Debug.LogWarning("导入任务数据为空或无效。");
                return;
            }

            // 2. 清理现有图视图
            ClearGraphViewElements();

            Node previousNode = null;

            // 3. 遍历数据并创建节点和连接
            for (int i = 0; i < data.tasks.Count; i++)
            {
                var pointData = data.tasks[i];
                Node currentNode = null;

                // --- A. 创建节点 ---
                if (pointData.type == "StartNode")
                {
                    // 重新创建或定位 StartNode
                    CreateStartNodeAtPosition(new Vector2(pointData.x, pointData.y), pointData.title);
                    currentNode = graphElements.OfType<Node>().FirstOrDefault(node => node.name == "StartNode");
                }
                else // 普通任务节点
                {
                    // 创建 SfTaskNodePointEditor
                    var newNode = new SfTaskNodePointEditor(pointData.title, new Vector2(pointData.x, pointData.y));

                    // --- B. 初始化任务组件和字段值 ---
                    if (newNode is SfTaskNodePointEditor sfTaskPointNode)
                    {
                        sfTaskPointNode.SetExecutionType(pointData.type);

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

                            sfTaskPointNode.TaskContainerAdd(sfTaskNodeTaskView);
                        }
                    }

                    AddElement(newNode);
                    currentNode = newNode;
                }

                // --- C. 连接节点 ---
                if (currentNode != null && previousNode != null)
                {
                    // 上一个节点的出口端口名称
                    string outputPortName = (previousNode.name == "StartNode") ? "任务开始" : "任务完成";

                    // 获取上一个节点的出口端口
                    var outputPort = GetOutputPortByName(previousNode, outputPortName);

                    // 获取当前节点的入口端口
                    var inputPort = GetInputPortByName(currentNode, "任务入口");

                    if (outputPort != null && inputPort != null)
                    {
                        // 创建并添加边
                        var edge = outputPort.ConnectTo(inputPort);
                        AddElement(edge);
                    }
                }

                previousNode = currentNode;
            }

            // 强制重新对齐视口，以便新加载的节点可见
            FrameAll();
        }
        
        // 辅助方法：清理图视图中的所有元素（除了 GridBackground）
        private void ClearGraphViewElements()
        {
            // 使用 LINQ 过滤所有元素，排除 GridBackground。
            var elementsToRemove = graphElements
                .Where(e => !e.GetType().Name.Contains("GridBackground"))
                .ToList();
    
            // 批量删除元素
            DeleteElements(elementsToRemove);
        }

        // 文件：您添加这两个辅助方法的文件 (例如：SfTaskGraphView.ExportFile.cs 或 SfTaskGraphView.cs)
        private Port GetOutputPortByName(Node node, string portName)
        {
            return node.Query<Port>()
                .Where(p => p.direction == Direction.Output)
                .Where(p => p.portName == portName)
                .ToList() // ⬅️【关键修复】将 VisualElementQuery 转换为 List<Port>
                .FirstOrDefault();
        }

        // 辅助方法：安全获取指定名称的入口端口
        private Port GetInputPortByName(Node node, string portName)
        {
            return node.Query<Port>()
                .Where(p => p.direction == Direction.Input)
                .Where(p => p.portName == portName)
                .ToList() // ⬅️【关键修复】将 VisualElementQuery 转换为 List<Port>
                .FirstOrDefault();
        }
    }
}
