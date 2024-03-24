using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class MapDrivingSceneManager : MonoBehaviour
    {

        private void OnEnable()
        {
            this.PerformAfterTrue(() => WarehouseManager.Instance.CurrentCar != null, () =>
            {
                OnCarChanged(WarehouseManager.Instance.CurrentCar);
                WarehouseManager.Instance.onCurrentCarChanged += OnCarChanged;
            });
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChanged;
            
            //set idle states
            AvatarManager.Instance.DriverAvatar.StateManager.SetState<AvatarStandingIdleState>();
            AvatarManager.Instance.CoDriverAvatar.StateManager.SetState<AvatarStandingIdleState>();
        }

        private void OnCarChanged(CarManager newCar)
        {
            DrivingCameraController.Instance.SetTarget(newCar.transform);
        }

        public static IEnumerator LoadMapDrivingSceneIE()
        {
            GlobalLoggers.LoadingLogger.Log($"Map loading started...");

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.MapDrivingSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MapDrivingSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
        }

    }
}
