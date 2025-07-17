using System;
using System.Collections.Generic;
using Song.Core.Module.State;
using UnityEngine;

namespace Song.Core.Support
{
    /// <summary>
    /// 状态管理器
    /// </summary>
    public class StateSupport:SongStateMonoManager
    {
        [Header("全部状态")]
        public List<SongStateMonoBase> states = new List<SongStateMonoBase>();

        [HideInInspector,Header("开始状态")]
        public string startState;


        public override void Start()
        {
            foreach (var songStateMonoBase in states)
            {
                AddState(songStateMonoBase);
            }
            CurrentState = States[startState];
            base.Start();
        }
    }
}