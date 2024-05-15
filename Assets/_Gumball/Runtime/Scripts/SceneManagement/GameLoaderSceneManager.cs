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
            Loading_scriptable_data_objects,
            Loading_save_data,
            Loading_mainscene,
            Waiting_for_save_data_to_load,
            Loading_vehicle,
            Loading_avatars,
            Loading_vehicle_and_drivers,
        }

        [SerializeField] private TextMeshProUGUI debugLabel;

        private Stage currentStage;
        private AsyncOperationHandle[] singletonScriptableHandles;
        private AsyncOperationHandle<SceneInstance> mainSceneHandle;
        private float loadingDurationSeconds;
        private float asyncLoadingDurationSeconds;
            
        public static bool HasLoaded { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            HasLoaded = false;
        }
        
        private IEnumerator Start()
        {
            loadingDurationSeconds = Time.realtimeSinceStartup - BootSceneManager.BootDurationSeconds;
#if ENABLE_LOGS
            Debug.Log($"{SceneManager.GameLoaderSceneName} loading complete in {TimeSpan.FromSeconds(loadingDurationSeconds).ToPrettyString(true)}");
#endif

            currentStage = Stage.Loading_scriptable_data_objects;
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            singletonScriptableHandles = LoadSingletonScriptables();
            TrackedCoroutine initialiseCoreParts = new TrackedCoroutine(CorePartManager.Initialise());
            TrackedCoroutine initialiseSubParts = new TrackedCoroutine(SubPartManager.Initialise());

            yield return new WaitUntil(() => singletonScriptableHandles.AreAllComplete() && !initialiseCoreParts.IsPlaying && !initialiseSubParts.IsPlaying);
#if ENABLE_LOGS
            Debug.Log($"Scriptable singletons loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif

            currentStage = Stage.Checking_for_new_version;
            yield return VersionUpdatedDetector.CheckIfNewVersionAsync();
            
            currentStage = Stage.Loading_save_data;
            TrackedCoroutine loadSaveDataAsync = new TrackedCoroutine(DataManager.LoadAllAsync());
            
            stopwatch.Restart();
            currentStage = Stage.Loading_mainscene;
            mainSceneHandle = Addressables.LoadSceneAsync(SceneManager.MainSceneName, LoadSceneMode.Additive, true);
            yield return mainSceneHandle;
#if ENABLE_LOGS
            Debug.Log($"{SceneManager.MainSceneName} loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            
            stopwatch.Restart();
            currentStage = Stage.Waiting_for_save_data_to_load;
            yield return new WaitUntil(() => !loadSaveDataAsync.IsPlaying);
#if ENABLE_LOGS
            Debug.Log($"Took additional {stopwatch.Elapsed.ToPrettyString(true)} to load the save data");
#endif
            
            stopwatch.Restart();
            currentStage = Stage.Loading_vehicle;
            Vector3 carStartingPosition = Vector3.zero;
            Quaternion carStartingRotation = Quaternion.Euler(Vector3.zero);
            TrackedCoroutine carLoadCoroutine = new TrackedCoroutine(WarehouseManager.Instance.SpawnSavedCar(carStartingPosition, carStartingRotation, (car) => WarehouseManager.Instance.SetCurrentCar(car)));
            
            currentStage = Stage.Loading_avatars;
            TrackedCoroutine driverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnDriver(MainSceneManager.Instance.DriverStandingPosition, MainSceneManager.Instance.DriverStandingRotation));
            TrackedCoroutine coDriverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnCoDriver(MainSceneManager.Instance.CoDriverStandingPosition, MainSceneManager.Instance.CoDriverStandingRotation));
            
            currentStage = Stage.Loading_vehicle_and_drivers;
            yield return new WaitUntil(() => !carLoadCoroutine.IsPlaying && !driverAvatarLoadCoroutine.IsPlaying && !coDriverAvatarLoadCoroutine.IsPlaying);
#if ENABLE_LOGS
            Debug.Log($"Vehicle and driver loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
            
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
                DecalManager.LoadInstanceAsync(),
                AvatarManager.LoadInstanceAsync(),
                WarehouseManager.LoadInstanceAsync(),
                GlobalPaintPresets.LoadInstanceAsync()
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