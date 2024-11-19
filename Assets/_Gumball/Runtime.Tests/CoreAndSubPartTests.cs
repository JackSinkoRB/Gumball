using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class CoreAndSubPartTests : BaseRuntimeTests
    {
        
        private const string carGUIDToUse = "b5028991c62dbc64a99eb0b82d8b0745"; //test with the XJ
        
        private bool isInitialised;
        
        protected override string sceneToLoadPath => TestManager.Instance.WarehouseScenePath;

        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            
            DataManager.RemoveAllData();
        }

        protected override void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            base.OnSceneLoadComplete(asyncOperation);
            
            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }
        
        private IEnumerator Initialise()
        {
            //require the part managers to spawn the player car
            yield return CorePartManager.Initialise();
            yield return SubPartManager.Initialise();

            yield return WarehouseManager.Instance.SpawnCar(carGUIDToUse, Vector3.zero, Quaternion.Euler(Vector3.zero),
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
            Assert.AreEqual(WarehouseManager.Instance.CurrentCar.CarGUID, carGUIDToUse);
        }

        [UnityTest]
        [Order(2)]
        public IEnumerator CorePartBecomesAvailableAfterUnlocking()
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
            CorePartManager.InstallPartOnCar(CorePart.PartType.ENGINE, TestManager.Instance.CorePartA, carGUIDToUse);
            
            //check it applied to the part
            Assert.IsTrue(TestManager.Instance.CorePartA.IsAppliedToCar);
            Assert.AreEqual(carGUIDToUse, TestManager.Instance.CorePartA.CarBelongsToGUID);

            //check it applied to the car
            Assert.AreEqual(TestManager.Instance.CorePartA, CorePartManager.GetCorePart(carGUIDToUse, CorePart.PartType.ENGINE));
            
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
            CorePartManager.InstallPartOnCar(CorePart.PartType.ENGINE, TestManager.Instance.CorePartB, carGUIDToUse);
            
            //check it removed from the previous part
            Assert.IsFalse(TestManager.Instance.CorePartA.IsAppliedToCar);
            Assert.AreEqual(null, TestManager.Instance.CorePartA.CarBelongsToGUID);
            
            //check it applied to the part
            Assert.IsTrue(TestManager.Instance.CorePartB.IsAppliedToCar);
            Assert.AreEqual(carGUIDToUse, TestManager.Instance.CorePartB.CarBelongsToGUID);

            //check it applied to the car
            Assert.AreEqual(TestManager.Instance.CorePartB, CorePartManager.GetCorePart(carGUIDToUse, CorePart.PartType.ENGINE));
            
            //ensure it is removed from spare parts
            Assert.AreEqual(1, CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.WHEELS).Count);
            Assert.AreEqual(0, CorePartManager.GetSpareParts(CorePart.PartType.DRIVETRAIN).Count);
            Assert.IsTrue(CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Contains(TestManager.Instance.CorePartA));
            Assert.IsFalse(CorePartManager.GetSpareParts(CorePart.PartType.ENGINE).Contains(TestManager.Instance.CorePartB));
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator SubPartBecomesAvailableAfterUnlocking()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //start with 0 spare parts
            Assert.IsNull(SubPartManager.GetSpareSubPart(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common));
            Assert.AreEqual(0, SubPartManager.GetSpareSubParts(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common).Count);

            //unlock sub parts
            TestManager.Instance.SubPartA.SetUnlocked(true);
            
            //ensure it is available
            Assert.IsNotNull(SubPartManager.GetSpareSubPart(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common));
            Assert.AreEqual(1, SubPartManager.GetSpareSubParts(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common).Count);
        }
        
        [UnityTest]
        [Order(6)]
        public IEnumerator SubPartInstalling()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //apply the part to a core part
            CorePart corePartToApplyTo = TestManager.Instance.CorePartA;
            SubPart subPartToApply = TestManager.Instance.SubPartA;
            SubPartSlot intakeSlot = corePartToApplyTo.SubPartSlots[0];
            
            //should not be applied at the start
            Assert.IsFalse(subPartToApply.IsAppliedToCorePart);

            intakeSlot.InstallSubPart(subPartToApply);
            
            //check it applied to the part
            Assert.IsTrue(subPartToApply.IsAppliedToCorePart);
            Assert.AreEqual(corePartToApplyTo, TestManager.Instance.SubPartA.CorePartBelongsTo);

            //check it applied to the core part
            Assert.AreEqual(TestManager.Instance.SubPartA, intakeSlot.CurrentSubPart);
            
            //ensure it is removed from spare parts
            Assert.IsNull(SubPartManager.GetSpareSubPart(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common));
            Assert.AreEqual(0, SubPartManager.GetSpareSubParts(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common).Count);
        }
        
        [UnityTest]
        [Order(7)]
        public IEnumerator SubPartRemoval()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //apply the part to a core part
            CorePart corePartToApplyTo = TestManager.Instance.CorePartA;
            SubPart subPartToApply = TestManager.Instance.SubPartA;
            SubPartSlot intakeSlot = corePartToApplyTo.SubPartSlots[0];
            
            //should be applied at the start
            Assert.IsTrue(subPartToApply.IsAppliedToCorePart);

            intakeSlot.UninstallSubPart();
            
            //check it applied to the part
            Assert.IsFalse(subPartToApply.IsAppliedToCorePart);

            //check it applied to the core part
            Assert.IsNull(intakeSlot.CurrentSubPart);
            
            //ensure it is added back to spare parts
            Assert.IsNotNull(SubPartManager.GetSpareSubPart(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common));
            Assert.AreEqual(1, SubPartManager.GetSpareSubParts(SubPart.SubPartType.ENGINE_Intake, SubPart.SubPartRarity.Common).Count);
        }
        
    }
}
