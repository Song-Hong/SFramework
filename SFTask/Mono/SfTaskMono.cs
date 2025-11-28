using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; // 引入 IO 命名空间
using SFramework.Core.Mono;
using SFramework.SFTask.Module;
using UnityEngine;
using UnityEngine.Networking;

namespace SFramework.SFTask.Mono
{
    /// <summary>
    /// 任务模块Mono单例
    /// </summary>
    public class SfTaskMono : SfMonoSingleton<SfTaskMono>
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
        /// <param name="fileName">文件名或相对路径</param>
        public void LoadSfTask(string fileName)
        {
            // 1. 检查后缀
            if (!fileName.EndsWith(".sftask"))
                fileName += ".sftask";

            string fullPath = "";

            // 2. 根据平台和类型构建路径
            switch (assetsPathType)
            {
                case AssetsPathType.StreamingAssets:
                    // Android 特殊处理：StreamingAssets 路径本身包含 "jar:file://"，不能再加 "file://"
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        fullPath = Path.Combine(Application.streamingAssetsPath, fileName);
                    }
                    else
                    {
                        // PC/Editor/iOS 需要 "file://" 前缀
                        fullPath = Path.Combine(Application.streamingAssetsPath, fileName);
                        fullPath = "file://" + fullPath;
                    }
                    break;

                case AssetsPathType.PersistentData:
                    // PersistentDataPath 在所有平台都需要 "file://" 用于 UnityWebRequest
                    fullPath = Path.Combine(Application.persistentDataPath, fileName);
                    fullPath = "file://" + fullPath;
                    break;

                case AssetsPathType.Url:
                    // 或者是直接的 HTTP 地址
                    fullPath = fileName; 
                    break;
            }

            // 3. 【关键修复】将所有反斜杠 '\' 替换为正斜杠 '/' 
            // 这一步解决了 "The hostname could not be parsed" 错误
            fullPath = fullPath.Replace("\\", "/");
            
            Debug.Log($"[SfTaskMono] 准备加载任务路径: {fullPath}");

            // 4. 开始读取文本
            StartCoroutine(ReadAllTextIEnumerator(fullPath, x =>
            {
                tasks = SfTaskParsing.ParseTask(x);
                if (tasks == null)
                {
                    Debug.LogError("[SfTaskMono] 任务解析结果为空！");
                    return;
                }
                
                if (taskStartType == TaskStartType.Auto)
                {
                    StartTask();
                }
            }));
        }

        /// <summary>
        /// 开始 读取文本
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="callback">文本</param>
        private IEnumerator ReadAllTextIEnumerator(string path, Action<string> callback = null)
        {
            using var uwr = UnityWebRequest.Get(path);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SfTaskMono] 读取任务文件失败: {path}\nError: {uwr.error}");
                yield break;
            }

            callback?.Invoke(uwr.downloadHandler.text);
        }

        #endregion

        #region 任务控制

        /// <summary>
        /// 启动所有任务
        /// </summary>
        public async void StartTask()
        {
            if (tasks == null || tasks.Count == 0) return;
            
            foreach (var sfTaskPoint in tasks)
            {
                await sfTaskPoint.Start();
            }
        }

        #endregion
    }
}