using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public static class SceneUtils
    {

        public static List<T> GetAllComponentsInActiveScene<T>() where T : MonoBehaviour
        {
            List<T> results = new List<T>();
            
            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObject in rootGameObjects)
                FindGameObjectsWithComponentRecursive<T>(rootObject, results);

            return results;
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
