using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SFramework.Core.Support;
using SFramework.SFHardware.Module;
using SFramework.SFHardware.Mono;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFHardware.Editor
{
    /// <summary>
    /// 相机模块单例样式
    /// </summary>
    [CustomEditor(typeof(SfCameraMono))]
    public class SfCameraMonoEditor:UnityEditor.Editor
    {
        /// <summary>
        /// 摄像头纹理
        /// </summary>
        private WebCamTexture _webCamTexture;
        /// <summary>
        /// 渲染纹理
        /// </summary>
        private RenderTexture _renderTexture; 
        /// <summary>
        /// 相机模块
        /// </summary>
        private SfCamera _camera;
        /// <summary>
        /// 根元素
        /// </summary>
        private VisualElement _rootElement;
        
        private void OnEnable()
        {
            _renderTexture = new RenderTexture(1920 / 2, 1080 / 2, 0);
        }

        /// <summary>
        /// 启动相机
        /// </summary>
        /// <param name="cameraName">相机名称</param>
        private void StartCamera(string cameraName)
        {
            _camera = new SfCamera();
            _camera.Start(cameraName);
            _webCamTexture = _camera.ActiveTexture;
        
            // 修正：如果尺寸与预设不同，可以重新创建 _renderTexture，但如果保持固定尺寸，则不需要再次创建
            // 如果需要适应摄像头分辨率，请在这里重新创建：
            // _renderTexture = new RenderTexture(_webCamTexture.width, _webCamTexture.height, 0);
        
            EditorApplication.update += UpdateCameraFeed;
        }
        
        /// <summary>
        /// 创建自定义的Inspector GUI
        /// </summary>
        /// <returns>自定义的Inspector GUI</returns>
        public override VisualElement CreateInspectorGUI()
        {
            // 创建根元素
            _rootElement = new VisualElement()
            {
                style =
                {
                    height = 240,
                    width = new StyleLength(StyleKeyword.Initial)
                }
            };
            
            // 添加标题
            var title = new Label("SFrameworkCamera 相机模块")
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            _rootElement.Add(title);
            
            
            
            #region 测试UI
            // 保留 base.OnInspectorGUI() 以显示默认属性
            var container = new IMGUIContainer(() =>
            {
                base.OnInspectorGUI();
            });
            _rootElement.Add(container);
            #endregion
            
            var visualElement = base.CreateInspectorGUI();
            _rootElement.Add(visualElement);

            // 1. 创建一个 VisualElement 用于显示
            var height = 160;
            var cameraDisplayElement = new VisualElement()
            {
                name = "cameraDisplay",
                style =
                {
                    height = height,
                    borderTopLeftRadius = 12,
                    borderBottomLeftRadius = 12,
                    borderBottomRightRadius = 12,
                    borderTopRightRadius = 12,
                    backgroundColor = SfColor.HexToColor("#606060"),
                    marginTop = 20,
                }
            };
            var sfCameraMono = target as SfCameraMono;
            if (sfCameraMono != null && sfCameraMono.isMirrored)
            {
                cameraDisplayElement.transform.scale = new Vector3(-1, 1, 1);
            }

            //创建显示的纹理
            cameraDisplayElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture));
            _rootElement.Add(cameraDisplayElement);
            
            // 创建相机组件控制
            var dropdownField = new DropdownField()
            {
                style =
                {
                    position = Position.Absolute,
                    marginTop = 100,
                    alignSelf = Align.Center,
                    backgroundColor = new Color(0.21f,0.21f,0.21f,0.3f)
                }
            };
            
            foreach (var element in dropdownField.Children())
            {
                element.style.backgroundColor = Color.clear;
                element.style.borderLeftWidth = 0;
                element.style.borderRightWidth = 0;
                element.style.borderTopWidth = 0;
                element.style.borderBottomWidth = 0;
                element.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
            
            var cameraNames = WebCamTexture.devices.Select(webCamDevice => webCamDevice.name).ToList();
            cameraNames.Add("关闭");
            dropdownField.choices = cameraNames;
            dropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (evt.newValue == "关闭")
                {
                    cameraDisplayElement.style.backgroundImage = new StyleBackground();
                    OnDisable();
                    return;
                }
                OnDisable();
                cameraDisplayElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture));
                StartCamera(evt.newValue);
            });
            dropdownField.value = "关闭";
            _rootElement.Add(dropdownField);

            return _rootElement;
        }

        private void UpdateCameraFeed()
        {
            // 检查 _webCamTexture 是否已初始化
            if (_webCamTexture == null || _renderTexture == null)
            {
                // 确保不会在对象被销毁后执行
                EditorApplication.update -= UpdateCameraFeed;
                return;
            }

            // 这些操作必须在主线程执行，EditorApplication.update 确保了这一点
            if (_webCamTexture.didUpdateThisFrame)
            {
                Graphics.Blit(_webCamTexture, _renderTexture);
                Repaint();
            }
        }
        
        /// <summary>
        /// 禁用时停止线程和摄像头
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.update -= UpdateCameraFeed;
            if (_camera == null) return;
            _camera.Stop();
            _camera = null;
        }
    }
}