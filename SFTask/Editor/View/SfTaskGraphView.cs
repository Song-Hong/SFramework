using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFTask.Editor.View
{
    /// <summary>
    /// 任务图视图
    /// </summary>
    public class SfTaskGraphView:GraphView
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SfTaskGraphView()
        {
            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            this.StretchToParentSize(); // 让任务图视图填充整个窗口
            this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); // 缩放
            this.AddManipulator(new ContentDragger()); // 拖动画布
            this.AddManipulator(new SelectionDragger()); // 拖动选中的元素
            this.AddManipulator(new RectangleSelector()); // 框选
        }
    }
}