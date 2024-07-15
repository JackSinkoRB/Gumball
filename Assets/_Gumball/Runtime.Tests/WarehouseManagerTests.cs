using System.Collections;
using System.Collections.Generic;
using MyBox;
using NUnit.Framework;
using UnityEditor;
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
        public void AllCarsAreVariantOfTemplate()
        {
            GameObject carTemplatePrefab = TestManager.Instance.CarTemplatePrefab;
            Assert.IsNotNull(carTemplatePrefab);
            
            string carsThatArentUsingTemplate = "";
            for (int index = 0; index < WarehouseManager.Instance.AllCars.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCars[index];
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                if (car == null)
                    continue;
                
                PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(car.gameObject);
                GameObject rootPrefab = PrefabUtility.GetCorrespondingObjectFromSource(car.gameObject);

                bool isUsingTemplate = prefabAssetType == PrefabAssetType.Variant && rootPrefab == carTemplatePrefab;
                if (!isUsingTemplate)
                    carsThatArentUsingTemplate += $"\n - {car.name} (index {index})";
            }
            
            Assert.IsTrue(carsThatArentUsingTemplate.IsNullOrEmpty(), $"Cars that aren't using the car template prefab: {carsThatArentUsingTemplate}");
        }
        
        [Test]
        [Order(4)]
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
        
        [Test]
        [Order(5)]
        public void AllCarsHaveSteeringWheelsAssigned()
        {
            string carsMissingSteeringWheelReferences = "";
            for (int index = 0; index < WarehouseManager.Instance.AllCars.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCars[index];
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                if (car == null)
                    continue;

                if (car.SteeringWheel == null)
                    carsMissingSteeringWheelReferences += $"\n - {car.name} (index {index})";
            }
            
            Assert.IsTrue(carsMissingSteeringWheelReferences.IsNullOrEmpty(), $"Cars missing steering wheel references: {carsMissingSteeringWheelReferences}");
        }
        
        [Test]
        [Order(6)]
        public void NoAvatarsExistInCars()
        {
            string carsWithAvatars = "";
            for (int index = 0; index < WarehouseManager.Instance.AllCars.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCars[index];
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                if (car == null)
                    continue;

                List<Avatar> avatarsInCar = car.transform.GetComponentsInAllChildren<Avatar>();
                if (avatarsInCar.Count > 0)
                    carsWithAvatars += $"\n - {car.name} (index {index})";
            }
            
            Assert.IsTrue(carsWithAvatars.IsNullOrEmpty(), $"Cars with avatars: {carsWithAvatars}");
        }
        
    }
}
