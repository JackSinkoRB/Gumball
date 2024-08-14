using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

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

        [SerializeField] private WorkshopCameraController cameraController;
        
        private void SetupCamera()
        {
            if (WarehouseManager.Instance.CurrentCar != null)
                cameraController.SetTarget(WarehouseManager.Instance.CurrentCar.transform, cameraController.DefaultTargetOffset);
            
            cameraController.SetInitialPosition();
        }

    }
}
