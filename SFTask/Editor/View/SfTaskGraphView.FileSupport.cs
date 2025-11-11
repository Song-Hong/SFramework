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
                                var fields = sfTaskNodeTaskView.PublicFields.Select(publicField =>
                                    new SfTaskComponentData()
                                    {
                                        fieldName = publicField.Item1,
                                        fieldType = publicField.Item2,
                                        // 【修复】
                                        // 1. 传入 sfTaskNodeTaskView (当前组件) 而不是 sfTaskPointNode (父节点)
                                        // 2. 传入 publicField.Item2 (字段类型) 以便正确解析
                                        fieldValue = GetTaskFieldValue(sfTaskNodeTaskView, publicField.Item1,
                                            publicField.Item2),
                                    }).ToList();

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

        /// <summary>
        /// 获取任务节点的字段值
        /// </summary>
        /// <param name="sfTaskNodePoint">任务节点编辑器</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>字段值</returns>
// 删掉这个旧的、有问题的函数
        /// <summary>
        /// 【已修复】获取【指定任务组件】的字段值
        /// </summary>
        /// <param name="taskView">要从中获取值的 SfTaskNodeTaskView 组件</param>
        /// <param name="fieldName">字段名称 (也应该是 UI 控件的 name)</param>
        /// <param name="fieldType">字段类型 (来自 publicField.Item2)</param>
        /// <returns>字段值</returns>
        private string GetTaskFieldValue(SFramework.SFTask.Editor.NodeStyle.SfTaskNodeTaskView taskView,
            string fieldName, string fieldType)
        {
            // A. 在 taskView 内部查找 UI 控件
            var uiElement = taskView.Q(name: fieldName);

            if (uiElement == null)
            {
                Debug.LogWarning($"在 {taskView.TitleLabel.text} 中未找到字段 '{fieldName}' 对应的 UI 控件。");
                return null;
            }

            // B. 根据 fieldType (或 UI 控件类型) 来安全地获取值
            try
            {
                // 简单类型 (ToString)
                if (uiElement is BaseField<string> stringField)
                    return stringField.value;
                if (uiElement is BaseField<int> intField)
                    return intField.value.ToString();
                if (uiElement is BaseField<float> floatField)
                    return floatField.value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (uiElement is BaseField<bool> boolField)
                    return boolField.value.ToString();

                // JSON 序列化类型
                if (uiElement is BaseField<Vector3> v3Field)
                    return JsonUtility.ToJson(v3Field.value);
                if (uiElement is BaseField<Vector2> v2Field)
                    return JsonUtility.ToJson(v2Field.value);
                if (uiElement is BaseField<Color> colorField)
                    return JsonUtility.ToJson(colorField.value);

                // 枚举类型 (ToString)
                if (uiElement is BaseField<System.Enum> enumField)
                    return enumField.value.ToString();

                // GUID 序列化类型 (ObjectField)
                if (uiElement is UnityEditor.UIElements.ObjectField objField)
                {
                    if (objField.value == null) return null;

                    // 必须是资产 (Asset)，不能是场景对象
                    if (AssetDatabase.Contains(objField.value))
                    {
                        string path = AssetDatabase.GetAssetPath(objField.value);
                        return AssetDatabase.AssetPathToGUID(path); // ⬅️ 保存 GUID
                    }

                    Debug.LogWarning($"字段 '{fieldName}' 引用了一个场景对象，不支持序列化。");
                    return null;
                }

                // 回退或未处理的类型
                Debug.LogWarning($"GetTaskFieldValue: 未处理的 UI 控件类型 '{uiElement.GetType()}' (字段名: '{fieldName}')。");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"获取字段 '{fieldName}' ({fieldType}) 的值时出错: {e.Message}");
                return null;
            }
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
                        // ⚠ 重要的：您可能需要在这里设置执行顺序 (Sequential/Parallel)
                        // sfTaskPointNode.SetExecutionType(pointData.type); 

                        foreach (var taskData in pointData.tasks)
                        {
                            // 重新构造一个包含 JSON 值的 List<Tuple<string,string,string>>
                            // 以便传递给 Init 方法，用于设置 UI 控件的初始值
                            var fieldsWithValues = taskData.fields.Select(f =>
                                new Tuple<string, string, string>(f.fieldName, f.fieldType, f.fieldValue)
                            ).ToList();

                            // 假设 SfTaskNodeTaskView.Init 已被修改以接受字段值
                            var sfTaskNodeTaskView = new SfTaskNodeTaskView();
                            sfTaskNodeTaskView.Init(taskData.taskName, taskData.taskType, fieldsWithValues);

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
            // 获取所有元素，但不包括 GridBackground（通常是索引 0）
            var elementsToRemove = graphElements.ToList();
            if (elementsToRemove.Count > 0 && elementsToRemove[0] is GridBackground)
            {
                elementsToRemove.RemoveAt(0);
            }

            // 批量删除元素
            DeleteElements(elementsToRemove);
        }

        // 文件：您添加这两个辅助方法的文件 (例如：SfTaskGraphView.ExportFile.cs 或 SfTaskGraphView.cs)

        // 辅助方法：安全获取指定名称的出口端口
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