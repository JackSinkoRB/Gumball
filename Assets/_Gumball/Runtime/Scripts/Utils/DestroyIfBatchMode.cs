using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DestroyIfBatchMode : MonoBehaviour
    {
        
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isBatchMode)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
#endif
        
    }
}
