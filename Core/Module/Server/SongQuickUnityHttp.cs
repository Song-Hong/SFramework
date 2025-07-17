using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Song.Core.Module.Server
{
    /// <summary>
    /// 用于快速进行Unity的HTTP的各项请求
    /// </summary>
    public static class SongQuickUnityHttp
    {
        #region Get请求
        /// <summary>
        /// 发送Get请求获取Sprite
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回Sprite</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void GetSprite(MonoBehaviour own, string url, Action<Sprite> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(GetRequest(url, uwr =>
            {
                var texture = DownloadHandlerTexture.GetContent(uwr);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1f);
                onSuccess?.Invoke(sprite);
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Get请求获取Texture2D
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回Texture2D</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void GetTexture2D(MonoBehaviour own, string url, Action<Texture2D> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(GetRequest(url, uwr =>
            {
                onSuccess?.Invoke(DownloadHandlerTexture.GetContent(uwr));
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Get请求获取Texture
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回Texture</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void GetTexture(MonoBehaviour own, string url, Action<Texture> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(GetRequest(url, uwr =>
            {
                onSuccess?.Invoke(DownloadHandlerTexture.GetContent(uwr));
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Get请求获取 AB包 AssetBundle
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回 AB包 AssetBundle </param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void GetAssetBundle(MonoBehaviour own,string url, Action<AssetBundle> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(GetRequest(url, uwr =>
            {
                var assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                onSuccess?.Invoke(assetBundle);
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Get请求获取 语音 AudioClip
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回 语音 AudioClip </param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void GetAudioClip(MonoBehaviour own,string url, Action<AudioClip> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(GetRequest(url, uwr =>
            {
                onSuccess?.Invoke(DownloadHandlerAudioClip.GetContent(uwr));
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Get请求获取字符串
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回字符串</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void GetString(MonoBehaviour own, string url, Action<string> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(GetRequest(url, uwr =>
            {
                onSuccess?.Invoke(uwr.downloadHandler.text);
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Get请求获取数据
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回数据</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void GetData(MonoBehaviour own,string url,Action<byte[]> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(GetRequest(url, uwr =>
            {
                onSuccess?.Invoke(uwr.downloadHandler.data);
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Get请求的协程实现
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="onSuccess">成功回调，返回下载处理器</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <returns></returns>
        private static IEnumerator GetRequest(string url, Action<UnityWebRequest> onSuccess, Action<string> onFailure)
        {
            using var uwr = UnityWebRequest.Get(new Uri(url));
            yield return uwr.SendWebRequest();
            
            if (uwr.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"GET请求失败: {url}\n错误: {uwr.error}");
                onFailure?.Invoke(uwr.error);
            }
            else {
                onSuccess?.Invoke(uwr);
            }
        }
        #endregion

        #region Post请求
        /// <summary>
        /// 发送Poset请求 数据
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数</param>
        /// <param name="onSuccess">成功回调，返回数据</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void PostString(MonoBehaviour own,string url,string data,Action<string> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(PostRequest(url,data, downloadHandler =>
            {
                onSuccess?.Invoke(downloadHandler.text);
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Poset请求 数据
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数</param>
        /// <param name="onSuccess">成功回调，返回数据</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        public static void PostData(MonoBehaviour own,string url,string data,Action<byte[]> onSuccess, Action<string> onFailure = null)
        {
            own.StartCoroutine(PostRequest(url,data, downloadHandler =>
            {
                onSuccess?.Invoke(downloadHandler.data);
            }, onFailure));
        }
        
        /// <summary>
        /// 发送Post请求的协程实现
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数</param>
        /// <param name="onSuccess">成功回调，返回下载处理器</param>
        /// <param name="onFailure">失败回调，返回错误信息</param>
        /// <returns></returns>
        private static IEnumerator PostRequest(string url,string data,Action<DownloadHandler> onSuccess, Action<string> onFailure)
        {
            using var uwr = UnityWebRequest.PostWwwForm(new Uri(url),data);
            yield return uwr.SendWebRequest();
            
            if (uwr.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"POST请求失败: {url}\n错误: {uwr.error}");
                onFailure?.Invoke(uwr.error);
            }
            else {
                onSuccess?.Invoke(uwr.downloadHandler);
            }
        }
        #endregion
    }
}