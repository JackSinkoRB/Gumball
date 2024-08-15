using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class WheelPaintModificationTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private const int carIndexToUse = 0; //test with the XJ
        
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

            AsyncOperation loadWorkshopScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.WarehouseScenePath, new LoadSceneParameters(LoadSceneMode.Single));
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

            foreach (WheelMesh wheelMesh in WarehouseManager.Instance.CurrentCar.AllWheelMeshes)
            {
                WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                Assert.IsNotNull(paintModification);
            }
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator DefaultPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            foreach (WheelMesh wheelMesh in WarehouseManager.Instance.CurrentCar.AllWheelMeshes)
            {
                WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                MeshRenderer meshToTest = paintModification.ColourableParts[0];

                //load from save, and ensure it is the first swatch preset
                paintModification.LoadFromSave();

                ColourSwatch defaultSwatch = GlobalPaintPresets.Instance.WheelSwatchPresets[paintModification.DefaultSwatchIndex];
                Assert.AreEqual(defaultSwatch.Color, meshToTest.sharedMaterial.GetColor(WheelPaintModification.BaseColorShaderID));
                Assert.AreEqual(defaultSwatch.Specular, meshToTest.sharedMaterial.GetColor(WheelPaintModification.SpecularShaderID));
                Assert.AreEqual(defaultSwatch.Smoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.SmoothnessShaderID));
                Assert.AreEqual(defaultSwatch.ClearCoat, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatShaderID));
                Assert.AreEqual(defaultSwatch.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatSmoothnessShaderID));
            }
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator PresetPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            foreach (WheelMesh wheelMesh in WarehouseManager.Instance.CurrentCar.AllWheelMeshes)
            {
                WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                MeshRenderer meshToTest = paintModification.ColourableParts[0];

                //assign a preset swatch
                ColourSwatch swatchToTest = GlobalPaintPresets.Instance.WheelSwatchPresets[1];
                paintModification.ApplySwatch(swatchToTest);
                Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(WheelPaintModification.BaseColorShaderID));
                Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(WheelPaintModification.SpecularShaderID));
                Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.SmoothnessShaderID));
                Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatShaderID));
                Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatSmoothnessShaderID));

                //check it is recognised as simple
                Assert.AreEqual(WheelPaintModification.PaintMode.SIMPLE, paintModification.CurrentPaintMode);

                //load from save and assert it is the same
                paintModification.LoadFromSave();
                Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(WheelPaintModification.BaseColorShaderID));
                Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(WheelPaintModification.SpecularShaderID));
                Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.SmoothnessShaderID));
                Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatShaderID));
                Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatSmoothnessShaderID));
            }
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator CustomPaintSwatch()
        {
            yield return new WaitUntil(() => isInitialised);

            foreach (WheelMesh wheelMesh in WarehouseManager.Instance.CurrentCar.AllWheelMeshes)
            {
                WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                MeshRenderer meshToTest = paintModification.ColourableParts[0];

                //create a custom swatch
                Color primaryColor = Color.red;
                Color secondaryColor = Color.blue;
                const float smoothness = 0.2f;
                const float clearcoat = 0.1f;

                ColourSwatch swatchToTest = new();
                swatchToTest.SetColor(primaryColor);
                swatchToTest.SetSpecular(secondaryColor);
                swatchToTest.SetSmoothness(smoothness);
                swatchToTest.SetClearcoat(clearcoat);

                paintModification.ApplySwatch(swatchToTest);
                Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(WheelPaintModification.BaseColorShaderID));
                Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(WheelPaintModification.SpecularShaderID));
                Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.SmoothnessShaderID));
                Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatShaderID));
                Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatSmoothnessShaderID));

                //check it is recognised as custom
                Assert.AreEqual(WheelPaintModification.PaintMode.ADVANCED, paintModification.CurrentPaintMode);

                //load from save and assert it is the same
                paintModification.LoadFromSave();
                Assert.AreEqual(swatchToTest.Color, meshToTest.sharedMaterial.GetColor(WheelPaintModification.BaseColorShaderID));
                Assert.AreEqual(swatchToTest.Specular, meshToTest.sharedMaterial.GetColor(WheelPaintModification.SpecularShaderID));
                Assert.AreEqual(swatchToTest.Smoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.SmoothnessShaderID));
                Assert.AreEqual(swatchToTest.ClearCoat, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatShaderID));
                Assert.AreEqual(swatchToTest.ClearCoatSmoothness, meshToTest.sharedMaterial.GetFloat(WheelPaintModification.ClearCoatSmoothnessShaderID));
            }
        }
        
    }
}
