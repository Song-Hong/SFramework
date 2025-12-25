using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SFramework.SFNet.Module.Quick
{
    /// <summary>
    /// 快速网络请求模块-Get请求
    /// </summary>
    public class SfNetQuickIEnumerator
    {
        #region Get请求
        /// <summary>
        /// 发送GET请求获取JSON数据并反序列化为指定类型。
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="onSuccess">成功回调，返回反序列化后的对象。</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        /// <typeparam name="T">目标类型，必须是可序列化的。</typeparam>
        public  void GetFromJson<T>(MonoBehaviour owner, string url, Action<T> onSuccess,
            Action<string> onFailure, Hashtable headers, int timeout)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(url),
                (uwr) => onSuccess?.Invoke(JsonUtility.FromJson<T>(uwr.downloadHandler.text)),
                onFailure, headers, timeout
            ));
        }
        
        /// <summary>
        /// 发送GET请求获取Sprite。
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="onSuccess">成功回调，返回Sprite。</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        public  void GetSprite(MonoBehaviour owner, string url, Action<Sprite> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
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
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="onSuccess">成功回调，返回Texture2D。</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        public  void GetTexture(MonoBehaviour owner, string url, Action<Texture2D> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequestTexture.GetTexture(url),
                (uwr) => onSuccess?.Invoke(DownloadHandlerTexture.GetContent(uwr)),
                onFailure, headers, timeout
            ));
        }

        /// <summary>
        /// 发送GET请求获取AudioClip。
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="audioType"></param>
        /// <param name="onSuccess">成功回调，返回AudioClip</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        public  void GetAudioClip(MonoBehaviour owner, string url, AudioType audioType, Action<AudioClip> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
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
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="onSuccess">成功回调，返回String</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        public  void GetString(MonoBehaviour owner, string url, Action<string> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(url),
                (uwr) => onSuccess?.Invoke(uwr.downloadHandler.text),
                onFailure, headers, timeout
            ));
        }

        /// <summary>
        /// 发送GET请求获取字节数组
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回字节数组</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        public  void GetData(MonoBehaviour owner, string url, Action<byte[]> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(url),
                (uwr) => onSuccess?.Invoke(uwr.downloadHandler.data),
                onFailure, headers, timeout
            ));
        }
        #endregion

        #region Post请求
        /// <summary>
        /// 发送GET请求获取JSON数据并反序列化为指定类型。
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址</param>
        /// <param name="body">请求参数 字符串类型</param>
        /// <param name="onSuccess">成功回调，返回反序列化后的对象。</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        /// <typeparam name="T">目标类型，必须是可序列化的。</typeparam>
        public  void PostFromJson<T>(MonoBehaviour owner, string url,string body, Action<T> onSuccess,
            Action<string> onFailure, Hashtable headers, int timeout)
        {
            owner.StartCoroutine(SendRequest(
                #if UNITY_2022_1_OR_NEWER
                UnityWebRequest.PostWwwForm(url,body),
                #else
                    UnityWebRequest.Post(url, body),
                #endif
                (uwr) => onSuccess?.Invoke(JsonUtility.FromJson<T>(uwr.downloadHandler.text)),
                onFailure, headers, timeout
            ));
        }
        
        /// <summary>
        /// 发送Post请求获取Sprite
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例</param>
        /// <param name="url">请求地址</param>
        /// <param name="body">请求参数 字符串类型</param>
        /// <param name="onSuccess">成功回调，返回Sprite</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        public  void PostSprite(MonoBehaviour owner,string url,string body,Action<Sprite> onSuccess,Action<string> onFailure,Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
#if UNITY_2022_1_OR_NEWER
                UnityWebRequest.PostWwwForm(url,body),
#else
                    UnityWebRequest.Post(url, body),
