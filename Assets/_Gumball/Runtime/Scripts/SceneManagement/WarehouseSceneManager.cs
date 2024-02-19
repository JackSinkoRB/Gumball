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
        
        public static void LoadWarehouse()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadWarehouseIE());
        }
        
        private static IEnumerator LoadWarehouseIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.WarehouseSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.WarehouseSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            AvatarManager.Instance.HideAvatars(true);
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

    }
}
