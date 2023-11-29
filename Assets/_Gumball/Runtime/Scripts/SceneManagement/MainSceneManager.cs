using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class MainSceneManager : MonoBehaviour
    {
        
        private void Start()
        {
            this.PerformAfterTrue(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals(SceneManager.MainSceneName),
                () => PanelManager.GetPanel<MainMenuPanel>().Show());
        }
        
        public static void LoadMainScene()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMainSceneIE());
        }
        
        private static IEnumerator LoadMainSceneIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.MainSceneName, LoadSceneMode.Single, true);
            stopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.MainSceneName} loading complete in {stopwatch.Elapsed.ToPrettyString(true)}");
            
            //move the car to the origin to be framed by the camera
            PlayerCarManager.Instance.CurrentCar.Rigidbody.velocity = Vector3.zero;
            PlayerCarManager.Instance.CurrentCar.Rigidbody.Move(Vector3.zero, Quaternion.Euler(Vector3.zero));
            
            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.Car, false);
            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.General);

            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

    }
}
