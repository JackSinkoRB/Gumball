using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class StanceModificationTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private const int carIndexToUse = 1; //test with the 911
        
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
            Assert.AreEqual(WarehouseManager.Instance.CurrentCar.CarIndex, carIndexToUse);

            //ensure all the wheels have the stance modification component
            const int numberOfWheels = 4;
            for (int index = 0; index < numberOfWheels; index++)
            {
                WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[index];
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                Assert.IsNotNull(stanceModification);
            }
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator SuspensionHeight()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.SuspensionHeight.MinValue;
            stanceModification.ApplySuspensionHeight(valueToUse);
            Assert.AreEqual(valueToUse, wheelCollider.suspensionDistance);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, wheelCollider.suspensionDistance);
            
            //check default value works
            float defaultValue = stanceModification.SuspensionHeight.DefaultValue;
            stanceModification.ApplySuspensionHeight(defaultValue);
            Assert.AreEqual(defaultValue, wheelCollider.suspensionDistance);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator Camber()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.Camber.MaxValue;
            stanceModification.ApplyCamber(valueToUse);
            Assert.AreEqual(valueToUse, stanceModification.CurrentCamber);
            Assert.AreEqual(valueToUse.NormaliseAngle(), stanceModification.WheelMesh.transform.rotation.eulerAngles.z.NormaliseAngle(), 0.01f);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, stanceModification.CurrentCamber);
            Assert.AreEqual(valueToUse.NormaliseAngle(), stanceModification.WheelMesh.transform.rotation.eulerAngles.z.NormaliseAngle(), 0.01f);
            
            //check default value works
            float defaultValue = stanceModification.Camber.DefaultValue;
            stanceModification.ApplyCamber(defaultValue);
            Assert.AreEqual(defaultValue, stanceModification.CurrentCamber);
            Assert.AreEqual(defaultValue.NormaliseAngle(), stanceModification.WheelMesh.transform.rotation.eulerAngles.z.NormaliseAngle(), 0.01f);
        }

        [UnityTest]
        [Order(4)]
        public IEnumerator Offset()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.Offset.MinValue;
            stanceModification.ApplyOffset(valueToUse);
            Assert.AreEqual(valueToUse, wheelCollider.transform.localPosition.x);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, wheelCollider.transform.localPosition.x);
            
            //check default value works
            float defaultValue = stanceModification.Offset.DefaultValue;
            stanceModification.ApplyOffset(defaultValue);
            Assert.AreEqual(defaultValue, wheelCollider.transform.localPosition.x);
        }
        
    }
}
