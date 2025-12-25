using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace SFramework.SFIo.Module
{
    /// <summary>
    /// Uwp/Android 多平台文件操作类 - 专门用于读取 StreamingAssets 资源
    /// </summary>
    public class SfFileUwp
    {
        
        /// <summary>
        /// 读取文本文件 (JSON/TXT等)
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="path">文件路径（包含文件名）</param>
        /// <param name="onSuccess">成功回调，参数为文件内容字符串</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadString(MonoBehaviour owner, string path, Action<string> onSuccess, Action<string> onFailure = null, int timeout = 10)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(path),
                (uwr) => onSuccess?.Invoke(uwr.downloadHandler.text),
                onFailure, timeout
            ));
        }

        /// <summary>
        /// 读取并反序列化 JSON 
        /// </summary>
        /// <typeparam name="T">目标 JSON 数据类型</typeparam>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="path">文件路径（包含文件名）</param>
        /// <param name="onSuccess">成功回调，参数为反序列化后的对象</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadJson<T>(MonoBehaviour owner, string path, Action<T> onSuccess, Action<string> onFailure = null, int timeout = 10)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(path),
                (uwr) => onSuccess?.Invoke(JsonUtility.FromJson<T>(uwr.downloadHandler.text)),
                onFailure, timeout
            ));
        }

        /// <summary>
        /// 读取二进制数据 (Bytes)
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="path">文件路径（包含文件名）</param>
        /// <param name="onSuccess">成功回调，参数为文件内容字节数组</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadBytes(MonoBehaviour owner, string path, Action<byte[]> onSuccess, Action<string> onFailure = null, int timeout = 10)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequest.Get(path),
                (uwr) => onSuccess?.Invoke(uwr.downloadHandler.data),
                onFailure, timeout
            ));
        }

        /// <summary>
        /// 读取图片并转为 Sprite
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="path">文件路径（包含文件名）</param>
        /// <param name="onSuccess">成功回调，参数为转换后的 Sprite 对象</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadSprite(MonoBehaviour owner, string path, Action<Sprite> onSuccess, Action<string> onFailure = null, int timeout = 10)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequestTexture.GetTexture(path),
                (uwr) => {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    if (texture != null)
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        onSuccess?.Invoke(sprite);
                    }
                },
                onFailure, timeout
            ));
        }

        /// <summary>
        /// 读取音频文件 (MP3/WAV等)
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="path">文件路径（包含文件名）</param>
        /// <param name="type">音频类型，如 AudioType.MPEG</param>
        /// <param name="onSuccess">成功回调，参数为转换后的 AudioClip 对象</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadAudio(MonoBehaviour owner, string path, AudioType type, Action<AudioClip> onSuccess, Action<string> onFailure = null, int timeout = 10)
        {
            owner.StartCoroutine(SendRequest(
                UnityWebRequestMultimedia.GetAudioClip(path, type),
                (uwr) => onSuccess?.Invoke(DownloadHandlerAudioClip.GetContent(uwr)),
                onFailure, timeout
            ));
        }
        
        #region 内部通用请求处理
        private IEnumerator SendRequest(UnityWebRequest uwr, Action<UnityWebRequest> onSuccess, Action<string> onFailure, int timeout)
        {
            using (uwr)
            {
                uwr.timeout = timeout;
                yield return uwr.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (uwr.result != UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    string err = $"[SfFileUwp] 读取失败: {uwr.url} | Error: {uwr.error}";
                    Debug.LogError(err);
                    onFailure?.Invoke(err);
                }
                else
                {
                    onSuccess?.Invoke(uwr);
                }
            }
        }
        #endregion
    }
}