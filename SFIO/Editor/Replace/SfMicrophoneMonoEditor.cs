using SFramework.SFIo.Module;
using SFramework.SFIo.Mono;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// 假设这些是您自定义的命名空间

namespace SFramework.SFIo.Editor.Replace
{
    /// <summary>
    /// 麦克风模块单例编辑器
    /// </summary>
    [CustomEditor(typeof(SfMicrophoneMono))]
    public class SfMicrophoneMonoEditor : UnityEditor.Editor
    {
        // **核心组件**
        private VisualElement _rootVisualElement;
        private SfMicrophone _sfMicrophone; // 麦克风模块实例
        private SfMicrophoneMono _microphoneMonoTarget; // 对 Target 的引用

        // **可视化参数**
        private const int BarCount = 32; // 可视化条数量
        private const float BaseHeight = 2f; // 基础高度
        private const float MaxHeight = 100f; // 最大高度
        private const float Sensitivity = 800f; // 灵敏度

        // **(删除这一行)**
        // private const float LerpSpeed = 0.3f; // 平滑速度

        // **(添加这三行)**
        private const float AttackSpeed = 0.8f; // 攻击速度 (值越大, 上升越快)
        private const float DecaySpeed = 0.2f;  // 衰减速度 (值越小, 下降越慢)
        private const float NoiseThreshold = 0.014f; // 噪音阈值 (屏蔽 1% 以下的微小信号)

        // **音频数据参数**
        private const int SampleRate = 44100;
        private const int ClipLength = 60; // 60秒循环长度
        private const int WindowSize = 1024; // 用于处理的样本数据窗口大小
        private float[] _sampleData; // 原始样本数据窗口
        private int _lastPosition = 0; // 上次读取的录音头位置

        // **对数映射所需数据**
        private VisualElement[] _visualizerBars;
        private int[] _logIndices; // 存储 32 条线的起始/结束索引

        /// <summary>
        /// 可视化面板
        /// </summary>
        private VisualElement _microphoneVisualBar;
        
        /// <summary>
        /// 创建Inspector GUI
        /// </summary>
        /// <returns>Inspector GUI</returns>
        public override VisualElement CreateInspectorGUI()
        {
            // 初始化根元素和目标引用
            _rootVisualElement = new VisualElement();
            _microphoneMonoTarget = (SfMicrophoneMono)target; // 获取正在编辑的目标组件

            // 初始化数据和麦克风实例
            InitializeData();
            
            // 添加标题
            var title = new Label("SFramework 麦克风模块")
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            _rootVisualElement.Add(title);

            // 当非运行状态时显示 麦克风测试工具
            if (!Application.isPlaying)
            {
                CreateMicrophoneVisual();
            }
            else
            {
                RemoveMicrophoneVisual();
            }
            

            // 启动 Editor 更新循环
            StartEditorUpdates();

            return _rootVisualElement;
        }

        /// <summary>
        /// 初始化所有必要的数据结构和麦克风实例
        /// </summary>
        private void InitializeData()
        {
            // 注意：这里创建了一个新的实例，仅用于此 Editor 窗口。
            // 这使其成为一个独立的测试工具，而不是控制场景中的 SfMicrophoneMono 实例。
            _sfMicrophone = new SfMicrophone();
            
            // 初始化数组
            _sampleData = new float[WindowSize];
            _visualizerBars = new VisualElement[BarCount];
            _logIndices = new int[BarCount];

            // 初始化对数索引
            InitializeLogIndices();
        }

        /// <summary>
        /// 显示可视化面板
        /// </summary>
        private void CreateMicrophoneVisual()
        {
            _microphoneVisualBar = new VisualElement();

            // 2. 创建麦克风选择下拉框
            var microphoneDropdown = CreateMicrophoneDropdown();
            _microphoneVisualBar.Add(microphoneDropdown);
            
            // 3. 创建音频可视化UI
            _microphoneVisualBar.Add(CreateAudioVisualizer());
            
            _rootVisualElement.Add(_microphoneVisualBar);
        }

        /// <summary>
        /// 删除可视化面板
        /// </summary>
        private void RemoveMicrophoneVisual()
        {
            if (_microphoneVisualBar != null)
            {
                _rootVisualElement.Remove(_microphoneVisualBar);
            }
        }
        
