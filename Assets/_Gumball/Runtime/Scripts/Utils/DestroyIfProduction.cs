using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DestroyIfProduction : MonoBehaviour
    {
        
#if UNITY_EDITOR
        private static bool isEditor = true;
#else
        private static bool isEditor = false;
#endif

        private static bool isProduction => !isEditor && VersionManager.Instance.BuildTypeFormatted.Equals("PRODUCTION");
        
        private void OnEnable()
        {
            if (isProduction)
                Destroy(gameObject);
        }
        
    }
}
