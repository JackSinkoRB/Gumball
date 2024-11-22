using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public static class GameReloadManager
    {

        public static void ReloadGame()
        {
            DestroyAllInDontDestroyOnLoad();
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneManager.BootSceneName, LoadSceneMode.Single);
        }
        
        public static void DestroyAllInDontDestroyOnLoad()
        {
            //create temporary object to get the DontDestroyOnLoad scene
            GameObject dontDestroyOnLoadTemp = new GameObject("DontDestroyOnLoadTemp");
            Object.DontDestroyOnLoad(dontDestroyOnLoadTemp);
            
            foreach(GameObject root in dontDestroyOnLoadTemp.scene.GetRootGameObjects())
                Object.Destroy(root);
        }
        
    }
}
