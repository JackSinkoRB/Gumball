using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gumball
{
    public class UnitTestCamera : MonoBehaviour
    {
        
#if UNITY_EDITOR
        public static bool IsRunningTests;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            IsRunningTests = false;
        }
        
        private void Awake()
        {
            if (IsRunningTests)
            {
                const int unitTestRendererIndex = 2;
                GetComponent<UniversalAdditionalCameraData>().SetRenderer(unitTestRendererIndex);
                Debug.Log("Renderer switched to unit test renderer");
            }
        }
#endif
        
    }
}
