using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class WorkshopSceneManager : Singleton<WorkshopSceneManager>
    {
        
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
        
    }
}
