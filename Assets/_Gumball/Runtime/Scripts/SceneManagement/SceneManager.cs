using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class SceneManager : Singleton<SceneManager>
    {

        public const string BootSceneName = "BootScene";
        public const string GameLoaderSceneName = "GameLoaderScene";
        public const string MainSceneName = "MainScene";
        public const string MapDrivingSceneName = "MapDrivingScene";

        protected override void Initialise()
        {
            base.Initialise();

#if UNITY_EDITOR
            TryLoadBootScene();
#endif
        }
        
        /// <summary>
        /// In the editor, make sure that we always load from the boot scene first. 
        /// </summary>
        private void TryLoadBootScene()
        {
            bool alreadyInBootScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0;
            if (alreadyInBootScene)
                return;
            
            Addressables.LoadSceneAsync(BootSceneName, LoadSceneMode.Additive, true);
            BootSceneManager.LoadedFromAnotherScene = true;
        }
    }
}
