using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class SceneManager : Singleton<SceneManager>
    {

        public const string BootSceneName = "BootScene";
        public const string LoadingSceneName = "LoadingScene";
        public const string InitialSceneName = "MainScene";

        protected override void Initialise()
        {
            base.Initialise();

            TryLoadBootScene();
        }

        /// <summary>
        /// In the editor, make sure that we always load from the boot scene first. 
        /// </summary>
        private void TryLoadBootScene()
        {
#if UNITY_EDITOR
            bool alreadyInBootScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0;
            if (alreadyInBootScene)
                return;
            
            Addressables.LoadSceneAsync(BootSceneName, LoadSceneMode.Additive, true);
            BootSceneManager.LoadedFromAnotherScene = true;
#endif
        }
    }
}
