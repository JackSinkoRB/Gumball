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

        public static void LoadEditor()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadDecalEditorIE());
        }
        
        private static IEnumerator LoadDecalEditorIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.AvatarEditorSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.AvatarEditorSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");

            // Instance.cameraController.gameObject.SetActive(true);
            Instance.StartSession();
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

        public void StartSession()
        {
            PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(false);
        }

        public void EndSession()
        {
            PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(true);
        }
        
    }
}
