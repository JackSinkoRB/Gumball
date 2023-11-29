using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Gumball
{
    /// <summary>
    /// A loading scene to handle anything that needs to be loaded before entering the game.
    /// </summary>
    public class GameLoaderSceneManager : MonoBehaviour
    {

        private enum Stage
        {
            LOADING_SAVE_DATA,
            LOADING_MAINSCENE,
            LOADING_VEHICLE,
        }

        [SerializeField] private TextMeshProUGUI debugLabel;

        private Stage currentStage;
        private AsyncOperationHandle<SceneInstance> mainSceneHandle;
        private Coroutine carLoadCoroutine;
        private Coroutine mapLoadCoroutine;
        private float loadingDurationSeconds;
        private float asyncLoadingDurationSeconds;
            
        public static bool HasLoaded { get; private set; }

        private IEnumerator Start()
        {
            loadingDurationSeconds = Time.realtimeSinceStartup - BootSceneManager.BootDurationSeconds;
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.GameLoaderSceneName} loading complete in {TimeSpan.FromSeconds(loadingDurationSeconds).ToPrettyString(true)}");

            currentStage = Stage.LOADING_SAVE_DATA;
            TrackedCoroutine loadSaveDataAsync = new TrackedCoroutine(DataManager.LoadAllAsync());
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            currentStage = Stage.LOADING_MAINSCENE;
            mainSceneHandle = Addressables.LoadSceneAsync(SceneManager.MainSceneName, LoadSceneMode.Additive, true);
            yield return mainSceneHandle;
            GlobalLoggers.LoadingLogger.Log($"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
            stopwatch.Restart();

            currentStage = Stage.LOADING_VEHICLE;
            Vector3 startingPosition = Vector3.zero;
            Vector3 startingRotation = Vector3.zero;
            carLoadCoroutine = CoroutineHelper.Instance.StartCoroutine(PlayerCarManager.Instance.SpawnCar(startingPosition, startingRotation));
            yield return carLoadCoroutine;
            GlobalLoggers.LoadingLogger.Log($"Vehicle loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
            stopwatch.Restart();

            yield return new WaitUntil(() => !loadSaveDataAsync.IsPlaying);
            
            asyncLoadingDurationSeconds = Time.realtimeSinceStartup - loadingDurationSeconds - BootSceneManager.BootDurationSeconds;
            GlobalLoggers.LoadingLogger.Log($"Async loading complete in {TimeSpan.FromSeconds(asyncLoadingDurationSeconds).ToPrettyString(true)}");

            OnLoadingComplete();
        }
        
        private void OnLoadingComplete()
        {
            //unload the loading scenes
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(SceneManager.BootSceneName);
            Addressables.UnloadSceneAsync(BootSceneManager.LoadingSceneInstance);

            GlobalLoggers.LoadingLogger.Log($"Total boot time = {TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToPrettyString(true)}");

            HasLoaded = true;
        }
        
        private void Update()
        {
            UpdateDebugLabel();
        }

        private void UpdateDebugLabel()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            debugLabel.gameObject.SetActive(false);
            return;
#endif
            debugLabel.text = currentStage switch
            {
                Stage.LOADING_MAINSCENE => $"Loading MainScene... ({(int)(mainSceneHandle.PercentComplete*100f)}%)",
                Stage.LOADING_VEHICLE => "Loading Vehicle...",
                _ => "Loading..."
            };
        }
        
    }
}