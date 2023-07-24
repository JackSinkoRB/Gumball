using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CoroutineHelper : Singleton<CoroutineHelper>
{

    public static void PerformAfterFixedUpdate(Action action, MonoBehaviour monoToRunOn = null)
    {
        if (action == null)
            return;
        
        if (monoToRunOn == null)
            monoToRunOn = Instance;
        monoToRunOn.StartCoroutine(PerformAfterFixedUpdateIE(action));       
    }
    
    public static void PerformAfterTrue(Func<bool> condition, Action action, MonoBehaviour monoToRunOn = null)
    {
        if (condition == null || condition.Invoke())
        {
            //can perform instantly
            action?.Invoke();
            return;
        }

        if (monoToRunOn == null)
            monoToRunOn = Instance;
        monoToRunOn.StartCoroutine(PerformAfterTrueIE(condition, action));
    }
    
    public static void PerformAfterDelay(float delay, Action action, MonoBehaviour monoToRunOn = null)
    {
        if (delay <= 0)
        {
            //can perform instantly
            action?.Invoke();
            return;
        }
        
        if (monoToRunOn == null)
            monoToRunOn = Instance;
        monoToRunOn.StartCoroutine(PerformAfterDelayIE(delay, action));
    }
    
    private static IEnumerator PerformAfterFixedUpdateIE(Action action)
    {
        yield return new WaitForFixedUpdate();
        action?.Invoke();
    }
    
    private static IEnumerator PerformAfterTrueIE(Func<bool> condition, Action action)
    {
        yield return new WaitUntil(condition);
        action?.Invoke();
    }
    
    private static IEnumerator PerformAfterDelayIE(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }

}