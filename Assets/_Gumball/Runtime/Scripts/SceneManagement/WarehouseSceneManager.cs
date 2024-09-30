using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    public class WarehouseSceneManager : Singleton<WarehouseSceneManager>
    {
        
        #region STATIC
        public static void LoadWarehouse()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadWarehouseIE());
        }
        
        private static IEnumerator LoadWarehouseIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.WarehouseSceneAddress, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.WarehouseSceneAddress} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            WarehouseManager.Instance.CurrentCar.SetGrounded();

            Instance.SetupCamera();
            
            AvatarManager.Instance.HideAvatars(true);

            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        #endregion
        
        [SerializeField] private WarehouseCameraController cameraController;
        
#if UNITY_EDITOR
        [Header("Testing")]
        [SerializeField] private Rewards blueprintsRewardTesting;
        
        [ButtonMethod]
        public void TestGiveRewards()
        {
            if (!Application.isPlaying)
                throw new InvalidOperationException("Cannot give rewards outside play mode.");

            CoroutineHelper.Instance.StartCoroutine(blueprintsRewardTesting.GiveRewards());
        }
#endif
        
        private readonly RaycastHit[] groundedHitsCached = new RaycastHit[1];
        
        public void ExitWarehouseScene()
        {
            SaveRideHeight();
            MainSceneManager.LoadMainScene();
        }
        
        /// <summary>
        /// Calculates the distance to the ground and saves it to file. 
        /// </summary>
        private void SaveRideHeight()
        {
            AICar currentCar = WarehouseManager.Instance.CurrentCar;
            int numberOfHitsDown = Physics.RaycastNonAlloc(currentCar.transform.position.OffsetY(10000), Vector3.down, groundedHitsCached, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayers(new []{LayersAndTags.Layer.Ground, LayersAndTags.Layer.Default}));

            if (numberOfHitsDown == 0)
            {
                Debug.LogWarning($"Could not save ride height for {currentCar.name} because there is no ground above or below.");
                return;
            }

            Vector3 offset = currentCar.transform.position - groundedHitsCached[0].point;
            float rideHeight = offset.y;
            
            DataManager.Cars.Set($"{currentCar.SaveKey}.RideHeight", rideHeight);
            
            GlobalLoggers.AICarLogger.Log($"Saved ride height for {currentCar.SaveKey} to {rideHeight}");
        }
        
        private void SetupCamera()
        {
            if (WarehouseManager.Instance.CurrentCar != null)
                cameraController.SetTarget(WarehouseManager.Instance.CurrentCar.transform, cameraController.DefaultTargetOffset);
            
            cameraController.SetInitialPosition();
        }

    }
}
