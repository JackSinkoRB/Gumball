using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class BlueprintTests : BaseRuntimeTests
    {

        private const string carGUIDToUse = "0860d6f6-93bc-4cde-941e-538b44020cbd";
        private bool isInitialised;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            
            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }
        
        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        [TearDown]
        public void TearDown()
        {
            TimeUtils.SetTimeOffset(0);
        }

        private IEnumerator Initialise()
        {
            yield return BlueprintManager.LoadInstanceAsync();
            yield return WarehouseManager.LoadInstanceAsync();
            
            isInitialised = true;
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator StartWithNoBlueprints()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(0, BlueprintManager.Instance.GetBlueprints(carGUIDToUse));
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator SetBlueprints()
        {
            yield return new WaitUntil(() => isInitialised);
            
            const int amount = 10;
            BlueprintManager.Instance.SetBlueprints(carGUIDToUse, amount);
            
            Assert.AreEqual(amount, BlueprintManager.Instance.GetBlueprints(carGUIDToUse));
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator AddBlueprints()
        {
            yield return new WaitUntil(() => isInitialised);
            
            const int before = 5;
            BlueprintManager.Instance.SetBlueprints(carGUIDToUse, before);
            
            const int amountToAdd = 4;
            BlueprintManager.Instance.AddBlueprints(carGUIDToUse, amountToAdd);
            
            Assert.AreEqual(before + amountToAdd, BlueprintManager.Instance.GetBlueprints(carGUIDToUse));
        }

    }
}
