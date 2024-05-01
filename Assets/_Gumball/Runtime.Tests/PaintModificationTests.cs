using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class PaintModificationTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private const int carIndexToUse = 0; //test with the XJ
        
        private bool isInitialised;

        private PaintModification paintModification => WarehouseManager.Instance.CurrentCar.PaintModification;
        
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
            CoroutineHelper.Instance.StartCoroutine(WarehouseManager.Instance.SpawnCar(carIndexToUse, 0, 
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
        public IEnumerator CarIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsNotNull(WarehouseManager.Instance.CurrentCar);
            Assert.IsNotNull(paintModification);
            Assert.AreEqual(WarehouseManager.Instance.CurrentCar.CarIndex, carIndexToUse);
        }

        [UnityTest]
        [Order(2)]
        public IEnumerator DefaultPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            MeshRenderer meshToTest = paintModification.ColourableBodyParts[0];
            
            //load from save, and ensure it is the first swatch preset
            paintModification.LoadFromSave();

            ColourSwatch defaultSwatch = paintModification.SwatchPresets[0];
            Assert.AreEqual(defaultSwatch.Color, meshToTest.sharedMaterial.GetColor(PaintModification.BaseColorShaderID));
            Assert.AreEqual(defaultSwatch.Emission, meshToTest.sharedMaterial.GetColor(PaintModification.EmissionShaderID));
            Assert.AreEqual(defaultSwatch.Metallic, meshToTest.sharedMaterial.GetFloat(PaintModification.MetallicShaderID));
            Assert.AreEqual(defaultSwatch.Smoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.SmoothnessShaderID));
            Assert.AreEqual(defaultSwatch.ClearCoat, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatShaderID));
            Assert.AreEqual(defaultSwatch.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatSmoothnessShaderID));
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator PresetPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            MeshRenderer meshToTest = paintModification.ColourableBodyParts[0];
            
            //assign a preset swatch
            ColourSwatch swatchToTest = paintModification.SwatchPresets[1];
            paintModification.ApplySwatch(swatchToTest);
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(PaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Emission, meshToTest.sharedMaterial.GetColor(PaintModification.EmissionShaderID));
            Assert.AreEqual(swatchToTest.Metallic, meshToTest.sharedMaterial.GetFloat(PaintModification.MetallicShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatSmoothnessShaderID));
            
            //check it is recognised as simple
            Assert.AreEqual(PaintModification.PaintMode.SIMPLE, paintModification.CurrentBodyPaintMode);
            
            //load from save and assert it is the same
            paintModification.LoadFromSave();
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(PaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Emission, meshToTest.sharedMaterial.GetColor(PaintModification.EmissionShaderID));
            Assert.AreEqual(swatchToTest.Metallic, meshToTest.sharedMaterial.GetFloat(PaintModification.MetallicShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatSmoothnessShaderID));
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator CustomPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            MeshRenderer meshToTest = paintModification.ColourableBodyParts[0];
            
            //create a custom swatch
            Color primaryColor = Color.red;
            Color secondaryColor = Color.blue;
            const float metallic = 0.5f;
            const float smoothness = 0.2f;
            const float clearcoat = 0.1f;
            
            ColourSwatch swatchToTest = new();
            swatchToTest.SetColor(primaryColor);
            swatchToTest.SetEmission(secondaryColor);
            swatchToTest.SetMetallic(metallic);
            swatchToTest.SetSmoothness(smoothness);
            swatchToTest.SetClearcoat(clearcoat);
            
            paintModification.ApplySwatch(swatchToTest);
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(PaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Emission, meshToTest.sharedMaterial.GetColor(PaintModification.EmissionShaderID));
            Assert.AreEqual(swatchToTest.Metallic, meshToTest.sharedMaterial.GetFloat(PaintModification.MetallicShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatSmoothnessShaderID));
            
            //check it is recognised as custom
            Assert.AreEqual(PaintModification.PaintMode.ADVANCED, paintModification.CurrentBodyPaintMode);
            
            //load from save and assert it is the same
            paintModification.LoadFromSave();
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(PaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Emission, meshToTest.sharedMaterial.GetColor(PaintModification.EmissionShaderID));
            Assert.AreEqual(swatchToTest.Metallic, meshToTest.sharedMaterial.GetFloat(PaintModification.MetallicShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(PaintModification.ClearCoatSmoothnessShaderID));
        }
        
    }
}
