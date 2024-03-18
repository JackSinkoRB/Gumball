using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class MainSceneManager : Singleton<MainSceneManager>
    {

        [SerializeField] private Vector3 driverStandingPosition;
        [SerializeField] private Vector3 driverStandingRotationEuler;

        [SerializeField] private Vector3 coDriverStandingPosition;
        [SerializeField] private Vector3 coDriverStandingRotationEuler;
        
        public Vector3 DriverStandingPosition => driverStandingPosition;
        public Quaternion DriverStandingRotation => Quaternion.Euler(driverStandingRotationEuler);
        
        public Vector3 CoDriverStandingPosition => coDriverStandingPosition;
        public Quaternion CoDriverStandingRotation => Quaternion.Euler(coDriverStandingRotationEuler);
        
        private void Start()
        {
            this.PerformAfterTrue(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals(SceneManager.MainSceneName),
                () => PanelManager.GetPanel<MainMenuPanel>().Show());
        }
        
        public static void LoadMainScene()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMainSceneIE());
        }
        
        private static IEnumerator LoadMainSceneIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.MainSceneName, LoadSceneMode.Single, true);
            stopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MainSceneName} loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
            
            //ensure car is showing
            WarehouseManager.Instance.CurrentCar.gameObject.SetActive(true);
            
            //ensure avatars are showing
            AvatarManager.Instance.HideAvatars(false);
            
            //move the car to the origin to be framed by the camera
            WarehouseManager.Instance.CurrentCar.Teleport(Vector3.zero, Quaternion.Euler(Vector3.zero));
            
            //move the avatars
            AvatarManager.Instance.DriverAvatar.Teleport(Instance.driverStandingPosition, Instance.DriverStandingRotation);
            AvatarManager.Instance.CoDriverAvatar.Teleport(Instance.coDriverStandingPosition, Instance.CoDriverStandingRotation);

            InputManager.Instance.CarInput.Disable();

            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

    }
}
