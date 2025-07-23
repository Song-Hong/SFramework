using System;
using System.Collections;
using SFramework.Core.Module.Task;
using UnityEngine;

namespace SFramework.Game.Tasks
{
    public class SongDebug:SongTaskBase
    {
        public override void OnEnable()
        {
            StartCoroutine(OnEnable1());
        }
        
        IEnumerator OnEnable1()
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("111");
            TaskFinished();
        }
    }
}