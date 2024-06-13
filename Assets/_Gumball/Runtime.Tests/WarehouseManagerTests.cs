using System.Collections;
using System.Collections.Generic;
using MyBox;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class WarehouseManagerTests : IPrebuildSetup, IPostBuildCleanup
    {
        
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
            DataManager.RemoveAllData();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
        }

        [Test]
        [Order(1)]
        public void NoNullCarsInCatalogue()
        {
            foreach (AssetReferenceGameObject carAsset in WarehouseManager.Instance.AllCars)
            {
                Assert.IsNotNull(carAsset, "Car catalogue cannot contain a null asset.");
            }
        }
        
        [Test]
        [Order(2)]
        public void AllCarsHaveCarComponent()
        {
            for (int index = 0; index < WarehouseManager.Instance.AllCars.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCars[index];
                
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                Assert.IsNotNull(car, $"Asset at index {index} is missing the car component.");
            }
        }
        
        [Test]
        [Order(3)]
        public void AllCarsHaveCameraPositionsAssigned()
        {
            string carsMissingCameraPositions = "";
            for (int index = 0; index < WarehouseManager.Instance.AllCars.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCars[index];
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                if (car == null)
                    continue;

                if (car.CockpitCameraTarget == null
                    || car.RearViewCameraTarget == null)
                {
                    carsMissingCameraPositions += $"\n - {car.name} (index {index})";
                }
            }
            
            Assert.IsTrue(carsMissingCameraPositions.IsNullOrEmpty(), $"Cars missing camera positions: {carsMissingCameraPositions}");
        }
        
    }
}
