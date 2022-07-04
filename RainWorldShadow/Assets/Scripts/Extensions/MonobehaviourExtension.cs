using System;
using UnityEngine;
using System.Collections;

public static class MonoBehaviourExtensions
{
    public static void Invoke(this MonoBehaviour me, Action action, float time)
    {
        me.StartCoroutine(executeAfterTime(action, time));
    }

    private static IEnumerator executeAfterTime(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}