using UnityEngine.UIElements;

namespace SFramework.Core.Editor.UIElementEditor
{
    /// <summary>
    /// 根元素
    /// </summary>
    public class SfRootEditor
    {
        /// <summary>
        /// 根元素
        /// </summary>
        private VisualElement _root;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public SfRootEditor()
        {
            _root = new VisualElement();
        }

        /// <summary>
        /// 添加标题
        /// </summary>
        /// <param name="title">标题内容</param>
        public void AddTitle(string title)
        {
            _root.Add(new SfTitleEditor(title));
        }
        
        /// <summary>
        /// 获取目前的布局模块
        /// </summary>
        /// <returns>布局</returns>
        public VisualElement Root()
        {
            return _root;
        }
    }
}