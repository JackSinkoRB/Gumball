using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ChunkManagerTests : IPrebuildSetup, IPostBuildCleanup
    {

        private bool isInitialised;

        public void Setup()
        {
            BootSceneClear.TrySetup();
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DataManager.EnableTestProviders(true);
            
            AsyncOperation loadMainScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.BootScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMainScene.completed += OnBootSceneLoadComplete;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        [TearDown]
        public void TearDown()
        {
            isInitialised = false;
        }
        
        private void OnBootSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMap());
        }
        
        private IEnumerator LoadMap()
        {
            yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals(SceneManager.MainSceneName));
            
            MapDrivingSceneManager.LoadMapDrivingScene(TestManager.Instance.ChunkTestingMap);
            
            yield return new WaitUntil(() => ChunkManager.Instance.HasLoaded);
            
            isInitialised = true;
        }
        
        [UnityTest]
        public IEnumerator ChunksStartLoaded()
        {
            yield return new WaitUntil(() => isInitialised);
            Assert.IsTrue(ChunkManager.ExistsRuntime);
        }
        
    }
}
