using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Gumball
{
    /// <summary>
    /// A loading scene to handle anything that needs to be loaded before entering the game.
    /// </summary>
    public class LoadingSceneManager : MonoBehaviour
    {

        private enum Stage
        {
            LOADING_MAINSCENE,
            LOADING_VEHICLE,
        }
        
        [SerializeField] private AssetReference sceneToLoad;
        [SerializeField] private TextMeshProUGUI debugLabel;

        private Stage currentStage;
        
        private AsyncOperationHandle<SceneInstance> mainSceneHandle;

        private IEnumerator Start()
        {
            currentStage = Stage.LOADING_MAINSCENE;
            mainSceneHandle = Addressables.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single, false);
            yield return mainSceneHandle;

            currentStage = Stage.LOADING_VEHICLE;
            AsyncOperationHandle<GameObject> carLoadHandle = PlayerCarManager.Instance.SpawnCar();
            yield return carLoadHandle;
            
            OnLoadingComplete();
        }

        private void OnLoadingComplete()
        {
            //activate the main scene
            mainSceneHandle.Result.ActivateAsync();
        }
        
        private void Update()
        {
            UpdateDebugLabel();
        }

        private void UpdateDebugLabel()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            debugLabel.gameObject.SetActive(false);
            return;
#endif
            debugLabel.text = currentStage switch
            {
                Stage.LOADING_MAINSCENE => "Loading MainScene... (" + (int)(mainSceneHandle.PercentComplete*100f) + "%)",
                Stage.LOADING_VEHICLE => "Loading Vehicle...",
                _ => "Loading..."
            };
        }
        
    }
}