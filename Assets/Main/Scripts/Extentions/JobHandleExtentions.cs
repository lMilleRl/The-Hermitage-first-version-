using System;
using System.Collections;
using Unity.Jobs;
using UnityEngine;

public static class JobHandleExtentions
{
    public static IEnumerator AsCoroutine(this JobHandle handle, Action onComplete = null)
    {
        WaitUntil wait = new WaitUntil(() => handle.IsCompleted);
        yield return wait;
        handle.Complete();        

        onComplete?.Invoke();
    }

    public static void NotifyCompletionJob(this JobHandle jobHandle, MonoBehaviour runner, Action onComplete)
    {
        runner.StartCoroutine(jobHandle.AsCoroutine(onComplete));
    }
}