        /// <summary>
        /// 创建麦克风选择下拉框
        /// </summary>
        private VisualElement CreateMicrophoneDropdown()
        {
            //设备背景
            var devicesBg = new VisualElement()
            {
                style =
                {
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    marginTop = 5,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                }
            };
            

            var dropdownField = new DropdownField()
            {
                style =
                {
                    marginTop = 5,
                    alignSelf = Align.Center,
                    // backgroundColor = new Color(0.21f,0.21f,0.21f,0.5f)   
                    // backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                }
            };
            dropdownField.choices.Add("关闭");
            
            // 安全地添加设备列表
            if (_sfMicrophone != null && _sfMicrophone.Devices != null)
            {
                foreach (var sfMicrophoneDevice in _sfMicrophone.Devices)
                {
                    dropdownField.choices.Add(sfMicrophoneDevice);
                }
            }
            
            dropdownField.value = "关闭";
            dropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (evt.newValue == "关闭")
                {
                    _sfMicrophone?.Stop();
                    return;
                }
                
                // 启动麦克风 (循环录制)
                _sfMicrophone?.Start(evt.newValue, true, ClipLength, SampleRate);
            });
            
            foreach (var element in dropdownField.Children())
            {
                element.style.backgroundColor = Color.clear;
                element.style.borderLeftWidth = 0;
                element.style.borderRightWidth = 0;
                element.style.borderTopWidth = 0;
                element.style.borderBottomWidth = 0;
                element.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
            devicesBg.Add(dropdownField);
            
            return devicesBg;
        }

        /// <summary>
        /// 创建音频可视化UI元素
        /// </summary>
        private VisualElement CreateAudioVisualizer()
        {
            // 声音可视化背景
            var audioViewBackground = new VisualElement()
            {
                style =
                {
                    height = MaxHeight,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceAround,
                    paddingBottom = 0,
                    paddingLeft = 5,
                    paddingRight = 5
                }
            };

            // 创建 32 条可视化线并存储引用
            for (int i = 0; i < BarCount; i++)
            {
                var visualElement = new VisualElement
                {
                    style =
                    {
                        height = BaseHeight,
                        width = 4,
                        backgroundColor = Color.white,
                        alignSelf = Align.FlexEnd,
                        marginBottom = 5
                    }
                };
                audioViewBackground.Add(visualElement);
                _visualizerBars[i] = visualElement; 
            }
            
            return audioViewBackground;
        }

        /// <summary>
        /// 初始化对数索引，用于将 1024 个样本映射到 32 个柱状条
        /// </summary>
        private void InitializeLogIndices()
        {
            float logWindowSize = Mathf.Log(WindowSize, 2); // 结果是 10

            for (int i = 0; i < BarCount; i++)
            {
                // 1. 将 0 到 BarCount-1 映射到 0 到 1
                float power = (float)i / (BarCount - 1); 

                // 2. 计算指数步进值
                // **关键修改 1: 移除了 "- 1"**
                // 这使得 i=0 时, indexValue = 1 (而不是0), 保证 Bar 0 至少有 1 个样本 (index 0)
                float indexValue = Mathf.Pow(2, power * logWindowSize); 
                
                // 3. 将结果限制在 [1, WindowSize] 的安全索引范围内
                // **关键修改 2: 最小值钳制为 1**
                _logIndices[i] = Mathf.Clamp(Mathf.FloorToInt(indexValue), 1, WindowSize);
            }
            
            // **关键修改 3: 添加后处理循环，确保“单调递增”**
            // 这一步修复了所有重复的索引 (例如 [1, 1, 1, 2] -> [1, 2, 3, 4])
            // 确保 _logIndices[i] > _logIndices[i-1]
            for (int i = 1; i < BarCount; i++)
            {
                if (_logIndices[i] <= _logIndices[i - 1])
                {
                    _logIndices[i] = _logIndices[i - 1] + 1;
                }
            }

            // 4. 确保最后一个索引是 WindowSize，以便最后一个 bar 覆盖所有剩余样本
            // (这一步在您的原始代码中是正确的，但要确保它在后处理循环之后)
            // 我们的后处理循环可能会使 _logIndices[BarCount - 1] 远小于 WindowSize
            // 但同时也要确保它不会小于倒数第二个值
            if (_logIndices[BarCount - 1] < WindowSize)
            {
                _logIndices[BarCount - 1] = WindowSize;
            }
        }

