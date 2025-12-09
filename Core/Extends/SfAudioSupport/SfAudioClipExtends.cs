using UnityEngine;

namespace SFramework.Core.Extends.SfAudioSupport
{
    /// <summary>
    /// 音频文件扩展方法
    /// </summary>
    public static class SfAudioClipExtends
    {
        /// <summary>
        /// 将音频文件转换为字节数组
        /// </summary>
        /// <param name="audioClip">音频文件</param>
        /// <returns>字节数组</returns>
        public static byte[] ToByteArray(this AudioClip audioClip)
        =>SfWav.ToBytes(audioClip);

        /// <summary>
        /// 将音频文件保存为 WAV 文件
        /// </summary>
        /// <param name="audioClip">音频文件</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveToWav(this AudioClip audioClip, string filePath)
        =>SfWav.Save(audioClip, filePath);
    }
}