using System.Linq;
using SFramework.Core.Extends.UIElement;
using SFramework.Core.Support;
using SFramework.SFIo.Module;
using SFramework.SFIo.Mono;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SFramework.SFIo.Editor.Replace
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
        /// <summary>
        /// 相机模块单例
        /// </summary>
        private SfCameraMono _sfCameraMono;
        
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
                    // height = 240,
                    width = new StyleLength(StyleKeyword.Initial)
                }
            };
            
            // 添加标题
            var title = new Label("SFramework 相机模块")
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            _rootElement.Add(title);
            
            // #region 测试UI
            // // 保留 base.OnInspectorGUI() 以显示默认属性
            // var container = new IMGUIContainer(() =>
            // {
            //     base.OnInspectorGUI();
            // });
            // _rootElement.Add(container);
            // #endregion
            
            // 添加相机模块单例
            _sfCameraMono = target as SfCameraMono;
            
            // 导出设置选项
            var exportTab = new SfTab();
            exportTab.SetTitle("导出设置:");
            exportTab.AddChoice("RawImage","RenderTexture");
            exportTab.ChooseBackground.style.marginLeft = 56;
            _rootElement.Add(exportTab);
            exportTab.OnChoiceChanged += choice => //添加变化事件
            {
                if (choice == "RawImage")
                {
                    ShowRawImageExport();
                    RemoveTextureExport();
                    if (_sfCameraMono != null) _sfCameraMono.exportType = SfCameraMono.ExportType.RawImage;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (choice == "RenderTexture")
                {
                    RemoveRawImageExport();
                    ShowTextureExport();
                    if (_sfCameraMono != null) _sfCameraMono.exportType = SfCameraMono.ExportType.RenderTexture;
                    serializedObject.ApplyModifiedProperties();
                }
            };
            switch (_sfCameraMono.exportType)
            {
                case SfCameraMono.ExportType.RawImage:
                    exportTab.Select("RawImage");
                    ShowRawImageExport();
                    RemoveTextureExport();
                    break;
                case SfCameraMono.ExportType.RenderTexture:
                    exportTab.Select("RenderTexture");
                    RemoveRawImageExport();
                    ShowTextureExport();
                    break;
            }
            
            
            // 仅在编辑模式下显示相机视图
            if(!Application.isPlaying)
                CameraView();
            
            return _rootElement;
        }

        #region RawImage显示
        /// <summary>
        /// 镜像设置选项
        /// </summary>
        private SfTab mirrorTab;
        /// <summary>
        /// RawImage导出选项
        /// </summary>
        private ObjectField rawImageField;
        
        /// <summary>
        /// 显示RawImage导出选项
        /// </summary>
        public void ShowRawImageExport()
        {
            if(mirrorTab!=null)
                RemoveRawImageExport();
            
            // 添加RawImage导出选项
            rawImageField = new ObjectField("RawImage:")
            {
                style =
                {
                    fontSize = 12,
                    color = Color.white,
                    height = 20,
                    marginTop = 10
                },
                name = "rawImageField",
                objectType = typeof(RawImage)
            };
            rawImageField.RegisterValueChangedCallback(e =>
            {
                if (_sfCameraMono != null) _sfCameraMono.image = e.newValue as RawImage;
                serializedObject.ApplyModifiedProperties();
            });
            rawImageField.value = _sfCameraMono.image;
            _rootElement.Add(rawImageField);
            
            // 添加镜像设置选项
            mirrorTab = new SfTab
            {
                name = "mirrorTab"
            };
            mirrorTab.SetTitle("镜像设置:");
            mirrorTab.AddChoice("镜像","不镜像");
            mirrorTab.ChooseBackground.style.marginLeft = 56;
            _rootElement.Add(mirrorTab);
            mirrorTab.OnChoiceChanged += choice => //添加变化事件
            {
                if (choice == "镜像")
                {
                    if (_sfCameraMono != null) _sfCameraMono.isMirrored = true;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (choice == "不镜像")
                {
                    if (_sfCameraMono != null) _sfCameraMono.isMirrored = false;
                    serializedObject.ApplyModifiedProperties();
                }
                UpdateMirrorSetting();
            };
            if (_sfCameraMono.isMirrored)
            {
                mirrorTab.Select("镜像");
            }
            else
            {
                mirrorTab.Select("不镜像");
            }
            if(cameraDisplayElement!=null)
                cameraDisplayElement.BringToFront();
        }
        
        /// <summary>
        /// 移除RawImage导出选项
        /// </summary>
        public void RemoveRawImageExport()
        {
            if (mirrorTab != null)
            {
                _rootElement.Remove(mirrorTab);
                mirrorTab = null;   
            }
            if (rawImageField != null)
            {
                _rootElement.Remove(rawImageField);
                rawImageField = null;
            }
        }
        #endregion

        #region RenderTexture显示
        /// <summary>
        /// 镜像设置选项
        /// </summary>
        private SfTab autoCreateTab;
        /// <summary>
        /// Texture 导出选项
        /// </summary>
        private ObjectField textureField;
        /// <summary>
        /// 导出纹理大小
        /// </summary>
        private Vector2IntField textureSizeField;
        /// <summary>
        /// 显示Texture导出选项
        /// </summary>
        public void ShowTextureExport()
        {
            if (autoCreateTab != null)
            {
                RemoveTextureExport();
            }
            // 添加Texture导出选项
            textureField = new ObjectField("RenderTexture:")
            {
                style =
                {
                    fontSize = 12,
                    color = Color.white,
                    height = 20,
                    marginTop = 10
                },
                name = "textureField",
                objectType = typeof(RenderTexture)
            };
            textureField.RegisterValueChangedCallback(e =>
            {
                if (_sfCameraMono != null) _sfCameraMono.texture = e.newValue as RenderTexture;
                serializedObject.ApplyModifiedProperties();
            });
            textureField.value = _sfCameraMono.texture;
            _rootElement.Add(textureField);
            
            // 添加自动创建选项
            autoCreateTab = new SfTab
            {
                name = "autoCreateTab"
            };
            autoCreateTab.SetTitle("自动创建:");
            autoCreateTab.AddChoice("自动创建","手动创建");
            autoCreateTab.ChooseBackground.style.marginLeft = 56;
            _rootElement.Add(autoCreateTab);
            autoCreateTab.OnChoiceChanged += choice => //添加变化事件
            {
                if (choice == "自动创建")
                {
                    CreateTextureSize();
                    if(cameraDisplayElement!=null)
                        cameraDisplayElement.BringToFront();
                    if (_sfCameraMono != null) _sfCameraMono.autoCreate = true;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (choice == "手动创建")
                {
                    RemoveTextureSize();
                    if (_sfCameraMono != null) _sfCameraMono.autoCreate = false;
                    serializedObject.ApplyModifiedProperties();
                }
            };
            if (_sfCameraMono.autoCreate)
            {
                autoCreateTab.Select("自动创建");
                CreateTextureSize();
            }
            else
            {
                autoCreateTab.Select("手动创建");
                RemoveTextureSize();
            }
            
            if(cameraDisplayElement!=null)
                cameraDisplayElement.BringToFront();
        }
        
        /// <summary>
        /// 创建导出纹理大小选项
        /// </summary>
        public void CreateTextureSize()
        {
            if (textureField != null)
                RemoveTextureSize();
            
            textureSizeField = new Vector2IntField("导出纹理大小:")
            {
                style =
                {
                    fontSize = 12,
                    color = Color.white,
                    height = 20,
                    marginTop = 10
                },
                name = "textureSizeField",
            };
            textureSizeField.RegisterValueChangedCallback(e =>
            {
                if (_sfCameraMono != null) _sfCameraMono.textureSize = e.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            textureSizeField.value = _sfCameraMono.textureSize;
            _rootElement.Add(textureSizeField);
        }
        
        /// <summary>
        /// 移除导出纹理大小选项
        /// </summary>
        public void RemoveTextureSize()
        {
            if (textureSizeField != null)
            {
                _rootElement.Remove(textureSizeField);
                textureSizeField = null;
            }
        }
        
        /// <summary>
        /// 移除Texture导出选项
        /// </summary>
        public void RemoveTextureExport()
        {
            if (autoCreateTab != null)
            {
                _rootElement.Remove(autoCreateTab);
                autoCreateTab = null;
            }
            if (textureField != null)
            {
                _rootElement.Remove(textureField);
                textureField = null;
            }

            if (textureSizeField != null)
            {
                _rootElement.Remove(textureSizeField);
                textureSizeField = null;
            }
        }
        #endregion
        
        #region 相机视图
        /// <summary>
        /// 相机视图显示元素
        /// </summary>
        private VisualElement cameraDisplayElement;
        /// <summary>
        /// 相机视图镜像选项
        /// </summary>
        private DropdownField cameraMirrorDropdownField;
        /// <summary>
        /// 相机视图
        /// </summary>
        private void CameraView()
        {
            // 1. 创建一个 VisualElement 用于显示
            var height = 160;
            cameraDisplayElement = new VisualElement()
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
 
            if (_sfCameraMono != null && _sfCameraMono.isMirrored)
            {
                cameraDisplayElement.transform.scale = new Vector3(-1, 1, 1);
            }

            //创建显示的纹理
            cameraDisplayElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture));
            _rootElement.Add(cameraDisplayElement);
            
            // 创建相机组件控制
            cameraMirrorDropdownField = new DropdownField()
            {
                style =
                {
                    marginTop = 5,
                    alignSelf = Align.Center,
                    backgroundColor = new Color(0.21f,0.21f,0.21f,0.5f)
                }
            };
            
            foreach (var element in cameraMirrorDropdownField.Children())
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
            cameraMirrorDropdownField.choices = cameraNames;
            cameraMirrorDropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
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
            cameraMirrorDropdownField.value = "关闭";
            cameraDisplayElement.Add(cameraMirrorDropdownField);

            UpdateMirrorSetting();
        }

        /// <summary>
        /// 更新镜像设置
        /// </summary>
        private void UpdateMirrorSetting()
        {
            if(_sfCameraMono==null || cameraMirrorDropdownField==null||cameraDisplayElement==null) return;
            
            if (_sfCameraMono.isMirrored)
            {
                cameraDisplayElement.transform.scale = new Vector3(-1, 1, 1);
                cameraMirrorDropdownField.transform.scale = new Vector3(-1, 1, 1);
            }
            else
            {
                cameraDisplayElement.transform.scale = new Vector3(1, 1, 1);
                cameraMirrorDropdownField.transform.scale = new Vector3(1, 1, 1);
            }
        }
        #endregion
        
        /// <summary>
        /// 更新相机显示
        /// </summary>
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