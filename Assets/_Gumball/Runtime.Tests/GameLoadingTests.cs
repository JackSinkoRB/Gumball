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
    public class GameLoadingTests : BaseRuntimeTests
    {
        
        private bool isInitialised;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            
            AsyncOperation loadBootScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.BootScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadBootScene.completed += OnSceneLoadComplete;
        }

        [OneTimeTearDown]
        public override void OneTimeTearDown()
        {
            base.OneTimeTearDown();
            
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
