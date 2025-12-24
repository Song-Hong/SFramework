using System;
using System.Collections.Generic;
using SFramework.SFTask.Editor.NodeStyle;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.View
{
    /// <summary>
    /// 任务图视图 右键菜单
    /// </summary>
    public partial class SfTaskGraphView
    {
        /// <summary>
        /// 重写此方法来构建上下文菜单
        /// </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 检查是否为图视图
            if (evt.target is not GraphView) return;
            // 转换鼠标位置到图视图坐标
            var position = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            //添加右键菜单事件
            evt.menu.AppendAction("创建节点", _ =>
            {
                var newPoint = new SfTaskNodePointEditor("新节点", position);
                // 添加节点到图视图
                AddElement(newPoint);
            });
            // 添加粘贴为新的节点
            evt.menu.AppendAction("粘贴为新的任务节点", _ => 
            {
                PasteFromClipboard(position);
            }, (action) => // 状态检查器 (statusCallback)
            {
                if (IsTaskDataInClipboard())
                {
                    return DropdownMenuAction.Status.Normal;
                }
                else
                {
                    return DropdownMenuAction.Status.Disabled;
                }
            });
            evt.menu.AppendSeparator();
            //保存任务图
            evt.menu.AppendAction("保存", _ =>
            {
                // SaveTaskFile();
                ExportTaskFile("已保存文件");
            });
            
            //导出为task
            evt.menu.AppendAction("导出为sftask", _ =>
            {
                ExportTaskFile();
            });
        }
        
        /// <summary>
        /// 创建开始节点，并使其居中显示
        /// </summary>
        private void CreateStartNode()
        {
            // 定义节点的尺寸
            const float nodeWidth = 180f;
            const float nodeHeight = 100f; // 估算的节点高度，用于居中计算
            var nodePosition = new Vector2(150,150);
            var node = new Node
            {
                title = "开始节点",
                style =
                {
                    width = nodeWidth, // 使用定义的常量
                }
            };
            node.name = "StartNode";
            node.SetPosition(new Rect(nodePosition, new Vector2(nodeWidth, nodeHeight)));
            
            var exitPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object));
            exitPort.portName = "任务开始";
            node.outputContainer.Add(exitPort);
    
            // 4. 刷新节点,并加入节点扩展区域
            node.RefreshExpandedState();
            node.RefreshPorts();
            AddElement(node);
        }
        
        /// <summary>
        /// 创建开始节点，并使其居中显示（或设置到指定位置）
        /// </summary>
        private void CreateStartNodeAtPosition(Vector2 nodePosition, string titleName)
        {
            // 定义节点的尺寸
            const float nodeWidth = 180f;
            const float nodeHeight = 100f; 
            
            var node = new Node
            {
                title = titleName,
                style =
                {
                    width = nodeWidth,
                }
            };
            node.name = "StartNode";
            node.SetPosition(new Rect(nodePosition, new Vector2(nodeWidth, nodeHeight))); // 使用传入的位置
            
            var exitPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object));
            exitPort.portName = "任务开始";
            node.outputContainer.Add(exitPort);
    
            // 4. 刷新节点,并加入节点扩展区域
            node.RefreshExpandedState();
            node.RefreshPorts();
            AddElement(node);
        }
    }
}