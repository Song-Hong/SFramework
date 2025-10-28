using SFramework.Core.Mono;
using SFramework.SFHardware.Module;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.SFHardware.Mono
{
    /// <summary>
    /// 相机模块单例
    /// </summary>
    public class SfCameraMono:SfMonoSingleton<SfCameraMono>
    {
        /// <summary>
        /// 相机模块
        /// </summary>
        public SfCamera Camera;
        /// <summary>
        /// 显示相机图像的RawImage组件
        /// </summary>
        public RawImage image;
        /// <summary>
        /// 是否镜像显示
        /// </summary>
        /// <returns></returns>
        public bool isMirrored = true;
        
        private void Start()
        {
            Camera = new SfCamera();
            Camera.StartDefault();
            image.texture = Camera.ActiveTexture;
            if (isMirrored)
            {
                image.rectTransform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}