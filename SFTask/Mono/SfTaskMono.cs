using System;
using System.Collections;
using System.Collections.Generic;
using SFramework.Core.Mono;
using SFramework.SFTask.Module;
using UnityEngine;
using UnityEngine.Networking;

namespace SFramework.SFTask.Mono
{
    /// <summary>
    /// 任务模块Mono单例
    /// </summary>
    public class SfTaskMono:SfMonoSingleton<SfTaskMono>
    {
        /// <summary>
        /// 任务点列表
        /// </summary>
        public List<SfTaskPoint> tasks = new List<SfTaskPoint>();
        
        /// <summary>
        /// 任务启动类型
        /// </summary>
        public enum TaskStartType
        {
            /// <summary>
            /// 自动开始
            /// </summary>
            Auto,
            
            /// <summary>
            /// 手动开始
            /// </summary>
            Manual,
        }
        /// <summary>
        /// 任务启动类型
        /// </summary>
        public TaskStartType taskStartType = TaskStartType.Auto;
        /// <summary>
        /// 资源路径类型
        /// </summary>
        public enum AssetsPathType
        {
            /// <summary>
            /// 流资源
            /// </summary>
            StreamingAssets,
            /// <summary>
            /// 持久资源
            /// </summary>
            PersistentData,
            /// <summary>
            /// Web资源
            /// </summary>
            Url,
        }
        /// <summary>
        /// 资源路径类型
        /// </summary>
        public AssetsPathType assetsPathType = AssetsPathType.StreamingAssets;
        /// <summary>
        /// 资源路径
        /// </summary>
        public string assetsPath = "";
        
        #region 初始化
        /// <summary>
        /// 启动任务模块Mono单例
        /// </summary>
        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(assetsPath))
            {
                LoadSfTask(assetsPath);
            }
        }
        
        /// <summary>
        /// 读取文本
        /// </summary>
        /// <param name="path">路径</param>
        public void LoadSfTask(string path)
        {
            // 检查路径是否以.sftask结尾
            if(!path.EndsWith(".sftask"))
                path += ".sftask";
            // 根据资源路径类型获取目录路径
            var dirPath = assetsPathType switch
            {
                AssetsPathType.StreamingAssets => $"file://{Application.streamingAssetsPath}/",
                AssetsPathType.PersistentData => $"file://{Application.persistentDataPath}/",
                _ => ""
            };
            // 合并目录路径和资源路径
            path = dirPath + path;
            // 开始读取文本
            StartCoroutine(ReadAllTextIEnumerator(path, x =>
            {
                tasks = SfTaskParsing.ParseTask(x);
                if (taskStartType != TaskStartType.Auto) return;
                StartTask();
            }));
        }
        
        /// <summary>
        /// 开始 读取文本
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="callback">文本</param>
        private IEnumerator ReadAllTextIEnumerator(string path,Action<string> callback = null)
        {
            using var uwr = UnityWebRequest.Get(path);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ProtocolError) yield break;
            callback?.Invoke(uwr.downloadHandler.text);
        }
        #endregion
        
        #region 任务控制
        /// <summary>
        /// 启动所有任务
        /// </summary>
        public async void StartTask()
        {
            foreach (var sfTaskPoint in tasks)
            {
                await sfTaskPoint.Start();
            }
        }
        #endregion
    }
}