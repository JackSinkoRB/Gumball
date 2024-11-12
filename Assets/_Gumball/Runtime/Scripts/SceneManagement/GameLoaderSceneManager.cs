using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
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

        public static GameLoaderSceneManager Instance;

        private enum Stage
        {
            Checking_for_new_version,
            Loading_loggers,
            Loading_save_data,
            Loading_scriptable_data_objects,
            Initialising_parts,
            Starting_async_loading,
            Loading_mainscene,
            Loading_vehicle,
            Setup_vehicle_and_drivers,
            Initialising_PlayFab,
            Initialising_Facebook,
            Initialising_Unity_services,
        }

        [SerializeField] private TextMeshProUGUI debugLabel;

        private Stage currentStage;
        private List<TrackedCoroutine> singletonScriptableHandles;
        public AsyncOperationHandle<SceneInstance> mainSceneHandle;
        private float loadingDurationSeconds;
        private float asyncLoadingDurationSeconds;

        public static bool HasLoaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            HasLoaded = false;
        }
        
        private IEnumerator Start()
        {
            Instance = this;
            loadingDurationSeconds = Time.realtimeSinceStartup - BootSceneManager.BootDurationSeconds;
#if ENABLE_LOGS
            Debug.Log($"{SceneManager.GameLoaderSceneAddress} loading complete in {TimeSpan.FromSeconds(loadingDurationSeconds).ToPrettyString(true)}");
#endif

            Stopwatch stopwatch = Stopwatch.StartNew();
            currentStage = Stage.Loading_loggers;
            yield return GlobalLoggers.LoadInstanceAsync();
            #if UNITY_EDITOR
            yield return new WaitUntil(() => GlobalLoggers.HasLoaded);
            #endif
            GlobalLoggers.LoadingLogger.Log($"Global logger loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");

            stopwatch.Restart();
            currentStage = Stage.Checking_for_new_version;
            yield return VersionUpdatedDetector.CheckIfNewVersionAsync();
            GlobalLoggers.LoadingLogger.Log($"Check for new version completed in {stopwatch.Elapsed.ToPrettyString(true)}");
            
            stopwatch.Restart();
            currentStage = Stage.Loading_save_data;
            yield return DataManager.LoadAllAsync();
            GlobalLoggers.LoadingLogger.Log($"Save data loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");

            stopwatch.Restart();
            currentStage = Stage.Loading_scriptable_data_objects;
            singletonScriptableHandles = LoadSingletonScriptables();
            yield return new WaitUntil(() => singletonScriptableHandles.AreAllComplete());
            GlobalLoggers.LoadingLogger.Log($"Scriptable singletons loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");

            currentStage = Stage.Starting_async_loading;
            //start loading playfab
            TrackedCoroutine playfabInitialisationCoroutine = new TrackedCoroutine(PlayFabManager.Initialise());
            //start loading facebook
            TrackedCoroutine facebookInitialisationCoroutine = new TrackedCoroutine(FacebookManager.Initialise());
            //start loading unity services
            TrackedCoroutine loadUnityServicesAsync = new TrackedCoroutine(UnityServicesManager.LoadAllServices());
            //start loading avatars
            TrackedCoroutine driverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnDriver());
            TrackedCoroutine coDriverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnCoDriver());
            
            stopwatch.Restart();
            currentStage = Stage.Initialising_parts;
            TrackedCoroutine initialiseCoreParts = new TrackedCoroutine(CorePartManager.Initialise());
            TrackedCoroutine initialiseSubParts = new TrackedCoroutine(SubPartManager.Initialise());
            yield return new WaitUntil(() => !initialiseCoreParts.IsPlaying 
                                             && !initialiseSubParts.IsPlaying);
            GlobalLoggers.LoadingLogger.Log($"Parts initialisation complete in {stopwatch.Elapsed.ToPrettyString(true)}");

            GlobalLoggers.LoadingLogger.Log($"Starting to load {SceneManager.MainSceneAddress} async...");
            stopwatch.Restart();
            currentStage = Stage.Loading_mainscene;
            mainSceneHandle = Addressables.LoadSceneAsync(SceneManager.MainSceneAddress, LoadSceneMode.Additive, true);
            yield return new WaitUntil(() => mainSceneHandle.PercentComplete.Approximately(1));
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MainSceneAddress} loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");

            stopwatch.Restart();
            currentStage = Stage.Loading_vehicle;
            Vector3 carStartingPosition = Vector3.zero;
            Quaternion carStartingRotation = Quaternion.Euler(Vector3.zero);
            TrackedCoroutine carLoadCoroutine = new TrackedCoroutine(WarehouseManager.Instance.SpawnSavedCar(carStartingPosition, carStartingRotation, (car) => WarehouseManager.Instance.SetCurrentCar(car)));
            
            currentStage = Stage.Setup_vehicle_and_drivers;
            yield return new WaitUntil(() => !carLoadCoroutine.IsPlaying && !driverAvatarLoadCoroutine.IsPlaying && !coDriverAvatarLoadCoroutine.IsPlaying);
            AvatarManager.Instance.DriverAvatar.Teleport(MainSceneManager.Instance.DriverStandingPosition, MainSceneManager.Instance.DriverStandingRotation);
            AvatarManager.Instance.CoDriverAvatar.Teleport(MainSceneManager.Instance.CoDriverStandingPosition, MainSceneManager.Instance.CoDriverStandingRotation);
            GlobalLoggers.LoadingLogger.Log($"Vehicle and driver setup complete in {stopwatch.Elapsed.ToPrettyString(true)}");
            
            currentStage = Stage.Initialising_PlayFab;
            yield return new WaitUntil(() => !playfabInitialisationCoroutine.IsPlaying);
            
            currentStage = Stage.Initialising_Facebook;
            yield return new WaitUntil(() => !facebookInitialisationCoroutine.IsPlaying);
            
            currentStage = Stage.Initialising_Unity_services;
            yield return new WaitUntil(() => !loadUnityServicesAsync.IsPlaying);
            
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

        private List<TrackedCoroutine> LoadSingletonScriptables()
        {
            List<TrackedCoroutine> trackedCoroutines = new List<TrackedCoroutine>
            {
                new(DecalManager.LoadInstanceAsync()),
                new(AvatarManager.LoadInstanceAsync()),
                new(WarehouseManager.LoadInstanceAsync()),
                new(GlobalPaintPresets.LoadInstanceAsync()),
                new(ExperienceManager.LoadInstanceAsync()),
                new(IAPManager.LoadInstanceAsync()),
                new(ChallengeTrackerManager.LoadInstanceAsync()),
                new(ChallengeManager.LoadInstanceAsync()),
                new(FuelManager.LoadInstanceAsync()),
                new(GlobalColourPalette.LoadInstanceAsync()),
                new(BlueprintManager.LoadInstanceAsync()),
                new(NightTimeMaterialAdjustment.LoadInstanceAsync()),
                new(GumballEventManager.LoadInstanceAsync()),
                new(DailyLoginManager.LoadInstanceAsync())
            };
            
            return trackedCoroutines;
        }

        private void Update()
        {
            UpdateDebugLabel();
        }

        private void UpdateDebugLabel()
        {
            debugLabel.text = currentStage switch
            {
                Stage.Loading_mainscene => $"Loading MainScene... ({(int)(mainSceneHandle.PercentComplete*100f)}%)",
                _ => $"{currentStage.ToString().Replace("_", " ")}..."
            };
        }
        
    }
}