using UnityEngine;

namespace SFramework.Core.Mono
{
    /// <summary>
    /// 单例类
    /// </summary>
    /// <typeparam name="T">单例类型</typeparam>
    public class SfMonoSingleton<T> : MonoBehaviour where T : SfMonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<T>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject(typeof(T).Name);
                                _instance = go.AddComponent<T>();
                            }
                        }
                    }
                }
                return _instance;
            }
        }
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}