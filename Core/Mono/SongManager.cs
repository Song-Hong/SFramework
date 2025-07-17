using System;
using Song.Core.Extends.Unity;
using Song.Core.Module;
using Song.Core.Module.IO;
using Song.Core.Module.Server;
using UnityEngine;
using System.Collections; // 确保包含这个命名空间
using System.IO; // 添加System.IO 命名空间

namespace Song.Core.Mono
{
    /// <summary>
    /// 管理器
    /// </summary>
    public class SongManager:MonoSingleton<SongManager>
    {
        /// <summary>
        /// 文件模块
        /// </summary>
        public SongFile File;
        
        protected override void Awake()
        {
            base.Awake();
            
            //初始化模块
            File = new SongFile();
            
            // var loadFromFile = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AB/test/cube");
            // foreach (var allAssetName in loadFromFile.GetAllAssetNames())
            // {
            //     Debug.Log(allAssetName);
            // }
        }
    }
}