using System;
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
        /// 导出类型
        /// </summary>
        public enum ExportType
        {
            /// 导出原始图像
            RawImage,
            /// 导出纹理
            RenderTexture,
        }
        /// <summary>
        /// 导出类型
        /// </summary>
        public ExportType exportType = ExportType.RawImage;
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
        public bool isMirrored = true;
        /// <summary>
        /// 导出的纹理
        /// </summary>
        public RenderTexture texture;
        /// <summary>
        /// 是否自动创建RawImage组件
        /// </summary>
        public bool autoCreate = false;
        /// <summary>
        /// 导出纹理大小
        /// </summary>
        public Vector2Int textureSize = new Vector2Int(1920, 1080);
        /// <summary>
        /// WebCamTexture
        /// </summary>
        private WebCamTexture _webCamTexture;
        
        private void Start()
        {
            Camera = new SfCamera();
            Camera.StartDefault();

            if (exportType == ExportType.RawImage)
            {
                CreateRawImage();
            }
            else
            {
                CreateTexture();
            }
        }

        /// <summary>
        /// 创建RawImage组件
        /// </summary>
        public void CreateRawImage()
        {
            image.texture = Camera.ActiveTexture;
            if (isMirrored)
            {
                image.rectTransform.localScale = new Vector3(-1, 1, 1);
            }
        }
        /// <summary>
        /// 创建Texture组件
        /// </summary>
        public void CreateTexture()
        {
            _webCamTexture = Camera.ActiveTexture;
            if(autoCreate)
                texture = new RenderTexture(textureSize.x, textureSize.y,24);
        }

        /// <summary>
        /// 更新导出纹理
        /// </summary>
        private void Update()
        {
            // 这些操作必须在主线程执行，EditorApplication.update 确保了这一点
            if (exportType == ExportType.RenderTexture && texture!=null && _webCamTexture.didUpdateThisFrame)
            {
                // Graphics.Blit(_webCamTexture, texture);
                Graphics.Blit(_webCamTexture, texture);
            }
        }
    }
}