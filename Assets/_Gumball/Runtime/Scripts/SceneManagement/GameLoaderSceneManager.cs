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
            Checking_for_new_version,
            Loading_scriptable_singletons,
            Loading_save_data,
            Loading_mainscene,
            Loading_vehicle,
            Loading_driver_avatar,
            Finishing_async_tasks,
        }

        [SerializeField] private TextMeshProUGUI debugLabel;

        private Stage currentStage;
        private AsyncOperationHandle[] singletonScriptableHandles;
        private AsyncOperationHandle<SceneInstance> mainSceneHandle;
        private Coroutine carLoadCoroutine;
        private Coroutine driverAvatarLoadCoroutine;
        private float loadingDurationSeconds;
        private float asyncLoadingDurationSeconds;
            
        public static bool HasLoaded { get; private set; }

        private IEnumerator Start()
        {
            loadingDurationSeconds = Time.realtimeSinceStartup - BootSceneManager.BootDurationSeconds;
#if ENABLE_LOGS
            Debug.Log($"{SceneManager.GameLoaderSceneName} loading complete in {TimeSpan.FromSeconds(loadingDurationSeconds).ToPrettyString(true)}");
#endif

            currentStage = Stage.Loading_scriptable_singletons;
            Stopwatch stopwatch = Stopwatch.StartNew();
            singletonScriptableHandles = LoadSingletonScriptables();
            yield return new WaitUntil(() => singletonScriptableHandles.AreAllComplete());
#if ENABLE_LOGS
            Debug.Log($"Scriptable singletons loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            
            currentStage = Stage.Checking_for_new_version;
            yield return VersionUpdatedDetector.CheckIfNewVersionAsync();
            
            stopwatch.Restart();
            currentStage = Stage.Loading_save_data;
            TrackedCoroutine loadSaveDataAsync = new TrackedCoroutine(DataManager.LoadAllAsync());
            
            currentStage = Stage.Loading_mainscene;
            mainSceneHandle = Addressables.LoadSceneAsync(SceneManager.MainSceneName, LoadSceneMode.Additive, true);
            yield return mainSceneHandle;
#if ENABLE_LOGS
            Debug.Log($"{SceneManager.MainSceneName} loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            stopwatch.Restart();

            currentStage = Stage.Loading_vehicle;
            Vector3 carStartingPosition = Vector3.zero;
            Vector3 carStartingRotation = Vector3.zero;
            carLoadCoroutine = CoroutineHelper.Instance.StartCoroutine(PlayerCarManager.Instance.SpawnCar(carStartingPosition, carStartingRotation));
            yield return carLoadCoroutine;
#if ENABLE_LOGS
            Debug.Log($"Vehicle loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            stopwatch.Restart();
            
            currentStage = Stage.Loading_driver_avatar;
            driverAvatarLoadCoroutine = CoroutineHelper.Instance.StartCoroutine(AvatarManager.Instance.SpawnAvatar());
            yield return driverAvatarLoadCoroutine;
#if ENABLE_LOGS
            Debug.Log($"Driver avatar loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            stopwatch.Restart();

            currentStage = Stage.Finishing_async_tasks;
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
                DecalManager.LoadInstanceAsync(),
                AvatarManager.LoadInstanceAsync()
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
                Stage.Loading_mainscene => $"Loading MainScene... ({(int)(mainSceneHandle.PercentComplete*100f)}%)",
                _ => $"{currentStage.ToString().Replace("_", " ")}..."
            };
        }
        
    }
}