#endif
                (uwr) =>
                {
                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    if (texture == null) return;
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    onSuccess?.Invoke(sprite);
                },
                onFailure, headers, timeout
            ));
        }
        
        /// <summary>
        /// 发送Post请求获取Texture2D
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例</param>
        /// <param name="url">请求地址</param>
        /// <param name="body">请求参数 字符串类型</param>
        /// <param name="onSuccess">成功回调，返回Texture2D</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        public  void PostTexture(MonoBehaviour owner,string url,string body,Action<Texture2D> onSuccess,Action<string> onFailure,Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
#if UNITY_2022_1_OR_NEWER
                UnityWebRequest.PostWwwForm(url,body),
#else
                    UnityWebRequest.Post(url, body),
#endif
                (uwr) =>DownloadHandlerTexture.GetContent(uwr),
                onFailure, headers, timeout
            ));
        }
        
        /// <summary>
        /// 发送GET请求获取字符串。
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例。</param>
        /// <param name="url">请求地址。</param>
        /// <param name="body"></param>
        /// <param name="onSuccess">成功回调，返回String</param>
        /// <param name="onFailure">失败回调，返回错误信息。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">请求超时时间（秒）。</param>
        public  void PostString(MonoBehaviour owner, string url,string body, Action<string> onSuccess, Action<string> onFailure = null, Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
#if UNITY_2022_1_OR_NEWER
                UnityWebRequest.PostWwwForm(url,body),
#else
                    UnityWebRequest.Post(url, body),
#endif
                (uwr) => onSuccess?.Invoke(uwr.downloadHandler.text),
                onFailure, headers, timeout
            ));
        }
        
        /// <summary>
        /// 发送Post请求获取字节数组
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例</param>
        /// <param name="url">请求地址</param>
        /// <param name="body">请求参数 字符串类型</param>
        /// <param name="onSuccess">成功回调，返回字节数组</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        public  void PostData(MonoBehaviour owner, string url,string body,Action<byte[]> onSuccess,
            Action<string> onFailure = null,Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
#if UNITY_2022_1_OR_NEWER
                UnityWebRequest.PostWwwForm(url,body),
#else
                    UnityWebRequest.Post(url, body),
#endif
                (uwr)=>onSuccess?.Invoke(uwr.downloadHandler.data),
                onFailure, headers, timeout
                ));
        }
        #endregion

        #region Put请求
        /// <summary>
        /// 发送Put请求 推送字符串
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例</param>
        /// <param name="url">请求地址</param>
        /// <param name="body">请求参数 字符串</param>
        /// <param name="onSuccess">成功回调，返回字符串</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        public  void PutString(MonoBehaviour owner, string url,string body,Action<string> onSuccess,
            Action<string> onFailure = null,Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Put(url, body),
                (uwr)=>onSuccess?.Invoke(uwr.downloadHandler.text),
                onFailure, headers, timeout
                ));
        }
        
        /// <summary>
        /// 发送Put请求 推送字节数组
        /// </summary>
        /// <param name="owner">发起协程的MonoBehaviour实例</param>
        /// <param name="url">请求地址</param>
        /// <param name="body">请求参数 字节数组类型</param>
        /// <param name="onSuccess">成功回调，返回字符串</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        public  void PutData(MonoBehaviour owner, string url,byte[] body,Action<string> onSuccess,
            Action<string> onFailure = null,Hashtable headers = null, int timeout = 30)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Put(url, body),
                (uwr)=>onSuccess?.Invoke(uwr.downloadHandler.text),
                onFailure, headers, timeout
                ));
        }
        #endregion
        
        #region Uwr请求
        /// <summary>
        /// 统一的发送请求的协程。
        /// </summary>
        /// <param name="uwr">已经配置好的UnityWebRequest对象。</param>
        /// <param name="onSuccess">成功回调。</param>
        /// <param name="onFailure">失败回调。</param>
        /// <param name="headers">自定义请求头。</param>
        /// <param name="timeout">超时时间（秒）。</param>
        private  IEnumerator SendRequest(UnityWebRequest uwr, Action<UnityWebRequest> onSuccess, Action<string> onFailure, Hashtable headers, int timeout)
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