using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Song.Core.Module.IO
{
    /// <summary>
    /// SongFile工具包 协程加载模块
    /// </summary>
    public class SongFileIEnumerator
    {
        /// <summary>
        /// 使用协程加载全部文件,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="callback">返回 文件路径 文件类型</param>
        public void LoadAllFilesFromStreamingAssets(MonoBehaviour own, Action<Type, string> callback = null)
            => LoadAllFiles(own, Application.streamingAssetsPath, callback);

        /// <summary>
        /// 使用协程加载全部文件,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">追加文件夹路径</param>
        /// <param name="callback">返回 文件路径 文件类型</param>
        public void LoadAllFilesFromStreamingAssets(MonoBehaviour own,string path, Action<Type, string> callback = null)
            => LoadAllFiles(own, Application.streamingAssetsPath+"/"+path, callback);
        
        /// <summary>
        /// 使用协程加载全部文件,从文件夹中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回 文件路径 文件类型</param>
        public void LoadAllFiles(MonoBehaviour own, string path, Action<Type, string> callback = null)
        {
            if (own)
                own.StartCoroutine(LoadAllFilesIEnumerator(path, callback));
            else
                Debug.LogError("Own无法使用协程加载文件");
        }

        /// <summary>
        /// 使用协程加载全部文件,从StreamingAssets根目录中
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回 文件路径1 文件类型</param>
        private IEnumerator LoadAllFilesIEnumerator(string path, Action<Type, string> callback = null)
        {
            //获取文件位置
            var dirPath = Path.Combine(Application.streamingAssetsPath, path);
            //判断文件夹是否存在
            if (!Directory.Exists(dirPath)) yield break;
            //获取一个文件夹下全部文件，包含子目录
            var files = Directory.GetFiles(dirPath, "*", System.IO.SearchOption.AllDirectories);
            //遍历文件
            foreach (var file in files)
            {
                if (SongFile.PathIsImg(file))
                {
                    callback?.Invoke(typeof(Texture2D),file);
                }
                else if (SongFile.PathIsAudio(file))
                {
                    callback?.Invoke(typeof(AudioClip),file);
                }
                yield return null;
            }
        }

        /// <summary>
        /// 使用协程加载全部图片,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllTextureFromStreamingAssets(MonoBehaviour own, Action<Texture2D> callback = null)
            => LoadAllTexture(own, Application.streamingAssetsPath, callback);

        /// <summary>
        /// 使用协程加载全部图片,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">追加文件夹路径</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllTextureFromStreamingAssets(MonoBehaviour own,string path ,Action<Texture2D> callback = null)
            => LoadAllTexture(own, Application.streamingAssetsPath+"/"+path, callback);
        
        /// <summary>
        /// 使用协程加载Textures
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllTexture(MonoBehaviour own, string path, Action<Texture2D> callback = null)
        {
            LoadAllFiles(own, path, (fileType,filePath) =>
            {
                if (fileType == typeof(Texture2D))
                {
                    callback?.Invoke(SongFile.LoadTextureFromPath(filePath));
                }
            });
        }

        /// <summary>
        /// 使用协程加载全部图片,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllSpriteFromStreamingAssets(MonoBehaviour own, Action<Sprite> callback = null)
            => LoadAllSprite(own, Application.streamingAssetsPath, callback);

        /// <summary>
        /// 使用协程加载全部图片,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">追加文件夹路径</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllSpriteFromStreamingAssets(MonoBehaviour own,string path,Action<Sprite> callback = null)
            => LoadAllSprite(own, Application.streamingAssetsPath+"/"+path, callback);
        
        /// <summary>
        /// 使用协程加载Sprites
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllSprite(MonoBehaviour own, string path, Action<Sprite> callback = null)
        {
            LoadAllFiles(own, path, (fileType,filePath) =>
            {
                if (fileType == typeof(Texture2D))
                {
                    callback?.Invoke(SongFile.LoadSpriteFromPath(filePath));
                }
            });
        }
        
        /// <summary>
        /// 使用协程加载全部语音,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="callback">返回 语音</param>
        public void LoadAllAudioFromStreamingAssets(MonoBehaviour own, Action<AudioClip> callback = null)
            => LoadAllAudio(own, Application.streamingAssetsPath, callback);
        
        /// <summary>
        /// 使用协程加载全部语音,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">追加文件夹路径</param>
        /// <param name="callback">返回 语音</param>
        public void LoadAllAudioFromStreamingAssets(MonoBehaviour own,string path, Action<AudioClip> callback = null)
            => LoadAllAudio(own, Application.streamingAssetsPath+"/"+path, callback);
        
        /// <summary>
        /// 使用协程加载全部语音
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回 语音</param>
        public void LoadAllAudio(MonoBehaviour own, string path, Action<AudioClip> callback = null)
        {
            LoadAllFiles(own, path, (fileType,filePath) =>
            {
                if (fileType == typeof(AudioClip))
                {
                    SongFile.LoadAudio(own,filePath, clip =>
                    {
                        callback?.Invoke(clip);
                    });
                }
            });
        }
    }
}