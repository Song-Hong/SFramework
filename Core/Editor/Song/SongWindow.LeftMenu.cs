
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Song.Core.Editor.Song
{
    /// <summary>
    /// 左侧菜单
    /// </summary>
    public partial class SongWindow
    {
        private List<Button>  _leftButtons = new List<Button>();
        private Button _nowLeftButtonSelected;
        
        /// <summary>
        /// 初始化左侧菜单
        /// </summary>
        public void InitLeftMenu()
        {
            _leftButtons.Add(rootVisualElement.Q<Button>("Normal"));
            _leftButtons.Add(rootVisualElement.Q<Button>("Extends"));
            
            foreach (var leftButton in _leftButtons)
            {
                leftButton.clickable.clicked += () =>
                {
                    OnLetftMenuClick(leftButton);
                };
            }
        }
        
        /// <summary>
        /// 左侧菜单点击
        /// </summary>
        /// <param name="button">点击按钮</param>
        public void OnLetftMenuClick(Button button)
        {
            if(_nowLeftButtonSelected!=null)
            {
                _nowLeftButtonSelected.RemoveFromClassList("LeftMenuItemSelected");
            }
            button.AddToClassList("LeftMenuItemSelected");
            _nowLeftButtonSelected = button;

            //切换页面
            ChangePage(button.name);
            
            //初始化页面
            InitPage(button.name);
        }
    }
}