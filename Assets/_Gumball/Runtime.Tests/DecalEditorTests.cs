using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
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
            if (WarehouseManager.Instance.CurrentCar != null)
                Object.DestroyImmediate(WarehouseManager.Instance.CurrentCar.gameObject);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            yield return DecalEditor.Instance.EndSession();
        }
        
        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(WarehouseManager.Instance.SpawnCar(0, 0, 
                Vector3.zero, 
                Quaternion.Euler(Vector3.zero), 
                    (car) =>
                    {
                        WarehouseManager.Instance.SetCurrentCar(car);
                        isInitialised = true;
                    }));
        }
        
        [UnityTest]
        [Order(1)]
        public IEnumerator DecalEditorIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            Assert.IsTrue(DecalEditor.ExistsRuntime);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator SessionStartsWithNoDecals()
        {
            yield return new WaitUntil(() => isInitialised);
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            Assert.AreEqual(0, DecalEditor.Instance.LiveDecals.Count);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator DecalPositionIsRelativeToCar()
        {
            yield return new WaitUntil(() => isInitialised);
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            //move the car from origin
            Vector3 carPosition = new Vector3(5, 1, 2);
            Vector3 carRotationEuler = new Vector3(20, 50, 90);
            WarehouseManager.Instance.CurrentCar.Teleport(carPosition, Quaternion.Euler(carRotationEuler));
            
            //create a live decal at a certain position
            Vector3 decalPositionOffset = new Vector3(1, 1, 1);
            Vector3 decalRotationOffsetEuler = new Vector3(0, 45, 0);
            LiveDecal newLiveDecal = DecalEditor.Instance.CreateLiveDecalFromData(new LiveDecal.LiveDecalData(0, 0, 1, decalPositionOffset, decalRotationOffsetEuler, Vector3.zero, Vector3.one, 0, 0));
        
            Vector3 desiredPosition = DecalEditor.Instance.CurrentCar.transform.TransformPoint(decalPositionOffset);
            Assert.IsTrue(desiredPosition.Approximately(newLiveDecal.transform.position), $"Expected {desiredPosition} but got {newLiveDecal.transform.position}");
        
            Vector3 desiredRotation = (DecalEditor.Instance.CurrentCar.transform.rotation * Quaternion.Euler(decalRotationOffsetEuler)).eulerAngles;
            Assert.IsTrue(desiredRotation.Approximately(newLiveDecal.transform.rotation.eulerAngles), $"Expected {desiredRotation} but got {newLiveDecal.transform.rotation.eulerAngles}");
            
            //try to restart session with car respawned
            yield return DecalEditor.Instance.EndSession();
            
            Object.Destroy(WarehouseManager.Instance.CurrentCar.gameObject);
            Vector3 newCarPosition = new Vector3(-3, 3, -1);
            Vector3 newCarRotationEuler = new Vector3(5, 180, 20);
            yield return WarehouseManager.Instance.SpawnCar(0, 0, 
                newCarPosition, 
                Quaternion.Euler(newCarRotationEuler), 
                (car) => WarehouseManager.Instance.SetCurrentCar(car));
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            Assert.AreEqual(1, DecalEditor.Instance.LiveDecals.Count);
            
            LiveDecal liveDecal = DecalEditor.Instance.LiveDecals[0];
            Vector3 newDesiredPosition = DecalEditor.Instance.CurrentCar.transform.TransformPoint(decalPositionOffset);
            Assert.IsTrue(newDesiredPosition.Approximately(liveDecal.transform.position), $"Expected {newDesiredPosition} but got {liveDecal.transform.position}");
        
            Vector3 newDesiredRotation = (DecalEditor.Instance.CurrentCar.transform.rotation * Quaternion.Euler(decalRotationOffsetEuler)).eulerAngles;
            Assert.IsTrue(newDesiredRotation.Approximately(liveDecal.transform.rotation.eulerAngles), $"Expected {newDesiredRotation} but got {liveDecal.transform.rotation.eulerAngles}");
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator NoDecalsSavedIfStartingAndEndingSession()
        {
            yield return new WaitUntil(() => isInitialised);
        
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            yield return DecalEditor.Instance.EndSession();
        
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(WarehouseManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(0, liveDecalData.Length);
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator DecalIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
        
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[0];
            DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[0]);
            
            yield return DecalEditor.Instance.EndSession();
        
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(WarehouseManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
        }
        
        [UnityTest]
        [Order(6)]
        public IEnumerator DecalSpriteIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
            
            const int categoryToUse = 1;
            const int textureToUse = 2;
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            
            yield return DecalEditor.Instance.EndSession();
        
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(WarehouseManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(categoryToUse, liveDecalData[0].CategoryIndex);
            Assert.AreEqual(textureToUse, liveDecalData[0].TextureIndex);
        }
        
        [UnityTest]
        [Order(7)]
        public IEnumerator DecalScaleIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
        
            const int categoryToUse = 0;
            const int textureToUse = 0;
            Vector3 scaleToUse = new Vector3(1.1f, 2.2f, 3.3f);
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            liveDecal.SetScale(scaleToUse);
            
            yield return DecalEditor.Instance.EndSession();
        
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(WarehouseManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(scaleToUse, liveDecalData[0].Scale.ToVector3());
        }
        
        [UnityTest]
        [Order(8)]
        public IEnumerator DecalAngleIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
        
            const int categoryToUse = 0;
            const int textureToUse = 0;
            
            const float angleToUse = 30.1f;
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            liveDecal.SetAngle(angleToUse);
            
            yield return DecalEditor.Instance.EndSession();
        
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(WarehouseManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(angleToUse, liveDecalData[0].Angle);
        }
        
        [UnityTest]
        [Order(9)]
        public IEnumerator DecalPositionIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
        
            const int categoryToUse = 0;
            const int textureToUse = 0;
            
            Vector3 positionToUse = new Vector3(4.4f, 5.5f, 6.6f);
            Quaternion rotationToUse = Quaternion.Euler(new Vector3(7.7f, 8.8f, 9.9f));
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, category.DecalTextures[textureToUse]);
            liveDecal.UpdatePosition(positionToUse, Vector3.zero, rotationToUse);
            
            yield return DecalEditor.Instance.EndSession();
        
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(WarehouseManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(positionToUse, liveDecalData[0].LocalPositionToCar.ToVector3());
            Assert.AreEqual(rotationToUse.eulerAngles, liveDecalData[0].LocalRotationToCar.ToVector3());
        }
        
        [UnityTest]
        [Order(10)]
        public IEnumerator DecalColourIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
        
            const int categoryToUse = 0;
            const int textureToUse = 0;
            
            const int colorIndexToUse = 3;
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
            //create a random decal
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryToUse];
            DecalTexture texture = category.DecalTextures[textureToUse];
            
            Assert.IsTrue(texture.CanColour, "The texture must be colourable for this test to work.");
        
            LiveDecal liveDecal = DecalEditor.Instance.CreateLiveDecal(category, texture);
            liveDecal.SetColorFromIndex(colorIndexToUse);
            
            yield return DecalEditor.Instance.EndSession();
        
            LiveDecal.LiveDecalData[] liveDecalData = DataManager.Cars.Get(DecalManager.GetDecalsSaveKey(WarehouseManager.Instance.CurrentCar), Array.Empty<LiveDecal.LiveDecalData>());
            Assert.AreEqual(1, liveDecalData.Length);
            Assert.AreEqual(colorIndexToUse, liveDecalData[0].ColorIndex);
        }
        
        [UnityTest]
        [Order(11)]
        public IEnumerator SavedDecalDataLoadsSuccessfully()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Vector3 positionToUse = new Vector3(50f, 60f, 70f);
            Vector3 scaleToUse = new Vector3(1.2f, 0.2f, 2.7f);
            const float angleToUse = 32.2f;
            const int colorIndexToUse = 6;
        
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
        
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
            
            yield return DecalEditor.Instance.EndSession();
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
        
            LiveDecal liveDecalAfterLoading = Object.FindObjectOfType<LiveDecal>();
            Assert.AreEqual(positionToUse, liveDecalAfterLoading.transform.localPosition);
            Assert.AreEqual(scaleToUse, liveDecalAfterLoading.Scale);
            Assert.AreEqual(angleToUse, liveDecalAfterLoading.Angle);
            Assert.AreEqual(DecalEditor.Instance.ColorPalette[colorIndexToUse], liveDecalAfterLoading.Color);
        }
        
        [UnityTest]
        [Order(12)]
        public IEnumerator SendBackwardPriorities()
        {
            yield return new WaitUntil(() => isInitialised);
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
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
        [Order(13)]
        public IEnumerator SendForwardPriorities()
        {
            yield return new WaitUntil(() => isInitialised);
            
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
            
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
        [Order(14)]
        public IEnumerator PrioritiesUpdateWithDeleteAndUndo()
        {
            yield return new WaitUntil(() => isInitialised);
        
            yield return DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
        
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
