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
        }

        private void OnCarChanged(CarManager newCar)
        {
            DrivingCameraController.Instance.SetTarget(newCar.transform);
        }

        public static void LoadMapDrivingScene(ChunkMap chunkMap)
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMapDrivingSceneIE(chunkMap));
        }
        
        private static IEnumerator LoadMapDrivingSceneIE(ChunkMap chunkMap)
        {
            PanelManager.GetPanel<LoadingPanel>().Show();
            GlobalLoggers.LoadingLogger.Log($"Map loading started...");

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.MapDrivingSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MapDrivingSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");

            yield return SetupMapDrivingScene(chunkMap);

            GlobalLoggers.LoadingLogger.Log($"Map loading complete!");
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

        public static IEnumerator SetupMapDrivingScene(ChunkMap chunkMap)
        {
            WarehouseManager.Instance.CurrentCar.gameObject.SetActive(true);
            
            //freeze the car
            Rigidbody currentCarRigidbody = WarehouseManager.Instance.CurrentCar.Rigidbody;
            currentCarRigidbody.velocity = Vector3.zero;
            currentCarRigidbody.angularVelocity = Vector3.zero;
            currentCarRigidbody.isKinematic = true;
            
            //move the car to the right position
            Vector3 startingPosition = chunkMap.VehicleStartingPosition;
            Vector3 startingRotation = chunkMap.VehicleStartingRotation;
            currentCarRigidbody.Move(startingPosition, Quaternion.Euler(startingRotation));
            GlobalLoggers.LoadingLogger.Log($"Moved vehicle to map's starting position: {startingPosition}");
            
            //load the map chunks
            Stopwatch chunkLoadingStopwatch = Stopwatch.StartNew();
            yield return ChunkManager.Instance.LoadMap(chunkMap);
            chunkLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"Loaded chunks for map '{chunkMap.name}' in {chunkLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            //set car rigidbody as dynamic
            currentCarRigidbody.isKinematic = false;

            //set driving states:
            AvatarManager.Instance.HideAvatars(false);
            AvatarDrivingState driverDrivingState = AvatarManager.Instance.DriverAvatar.StateManager.GetState<AvatarDrivingState>();
            AvatarDrivingState coDriverDrivingState = AvatarManager.Instance.CoDriverAvatar.StateManager.GetState<AvatarDrivingState>();
            AvatarManager.Instance.DriverAvatar.StateManager.SetState(driverDrivingState);
            AvatarManager.Instance.CoDriverAvatar.StateManager.SetState(coDriverDrivingState);

            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.Car);
        }
        
    }
}
