using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MagneticScrollUtils
{
    public class CoroutineHelper : MonoBehaviour
    {

        private static CoroutineHelper instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (!Application.isPlaying)
                    throw new NullReferenceException("CoroutineHelper shouldn't be accessed outside of play mode.");
                
                if (instance == null)
                {
                    //create a new instance
                    GameObject newGameObject = new GameObject("MagneticScroll-CoroutineHelper");
                    instance = newGameObject.AddComponent<CoroutineHelper>();
                }

                return instance;
            }
        }
        
        public static Coroutine PerformAtEndOfFrame(Action action, MonoBehaviour monoToRunOn = null)
        {
            if (action == null)
                return null;

            if (monoToRunOn == null)
                monoToRunOn = Instance;
            return monoToRunOn.StartCoroutine(PerformAtEndOfFrameIE(action));
        }

        private static IEnumerator PerformAtEndOfFrameIE(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

    }
}