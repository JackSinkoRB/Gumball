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
            DrivingCameraController.Instance.SetTarget(PlayerCarManager.Instance.CurrentCar.transform);
            PlayerCarManager.Instance.onCurrentCarChanged += OnCarChanged;
        }

        private void OnDisable()
        {
            if (PlayerCarManager.ExistsRuntime)
                PlayerCarManager.Instance.onCurrentCarChanged -= OnCarChanged;
        }

        private void OnCarChanged(CarManager newCar)
        {
            DrivingCameraController.Instance.SetTarget(newCar.transform);
        }

        public static void LoadMapDrivingScene(MapData map)
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMapDrivingSceneIE(map));
        }
        
        private static IEnumerator LoadMapDrivingSceneIE(MapData map)
        {
            PanelManager.GetPanel<LoadingPanel>().Show();
            GlobalLoggers.LoadingLogger.Log($"Map loading started...");

            //freeze the car
            Rigidbody currentCarRigidbody = PlayerCarManager.Instance.CurrentCar.Rigidbody;
            currentCarRigidbody.velocity = Vector3.zero;
            currentCarRigidbody.angularVelocity = Vector3.zero;
            currentCarRigidbody.isKinematic = true;

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.MapDrivingSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MapDrivingSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            //move the car to the right position
            Vector3 startingPosition = map.VehicleStartingPosition;
            Vector3 startingRotation = map.VehicleStartingRotation;
            currentCarRigidbody.Move(startingPosition, Quaternion.Euler(startingRotation));
            GlobalLoggers.LoadingLogger.Log($"Moved vehicle to map's starting position: {startingPosition}");
            
            //load the map chunks
            Stopwatch chunkLoadingStopwatch = Stopwatch.StartNew();
            yield return ChunkManager.Instance.LoadMap(map);
            chunkLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"Loaded chunks for map '{map.name}' in {chunkLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            //set car rigidbody as dynamic
            currentCarRigidbody.isKinematic = false;

            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.Car);

            GlobalLoggers.LoadingLogger.Log($"Map loading complete!");
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        
    }
}
