using System.IO;
using UnityEngine.UIElements;

namespace Song.Core.Editor.Song
{
    /// <summary>
    /// 扩展页面
    /// </summary>
    public partial class SongWindow
    {
        private GroupBox _extendsItemArea;
        
        public void InitExtendsPage()
        {
            _extendsItemArea = _content.Q<GroupBox>("ExtendsArea");

            foreach (var directory in Directory.GetDirectories("Assets/Song/Core/Extends"))
            {
                CreateExtnedItemButton(directory);
            }
        }

        /// <summary>
        /// 创建扩展物体按钮
        /// </summary>
        /// <param name="name">按钮名</param>
        public void CreateExtnedItemButton(string path)
        {
            var strings = path.Split('\\');
            var extendName = strings[^1];
            
            var button = new Button
            {
                text = extendName,
                name = extendName,
                style =
                {
                    width = 80f,
                    height = 80f,
                }
            };

            _extendsItemArea.Add(button);
        }
    }
}