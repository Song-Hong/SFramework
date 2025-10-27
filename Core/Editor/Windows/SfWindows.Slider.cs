using UnityEngine.UIElements;
using UnityEngine; // 确保在 Editor 脚本中如果用 Debug.Log 需要 using UnityEditor; 或直接使用 Debug.Log

namespace SFramework.Core.Editor.Windows
{
    /// <summary>
    /// 侧边栏
    /// </summary>
    public partial class SfWindows
    {
        /// <summary>
        /// 包管理按钮
        /// </summary>
        private Button _packagesButton;

        /// <summary>
        /// 设置按钮
        /// </summary>
        private Button _settingButton;

        /// <summary>
        /// 初始化侧边栏
        /// </summary>
        private void InitSlider()
        {
            // 初始化包管理按钮
            _packagesButton = rootVisualElement.Q<Button>("packages");
            // 初始化设置按钮
            _settingButton = rootVisualElement.Q<Button>("setting");

            // 绑定 PackagesButton 的鼠标按下事件 (强制在捕获阶段执行)
            _packagesButton.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.LeftMouse)
                {
                    OnClickButton(_packagesButton);
                }
            }, TrickleDown.TrickleDown);

            // 绑定 SettingButton 的鼠标按下事件 (强制在捕获阶段执行)
            _settingButton.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.LeftMouse)
                {
                    OnClickButton(_settingButton);
                }
            }, TrickleDown.TrickleDown);
            
            // 初始化包管理按钮
            OnClickButton(_packagesButton);
        }

        /// <summary>
        /// 点击按钮事件
        /// </summary>
        /// <param name="button">点击的按钮</param>
        private void OnClickButton(Button button)
        {
            if (button == _packagesButton)
            {
                _packagesButton.RemoveFromClassList("item");
                _packagesButton.AddToClassList("item_select");
                _settingButton.RemoveFromClassList("item_select");
                _settingButton.AddToClassList("item");
                InitPackages();
            }
            else if (button == _settingButton)
            {
                _settingButton.RemoveFromClassList("item");
                _settingButton.AddToClassList("item_select");
                _packagesButton.RemoveFromClassList("item_select");
                _packagesButton.AddToClassList("item");
                InitSetting();
            }
        }
    }
}