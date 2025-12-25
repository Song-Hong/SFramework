using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace SFramework.SFIo.Module
{
    /// <summary>
    /// 第三层：具体平台读取实现
    /// </summary>
    public class SfFileQuickUwr
    {
        /// <summary>
        /// 读取StreamingAssets目录下的文件
        /// </summary>
        public SfFileQuickStreamingAssets StreamingAssets => new SfFileQuickStreamingAssets();
    }
}