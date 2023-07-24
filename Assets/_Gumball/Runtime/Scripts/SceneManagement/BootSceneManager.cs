using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class BootSceneManager : MonoBehaviour
    {

        public static float BootDurationSeconds { get; private set; }

        [Scene, SerializeField] private string loadingScene = "LoadingScene";
        
        private void Start()
        {
            BootDurationSeconds = Time.realtimeSinceStartup;
            GlobalLoggers.LoadingLogger.Log($"Boot loading complete in {TimeSpan.FromSeconds(BootDurationSeconds).ToPrettyString(true)}");
            
            Addressables.LoadSceneAsync(loadingScene, LoadSceneMode.Additive, true);
        }

    }
}
