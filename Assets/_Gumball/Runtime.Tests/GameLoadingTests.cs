using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class GameLoadingTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private static SceneAsset loadBootScene;
        private bool isInitialised;
        
        public void Setup()
        {
            loadBootScene = EditorSceneManager.playModeStartScene;
            EditorSceneManager.playModeStartScene = null;
        }

        public void Cleanup()
        {
            EditorSceneManager.playModeStartScene = loadBootScene;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DataManager.EnableTestProviders(true);
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
        }

        [UnityTest]
        public IEnumerator EnsureGameLoadsInUnder30Seconds()
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneManager.BootSceneName, new LoadSceneParameters(LoadSceneMode.Single));
            
            const float timeout = 30;
            float elapsedTime = 0;

            while (!GameLoaderSceneManager.HasLoaded && elapsedTime < timeout)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
            }

            Assert.IsTrue(GameLoaderSceneManager.HasLoaded, "Game did not load within 30 seconds");
        }
        
    }
}
