using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SAI.Editor.Quick.Support
{
    /// <summary>
    /// 思维链逐项动画：圆圈缩放出现 → 文字淡入；运行中当前项图标呼吸，结束后强制停止。
    /// </summary>
    public sealed class SfAiQuickChainAnimator
    {
        struct Item
        {
            public VisualElement Row;
            public VisualElement Circle;
            public VisualElement Text;
            public Action OnShown;
            public bool BreathAfterShow;
        }

        readonly Queue<Item> _queue = new Queue<Item>();
        bool _playing;
        bool _hooked;
        double _phaseStart;
        int _phase;
        Item _current;

        VisualElement _breathCircle;
        bool _breathing;
        bool _breathAllowed;
        double _breathStart;

        const float ScaleDuration = 0.18f;
        const float TextDuration = 0.14f;
        const float GapDuration = 0.06f;
        const float BreathPeriod = 1.35f;
        const float BreathMin = 0.88f;
        const float BreathMax = 1.08f;
        const float BreathOpacityMin = 0.55f;
        const float BreathOpacityMax = 1f;

        /// <summary>仅在整轮任务运行中允许呼吸</summary>
        public void SetBreathAllowed(bool allowed)
        {
            _breathAllowed = allowed;
            if (!allowed)
                StopBreathingVisual();
        }

        public void Enqueue(
            VisualElement parent,
            VisualElement row,
            VisualElement circle,
            VisualElement text,
            Action onShown = null,
            bool breathAfterShow = true)
        {
            if (parent == null || row == null) return;

            // 结束类节点或不允许呼吸时，立刻停掉上一项
            if (!breathAfterShow || !_breathAllowed)
                StopBreathingVisual();
            else
                StopBreathingVisual(); // 换到新操作项前也先停

            circle.style.scale = new StyleScale(new Scale(new Vector2(0.01f, 0.01f)));
            text.style.opacity = 0f;

            parent.Add(row);
            _queue.Enqueue(new Item
            {
                Row = row,
                Circle = circle,
                Text = text,
                OnShown = onShown,
                BreathAfterShow = breathAfterShow
            });

            EnsureHook();
            TryStartNext();
        }

        public void Clear()
        {
            _queue.Clear();
            _playing = false;
            _phase = 0;
            _breathAllowed = false;
            StopBreathingVisual();
            Unhook();
        }

        public void StopBreathing()
        {
            _breathAllowed = false;
            StopBreathingVisual();
            if (!_playing && _queue.Count == 0)
                Unhook();
        }

        void StopBreathingVisual()
        {
            _breathing = false;
            if (_breathCircle != null)
            {
                _breathCircle.style.scale = new StyleScale(new Scale(Vector2.one));
                _breathCircle.style.opacity = 1f;
                _breathCircle = null;
            }
        }

        void StartBreathing(VisualElement circle)
        {
            if (!_breathAllowed || circle == null)
            {
                StopBreathingVisual();
                return;
            }

            StopBreathingVisual();
            _breathCircle = circle;
            _breathing = true;
            _breathStart = EditorApplication.timeSinceStartup;
            EnsureHook();
        }

        void EnsureHook()
        {
            if (_hooked) return;
            EditorApplication.update += Tick;
            _hooked = true;
        }

        void Unhook()
        {
            if (!_hooked) return;
            EditorApplication.update -= Tick;
            _hooked = false;
        }

        void TryStartNext()
        {
            if (_playing || _queue.Count == 0)
            {
                if (!_playing && _queue.Count == 0 && !_breathing)
                    Unhook();
                return;
            }

            _current = _queue.Dequeue();
            _playing = true;
            _phase = 0;
            _phaseStart = EditorApplication.timeSinceStartup;

            if (_current.Circle != null)
                _current.Circle.style.scale = new StyleScale(new Scale(new Vector2(0.01f, 0.01f)));
            if (_current.Text != null)
                _current.Text.style.opacity = 0f;
        }

        void Tick()
        {
            if (_breathing && _breathCircle != null && _breathAllowed)
                ApplyBreath(_breathCircle);
            else if (_breathing && !_breathAllowed)
                StopBreathingVisual();

            if (!_playing)
            {
                TryStartNext();
                return;
            }

            var t = (float)(EditorApplication.timeSinceStartup - _phaseStart);

            if (_phase == 0)
            {
                var p = Mathf.Clamp01(t / ScaleDuration);
                var eased = EaseOutBack(p);
                var s = Mathf.Lerp(0.01f, 1f, eased);
                if (_current.Circle != null)
                    _current.Circle.style.scale = new StyleScale(new Scale(new Vector2(s, s)));

                if (p >= 1f)
                {
                    if (_current.Circle != null)
                        _current.Circle.style.scale = new StyleScale(new Scale(Vector2.one));
                    _phase = 1;
                    _phaseStart = EditorApplication.timeSinceStartup;
                }
            }
            else if (_phase == 1)
            {
                var p = Mathf.Clamp01(t / TextDuration);
                if (_current.Text != null)
                    _current.Text.style.opacity = p;

                if (p >= 1f)
                {
                    if (_current.Text != null)
                        _current.Text.style.opacity = 1f;
                    _current.OnShown?.Invoke();
                    if (_current.BreathAfterShow && _breathAllowed)
                        StartBreathing(_current.Circle);
                    else
                        StopBreathingVisual();
                    _phase = 2;
                    _phaseStart = EditorApplication.timeSinceStartup;
                }
            }
            else
            {
                if (t >= GapDuration)
                {
                    _playing = false;
                    TryStartNext();
                }
            }
        }

        void ApplyBreath(VisualElement circle)
        {
            var elapsed = (float)(EditorApplication.timeSinceStartup - _breathStart);
            var wave = (Mathf.Sin(elapsed * (Mathf.PI * 2f / BreathPeriod) - Mathf.PI * 0.5f) + 1f) * 0.5f;
            var scale = Mathf.Lerp(BreathMin, BreathMax, wave);
            var opacity = Mathf.Lerp(BreathOpacityMin, BreathOpacityMax, wave);
            circle.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
            circle.style.opacity = opacity;
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
