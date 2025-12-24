using System;
using UnityEngine;

namespace SFramework.SFArchitecture.MVC
{
    /// <summary>
    /// 视图基类，用于显示和接收用户输入
    /// </summary>
    /// <typeparam name="TData">视图显示的数据类型</typeparam>
    public abstract class SfViewBase<TData> : MonoBehaviour
    {
        // 事件：当用户输入发生时触发，由 Controller 订阅
        public event Action<object> OnUserAction; 

        // 抽象方法：用于更新 View 的显示
        public abstract void UpdateDisplay(TData data);

        // 受保护的方法：用于子类在捕获到输入时通知 Controller
        protected void SendAction(object inputParams = null)
        {
            OnUserAction?.Invoke(inputParams);
        }

        private void Reset()
        {
#if UNITY_EDITOR
            var controllerTypeName = "Song.Controller." + this.GetType().Name.Replace("View", "Controller");
            var controllerType = Type.GetType(controllerTypeName);
            if (controllerType != null)
            {
                var controllerComponent = GetComponent(controllerType);
                if (controllerComponent == null)
                {
                    controllerComponent = gameObject.AddComponent(controllerType);
                }
            }
#endif
        }
    }
}