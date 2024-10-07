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
            DecalEditor.IsRunningTests = true;
            IAPManager.IsRunningTests = true;
            DataManager.EnableTestProviders(true);

            AsyncOperation loadBootScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.BootScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadBootScene.completed += OnSceneLoadComplete;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
            if (WarehouseManager.HasLoaded && WarehouseManager.Instance.CurrentCar != null)
                Object.DestroyImmediate(WarehouseManager.Instance.CurrentCar.gameObject);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            isInitialised = true;
        }
        
        [UnityTest]
        [Order(1)]
        public IEnumerator GameLoadsSuccessfully()
        {
            if (Application.isBatchMode)
                LogAssert.Expect(LogType.Error, "Trying to set 2 renderTargets, which is more than the maximum supported:1"); //continue with this error - it's related to headless mode only
            
            yield return new WaitUntil(() => isInitialised);
            
            const float maxLoadTimeAllowed = 180; //in seconds
            
            float totalTimeWaiting = 0;
            while (true)
            {
                if (GameLoaderSceneManager.HasLoaded)
                    break;
                
                yield return new WaitForSeconds(1);
                totalTimeWaiting += 1;

                if (totalTimeWaiting > maxLoadTimeAllowed)
                    break;
            }
            
            Assert.Less(totalTimeWaiting, maxLoadTimeAllowed);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator FoundCoreParts()
        {
            yield return new WaitUntil(() => isInitialised);
            Assert.IsTrue(GameLoaderSceneManager.HasLoaded);
            
            Assert.IsTrue(CorePartManager.AllParts.Count > 0);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator FoundSubParts()
        {
            yield return new WaitUntil(() => isInitialised);
            Assert.IsTrue(GameLoaderSceneManager.HasLoaded);
            
            Assert.IsTrue(SubPartManager.AllParts.Count > 0);
        }

    }
}
