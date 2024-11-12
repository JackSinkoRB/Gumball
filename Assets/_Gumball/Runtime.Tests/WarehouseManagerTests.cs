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
    public class WarehouseManagerTests : BaseRuntimeTests
    {

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            
            DataManager.RemoveAllData();
        }

        [Test]
        [Order(1)]
        public void NoNullCarsInCatalogue()
        {
            foreach (WarehouseCarData carData in WarehouseManager.Instance.AllCarData)
            {
                Assert.IsNotNull(carData.CarPrefabReference, "Car catalogue cannot contain a null asset.");
            }
        }
        
        [Test]
        [Order(2)]
        public void AllCarsHaveCarComponent()
        {
            for (int index = 0; index < WarehouseManager.Instance.AllCarData.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCarData[index].CarPrefabReference;
                
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
            for (int index = 0; index < WarehouseManager.Instance.AllCarData.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCarData[index].CarPrefabReference;
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
            for (int index = 0; index < WarehouseManager.Instance.AllCarData.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCarData[index].CarPrefabReference;
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
            for (int index = 0; index < WarehouseManager.Instance.AllCarData.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCarData[index].CarPrefabReference;
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                if (car == null)
                    continue;

                if (car.SteeringWheel == null)
                    carsMissingSteeringWheelReferences += $"\n - {car.name} (index {index})";
            }
            
            Assert.IsTrue(carsMissingSteeringWheelReferences.IsNullOrEmpty(), $"Cars missing steering wheel references: {carsMissingSteeringWheelReferences}");
        }
        
        //TODO: add back
        // [Test]
        // [Order(5)]
        // public void AllCarsHaveBrakesSetup()
        // {
        //     string carsWithoutBrakesSetup = "";
        //     for (int index = 0; index < WarehouseManager.Instance.AllCarData.Count; index++)
        //     {
        //         AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCarData[index].CarPrefabReference;
        //         AICar car = carAsset.editorAsset.GetComponent<AICar>();
        //         if (car == null)
        //             continue;
        //
        //         if (car.FrontWheelBrakes.IsNullOrEmpty() || car.RearWheelBrakes.IsNullOrEmpty())
        //         {
        //             carsWithoutBrakesSetup += $"\n - {car.name} (index {index})";
        //             continue;
        //         }
        //
        //         foreach (Transform brake in car.FrontWheelBrakes)
        //         {
        //             if (brake.childCount == 0)
        //                 carsWithoutBrakesSetup += $"\n - {car.name} (index {index}) ({brake.name})";
        //         }
        //         
        //         foreach (Transform brake in car.RearWheelBrakes)
        //         {
        //             if (brake.childCount == 0)
        //                 carsWithoutBrakesSetup += $"\n - {car.name} (index {index}) ({brake.name})";
        //         }
        //     }
        //     
        //     Assert.IsTrue(carsWithoutBrakesSetup.IsNullOrEmpty(), $"Cars without brakes/calipers setup: {carsWithoutBrakesSetup}");
        // }
        
        [Test]
        [Order(6)]
        public void NoAvatarsExistInCars()
        {
            string carsWithAvatars = "";
            for (int index = 0; index < WarehouseManager.Instance.AllCarData.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCarData[index].CarPrefabReference;
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                if (car == null)
                    continue;

                List<Avatar> avatarsInCar = car.transform.GetComponentsInAllChildren<Avatar>();
                if (avatarsInCar.Count > 0)
                    carsWithAvatars += $"\n - {car.name} (index {index})";
            }
            
            Assert.IsTrue(carsWithAvatars.IsNullOrEmpty(), $"Cars with avatars: {carsWithAvatars}");
        }
        
        [Test]
        [Order(7)]
        public void PaintableMeshesAreReadable()
        {
            string invalidPaintableMeshes = "";
            for (int index = 0; index < WarehouseManager.Instance.AllCarData.Count; index++)
            {
                AssetReferenceGameObject carAsset = WarehouseManager.Instance.AllCarData[index].CarPrefabReference;
                AICar car = carAsset.editorAsset.GetComponent<AICar>();
                if (car == null)
                    continue;

                foreach (PaintableMesh paintableMesh in car.transform.GetComponentsInAllChildren<PaintableMesh>())
                {
                    if (paintableMesh.MeshFilter.sharedMesh == null || !paintableMesh.MeshFilter.sharedMesh.isReadable)
                        invalidPaintableMeshes += $"\n - {car.name}-{paintableMesh.gameObject.name} (index {index})";
                }
            }

            Assert.IsTrue(invalidPaintableMeshes.IsNullOrEmpty(), $"PaintableMesh with missing mesh or non-readable: {invalidPaintableMeshes}");
        }
        
    }
}
