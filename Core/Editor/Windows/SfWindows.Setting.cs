using UnityEngine.UIElements;

namespace SFramework.Core.Editor.Windows
{
    /// <summary>
    /// 设置窗口
    /// </summary>
    public partial class SfWindows
    {
        /// <summary>
        /// 初始化设置
        /// </summary>
        public void InitSetting()
        {
            // 初始化设置
            var content = rootVisualElement.Q<GroupBox>("content_items");
            content.Clear();
        }
    }
}