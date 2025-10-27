using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using Unity.Content;

namespace SFramework.Core.Editor.Windows
{
    /// <summary>
    /// 扩展项窗口
    /// </summary>
    public partial class SfWindows
    {
        /// <summary>
        /// 初始化扩展项
        /// </summary>
        public void InitPackages()
        {
            var content = rootVisualElement.Q<GroupBox>("content_items");
            content.Clear();
            var packages = File.ReadAllText(Application.dataPath + "/SFramework/Core/Editor/Config/packages.json");
            var packagesDatas = JsonUtility.FromJson<PackagesDatas>(packages);
            
            // 遍历扩展项列表
            foreach (var itemData in packagesDatas.packages)
            {
                // 创建扩展项
                var item = CreatePackagesItems(itemData);
                content.Add(item);
            }
        }
        
        /// <summary>
        /// 创建扩展项
        /// </summary>
        /// <param name="itemData">扩展项数据</param>
        /// <returns>扩展项</returns>
        public VisualElement CreatePackagesItems(PackagesData itemData)
        {
            // 创建扩展项
            var item = new VisualElement();
            item.AddToClassList("packagesItem");

            //创建图标
            var icon = new VisualElement();
            icon.AddToClassList("packagesItemIcon");
            icon.style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>(itemData.icon);
            item.Add(icon);
            
            //创建名称
            var itemName = new Label(itemData.name);
            itemName.AddToClassList("packagesItemName");
            item.Add(itemName);
            
            //创建版本
            var itemVersion = new Label(itemData.version);
            itemVersion.AddToClassList("packagesItemVersion");
            item.Add(itemVersion);
            
            //创建描述
            var itemDescription = new Label(itemData.description);
            itemDescription.AddToClassList("packagesItemDesc");
            item.Add(itemDescription);

            // 创建状态按钮
            var stateButton = new Button();
            if (Directory.Exists(Application.dataPath + "/SFramework/"+ itemData.name))
            {
                stateButton.text = "卸载";
                stateButton.AddToClassList("packagesItemUninstall");
            }
            else
            {
                stateButton.text = "安装";
                stateButton.AddToClassList("packagesItemInstall");
            }
            item.Add(stateButton);

            return item;
        }
    }
}