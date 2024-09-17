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

        public static Coroutine PerformAfterFixedUpdate(this MonoBehaviour monoBehaviour, Action action)
        {
            return CoroutineHelper.PerformAfterFixedUpdate(action, monoBehaviour);
        }

        public static Coroutine PerformAfterTrue(this MonoBehaviour monoBehaviour, Func<bool> condition, Action action)
        {
            return CoroutineHelper.PerformAfterTrue(condition, action, monoBehaviour);
        }

        public static Coroutine PerformAfterDelay(this MonoBehaviour monoBehaviour, float delay, Action action)
        {
            return CoroutineHelper.PerformAfterDelay(delay, action, monoBehaviour);
        }

    }
}