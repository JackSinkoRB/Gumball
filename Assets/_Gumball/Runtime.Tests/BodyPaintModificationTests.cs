using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class BodyPaintModificationTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private const int carIndexToUse = 0; //test with the XJ
        
        private bool isInitialised;

        private BodyPaintModification BodyPaintModification => WarehouseManager.Instance.CurrentCar.BodyPaintModification; //test with body paint
        
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
            Assert.IsNotNull(BodyPaintModification);
            Assert.AreEqual(WarehouseManager.Instance.CurrentCar.CarIndex, carIndexToUse);
        }

        [UnityTest]
        [Order(2)]
        public IEnumerator DefaultPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            MeshRenderer meshToTest = BodyPaintModification.ColourableParts[0];
            
            //load from save, and ensure it is the first swatch preset
            BodyPaintModification.LoadFromSave();

            ColourSwatch defaultSwatch = GlobalPaintPresets.Instance.BodySwatchPresets[0];
            Assert.AreEqual(defaultSwatch.Color, meshToTest.sharedMaterial.GetColor(BodyPaintModification.BaseColorShaderID));
            Assert.AreEqual(defaultSwatch.Specular, meshToTest.sharedMaterial.GetColor(BodyPaintModification.SpecularShaderID));
            Assert.AreEqual(defaultSwatch.Smoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.SmoothnessShaderID));
            Assert.AreEqual(defaultSwatch.ClearCoat, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatShaderID));
            Assert.AreEqual(defaultSwatch.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatSmoothnessShaderID));
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator PresetPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            MeshRenderer meshToTest = BodyPaintModification.ColourableParts[0];
            
            //assign a preset swatch
            ColourSwatch swatchToTest = GlobalPaintPresets.Instance.BodySwatchPresets[1];
            BodyPaintModification.ApplySwatch(swatchToTest);
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(BodyPaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(BodyPaintModification.SpecularShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatSmoothnessShaderID));
            
            //check it is recognised as simple
            Assert.AreEqual(BodyPaintModification.PaintMode.SIMPLE, BodyPaintModification.CurrentPaintMode);
            
            //load from save and assert it is the same
            BodyPaintModification.LoadFromSave();
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(BodyPaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(BodyPaintModification.SpecularShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatSmoothnessShaderID));
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator CustomPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            MeshRenderer meshToTest = BodyPaintModification.ColourableParts[0];
            
            //create a custom swatch
            Color primaryColor = Color.red;
            Color secondaryColor = Color.blue;
            const float metallic = 0.5f;
            const float smoothness = 0.2f;
            const float clearcoat = 0.1f;
            
            ColourSwatch swatchToTest = new();
            swatchToTest.SetColor(primaryColor);
            swatchToTest.SetSpecular(secondaryColor);
            swatchToTest.SetSmoothness(smoothness);
            swatchToTest.SetClearcoat(clearcoat);
            
            BodyPaintModification.ApplySwatch(swatchToTest);
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(BodyPaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(BodyPaintModification.SpecularShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatSmoothnessShaderID));
            
            //check it is recognised as custom
            Assert.AreEqual(BodyPaintModification.PaintMode.ADVANCED, BodyPaintModification.CurrentPaintMode);
            
            //load from save and assert it is the same
            BodyPaintModification.LoadFromSave();
            Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(BodyPaintModification.BaseColorShaderID));
            Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(BodyPaintModification.SpecularShaderID));
            Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.SmoothnessShaderID));
            Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatShaderID));
            Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(BodyPaintModification.ClearCoatSmoothnessShaderID));
        }
        
    }
}
