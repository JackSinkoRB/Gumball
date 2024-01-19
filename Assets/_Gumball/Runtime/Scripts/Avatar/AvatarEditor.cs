using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class AvatarEditor : Singleton<AvatarEditor>
    {

        #region STATIC
        
        public static event Action onSessionStart;
        public static event Action onSessionEnd;

        public delegate void OnSelectedAvatarChangedDelegate(Avatar oldAvatar, Avatar newAvatar);
        /// <summary>
        /// Called when switching between driver and co-driver.
        /// </summary>
        public static event OnSelectedAvatarChangedDelegate onSelectedAvatarChanged; 
        
        public static void LoadEditor()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadEditorIE());
        }
        
        private static IEnumerator LoadEditorIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.AvatarEditorSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.AvatarEditorSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            yield return Instance.StartSession();
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        
        public static void SaveCurrentAvatarBody()
        {
            Avatar driver = AvatarManager.Instance.DriverAvatar;
            if (driver.CurrentBodyType != AvatarBodyType.NONE)
                driver.SaveBodyCosmetics(driver.CurrentBodyType == AvatarBodyType.MALE ? driver.CurrentMaleBody : driver.CurrentFemaleBody);

            Avatar coDriver = AvatarManager.Instance.CoDriverAvatar;
            if (coDriver.CurrentBodyType != AvatarBodyType.NONE)
                coDriver.SaveBodyCosmetics(coDriver.CurrentBodyType == AvatarBodyType.MALE ? coDriver.CurrentMaleBody : coDriver.CurrentFemaleBody);
            
            GlobalLoggers.SaveDataLogger.Log("Saved avatar body data");
        }
        #endregion

        [SerializeField] private AvatarCameraController cameraController;
            
        [Space(5)]
        [SerializeField] private Vector3 driverStartingPosition;
        [SerializeField] private Vector3 driverStartingRotationEuler;

        [Space(5)]
        [SerializeField] private Vector3 coDriverStartingPosition;
        [SerializeField] private Vector3 coDriverStartingRotationEuler;
        
        public Avatar CurrentSelectedAvatar { get; private set; }
        public bool SessionInProgress { get; private set; }
        public AvatarCameraController CameraController => cameraController;
        
        public IEnumerator StartSession()
        {
            SessionInProgress = true;
            
            DataProvider.onBeforeSaveAllDataOnAppExit += OnBeforeSaveAllDataOnAppExit;

            if (PlayerCarManager.ExistsRuntime && PlayerCarManager.Instance.CurrentCar != null)
                PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(false);

            SelectAvatar(true); //always start with driver selected

            //load all the body types for driver and co-driver so they can be switched instantly
            TrackedCoroutine driverBodyLoading = new TrackedCoroutine(AvatarManager.Instance.DriverAvatar.EnsureAllBodiesExist());
            TrackedCoroutine coDriverBodyLoading = new TrackedCoroutine(AvatarManager.Instance.CoDriverAvatar.EnsureAllBodiesExist());
            yield return new WaitUntil(() => !driverBodyLoading.IsPlaying && !coDriverBodyLoading.IsPlaying);
            
            onSessionStart?.Invoke();
        }

        public void EndSession()
        {
            DataProvider.onBeforeSaveAllDataOnAppExit -= OnBeforeSaveAllDataOnAppExit;
            
            if (PlayerCarManager.ExistsRuntime && PlayerCarManager.Instance.CurrentCar != null)
                PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(true);

            SaveCurrentAvatarBody();

            CurrentSelectedAvatar = null;
            
            //only keep the body type that we use
            AvatarManager.Instance.DriverAvatar.DestroyAllBodyTypesExceptCurrent();
            AvatarManager.Instance.CoDriverAvatar.DestroyAllBodyTypesExceptCurrent();

            onSessionEnd?.Invoke();
            SessionInProgress = false;
        }

        public void SelectAvatar(bool driver)
        {
            Avatar newAvatar = driver ? AvatarManager.Instance.DriverAvatar : AvatarManager.Instance.CoDriverAvatar;
            
            if (CurrentSelectedAvatar == newAvatar)
                return; //already selected

            SaveCurrentAvatarBody();

            if (driver)
            {
                AvatarManager.Instance.DriverAvatar.Teleport(driverStartingPosition, Quaternion.Euler(driverStartingRotationEuler));
                AvatarManager.Instance.CoDriverAvatar.Teleport(coDriverStartingPosition, Quaternion.Euler(coDriverStartingRotationEuler));
            }
            else
            {
                AvatarManager.Instance.DriverAvatar.Teleport(coDriverStartingPosition, Quaternion.Euler(coDriverStartingRotationEuler));
                AvatarManager.Instance.CoDriverAvatar.Teleport(driverStartingPosition, Quaternion.Euler(driverStartingRotationEuler));
            }

            Avatar oldAvatar = CurrentSelectedAvatar;
            CurrentSelectedAvatar = newAvatar;
            onSelectedAvatarChanged?.Invoke(oldAvatar, CurrentSelectedAvatar);
        }

        private void OnBeforeSaveAllDataOnAppExit()
        { 
            SaveCurrentAvatarBody();
        }

    }
}
