using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Gumball.Runtime.Tests
{
    public class AvatarEditorTests : IPrebuildSetup, IPostBuildCleanup
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

            AsyncOperation loadMainScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.AvatarEditorScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMainScene.completed += OnSceneLoadComplete;
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
        
        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(SpawnAvatars());
        }

        private IEnumerator SpawnAvatars()
        {
            TrackedCoroutine driverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnDriver(Vector3.zero, Quaternion.Euler(Vector3.zero)));
            TrackedCoroutine coDriverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnCoDriver(Vector3.zero, Quaternion.Euler(Vector3.zero)));
            
            yield return new WaitUntil(() => !driverAvatarLoadCoroutine.IsPlaying && !coDriverAvatarLoadCoroutine.IsPlaying);
            
            isInitialised = true;
        }
        
        [UnityTest]
        public IEnumerator AvatarEditorIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            Assert.IsTrue(AvatarEditor.ExistsRuntime);
        }
    }
}
