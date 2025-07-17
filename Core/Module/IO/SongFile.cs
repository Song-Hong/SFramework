using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Song.Core.Module.IO
{
    /// <summary>
    /// 文件管理器
    /// </summary>
    public class SongFile
    {
        #region 字段
        /// <summary>
        /// 线程
        /// </summary>
        public SongFileThread Thread;
        
        /// <summary>
        /// 协程
        /// </summary>
        public SongFileIEnumerator IEnumerator;
        #endregion
        
        #region 构造函数
        /// <summary>
        /// 文件管理器
        /// </summary>
        public SongFile()
        {
            IEnumerator = new SongFileIEnumerator();
            Thread = new SongFileThread();
        }
        
        /// <summary>
        /// 新建一个文件管理器
        /// </summary>
        /// <returns>文件管理器</returns>
        public static SongFile New()
        {
            return new SongFile();
        }
        #endregion
        
        #region 静态方法 读取图片
        /// <summary>
        /// 协程读取图片
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回图片</param>
        /// <returns>文件</returns>
        public static void LoadTexture(MonoBehaviour own,string path,Action<Texture2D> callback = null)
        {
            own.StartCoroutine(EnReadTexture(path,callback));
        }
            
        private static IEnumerator EnReadTexture(string path,Action<Texture2D> callback = null)
        {
            Texture2D texture;
            using var uwr = UnityWebRequestTexture.GetTexture(path);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ProtocolError) yield break;
            texture = DownloadHandlerTexture.GetContent(uwr);
            callback?.Invoke(texture);
        }
        
        /// <summary>
        /// 从路径中读取图片至Texture2D
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Texture2D</returns>
        public static Texture2D LoadTextureFromPath(string filePath)
        =>LoadTextureFromBytes(File.ReadAllBytes(filePath));
        
        /// <summary>
        /// 从路径中读取图片至Sprite
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Sprite</returns>
        public static Sprite LoadSpriteFromPath(string filePath)
        =>Texture2D2Sprite(LoadTextureFromPath(filePath));
        
        /// <summary>
        /// 从数据读取图片至Texture2D
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Texture2D</returns>
        public static Texture2D LoadTextureFromBytes(byte[] data)
        {
            var texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(data);
            return texture2D;
        }
        
        /// <summary>
        /// 从数据读取图片至Sprite
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Sprite</returns>
        public static Sprite LoadSpriteFromBytes(byte[] data)
        =>Texture2D2Sprite(LoadTextureFromBytes(data));
        #endregion

        #region 静态方法 读取文件
        /// <summary>
        /// 读取全部字节
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">路径</param>
        /// <param name="callback">字节数据</param>
        public static void ReadAllBytes(MonoBehaviour own,string path,Action<byte[]> callback = null)
        {
            if (own)
                own.StartCoroutine(ReadAllBytesIEnumerator(path,callback));
            else
                Debug.LogError("Own无法使用协程加载文件");
        }

        /// <summary>
        /// 开始 读取全部字节
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="callback">字节数据</param>
        private static IEnumerator ReadAllBytesIEnumerator(string path,Action<byte[]> callback = null)
        {
            if(!path.StartsWith("file://"))
                path = "file://"+path;
            using var uwr = UnityWebRequest.Get(path);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ProtocolError) yield break;
            callback?.Invoke(uwr.downloadHandler.data);
        }
        
        /// <summary>
        /// 读取文本
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">路径</param>
        /// <param name="callback">文本</param>
        public static void ReadAllText(MonoBehaviour own, string path, Action<string> callback = null)
        {
            if (own)
                own.StartCoroutine(ReadAllTextIEnumerator(path,callback));
            else
                Debug.LogError("Own无法使用协程加载文件");
        }
        
        /// <summary>
        /// 开始 读取文本
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="callback">文本</param>
        private static IEnumerator ReadAllTextIEnumerator(string path,Action<string> callback = null)
        {
            if(!path.StartsWith("file://"))
                path = "file://"+path;
            using var uwr = UnityWebRequest.Get(path);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ProtocolError) yield break;
            callback?.Invoke(uwr.downloadHandler.text);
        }
        #endregion
        
        #region 静态方法 加载音频
        /// <summary>
        /// 协程加载音频
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回音频</param>
        /// <returns>文件</returns>
        public static void LoadAudio(MonoBehaviour own,string path,Action<AudioClip> callback = null)
        {
            own.StartCoroutine(EnReadAudio(path,callback));
        }

        private static IEnumerator EnReadAudio(string path,Action<AudioClip> callback = null)
        {
            AudioClip audio;
            using var uwr = UnityWebRequestMultimedia.GetAudioClip(path,AudioType.UNKNOWN);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ProtocolError) yield break;
            audio = DownloadHandlerAudioClip.GetContent(uwr);
            callback?.Invoke(audio);
        }
        #endregion
        
        #region 静态方法 工具
        /// <summary>
        /// 路径是否是图片
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否是图片</returns>
        public static bool PathIsImg(string path)
        {
            var type = Path.GetExtension(path).ToLower();
            return type.EndsWith("jpg")||type.EndsWith("jpeg")||type.EndsWith("png");
        }
        
        /// <summary>
        /// 路径是否是音频
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否是音频</returns>
        public static bool PathIsAudio(string path)
        {
            var type = Path.GetExtension(path).ToLower();
            return type.EndsWith("mp3")||type.EndsWith("wav")||type.EndsWith("ogg");
        }

        /// <summary>
        /// 将Texture转为Sprite
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <returns>Sprite</returns>
        public static Sprite Texture2D2Sprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1f);//将Texture转为Sprite 
        }
        #endregion
    }
}