using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class DetectMainCameraChange : MonoBehaviour
    {

        public event Action<Camera> onMainCameraChange;

        private Camera current;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            current = Camera.main;
            onMainCameraChange?.Invoke(current);
        }

        private void Update()
        {
            if (Camera.main != current)
            {
                Debug.Log("Main camera changed");
                current = Camera.main;
                onMainCameraChange?.Invoke(current);
            }
        }
        
    }
}
