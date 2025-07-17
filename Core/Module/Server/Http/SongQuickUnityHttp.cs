using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SFramework.Core.Module.Server.Http
{
    /// <summary>
    /// 用于快速进行Unity的HTTP的各项请求的优化版本。
    /// 提供了更健壮的错误处理、支持自定义请求头和超时，并使用了最新的UnityWebRequest API。
    /// </summary>
    public static class SongQuickUnityHttp
    {
        #region Get 请求
        /// <summary>
        /// 发送GET请求获取Sprite。
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="onSuccess">成功回调，返回Sprite。</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        public static void GetSprite(MonoBehaviour owner, string url, Action<Sprite> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequestTexture.GetTexture(url),
                (uwr) => {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    if (texture != null)
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                        onSuccess?.Invoke(sprite);
                    }
                },
                onFailure, headers, timeout
            ));
        }

        /// <summary>
        /// 发送GET请求获取Texture2D。
        /// </summary>
        public static void GetTexture2D(MonoBehaviour owner, string url, Action<Texture2D> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequestTexture.GetTexture(url),
                (uwr) => onSuccess?.Invoke(DownloadHandlerTexture.GetContent(uwr)),
                onFailure, headers, timeout
            ));
        }

        /// <summary>
        /// 发送GET请求获取AssetBundle。
        /// </summary>
        public static void GetAssetBundle(MonoBehaviour owner, string url, Action<AssetBundle> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 60)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequestAssetBundle.GetAssetBundle(url),
                (uwr) => onSuccess?.Invoke(DownloadHandlerAssetBundle.GetContent(uwr)),
                onFailure, headers, timeout
            ));
        }

        /// <summary>
        /// 发送GET请求获取AudioClip。
        /// </summary>
        public static void GetAudioClip(MonoBehaviour owner, string url, AudioType audioType, Action<AudioClip> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequestMultimedia.GetAudioClip(url, audioType),
                (uwr) => onSuccess?.Invoke(DownloadHandlerAudioClip.GetContent(uwr)),
                onFailure, headers, timeout
            ));
        }

        /// <summary>
        /// 发送GET请求获取字符串。
        /// </summary>
        public static void GetString(MonoBehaviour owner, string url, Action<string> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(url),
                (uwr) => onSuccess?.Invoke(uwr.downloadHandler.text),
                onFailure, headers, timeout
            ));
        }

        /// <summary>
        /// 发送GET请求获取字节数组。
        /// </summary>
        public static void GetData(MonoBehaviour owner, string url, Action<byte[]> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(url),
                (uwr) => onSuccess?.Invoke(uwr.downloadHandler.data),
                onFailure, headers, timeout
            ));
        }

        #endregion

        #region Post 请求
        /// <summary>
        /// 发送POST请求，通常用于提交JSON数据。
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="json">要发送的JSON字符串。</param>
        /// <param name="onSuccess">成功回调，返回服务器响应的字符串。</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        public static void PostJson(MonoBehaviour owner, string url, string json, Action<string> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            // 使用 using 语句确保 UnityWebRequest 在完成后被正确释放
            using (var uwr = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                // 关键：设置Content-Type为application/json
                uwr.SetRequestHeader("Content-Type", "application/json");

                owner.StartCoroutine(SendRequest(uwr, (req) => onSuccess?.Invoke(req.downloadHandler.text), onFailure, headers, timeout));
            }
        }
        
        /// <summary>
        /// 发送POST请求，并获取字节数组作为响应。
        /// </summary>
        public static void PostData(MonoBehaviour owner, string url, string postData, Action<byte[]> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            using (var uwr = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json"); // 假设是JSON，如果不是可以修改或作为参数传入

                owner.StartCoroutine(SendRequest(uwr, (req) => onSuccess?.Invoke(req.downloadHandler.data), onFailure, headers, timeout));
            }
        }

        #endregion

        #region 核心请求协程
        /// <summary>
        /// 统一的发送请求的协程。
        /// </summary>
        /// <param name="uwr">已经配置好的UnityWebRequest对象。</param>
        /// <param name="onSuccess">成功回调。</param>
        /// <param name="onFailure">失败回调。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">超时时间（秒）。</param>
        private static IEnumerator SendRequest(UnityWebRequest uwr, Action<UnityWebRequest> onSuccess, Action<string> onFailure, Hashtable headers, int timeout)
        {
            // 使用 using 语句确保 UnityWebRequest 在协程结束时被正确释放
            using (uwr)
            {
                // 设置超时
                uwr.timeout = timeout;

                // 添加自定义请求头
                if (headers != null)
                {
                    foreach (DictionaryEntry header in headers)
                    {
                        uwr.SetRequestHeader(header.Key.ToString(), header.Value.ToString());
                    }
                }

                // 发送请求
                yield return uwr.SendWebRequest();

                // 检查结果
                #if UNITY_2020_2_OR_NEWER
                if (uwr.result != UnityWebRequest.Result.Success)
                #else
                if (uwr.isNetworkError || uwr.isHttpError)
                #endif
                {
                    // 记录更详细的错误信息
                    string errorMsg = $"请求失败: {uwr.url}\nHTTP Status Code: {uwr.responseCode}\nError: {uwr.error}";
                    Debug.LogError(errorMsg);
                    onFailure?.Invoke(errorMsg);
                }
                else
                {
                    // Debug.Log($"请求成功: {uwr.url}\nHTTP Status Code: {uwr.responseCode}");
                    onSuccess?.Invoke(uwr);
                }
            } // uwr 会在这里被自动 Dispose
        }

        #endregion
    }
}
