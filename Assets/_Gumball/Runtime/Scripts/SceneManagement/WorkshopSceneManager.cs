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
    public class WorkshopSceneManager : Singleton<WorkshopSceneManager>
    {
        
        private readonly RaycastHit[] groundedHitsCached = new RaycastHit[1];
        
        #region STATIC
        public static void LoadWorkshop()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadWorkshopIE());
        }
        
        private static IEnumerator LoadWorkshopIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.WorkshopSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.WorkshopSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            AvatarManager.Instance.HideAvatars(true);

            //disable X and Z position movement while modifying the car to prevent it sliding away
            WarehouseManager.Instance.CurrentCar.Rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        #endregion

        public void ExitWorkshopScene()
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
            int numberOfHitsDown = Physics.RaycastNonAlloc(currentCar.transform.position.OffsetY(10000), Vector3.down, groundedHitsCached, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.Ground));

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
        
    }
}
