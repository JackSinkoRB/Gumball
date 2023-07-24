using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineHelperExtensions
{

    public static void PerformAfterFixedUpdate(this MonoBehaviour monoBehaviour, Action action)
    {
        CoroutineHelper.PerformAfterFixedUpdate(action, monoBehaviour);
    }
    
    public static void PerformAfterTrue(this MonoBehaviour monoBehaviour, Func<bool> condition, Action action)
    {
        CoroutineHelper.PerformAfterTrue(condition, action, monoBehaviour);
    }
    
    public static void PerformAfterDelay(this MonoBehaviour monoBehaviour, float delay, Action action)
    {
        CoroutineHelper.PerformAfterDelay(delay, action, monoBehaviour);
    }
    
}