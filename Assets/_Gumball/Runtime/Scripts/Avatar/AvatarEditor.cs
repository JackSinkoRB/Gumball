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

        public static event Action onSessionStart;
        public static event Action onSessionEnd;

        /// <summary>
        /// Called when switching between driver and co-driver.
        /// </summary>
        public static event Action<Avatar> onSelectedAvatarChanged; 
        
        #region STATIC
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
            
            Instance.StartSession();
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        #endregion

        [SerializeField] private Vector3 driverStartingPosition;
        [SerializeField] private Vector3 driverStartingRotationEuler;
        
        [SerializeField] private Vector3 coDriverStartingPosition;
        [SerializeField] private Vector3 coDriverStartingRotationEuler;
        
        public Avatar CurrentSelectedAvatar { get; private set; }
        
        public void StartSession()
        {
            DataProvider.onBeforeSaveAllDataOnAppExit += OnBeforeSaveAllDataOnAppExit;

            PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(false);

            SelectAvatar(true); //always start with driver selected
            
            AvatarManager.Instance.DriverAvatar.Teleport(driverStartingPosition, Quaternion.Euler(driverStartingRotationEuler));
            AvatarManager.Instance.CoDriverAvatar.Teleport(coDriverStartingPosition, Quaternion.Euler(coDriverStartingRotationEuler));
            
            onSessionStart?.Invoke();
        }

        public void EndSession()
        {
            DataProvider.onBeforeSaveAllDataOnAppExit -= OnBeforeSaveAllDataOnAppExit;
            
            PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(true);

            SaveAvatars();

            onSessionEnd?.Invoke();
        }

        public void SelectAvatar(bool driver)
        {
            CurrentSelectedAvatar = driver ? AvatarManager.Instance.DriverAvatar : AvatarManager.Instance.CoDriverAvatar;
            onSelectedAvatarChanged?.Invoke(CurrentSelectedAvatar);
            
            //TODO: reset selected category/cosmetic
            //TODO: camera select
        }

        private void OnBeforeSaveAllDataOnAppExit()
        { 
            SaveAvatars();
        }
        
        private void SaveAvatars()
        {
            AvatarManager.Instance.DriverAvatar.SaveCurrentBody();
            AvatarManager.Instance.CoDriverAvatar.SaveCurrentBody();
            GlobalLoggers.SaveDataLogger.Log("Saved avatar data");
        }
        
    }
}
