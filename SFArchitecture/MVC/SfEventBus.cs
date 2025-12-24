using System;
using System.Collections.Generic;

namespace SFramework.SFArchitecture.MVC
{
    /// <summary>
    /// 全局事件总线，用于跨模块的解耦通信。
    /// 使用泛型委托和字典管理事件，任何对象都可以发布或订阅事件。
    /// </summary>
    public static class SfEventBus
    {
        // 存储事件类型（Type）到委托列表（List<Delegate>）的映射
        private static readonly Dictionary<Type, List<Delegate>> _eventDelegates = 
            new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// 订阅一个特定类型的事件。
        /// </summary>
        /// <typeparam name="TEvent">事件类型（例如：OnDamageDealt）</typeparam>
        /// <param name="handler">事件处理器委托（Action<TEvent>）</param>
        public static void Subscribe<TEvent>(Action<TEvent> handler)
        {
            Type eventType = typeof(TEvent);

            if (!_eventDelegates.ContainsKey(eventType))
            {
                _eventDelegates.Add(eventType, new List<Delegate>());
            }

            // 防止重复订阅
            if (!_eventDelegates[eventType].Contains(handler))
            {
                _eventDelegates[eventType].Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅一个特定类型的事件。
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件处理器委托</param>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            Type eventType = typeof(TEvent);

            if (_eventDelegates.ContainsKey(eventType))
            {
                _eventDelegates[eventType].Remove(handler);

                // 如果列表为空，则移除该事件类型以节省内存
                if (_eventDelegates[eventType].Count == 0)
                {
                    _eventDelegates.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 发布一个事件，触发所有订阅者的处理器。
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="eventData">事件实例</param>
        public static void Publish<TEvent>(TEvent eventData)
        {
            Type eventType = typeof(TEvent);

            if (_eventDelegates.ContainsKey(eventType))
            {
                // 复制列表，以防在遍历时（即在处理事件时）发生取消订阅操作导致集合修改
                List<Delegate> handlers = new List<Delegate>(_eventDelegates[eventType]);

                foreach (Delegate handler in handlers)
                {
                    // 安全地将通用委托转换为特定类型的 Action<TEvent> 并调用
                    (handler as Action<TEvent>)?.Invoke(eventData);
                }
            }
            else
            {
                // Debug.LogWarning($"EventBus: No subscribers found for event type {eventType.Name}");
            }
        }
    }
}