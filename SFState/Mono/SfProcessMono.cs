using System;
using SFramework.Core.Mono;
using SFramework.SFState.Module;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFramework.SFState.Mono
{
    /// <summary>
    /// 流程单例
    /// </summary>
    public class SfProcessMono : SfMonoSingleton<SfProcessMono>
    {
        /// <summary>
        /// 当前流程
        /// </summary>
        [Header("当前流程")] public SfProcessBase CurrentProcess;

        /// <summary>
        /// 开始流程
        /// </summary>
        [Header("开始流程")] public string startProcess;

        private void Start()
        {
            if (startProcess != null)
                CurrentProcess = Activator.CreateInstance(Type.GetType(startProcess)!) as SfProcessBase;
            CurrentProcess?.Enter();
        }

        /// <summary>
        /// 切换流程
        /// </summary>
        /// <typeparam name="T">流程</typeparam>
        public void ChangeProcess<T>() where T : SfProcessBase
        {
            CurrentProcess?.Exit();
            CurrentProcess = Activator.CreateInstance<T>();
            CurrentProcess?.Enter();
        }

        /// <summary>
        /// 更新流程
        /// </summary>
        private void Update()
        {
            CurrentProcess?.Update();
        }
    }
}