        /// <summary>
        /// Editor 实时更新
        /// </summary>
        public void Update()
        {
            // (前面的检查逻辑保持不变)
            if (_rootVisualElement == null || _sfMicrophone == null || !_sfMicrophone.IsRecording() || _sfMicrophone.RecordingClip == null) 
                return;
            
            var microphoneClip = _sfMicrophone.RecordingClip;
            int currentPosition = Microphone.GetPosition(_sfMicrophone.SelectedDevice);

            bool dataProcessed = false; 

            if (currentPosition > _lastPosition && currentPosition - _lastPosition >= WindowSize / 2)
            {
                // ... (获取 startPosition 和 GetData 的逻辑保持不变)
                int startPosition = currentPosition - WindowSize;
                if (startPosition < 0) 
                    startPosition = microphoneClip.samples - WindowSize; 
                microphoneClip.GetData(_sampleData, startPosition);


                // 2. 对数分段计算峰值
                for (int i = 0; i < BarCount; i++)
                {
                    float peakValue = 0f;
                    
                    int startIndex = (i == 0) ? 0 : _logIndices[i - 1];
                    int endIndex = _logIndices[i]; 
                    
                    if (endIndex <= startIndex || startIndex >= WindowSize) continue;
                    endIndex = Mathf.Min(endIndex, WindowSize);

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        peakValue = Mathf.Max(peakValue, Mathf.Abs(_sampleData[j]));
                    }
                    
                    // **[新增] 2.5 应用噪音阈值 (Noise Gate)**
                    // 如果计算出的峰值低于我们设定的阈值，就当它是 0
                    if (peakValue < NoiseThreshold)
                    {
                        peakValue = 0f;
                    }

                    // 3. 将峰值映射到可视化高度
                    // (注意: 如果 peakValue 为 0, targetHeight 将等于 BaseHeight)
                    float targetHeight = BaseHeight + peakValue * Sensitivity; 
                    targetHeight = Mathf.Clamp(targetHeight, BaseHeight, MaxHeight);
                    
                    // **[修改] 4. 应用平滑过渡 (区分攻击和衰减)**
                    var bar = _visualizerBars[i];
                    if (bar == null) continue; 
                    
                    float currentHeight = bar.style.height.value.value;
                    
                    // 决定使用攻击速度还是衰减速度
                    // - 如果目标高于当前：使用 AttackSpeed 快速上升
                    // - 如果目标低于当前：使用 DecaySpeed 缓慢下降
                    float lerpSpeed = (targetHeight > currentHeight) ? AttackSpeed : DecaySpeed;
                    
                    float newHeight = Mathf.Lerp(currentHeight, targetHeight, lerpSpeed); 

                    bar.style.height = newHeight;
                }

                // 5. 更新上次读取位置
                _lastPosition = currentPosition;
                dataProcessed = true; 
            }
            else if (currentPosition < _lastPosition)
            {
                _lastPosition = 0;
            }
            
            if (dataProcessed)
            {
                _rootVisualElement.MarkDirtyRepaint();
            }
        }
        
        /// <summary>
        /// 禁用时调用，停止录音并取消订阅
        /// </summary>
        private void OnDisable()
        {
            // 停止 Editor 更新
            StopEditorUpdates();
            
            // 停止麦克风（使用 ?. 避免 _sfMicrophone 为 null 时出错）
            _sfMicrophone?.Stop();
        }

        /// <summary>
        /// 订阅 EditorApplication.update
        /// </summary>
        private void StartEditorUpdates()
        {
            EditorApplication.update -= Update; 
            EditorApplication.update += Update;
        }

        /// <summary>
        /// 取消订阅 EditorApplication.update
        /// </summary>
        private void StopEditorUpdates()
        {
            EditorApplication.update -= Update; 
        }
    }
}