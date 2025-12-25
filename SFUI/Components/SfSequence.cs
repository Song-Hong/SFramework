using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SFramework.SfUI.Components
{
    /// <summary>
    /// 
    /// </summary>
    public class SfSequence : MonoBehaviour
    {
        public enum RenderType { SpriteRenderer, UIImage, RawImage }

        [Header("渲染设置")] 
        public RenderType renderType = RenderType.UIImage;
        public List<Sprite> frames = new List<Sprite>();
        public List<Texture2D> textureFrames = new List<Texture2D>();

        [Header("播放范围控制")]
        [Tooltip("起始帧索引")] public int startFrame;
        [Tooltip("结束帧索引（包含该帧）")] public int endFrame = 100;
        
        [Header("常规设置")]
        public int frameRate = 24;
        public bool playOnAwake = true;
        public bool loop = true;

        [Header("引用")] 
        public Image uiImage;
        public SpriteRenderer spriteRenderer;
        public RawImage rawImage;

        private int _currentIndex;
        private float _timer;
        private bool _isPlaying;

        private void Awake()
        {
            if (renderType == RenderType.UIImage && uiImage == null) uiImage = GetComponent<Image>();
            if (renderType == RenderType.SpriteRenderer && spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (renderType == RenderType.RawImage && rawImage == null) rawImage = GetComponent<RawImage>();

            // 初始化时将当前帧设为起始帧
            _currentIndex = startFrame;
            if (playOnAwake) Play();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            
            int totalCount = GetTotalCount();
            if (totalCount == 0) return;

            _timer += Time.deltaTime;
            float frameDuration = 1f / frameRate;

            if (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                UpdateFrame(totalCount);
            }
        }

        private void UpdateFrame(int totalCount)
        {
            _currentIndex++;

            // 获取有效的结束索引（不能超过数组实际长度）
            var effectiveEndFrame = Math.Min(endFrame, totalCount - 1);
            // 获取有效的开始索引
            var effectiveStartFrame = Math.Clamp(startFrame, 0, effectiveEndFrame);

            // 范围循环逻辑
            if (_currentIndex > effectiveEndFrame)
            {
                if (loop)
                {
                    _currentIndex = effectiveStartFrame;
                }
                else
                {
                    _isPlaying = false;
                    _currentIndex = effectiveEndFrame;
                    return;
                }
            }
            
            // 安全检查：如果 currentIndex 小于当前的 startFrame（比如运行时突然改大 startFrame）
            if (_currentIndex < effectiveStartFrame) _currentIndex = effectiveStartFrame;

            ApplyResource(_currentIndex);
        }

        private void ApplyResource(int index)
        {
            if (renderType == RenderType.RawImage)
            {
                if (index < textureFrames.Count)
                    ApplyTexture(textureFrames[index]);
            }
            else
            {
                if (index < frames.Count)
                    ApplySprite(frames[index]);
            }
        }

        private int GetTotalCount()
        {
            return (renderType == RenderType.RawImage) ? textureFrames.Count : frames.Count;
        }

        // --- 核心修改：支持运行时修改范围 ---

        /// <summary>
        /// 动态设置循环区间
        /// </summary>
        public void SetRange(int start, int end)
        {
            startFrame = start;
            endFrame = end;
            
            // 如果当前播放进度不在新范围内，重置到起点
            if (_currentIndex < startFrame || _currentIndex > endFrame)
            {
                _currentIndex = startFrame;
                ApplyResource(_currentIndex);
            }
        }

        // --- 基础控制 ---

        private void ApplySprite(Sprite sprite)
        {
            if (renderType == RenderType.UIImage && uiImage != null) uiImage.sprite = sprite;
            else if (renderType == RenderType.SpriteRenderer && spriteRenderer != null) spriteRenderer.sprite = sprite;
        }

        private void ApplyTexture(Texture2D tex)
        {
            if (renderType == RenderType.RawImage && rawImage != null) rawImage.texture = tex;
        }

        public void Play()
        {
            _isPlaying = true;
            // 每次 Play 是否回到 startFrame 取决于你的需求，通常建议保留当前位置继续
            if (_currentIndex < startFrame || _currentIndex > endFrame) _currentIndex = startFrame;
        }

        public void Pause() => _isPlaying = false;

        public void Stop()
        {
            _isPlaying = false;
            _currentIndex = startFrame;
            ApplyResource(_currentIndex);
        }
    }
}