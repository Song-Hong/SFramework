using System;
using System.Collections.Generic;
using System.Linq;
using SFramework.Core.Extends.UIElement;
using SFramework.Core.Support;
using SFramework.SFTask.Editor.View;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.NodeStyle
{
    /// <summary>
    /// 任务节点端口编辑器
    /// </summary>
    public class SfTaskNodePointEditor : Node
    {
        /// <summary>
        /// 端口标题标签
        /// </summary>
        private Label _titleLabel;

        /// <summary>
        /// 端口标题输入框
        /// </summary>
        private TextField _titleTextField;

        /// <summary>
        /// 顺序和并行选择标签
        /// </summary>
        private SfTab _sfTab;

        /// <summary>
        /// 任务组件列表
        /// </summary>
        private List<SfTaskNodeTaskView> _taskComponents = new List<SfTaskNodeTaskView>();
        
        /// <summary>
        /// 任务列表视图（支持拖拽排序）
        /// </summary>
        private ListView _taskListView;

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="titleName">节点标题</param>
        /// <param name="mousePosition">鼠标位置</param>
        public SfTaskNodePointEditor(string titleName = "new Point", Vector2 mousePosition = default) : base()
        {
            //设置节点名称
            title = titleName;
            //设置默认宽度
            style.width = 200;

            //创建标题标签
            _titleLabel = titleContainer.Q<Label>();
            _titleLabel.text = titleName;
            //创建标题输入框
            _titleTextField = new TextField
            {
                isDelayed = true,
                style =
                {
                    display = DisplayStyle.None
                }
            };
            _titleTextField.value = titleName;
            //将标题输入框添加到标题栏中
            titleContainer.Insert(1, _titleTextField);
            //添加顺序和并行选择
            _sfTab = new SfTab
            {
                name = "TaskExec",
                style =
                {
                    marginBottom = 10,
                    marginLeft = 5
                }
            };
            _sfTab.SetTitle("执行顺序:");
            _sfTab.AddChoice("顺序", "并行");
            _sfTab.TitleLabel.style.fontSize = 12;
            // _sfTab.Select();
            //将顺序和并行选择添加到标题栏中
            extensionContainer.Add(_sfTab);
            extensionContainer.style.backgroundColor = SfColor.HexToColor("#2D2D2D");
            RefreshExpandedState();
            
            // 初始化任务列表视图（支持拖拽排序）
            _taskListView = new ListView
            {
                selectionType = SelectionType.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = true
            };
            _taskListView.itemsSource = _taskComponents;
            _taskListView.makeItem = () =>
            {
                var container = new VisualElement();
                container.style.flexGrow = 1;
                container.style.marginBottom = 6;
                return container;
            };
            _taskListView.bindItem = (ve, i) =>
            {
                ve.Clear();
                if (i < 0 || i >= _taskComponents.Count) return;
                var view = _taskComponents[i];
                // 确保不会重复挂载到多个父节点
                if (view.parent != null)
                {
                    view.RemoveFromHierarchy();
                }
                ve.Add(view);
            };
            _taskListView.style.flexGrow = 1;
            _taskListView.style.marginLeft = 4;
            _taskListView.style.marginRight = 4;
            _taskListView.style.marginTop = 6;
            _taskListView.style.marginBottom = 6;
            extensionContainer.Add(_taskListView);
            RefreshExpandedState();
            //初始化节点位置
            SetPosition(new Rect(mousePosition, new Vector2(360, 150)));
            //创建端口
            var controlFlowType = typeof(object); // 使用 object 允许连接任何类型，或用一个特定的标记类/结构体
            // 输入端口：用于控制流的进入点
            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                controlFlowType);
            entryPort.portName = "任务入口";
            inputContainer.Add(entryPort);
            // 输出端口：用于控制流的流出点
            var exitPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                controlFlowType);
            exitPort.portName = "任务完成";
            outputContainer.Add(exitPort);
            // 刷新节点,并加入节点扩展区域
            RefreshExpandedState();
            RefreshPorts();
            //注册双击事件
            _titleLabel.RegisterCallback<MouseDownEvent>(OnTitleMouseDown);
            // 当按下回车键时
            _titleTextField.RegisterCallback<KeyDownEvent>(OnTitleKeyDown);
            // 当失去焦点时
            _titleTextField.RegisterCallback<FocusOutEvent>(OnTitleFocusOut);
            // 当事件来源于任务列表（_taskListView）时，阻止事件继续冒泡到 GraphView/节点，
            // 以免触发节点的拖拽或画布的滚动缩放，确保 ListView 内部的重排与滚动正常工作。
            // 指针按下拦截
            RegisterCallback<PointerDownEvent>(evt =>
            {
                var t = evt.target as VisualElement;
                if (t != null && t.GetFirstAncestorOfType<ListView>() == _taskListView)
                {
                    evt.StopPropagation();
                }
            });
            // 鼠标按下拦截
            RegisterCallback<MouseDownEvent>(evt =>
            {
                var t = evt.target as VisualElement;
                if (t != null && t.GetFirstAncestorOfType<ListView>() == _taskListView)
                {
                    evt.StopPropagation();
                }
            });
            // 滚轮事件拦截
            RegisterCallback<WheelEvent>(evt =>
            {
                var t = evt.target as VisualElement;
                if (t != null && t.GetFirstAncestorOfType<ListView>() == _taskListView)
                {
                    evt.StopPropagation();
                }
            });
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 设置节点位置
        /// </summary>
        /// <param name="newPos">新位置</param>
        public sealed override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
        }

        #endregion

        #region 顶部栏可编辑区域

        /// <summary>
        /// 当鼠标在标题 Label 上按下时调用
        /// </summary>
        private void OnTitleMouseDown(MouseDownEvent e)
        {
            // 检查是否为双击
            if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
            {
                // 开始编辑
                StartEditingTitle();
            }
        }

        /// <summary>
        /// 开始编辑标题
        /// </summary>
        private void StartEditingTitle()
        {
            // 隐藏 Label
            _titleLabel.style.display = DisplayStyle.None;

            // 显示 TextField
            _titleTextField.style.display = DisplayStyle.Flex;

            // 将当前标题设置给 TextField
            _titleTextField.value = _titleTextField.text.Trim();

            // 立即聚焦到 TextField 并全选
            _titleTextField.Focus();
            _titleTextField.SelectAll();
        }

        /// <summary>
        /// 当在 TextField 中按下按键时调用
        /// </summary>
        private void OnTitleKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return) // 按下回车
            {
                // 确认修改
                CommitTitleChange();
            }
            else if (e.keyCode == KeyCode.Escape) // 按下 Esc
            {
                // 取消修改
                CancelTitleChange();
            }
        }

        /// <summary>
        /// 当 TextField 失去焦点时调用
        /// </summary>
        private void OnTitleFocusOut(FocusOutEvent e)
        {
            // 失去焦点时，也确认修改
            CommitTitleChange();
        }

        /// <summary>
        /// 确认标题修改
        /// </summary>
        private void CommitTitleChange()
        {
            // 1. 将 TextField 的值赋给 Node 的 title
            // 这会自动更新 titleLabel 的 text
            this.title = _titleTextField.value;

            // 2. 隐藏 TextField
            _titleTextField.style.display = DisplayStyle.None;

            // 3. 显示 Label
            _titleLabel.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// 取消标题修改
        /// </summary>
        private void CancelTitleChange()
        {
            // 只是隐藏 TextField 并显示 Label，不应用任何更改
            _titleTextField.style.display = DisplayStyle.None;
            _titleLabel.style.display = DisplayStyle.Flex;
        }

        #endregion

        #region 右键菜单

        /// <summary>
        /// 重写此方法来构建上下文菜单
        /// </summary>
        /// <param name="evt">上下文菜单事件参数</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is not Node) return;
            // 获取 GraphView 实例
            var graphView = this.GetFirstAncestorOfType<SfTaskGraphView>();

            if (graphView == null) return; // 无法操作
            // --- 复制节点 ---
            evt.menu.AppendAction("复制节点", _ =>
            {
                // 调用 GraphView 中的复制逻辑
                graphView.CopyToClipboard(this);
            });

            // --- 粘贴节点 ---
            evt.menu.AppendAction("粘贴任务数据", _ => 
            {
                graphView.PasteDataToTargetNode(this); 
            }, (action) => // 状态检查器 (statusCallback)
            {
                if (graphView.IsTaskDataInClipboard())
                {
                    return DropdownMenuAction.Status.Normal;
                }
                else
                {
                    return DropdownMenuAction.Status.Disabled;
                }
            });
            evt.menu.AppendSeparator();

            //创建右键选择菜单
            foreach (var node in SfTaskGraphView.Nodes)
            {
                evt.menu.AppendAction("" + node.Item1, _ => { CreateTask(node.Item1, node.Item2); });
            }
            // base.BuildContextualMenu(evt);

            //删除节点
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("删除节点", _ =>
            {
                // 找到 GraphView 的祖先节点
                var graphView = this.GetFirstAncestorOfType<SfTaskGraphView>();
                if (graphView == null)
                {
                    // 如果无法找到 GraphView，则无法清理连线
                    parent.Remove(this);
                    return;
                }

                // 2. 收集所有要删除的连线 (Edges)
                var edgesToDelete = new List<Edge>();
                // 遍历所有输入端口
                foreach (var port in inputContainer.Children().OfType<Port>())
                {
                    edgesToDelete.AddRange(port.connections);
                }

                // 遍历所有输出端口
                foreach (var port in outputContainer.Children().OfType<Port>())
                {
                    edgesToDelete.AddRange(port.connections);
                }

                // 转换节点本身为可删除的元素
                var nodesToDelete = new List<GraphElement> { this };
                // 使用 GraphView 的 DeleteElements 方法进行批量删除
                graphView.DeleteElements(edgesToDelete.Distinct().Concat(nodesToDelete));
            });
        }

        #endregion

        #region 创建节点任务

        /// <summary>
        /// 创建任务节点
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="taskType">任务类型</param>
        private void CreateTask(string nodeName, string taskType)
        {
            var sfTaskNodeTaskView = new SfTaskNodeTaskView();
            sfTaskNodeTaskView.TitleLabel.text = nodeName;
            sfTaskNodeTaskView.TaskType = taskType;

            var type = ResolveType(taskType);
            if (type != null && typeof(SFramework.SFTask.Module.SfTaskNode).IsAssignableFrom(type))
            {
                var so = ScriptableObject.CreateInstance(type) as SFramework.SFTask.Module.SfTaskNode;
                if (so != null)
                {
                    var serialized = new UnityEditor.SerializedObject(so);
                    sfTaskNodeTaskView.Init(serialized);
                }
            }

            // 刷新节点,并加入节点扩展区域
            TaskContainerAdd(sfTaskNodeTaskView);
            RefreshExpandedState();
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

        #endregion

        #region 任务容器

        /// <summary>
        /// 添加任务容器元素
        /// </summary>
        /// <param name="element">任务容器元素</param>
        public void TaskContainerAdd(VisualElement element)
        {
            _taskComponents.Add(element as SfTaskNodeTaskView);
            _taskListView.Rebuild();
            RefreshExpandedState();
        }

        /// <summary>
        /// 删除任务节点
        /// </summary>
        /// <param name="taskNode">任务节点</param>
        public void RemoveTaskNode(SfTaskNodeTaskView taskNode)
        {
            _taskComponents.Remove(taskNode);
            _taskListView.Rebuild();
            RefreshExpandedState();
        }

        /// <summary>
        /// 获取任务容器元素
        /// </summary>
        /// <returns>任务容器元素列表</returns>
        public List<SfTaskNodeTaskView> GetTaskComponents()
        {
            return _taskComponents;
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 获取任务类型
        /// </summary>
        /// <returns>任务类型</returns>
        public string GetTaskType()
        {
            var text = _sfTab.nowChooseBtn.text;
            return text switch
            {
                "顺序" => "Sequential",
                "并行" => "Parallel",
                _ => "Sequential"
            };
        }

        public void SetExecutionType(string type)
        {
            switch (type)
            {
                case "Parallel":
                    _sfTab.Select("并行");
                    break;
                default:
                    _sfTab.Select("顺序");
                    break;
            }
        }

        #endregion
    }
}
