using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public static class SceneUtils
    {

        
        public static List<T> GetAllComponentsInActiveScene<T>(bool includeDontDestroyOnLoad = false) where T : MonoBehaviour
        {
            return GetAllComponentsInScene<T>(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), includeDontDestroyOnLoad);
        }
    
        public static List<T> GetAllComponentsInScene<T>(string sceneName, bool includeDontDestroyOnLoad = false) where T : MonoBehaviour
        {
            return GetAllComponentsInScene<T>(UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName), includeDontDestroyOnLoad);
        }
    
        public static List<T> GetAllComponentsInScene<T>(Scene scene, bool includeDontDestroyOnLoad = false) where T : MonoBehaviour
        {
            List<T> results = new List<T>();
            
            List<GameObject> rootGameObjects = new(scene.GetRootGameObjects());
            
            if (includeDontDestroyOnLoad)
                rootGameObjects.AddRange(GetDontDestroyOnLoadObjects());
            
            foreach (GameObject rootObject in rootGameObjects)
                FindGameObjectsWithComponentRecursive<T>(rootObject, results);

            return results;
        }

        private static GameObject[] GetDontDestroyOnLoadObjects()
        {
            GameObject temp = null;
            try
            {
                temp = new GameObject();
                Object.DontDestroyOnLoad( temp );
                UnityEngine.SceneManagement.Scene dontDestroyOnLoad = temp.scene;
                Object.DestroyImmediate( temp );
                temp = null;
     
                return dontDestroyOnLoad.GetRootGameObjects();
            }
            finally
            {
                if( temp != null )
                    Object.DestroyImmediate( temp );
            }
        }

        
        private static void FindGameObjectsWithComponentRecursive<T>(GameObject parent, List<T> resultsOutput)
        {
            if (parent.GetComponent<T>() != null)
                resultsOutput.Add(parent.GetComponent<T>());

            foreach (Transform child in parent.transform)
                FindGameObjectsWithComponentRecursive<T>(child.gameObject, resultsOutput);
        }
        
    }
}
