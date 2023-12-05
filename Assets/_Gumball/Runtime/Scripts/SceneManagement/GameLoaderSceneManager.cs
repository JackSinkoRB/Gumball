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
using Debug = UnityEngine.Debug;

namespace Gumball
{
    /// <summary>
    /// A loading scene to handle anything that needs to be loaded before entering the game.
    /// </summary>
    public class GameLoaderSceneManager : MonoBehaviour
    {

        private enum Stage
        {
            LOADING_SINGLETON_SCRIPTABLES,
            LOADING_SAVE_DATA,
            LOADING_MAINSCENE,
            LOADING_VEHICLE,
            FINISHING_ASYNC_TASKS,
        }

        [SerializeField] private TextMeshProUGUI debugLabel;

        private Stage currentStage;
        private AsyncOperationHandle[] singletonScriptableHandles;
        private AsyncOperationHandle<SceneInstance> mainSceneHandle;
        private Coroutine carLoadCoroutine;
        private Coroutine mapLoadCoroutine;
        private float loadingDurationSeconds;
        private float asyncLoadingDurationSeconds;
            
        public static bool HasLoaded { get; private set; }

        private IEnumerator Start()
        {
            loadingDurationSeconds = Time.realtimeSinceStartup - BootSceneManager.BootDurationSeconds;
#if ENABLE_LOGS
            Debug.Log($"{SceneManager.GameLoaderSceneName} loading complete in {TimeSpan.FromSeconds(loadingDurationSeconds).ToPrettyString(true)}");
#endif
            
            currentStage = Stage.LOADING_SINGLETON_SCRIPTABLES;
            Stopwatch stopwatch = Stopwatch.StartNew();
            singletonScriptableHandles = LoadSingletonScriptables();
            yield return new WaitUntil(() => singletonScriptableHandles.AreAllComplete());
#if ENABLE_LOGS
            Debug.Log($"Scriptable singletons loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            
            stopwatch.Restart();
            currentStage = Stage.LOADING_SAVE_DATA;
            TrackedCoroutine loadSaveDataAsync = new TrackedCoroutine(DataManager.LoadAllAsync());
            
            currentStage = Stage.LOADING_MAINSCENE;
            mainSceneHandle = Addressables.LoadSceneAsync(SceneManager.MainSceneName, LoadSceneMode.Additive, true);
            yield return mainSceneHandle;
#if ENABLE_LOGS
            Debug.Log($"{SceneManager.MainSceneName} loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            stopwatch.Restart();

            currentStage = Stage.LOADING_VEHICLE;
            Vector3 startingPosition = Vector3.zero;
            Vector3 startingRotation = Vector3.zero;
            carLoadCoroutine = CoroutineHelper.Instance.StartCoroutine(PlayerCarManager.Instance.SpawnCar(startingPosition, startingRotation));
            yield return carLoadCoroutine;
#if ENABLE_LOGS
            Debug.Log($"Vehicle loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            stopwatch.Restart();

            currentStage = Stage.FINISHING_ASYNC_TASKS;
            yield return new WaitUntil(() => !loadSaveDataAsync.IsPlaying);
            
            asyncLoadingDurationSeconds = Time.realtimeSinceStartup - loadingDurationSeconds - BootSceneManager.BootDurationSeconds;
#if ENABLE_LOGS
            Debug.Log($"Async loading complete in {TimeSpan.FromSeconds(asyncLoadingDurationSeconds).ToPrettyString(true)}");
#endif
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

        private AsyncOperationHandle[] LoadSingletonScriptables()
        {
            var handles = new[] {
                //LOAD ALL SINGLETON SCRIPTABLES HERE
                GlobalLoggers.LoadInstanceAsync(),
                SettingsManager.LoadInstanceAsync(),
                VersionManager.LoadInstanceAsync(),
                DecalManager.LoadInstanceAsync()
            };
            return handles;
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
                Stage.LOADING_SINGLETON_SCRIPTABLES => "Loading singleton scriptables...",
                Stage.LOADING_MAINSCENE => $"Loading MainScene... ({(int)(mainSceneHandle.PercentComplete*100f)}%)",
                Stage.LOADING_VEHICLE => "Loading vehicle...",
                Stage.FINISHING_ASYNC_TASKS => "Finishing async tasks...",
                _ => "Loading..."
            };
        }
        
    }
}