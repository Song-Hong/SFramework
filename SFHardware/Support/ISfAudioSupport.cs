using UnityEngine;

namespace SFramework.SFHardware.Support
{
    /// <summary>
    /// 音频支持接口
    /// </summary>
    public interface ISfAudioSupport
    {
        /// <summary>
        /// 加载音频文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>加载的音频文件</returns>
        public AudioClip Load(string path);
        
        /// <summary>
        /// 保存音频文件
        /// </summary>
        /// <param name="clip">音频文件</param>
        /// <param name="path">文件路径</param>
        /// <returns>是否保存成功</returns>
        public bool Save(AudioClip clip, string path);
    }
}