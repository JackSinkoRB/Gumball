using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class BootSceneManager : MonoBehaviour
    {

        public static float BootDurationSeconds { get; private set; }
        public static SceneInstance LoadingSceneInstance;
        
        private IEnumerator Start()
        {
            BootDurationSeconds = Time.realtimeSinceStartup;
#if ENABLE_LOGS
            Debug.Log($"Boot loading complete in {TimeSpan.FromSeconds(BootDurationSeconds).ToPrettyString(true)}");
#endif
            
            var handle = Addressables.LoadSceneAsync(SceneManager.GameLoaderSceneName, LoadSceneMode.Additive, true);
            yield return handle;
            LoadingSceneInstance = handle.Result;
        }

    }
}
