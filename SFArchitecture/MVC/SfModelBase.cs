using System;
using System.Collections.Generic;

namespace SFramework.SFArchitecture.MVC
{
    // TData 可以是 struct 或 class（如果用 class，需要确保 SetData 能够正确检测到变化）
    public abstract class SfModelBase<TData>
    {
        // 内部事件：用于通知 Controller 自己的数据变化
        public event Action<TData> OnDataChanged; 

        protected TData _data;

        public TData Data => _data;

        // 供 Controller 调用的初始化方法
        public abstract void Initialize(TData initialData);

        // 受保护的方法，用于在子类中更新数据并触发事件
        protected virtual void SetData(TData newData)
        {
            // 注意：如果 TData 是 struct，需要进行值比较
            // 如果 TData 是 class，比较引用（即 _data != newData），或者在 SetData 内部实现深层比较逻辑。
            if (!EqualityComparer<TData>.Default.Equals(_data, newData))
            {
                _data = newData;
                // 触发内部事件通知 Controller
                OnDataChanged?.Invoke(_data); 
            }
        }

        /// <summary>
        /// ⚠️ 抽象方法：强制子类实现清理逻辑，特别是取消 EventBus 订阅。
        /// </summary>
        public abstract void OnDestroy();
    }
}