using System.Collections;
using System.Collections.Generic;
using SFramework.Core.Module.Task;
using UnityEngine;

public class SongWaitDebug : SongTaskBase
{
    public float waitTime = 0.5f;
    
    public override void OnEnable()
    {
        StartCoroutine(WaitToNextTask());
    }

    IEnumerator WaitToNextTask()
    {
        yield return new WaitForSeconds(waitTime);
        TaskFinished();
        Debug.Log("wait 111");
    }
}
