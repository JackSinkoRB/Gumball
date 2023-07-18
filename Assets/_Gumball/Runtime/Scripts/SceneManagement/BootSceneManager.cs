using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class BootSceneManager : MonoBehaviour
    {

        [Scene, SerializeField] private string loadingScene = "LoadingScene";

        private void Start()
        {
            Debug.Log($"Loading {loadingScene}");
            Addressables.LoadSceneAsync(loadingScene, LoadSceneMode.Additive, true);
        }

    }
}
