using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class CarPartTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private const int carIndexToUse = 1; //test with the 911
        
        private bool isInitialised;

        public void Setup()
        {
            BootSceneClear.TrySetup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = true;
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = false;
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DecalEditor.IsRunningTests = true;
            DataManager.EnableTestProviders(true);

            AsyncOperation loadWorkshopScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.WorkshopScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadWorkshopScene.completed += OnSceneLoadComplete;
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

        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }
        
        private IEnumerator Initialise()
        {
            //require the part managers to spawn the player car
            yield return CorePartManager.Initialise();
            yield return SubPartManager.Initialise();

            yield return WarehouseManager.Instance.SpawnCar(carIndexToUse, Vector3.zero, Quaternion.Euler(Vector3.zero),
                (carInstance) =>
                {
                    WarehouseManager.Instance.SetCurrentCar(carInstance);
                    isInitialised = true;
                });
        }
        
        [UnityTest]
        [Order(1)]
        public IEnumerator CarIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsNotNull(WarehouseManager.Instance.CurrentCar);
            Assert.AreEqual(WarehouseManager.Instance.CurrentCar.CarIndex, carIndexToUse);
            Assert.IsNotNull(WarehouseManager.Instance.CurrentCar.CarPartManager);
            Assert.Greater(WarehouseManager.Instance.CurrentCar.CarPartManager.CarPartGroups.Length, 0);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator CarPartSwitching()
        {
            yield return new WaitUntil(() => isInitialised);

            CarPartGroup partGroupToTest = WarehouseManager.Instance.CurrentCar.CarPartManager.CarPartGroups[0];
            
            Assert.AreEqual(partGroupToTest.SavedPartIndex, 0);
            Assert.AreEqual(partGroupToTest.CurrentPartIndex, 0);

            const int indexToUse = 1;
            partGroupToTest.SetPartActive(indexToUse);
            
            //ensure it applied
            Assert.AreEqual(partGroupToTest.CurrentPartIndex, indexToUse);
            
            //ensure it is the only part active
            for (int index = 0; index < partGroupToTest.CarParts.Length; index++)
            {
                CarPart part = partGroupToTest.CarParts[index];
                Assert.AreEqual(index == indexToUse, part.gameObject.activeSelf);
            }

            //ensure it saved
            Assert.AreEqual(partGroupToTest.SavedPartIndex, indexToUse);
            
            //set back to 0:
            partGroupToTest.SetPartActive(0);
            
            //ensure it applied
            Assert.AreEqual(partGroupToTest.CurrentPartIndex, 0);
            
            //ensure it is the only part active
            for (int index = 0; index < partGroupToTest.CarParts.Length; index++)
            {
                CarPart part = partGroupToTest.CarParts[index];
                Assert.AreEqual(index == 0, part.gameObject.activeSelf);
            }

            //ensure it saved
            Assert.AreEqual(partGroupToTest.SavedPartIndex, 0);
        }

    }
}
