using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DetectMainCameraChange : MonoBehaviour
    {

        public event Action<Camera> onMainCameraChange;

        private Camera current;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Camera.main != null && Camera.main != current)
            {
                Debug.Log("Main camera changed");
                current = Camera.main;
                onMainCameraChange?.Invoke(current);
            }
        }
        
    }
}
