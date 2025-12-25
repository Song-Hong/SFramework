using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SFramework.SFIo.Module
{
    /// <summary>
    /// StreamingAssets 读取实现
    /// </summary>
    public class SfFileQuickStreamingAssets
    {
        /// <summary>
        /// 全平台兼容的 StreamingAssets 路径转换 (宏定义封装)
        /// </summary>
        private string GetStreamingAssetsPath(string fileName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, fileName);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA || UNITY_IOS
            // 这些平台需要显式指定 file:// 协议，并确保路径分隔符为正斜杠 /
            if (!path.Contains("://"))
            {
                path = "file://" + path.Replace("\\", "/");
            }
#elif UNITY_ANDROID
            // Android 路径本身已包含 jar:file://，UnityWebRequest 直接识别，无需处理
#elif UNITY_WEBGL
            // WebGL 环境下 StreamingAssets 是相对或绝对的 URL，不应添加 file://
#endif
            return path;
        }
        
        /// <summary>
        /// 读取文本文件
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="fileName">文件名（包含路径）</param>
        /// <param name="onSuccess">成功回调，参数为文件内容字符串</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadString(MonoBehaviour owner, string fileName, Action<string> onSuccess, Action<string> onFailure = null, int timeout = 10)
        =>new SfFileUwp().ReadString(owner, GetStreamingAssetsPath(fileName), onSuccess, onFailure, timeout);
        
        /// <summary>
        /// 读取 JSON 文件
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="fileName">文件名（包含路径）</param>
        /// <param name="onSuccess">成功回调，参数为解析后的对象（T 类型）</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        /// <typeparam name="T">目标对象类型，必须是可序列化的</typeparam>
        public void ReadJson<T>(MonoBehaviour owner, string fileName, Action<T> onSuccess, Action<string> onFailure = null, int timeout = 10)
        =>new SfFileUwp().ReadJson(owner, GetStreamingAssetsPath(fileName), onSuccess, onFailure, timeout);
        
        /// <summary>
        /// 读取二进制文件
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="fileName">文件名（包含路径）</param>
        /// <param name="onSuccess">成功回调，参数为字节数组</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadBytes(MonoBehaviour owner, string fileName, Action<byte[]> onSuccess, Action<string> onFailure = null, int timeout = 10)
        =>new SfFileUwp().ReadBytes(owner, GetStreamingAssetsPath(fileName), onSuccess, onFailure, timeout);
        
        /// <summary>
        /// 读取图片文件 (PNG/JPG等)
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="fileName">文件名（包含路径）</param>
        /// <param name="onSuccess">成功回调，参数为转换后的 Sprite 对象</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadSprite(MonoBehaviour owner, string fileName, Action<Sprite> onSuccess, Action<string> onFailure = null, int timeout = 10)
        =>new SfFileUwp().ReadSprite(owner, GetStreamingAssetsPath(fileName), onSuccess, onFailure, timeout);
        
        /// <summary>
        /// 读取音频文件 (MP3/WAV等)
        /// </summary>
        /// <param name="owner">调用者 MonoBehaviour 实例</param>
        /// <param name="fileName">文件名（包含路径）</param>
        /// <param name="type">音频类型，如 AudioType.MPEG</param>
        /// <param name="onSuccess">成功回调，参数为转换后的 AudioClip 对象</param>
        /// <param name="onFailure">失败回调，参数为错误信息字符串</param>
        /// <param name="timeout">超时时间（秒），默认 10 秒</param>
        public void ReadAudio(MonoBehaviour owner, string fileName, AudioType type, Action<AudioClip> onSuccess, Action<string> onFailure = null, int timeout = 10)
        =>new SfFileUwp().ReadAudio(owner, GetStreamingAssetsPath(fileName), type, onSuccess, onFailure, timeout);
    }
}