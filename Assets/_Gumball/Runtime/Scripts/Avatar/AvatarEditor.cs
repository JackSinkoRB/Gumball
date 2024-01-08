using System;
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

        public static event Action onSessionStart;
        public static event Action onSessionEnd;
        
        #region STATIC
        public static void LoadEditor()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadEditorIE());
        }
        
        private static IEnumerator LoadEditorIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.AvatarEditorSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.AvatarEditorSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            Instance.StartSession();
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        #endregion

        [SerializeField] private Vector3 startingPosition;
        [SerializeField] private Vector3 startingRotationEuler;
        
        public Avatar CurrentSelectedAvatar { get; private set; }
        
        public void StartSession()
        {
            PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(false);

            CurrentSelectedAvatar = AvatarManager.Instance.DriverAvatar; //always start with driver selected
            CurrentSelectedAvatar.Teleport(startingPosition, Quaternion.Euler(startingRotationEuler));
        }

        public void EndSession()
        {
            PlayerCarManager.Instance.CurrentCar.gameObject.SetActive(true);
        }
        
    }
}
