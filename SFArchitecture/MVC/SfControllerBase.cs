using UnityEngine;

namespace SFramework.SFArchitecture.MVC
{
    // 保持不变，专注于 M 和 V 之间的协调
    public abstract class SfControllerBase<TModelType, TViewType, TDataType> : MonoBehaviour
        where TModelType : SfModelBase<TDataType>, new()
        where TViewType : SfViewBase<TDataType>
        where TDataType : struct
    {
        [Header("Dependencies")] [SerializeField]
        protected TViewType view;

        protected TModelType model;

        protected virtual void Awake()
        {
            model = new TModelType();

            // 绑定通信：Model -> Controller -> View
            model.OnDataChanged += OnModelDataChanged;

            // 绑定通信：View -> Controller -> Model
            if (view != null)
            {
                view.OnUserAction += OnViewUserAction;
            }

            // 初始更新 View
            // 注意：Model 初始化通常在子类 Controller 的 Awake/Start 中进行
            // 此处使用 model.Data 可能会使用 TDataType 的默认值
            OnModelDataChanged(model.Data);
        }

        // --- Model 到 View 的更新流 ---
        protected virtual void OnModelDataChanged(TDataType data)
        {
            if (view != null)
            {
                view.UpdateDisplay(data);
            }
        }

        // --- View 到 Model 的输入流 ---
        protected abstract void OnViewUserAction(object inputParams);

        protected virtual void OnDestroy()
        {
            // 1. 取消内部订阅
            model.OnDataChanged -= OnModelDataChanged;
            if (view != null)
            {
                view.OnUserAction -= OnViewUserAction;
            }

            // 2. 调用 Model 的清理方法，让 Model 取消 EventBus 的订阅
            model.OnDestroy();
        }

        /// <summary>
        /// 重置控制器，将模型数据重置为默认值。
        /// </summary>
        private void Reset()
        {
#if UNITY_EDITOR
            // --- 1. 自动查找和挂载 TViewType 组件 ---
            // 确保当前组件的 view 字段为空，才进行自动查找
            if (view == null)
            {
                // 尝试在自身 GameObject 上查找 TViewType
                var viewOnSelf = GetComponent<TViewType>();
                if (viewOnSelf != null)
                {
                    view = viewOnSelf;
                    return; // 找到了就结束
                }

                // 尝试在整个场景中查找 TViewType 的第一个实例
                var foundView = FindObjectOfType<TViewType>();
                if (foundView != null)
                {
                    view = foundView;
                }
                else
                {
                    Debug.LogWarning($"Controller Reset: 场景中找不到类型为 {typeof(TViewType).Name} 的组件，请手动挂载 View。", this);
                }
            }
            
            // 修改物体名字
            name = $"{this.GetType().Name}";
#endif
        }
    }
}