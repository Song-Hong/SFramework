using UnityEngine;

namespace Song.Core.Module
{
    /// <summary>
    /// 单例类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T>:MonoBehaviour where T:MonoSingleton<T>
    {
        public static T Instance;
        protected virtual void Awake()
        {
            Instance = this as T;
        }
    }
}