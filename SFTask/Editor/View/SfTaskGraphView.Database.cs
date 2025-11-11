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
        public static List<Tuple<string,string,List<Tuple<string,string,string>>>> Nodes = new List<Tuple<string,string,List<Tuple<string,string,string>>>> ();
        
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
                //获取所有公开的字段
                var publicFields = SfReflection.GetPublicFields(runtimeSubclass);
                publicFields.RemoveAll(x => x.Item1 == "isComplete");
                
                //获取节点的名称
                if (System.Activator.CreateInstance(runtimeSubclass) is not SfTaskNode createdInstance) continue;
                Nodes.Add(new Tuple<string,string,List<Tuple<string,string,string>>>(
                    createdInstance.GetTaskNodeName(), // 任务名
                    createdInstance.GetType().FullName
                    ,publicFields));
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
                    var fields = sfTaskNodeTaskView.PublicFields.Select(publicField =>
                        new SfTaskFieldData()
                        {
                            fieldName = publicField.Item1,
                            fieldType = publicField.Item2,
                            // 重新使用已有的 GetTaskFieldValue 方法获取当前 UI 控件的值
                            fieldValue = GetTaskFieldValue(sfTaskNodeTaskView, publicField.Item1, publicField.Item2),
                        }).ToList();

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
                // 构造一个包含 JSON 值的 List<Tuple<string,string,string>>
                var fieldsWithValues = taskData.fields.Select(f =>
                    new Tuple<string, string, string>(f.fieldName, f.fieldType, f.fieldValue)
                ).ToList();

                // 假设 SfTaskNodeTaskView.Init 已被修改以接受字段值
                var sfTaskNodeTaskView = new SfTaskNodeTaskView();
                // 假设 taskData.taskName, taskData.taskType, fieldsWithValues 包含正确的数据
                sfTaskNodeTaskView.Init(taskData.taskName, taskData.taskType, fieldsWithValues);

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
                // 构造一个包含 JSON 值的 List<Tuple<string,string,string>>
                var fieldsWithValues = taskData.fields.Select(f =>
                    new Tuple<string, string, string>(f.fieldName, f.fieldType, f.fieldValue)
                ).ToList();

                // 创建新的任务组件视图
                var sfTaskNodeTaskView = new SfTaskNodeTaskView();
        
                // 初始化新视图的值和类型
                // 假设 Init 方法负责创建并设置 UI 控件的值
                sfTaskNodeTaskView.Init(taskData.taskName, taskData.taskType, fieldsWithValues);

                // 4. 将新的任务组件添加到目标节点
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
    }
}