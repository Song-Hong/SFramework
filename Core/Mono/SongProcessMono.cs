using System;
using Song.Core.Module;
using Song.Core.Process;
using UnityEngine;

namespace Song.Scripts.Core.Mono
{
    /// <summary>
    /// 流程管理类
    /// </summary>
    public class SongProcessMono:MonoSingleton<SongProcessMono>
    {
        /// <summary>
        /// 当前流程
        /// </summary>
        [Header("当前流程")]
        public SongProcessBase CurrentProcess;
        
        /// <summary>
        /// 开始流程
        /// </summary>
        [Header("开始流程")]
        public string StartProcess;

        private void Start()
        {
            if(StartProcess!=null)
                CurrentProcess = Activator.CreateInstance(Type.GetType(StartProcess)) as SongProcessBase;
            CurrentProcess?.Enter();
        }
        
        /// <summary>
        /// 切换流程
        /// </summary>
        /// <typeparam name="T">流程</typeparam>
        public void ChangeProcess<T>() where T:SongProcessBase
        {
            CurrentProcess?.Exit();
            CurrentProcess = Activator.CreateInstance<T>();
            CurrentProcess?.Enter();
        }

        private void Update()
        {
            CurrentProcess?.Update();
        }
    }
}