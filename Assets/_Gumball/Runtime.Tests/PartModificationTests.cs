using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class PartModificationTests : IPrebuildSetup, IPostBuildCleanup
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
            DataManager.RemoveAllData();

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

        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(WarehouseManager.Instance.SpawnCar(carIndexToUse, 
                Vector3.zero, 
                Quaternion.Euler(Vector3.zero), 
                (car) =>
                {
                    WarehouseManager.Instance.SetCurrentCar(car);
                    CoroutineHelper.Instance.StartCoroutine(Initialise());
                }));
        }

        private IEnumerator Initialise()
        {
            //load the core part manager
            yield return CorePartManager.Initialise();
            isInitialised = true;
        }
        
        [UnityTest]
        [Order(1)]
        public IEnumerator CarIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsNotNull(WarehouseManager.Instance.CurrentCar);
            Assert.IsNotNull(WarehouseManager.Instance.CurrentCar.PartModification);
            Assert.AreEqual(WarehouseManager.Instance.CurrentCar.CarIndex, carIndexToUse);
        }

        [UnityTest]
        [Order(2)]
        public IEnumerator CorePartUnlocking()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //start with 0 spare parts
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.WHEELS).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.DRIVETRAIN).Count);
            
            //unlock engine parts
            TestManager.Instance.CorePartA.SetUnlocked(true);
            TestManager.Instance.CorePartB.SetUnlocked(true);
            Assert.AreEqual(2, CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.WHEELS).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.DRIVETRAIN).Count);
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator CorePartInstalling()
        {
            yield return new WaitUntil(() => isInitialised);

            Assert.IsFalse(TestManager.Instance.CorePartA.IsAppliedToCar);

            //apply the part to the car
            CorePartManager.InstallPartOnCar(CorePart.PartType.ENGINE, TestManager.Instance.CorePartA, carIndexToUse);
            
            //check it applied to the part
            Assert.IsTrue(TestManager.Instance.CorePartA.IsAppliedToCar);
            Assert.AreEqual(carIndexToUse, TestManager.Instance.CorePartA.CarBelongsToIndex);

            //check it applied to the car
            Assert.AreEqual(TestManager.Instance.CorePartA, PartModification.GetCorePart(carIndexToUse, CorePart.PartType.ENGINE));
            
            //ensure it is removed from spare parts
            Assert.AreEqual(1, CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.WHEELS).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.DRIVETRAIN).Count);
            Assert.IsFalse(CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Contains(TestManager.Instance.CorePartA));
            Assert.IsTrue(CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Contains(TestManager.Instance.CorePartB));
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator CorePartSwitching()
        {
            yield return new WaitUntil(() => isInitialised);

            Assert.IsTrue(TestManager.Instance.CorePartA.IsAppliedToCar);

            //apply the part to the car
            CorePartManager.InstallPartOnCar(CorePart.PartType.ENGINE, TestManager.Instance.CorePartB, carIndexToUse);
            
            //check it removed from the previous part
            Assert.IsFalse(TestManager.Instance.CorePartA.IsAppliedToCar);
            Assert.AreEqual(-1, TestManager.Instance.CorePartA.CarBelongsToIndex);
            
            //check it applied to the part
            Assert.IsTrue(TestManager.Instance.CorePartB.IsAppliedToCar);
            Assert.AreEqual(carIndexToUse, TestManager.Instance.CorePartB.CarBelongsToIndex);

            //check it applied to the car
            Assert.AreEqual(TestManager.Instance.CorePartB, PartModification.GetCorePart(carIndexToUse, CorePart.PartType.ENGINE));
            
            //ensure it is removed from spare parts
            Assert.AreEqual(1, CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.WHEELS).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.DRIVETRAIN).Count);
            Assert.IsTrue(CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Contains(TestManager.Instance.CorePartA));
            Assert.IsFalse(CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Contains(TestManager.Instance.CorePartB));
        }

        [UnityTest]
        [Order(5)]
        public IEnumerator PeakTorqueModifier()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //part B should be applied to the car
            Assert.AreEqual(TestManager.Instance.CorePartB, PartModification.GetCorePart(carIndexToUse, CorePart.PartType.ENGINE));
            
            //ensure default peak torque has been cached/initialised
            float defaultPeakTorque = WarehouseManager.Instance.CurrentCar.DefaultPeakTorque;
            Assert.Greater(defaultPeakTorque, 0);

            //core part B is applied - gives 10 peak torque
            float additionalPeakTorque = WarehouseManager.Instance.CurrentCar.PartModification.GetTotalPeakTorqueModifiers();
            Assert.AreEqual(10, additionalPeakTorque);

            //ensure it is actually applied to the torque curve
            float currentPeakTorque = 0;
            foreach (Keyframe key in WarehouseManager.Instance.CurrentCar.TorqueCurve.keys)
            {
                float torque = key.value;
                if (torque > currentPeakTorque)
                    currentPeakTorque = torque;
            }
            Assert.AreEqual(defaultPeakTorque + additionalPeakTorque, currentPeakTorque);
        }

        [UnityTest]
        [Order(6)]
        public IEnumerator PeakTorqueModifierAfterPartSwitch()
        {
            yield return new WaitUntil(() => isInitialised);
            
            float defaultPeakTorque = WarehouseManager.Instance.CurrentCar.DefaultPeakTorque;
            
            //switch to part A
            CorePartManager.InstallPartOnCar(CorePart.PartType.ENGINE, TestManager.Instance.CorePartA, carIndexToUse);
            
            //ensure new torque has been applied (part A has 15)
            float additionalPeakTorque = WarehouseManager.Instance.CurrentCar.PartModification.GetTotalPeakTorqueModifiers();
            Assert.AreEqual(15, additionalPeakTorque);
            
            //ensure it is actually applied to the torque curve
            float currentPeakTorque = 0;
            foreach (Keyframe key in WarehouseManager.Instance.CurrentCar.TorqueCurve.keys)
            {
                float torque = key.value;
                if (torque > currentPeakTorque)
                    currentPeakTorque = torque;
            }
            Assert.AreEqual(defaultPeakTorque + additionalPeakTorque, currentPeakTorque);
        }

    }
}
