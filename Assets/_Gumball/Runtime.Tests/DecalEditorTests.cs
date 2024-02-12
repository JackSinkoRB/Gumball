using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gumball.Runtime.Tests
{
    public class DecalEditorTests : IPrebuildSetup, IPostBuildCleanup
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
            DataManager.EnableTestProviders(true);

            AsyncOperation loadMainScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.DecalEditorScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMainScene.completed += OnSceneLoadComplete;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
            Object.DestroyImmediate(PlayerCarManager.Instance.CurrentCar);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        [TearDown]
        public void TearDown()
        {
            DecalEditor.Instance.EndSession(true);
        }
        
        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(PlayerCarManager.Instance.SpawnCar(
                Vector3.zero, 
                Quaternion.Euler(Vector3.zero), 
                    () => isInitialised = true));
        }
        
        [UnityTest]
        public IEnumerator DecalEditorIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            Assert.IsTrue(DecalEditor.ExistsRuntime);
        }
        
        [UnityTest]
        public IEnumerator SessionStartsWithNoDecals()
        {
            yield return new WaitUntil(() => isInitialised);

            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            Assert.AreEqual(0, DecalEditor.Instance.LiveDecals.Count);
        }
        
        [UnityTest]
        public IEnumerator NoDecalsSavedIfStartingAndEndingSession()
        {
            yield return new WaitUntil(() => isInitialised);

            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            DecalEditor.Instance.EndSession();

            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(PlayerCarManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(0, liveDecalData.Length);
        }
        
        [UnityTest]
        public IEnumerator DecalIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[0];
            DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[0]);
            
            DecalEditor.Instance.EndSession();

            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(PlayerCarManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
        }
        
        [UnityTest]
        public IEnumerator DecalSpriteIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
            
            const int categoryToUse = 1;
            const int textureToUse = 2;
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            
            DecalEditor.Instance.EndSession();

            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(PlayerCarManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(categoryToUse, liveDecalData[0].CategoryIndex);
            Assert.AreEqual(textureToUse, liveDecalData[0].TextureIndex);
        }
        
        [UnityTest]
        public IEnumerator DecalScaleIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            const int categoryToUse = 0;
            const int textureToUse = 0;
            Vector3 scaleToUse = new Vector3(1.1f, 2.2f, 3.3f);
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            liveDecal.SetScale(scaleToUse);
            
            DecalEditor.Instance.EndSession();

            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(PlayerCarManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(scaleToUse, liveDecalData[0].Scale.ToVector3());
        }
        
        [UnityTest]
        public IEnumerator DecalAngleIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            const int categoryToUse = 0;
            const int textureToUse = 0;
            
            const float angleToUse = 30.1f;
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            liveDecal.SetAngle(angleToUse);
            
            DecalEditor.Instance.EndSession();

            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(PlayerCarManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(angleToUse, liveDecalData[0].Angle);
        }
        
        [UnityTest]
        public IEnumerator DecalPositionIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            const int categoryToUse = 0;
            const int textureToUse = 0;
            
            Vector3 positionToUse = new Vector3(4.4f, 5.5f, 6.6f);
            Quaternion rotationToUse = Quaternion.Euler(new Vector3(7.7f, 8.8f, 9.9f));
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            liveDecal.UpdatePosition(positionToUse, Vector3.zero, rotationToUse);
            
            DecalEditor.Instance.EndSession();

            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(PlayerCarManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(positionToUse, liveDecalData[0].LastKnownPosition.ToVector3());
            Assert.AreEqual(rotationToUse.eulerAngles, liveDecalData[0].LastKnownRotationEuler.ToVector3());
        }
        
        [UnityTest]
        public IEnumerator DecalColourIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            const int categoryToUse = 0;
            const int textureToUse = 0;
            
            const int colorIndexToUse = 3;
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            DecalTexture texture = category.DecalTextures[textureToUse];
            
            Assert.IsTrue(texture.CanColour, "The texture must be colourable for this test to work.");

            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, texture);
            liveDecal.SetColorFromIndex(colorIndexToUse);
            
            DecalEditor.Instance.EndSession();

            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(PlayerCarManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(colorIndexToUse, liveDecalData[0].ColorIndex);
        }

        [UnityTest]
        public IEnumerator SavedDecalDataLoadsSuccessfully()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Vector3 positionToUse = new Vector3(50f, 60f, 70f);
            Vector3 scaleToUse = new Vector3(1.2f, 0.2f, 2.7f);
            const float angleToUse = 32.2f;
            const int colorIndexToUse = 6;

            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);

            const int categoryToUse = 0;
            const int textureToUse = 0;
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            DecalTexture texture = category.DecalTextures[textureToUse];
            
            Assert.IsTrue(texture.CanColour, "The texture must be colourable for this test to work.");
            
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            liveDecal.UpdatePosition(positionToUse, Vector3.zero, Quaternion.Euler(Vector3.zero));
            liveDecal.SetScale(scaleToUse);
            liveDecal.SetAngle(angleToUse);
            liveDecal.SetColorFromIndex(colorIndexToUse);
            
            DecalEditor.Instance.EndSession();
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);

            LiveDecal liveDecalAfterLoading = Object.FindObjectOfType<LiveDecal>();
            Assert.AreEqual(positionToUse, liveDecalAfterLoading.transform.position);
            Assert.AreEqual(scaleToUse, liveDecalAfterLoading.Scale);
            Assert.AreEqual(angleToUse, liveDecalAfterLoading.Angle);
            Assert.AreEqual(DecalEditor.Instance.ColorPalette[colorIndexToUse], liveDecalAfterLoading.Color);
        }

        [UnityTest]
        public IEnumerator SendBackwardPriorities()
        {
            yield return new WaitUntil(() => isInitialised);
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            const int categoryToUse = 0;
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal1 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[0]);
            LiveDecal liveDecal2 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[1]);
            LiveDecal liveDecal3 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[2]);
            
            Assert.AreEqual(1, liveDecal1.Priority);
            Assert.AreEqual(2, liveDecal2.Priority);
            Assert.AreEqual(3, liveDecal3.Priority);
            Assert.AreEqual(liveDecal1, DecalEditor.Instance.LiveDecals[0]);
            Assert.AreEqual(liveDecal2, DecalEditor.Instance.LiveDecals[1]);
            Assert.AreEqual(liveDecal3, DecalEditor.Instance.LiveDecals[2]);

            liveDecal3.SendBackwardOrForward(false, liveDecal3.GetOverlappingLiveDecals());
            
            Assert.AreEqual(1, liveDecal1.Priority);
            Assert.AreEqual(2, liveDecal3.Priority);
            Assert.AreEqual(3, liveDecal2.Priority);
            Assert.AreEqual(liveDecal1, DecalEditor.Instance.LiveDecals[0]);
            Assert.AreEqual(liveDecal3, DecalEditor.Instance.LiveDecals[1]);
            Assert.AreEqual(liveDecal2, DecalEditor.Instance.LiveDecals[2]);
        }
        
        [UnityTest]
        public IEnumerator SendForwardPriorities()
        {
            yield return new WaitUntil(() => isInitialised);
            
            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);
            
            const int categoryToUse = 0;
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal1 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[0]);
            LiveDecal liveDecal2 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[1]);
            LiveDecal liveDecal3 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[2]);
            
            Assert.AreEqual(1, liveDecal1.Priority);
            Assert.AreEqual(2, liveDecal2.Priority);
            Assert.AreEqual(3, liveDecal3.Priority);
            Assert.AreEqual(liveDecal1, DecalEditor.Instance.LiveDecals[0]);
            Assert.AreEqual(liveDecal2, DecalEditor.Instance.LiveDecals[1]);
            Assert.AreEqual(liveDecal3, DecalEditor.Instance.LiveDecals[2]);

            liveDecal1.SendBackwardOrForward(true, liveDecal1.GetOverlappingLiveDecals());
            
            Assert.AreEqual(1, liveDecal2.Priority);
            Assert.AreEqual(2, liveDecal1.Priority);
            Assert.AreEqual(3, liveDecal3.Priority);
            Assert.AreEqual(liveDecal2, DecalEditor.Instance.LiveDecals[0]);
            Assert.AreEqual(liveDecal1, DecalEditor.Instance.LiveDecals[1]);
            Assert.AreEqual(liveDecal3, DecalEditor.Instance.LiveDecals[2]);
        }

        [UnityTest]
        public IEnumerator PrioritiesUpdateWithDeleteAndUndo()
        {
            yield return new WaitUntil(() => isInitialised);

            DecalEditor.Instance.StartSession(PlayerCarManager.Instance.CurrentCar);

            const int categoryToUse = 0;
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal1 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[0]);
            LiveDecal liveDecal2 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[1]);
            LiveDecal liveDecal3 = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[2]);

            Assert.AreEqual(1, liveDecal1.Priority);
            Assert.AreEqual(2, liveDecal2.Priority);
            Assert.AreEqual(3, liveDecal3.Priority);
            Assert.AreEqual(liveDecal1, DecalEditor.Instance.LiveDecals[0]);
            Assert.AreEqual(liveDecal2, DecalEditor.Instance.LiveDecals[1]);
            Assert.AreEqual(liveDecal3, DecalEditor.Instance.LiveDecals[2]);
            
            //delete the second decal and check priorities have updated
            DecalStateManager.LogStateChange(new DecalStateManager.DestroyStateChange(liveDecal2));
            DecalEditor.Instance.DisableLiveDecal(liveDecal2);
            
            Assert.IsTrue(DecalEditor.Instance.LiveDecals.Contains(liveDecal1));
            Assert.IsFalse(DecalEditor.Instance.LiveDecals.Contains(liveDecal2));
            Assert.IsTrue(DecalEditor.Instance.LiveDecals.Contains(liveDecal3));
            Assert.AreEqual(2, DecalEditor.Instance.LiveDecals.Count);
            
            Assert.AreEqual(1, liveDecal1.Priority);
            Assert.AreEqual(2, liveDecal3.Priority);
            Assert.AreEqual(liveDecal1, DecalEditor.Instance.LiveDecals[0]);
            Assert.AreEqual(liveDecal3, DecalEditor.Instance.LiveDecals[1]);
            
            DecalStateManager.UndoLatestChange();
            
            //ensure the priorities are the same as before:
            Assert.IsTrue(DecalEditor.Instance.LiveDecals.Contains(liveDecal1));
            Assert.IsTrue(DecalEditor.Instance.LiveDecals.Contains(liveDecal2));
            Assert.IsTrue(DecalEditor.Instance.LiveDecals.Contains(liveDecal3));
            Assert.AreEqual(3, DecalEditor.Instance.LiveDecals.Count);
            
            Assert.AreEqual(1, liveDecal1.Priority);
            Assert.AreEqual(2, liveDecal2.Priority);
            Assert.AreEqual(3, liveDecal3.Priority);
            Assert.AreEqual(liveDecal1, DecalEditor.Instance.LiveDecals[0]);
            Assert.AreEqual(liveDecal2, DecalEditor.Instance.LiveDecals[1]);
            Assert.AreEqual(liveDecal3, DecalEditor.Instance.LiveDecals[2]);
        }

    }
}
