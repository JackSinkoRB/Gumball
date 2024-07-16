using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class CoroutineHelperExtensions
    {

        public static Coroutine PerformAtEndOfFrame(this MonoBehaviour monoBehaviour, Action action)
        {
            if (!monoBehaviour.isActiveAndEnabled)
                return null;
            
            if (!Application.isPlaying)
            {
                action?.Invoke();
                return null;
            }

            return CoroutineHelper.PerformNextFrame(action, monoBehaviour);
        }

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
}