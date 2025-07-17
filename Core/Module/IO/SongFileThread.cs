using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Song.Core.Module.IO
{
    /// <summary>
    /// SongFile工具包 线程加载模块
    /// </summary>
    public class SongFileThread
    {
        /// <summary>
        /// 使用协程加载全部文件,从StreamingAssets根目录中
        /// </summary>
        /// <param name="own">拥有者</param>
        /// <param name="callback">返回 文件路径 文件类型</param>
        public void LoadAllFilesFromStreamingAssets(MonoBehaviour own, Action<Type, string> callback = null)
            => LoadAllFiles(Application.streamingAssetsPath, callback);
        
        /// <summary>
        /// 使用线程加载全部文件路径
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="callback">读取到一个文件后回调</param>
        public void LoadAllFiles(string path,Action<Type, string> callback = null)
        {
            var mainContext = SynchronizationContext.Current;
            new Thread(() =>
            {
                //获取一个文件夹下全部文件，包含子目录
                var files = Directory.GetFiles(path, "*",
                    System.IO.SearchOption.AllDirectories);

                //遍历文件
                foreach (var file in files)
                {
                    //读取图片
                    if (SongFile.PathIsImg(file))
                    {
                        mainContext.Post(x => { callback?.Invoke(typeof(Texture2D), file); }, null);
                    }
                    //读取音频
                    else if (SongFile.PathIsAudio(file))
                    {
                        mainContext.Post(x => { callback?.Invoke(typeof(AudioClip), file); }, null);
                    }
                }
            }).Start();
        }

        /// <summary>
        /// 使用协程加载全部图片,从StreamingAssets根目录中
        /// </summary>
        /// <param name="callback">返回 图片</param>
        public void LoadAllTextureFromStreamingAssets(Action<Texture2D> callback = null)
            => LoadAllTexture(Application.streamingAssetsPath, callback);

        /// <summary>
        /// 使用协程加载Textures
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllTexture(string path, Action<Texture2D> callback = null)
        {
            LoadAllFiles(path, (fileType,filePath) =>
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
        /// <param name="callback">返回 图片</param>
        public void LoadAllSpriteFromStreamingAssets(Action<Sprite> callback = null)
            => LoadAllSprite(Application.streamingAssetsPath, callback);

        /// <summary>
        /// 使用协程加载Sprites
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="callback">返回 图片</param>
        public void LoadAllSprite(string path, Action<Sprite> callback = null)
        {
            LoadAllFiles(path, (fileType,filePath) =>
            {
                if (fileType == typeof(Texture2D))
                {
                    callback?.Invoke(SongFile.LoadSpriteFromPath(filePath));
                }
            });
        }
    }
}