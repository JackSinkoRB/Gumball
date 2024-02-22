using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class DontDestroyOnLoadUtils
    {

        private static Transform localObject;
        
        public static void RemoveDontDestroyOnLoad(Transform transform)
        {
            if (localObject == null)
                localObject = new GameObject("TEMP-remove DontDestroyOnLoad()").transform;
            
            transform.SetParent(localObject);
        }
        
    }
}
