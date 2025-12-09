using System;
using System.Collections.Generic;
using UnityEngine.Serialization;


namespace SFramework.Core.Editor.Windows
{
    /// <summary>
    /// 扩展项数据
    /// </summary>
    [Serializable]
    public class PackagesDatas
    {
        /// <summary>
        /// 扩展项列表
        /// </summary>
        public List<PackagesData> packages = new List<PackagesData>();
    }

    [Serializable]
    public class PackagesData
    {
        /// <summary>
        /// 扩展项名称
        /// </summary>
        public string name;
        /// <summary>
        /// 扩展项版本
        /// </summary>
        public string version;
        /// <summary>
        /// 扩展项描述
        /// </summary>
        public string description;
        /// <summary>
        /// 扩展项URL
        /// </summary>
        public string url;
        /// <summary>
        /// 扩展项图标
        /// </summary>
        public string icon;
    }
}