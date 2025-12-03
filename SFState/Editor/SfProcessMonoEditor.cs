using System.Collections.Generic;
using SFramework.Core.Editor.Support;
using SFramework.SFState.Module;
using SFramework.SFState.Mono;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SFramework.SFState.Editor
{
    /// <summary>
    /// 流程单例编辑器
    /// </summary>
    [CustomEditor(typeof(SfProcessMono))]
    public class SfProcessMonoEditor:UnityEditor.Editor
    {
        /// <summary>
        /// 流程完整命名空间
        /// </summary>
        private Dictionary<string,string> processFullNameSpace = new Dictionary<string, string>();
        
        /// <summary>
        /// 流程名称
        /// </summary>
        private List<string> processNames = new List<string>();
        
        /// <summary>
        /// 绘制编辑器
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            // 获取目标
            var sfProcessMono = (SfProcessMono)this.target;
            
            // 创建根节点
            var rootVisualElement = new VisualElement();

            // 添加标题
            var title = new Label("SFramework 流程模块")
            {
                style =
                {
                    fontSize = 16,
                    color = Color.white,
                    alignSelf = Align.Center,
                }
            };
            rootVisualElement.Add(title);
            
            // 获取所有的流程
            // 创建容器
            var processSelectContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 15
                }
            };
            // 创建名称
            var processSelectName = new Label("开始流程 : ")
            {
                style =
                {
                    // marginBottom = 5
                    fontSize = 15
                }
            };
            processSelectContainer.Add(processSelectName);
            // 创建选项框
            processFullNameSpace.Clear();
            processNames.Clear();
            foreach (var runtimeSubclass in SfReflection.GetRuntimeSubclasses<SfProcessBase>())
            {
                processNames.Add(runtimeSubclass.Name);
                processFullNameSpace.Add(runtimeSubclass.Name, runtimeSubclass.FullName);
            }
            var dropdownField = new DropdownField
            {
                style =
                {
                    flexBasis = 1,
                    flexGrow = 1
                },
                choices = processNames
            };
            dropdownField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    sfProcessMono.startProcess = processFullNameSpace[evt.newValue];
                }
            });
            // 设置初始值
            var dropdownFieldValue = dropdownField.value;
            foreach (var process in processFullNameSpace)
            {
                if (process.Value == sfProcessMono.startProcess)
                {
                    dropdownFieldValue = process.Key;
                }
            }
            dropdownField.value = dropdownFieldValue;
            // sfProcessMono.startProcess
            processSelectContainer.Add(dropdownField);
            rootVisualElement.Add(processSelectContainer);
            
            // 创建当前流程容器
            var processCurrentContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 15
                }
            };
            // 创建名称
            var processCurrentName = new Label("当前流程 : ")
            {
                style =
                {
                    // marginBottom = 5
                    fontSize = 15
                }
            };
            var currentProcess = new Label()
            {
                style =
                {
                    // marginBottom = 5
                    fontSize = 15
                }
            };
            processCurrentContainer.Add(processCurrentName);
            processCurrentContainer.Add(currentProcess);
            rootVisualElement.Add(processCurrentContainer);
            rootVisualElement.schedule.Execute(() =>
            {
                // 获取当前流程的名称，如果为 null 则显示 "None" 或 "未运行"
                string processName = sfProcessMono.CurrentProcess == null
                    ? "None (未运行)"
                    // 如果不为 null，则显示其类型名
                    : sfProcessMono.CurrentProcess.GetType().Name; 

                // 更新 Label 的值
                currentProcess.text = processName;

                // 告诉编辑器需要重绘 Inspector（确保其他相关更新也生效）
                if (Selection.activeGameObject == sfProcessMono.gameObject)
                {
                    // 仅在 Inspector 处于打开状态时请求重绘
                    EditorUtility.SetDirty(sfProcessMono); 
                }
                
            }).Every(100);
            
            return rootVisualElement;
        }
    }
}