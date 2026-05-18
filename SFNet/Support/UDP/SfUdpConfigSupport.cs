using System;
using System.IO;
using SFramework.SFNet.Module.Udp;
using SFramework.SFNet.Mono;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFNet.Support.UDP
{
    /// <summary>
    /// UDP 配置文件支持
    /// </summary>
    public class SfUdpConfigSupport:SfUdpSupport
    {
        /// <summary>
        /// 是否在加载时自动开启UDP服务器
        /// </summary>
        [Header("是否在加载成功后自动开启UDP服务器")]
        public bool loadingAutoStart = true;
        
        /// <summary>
        /// 初始化UDP配置文件支持
        /// </summary>
        /// <param name="server">UDP服务器</param>
        public override void Init(SfUDPServer server)
        {
            
        }

        /// <summary>
        /// 重置UDP服务器自动开启为false
        /// </summary>
        private void Reset()
        {
            GetComponent<SfUdpServerMono>().serverData.autoStart = false;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// UDP 配置文件支持编辑器
    /// </summary>
    [UnityEditor.CustomEditor(typeof(SfUDPServer))]
    public class SfUdpConfigSupportEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var rootVisualElement = new VisualElement();
            // rootVisualElement

            var label = new Label();
            
            


            return rootVisualElement;
        }
    }
#endif
}