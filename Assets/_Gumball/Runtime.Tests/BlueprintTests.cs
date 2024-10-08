using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class BlueprintTests : BaseRuntimeTests
    {

        private const int carIndex = 0;
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
            
            Assert.AreEqual(0, BlueprintManager.Instance.GetBlueprints(carIndex));
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator SetBlueprints()
        {
            yield return new WaitUntil(() => isInitialised);
            
            const int amount = 10;
            BlueprintManager.Instance.SetBlueprints(carIndex, amount);
            
            Assert.AreEqual(amount, BlueprintManager.Instance.GetBlueprints(carIndex));
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator AddBlueprints()
        {
            yield return new WaitUntil(() => isInitialised);
            
            const int before = 5;
            BlueprintManager.Instance.SetBlueprints(carIndex, before);
            
            const int amountToAdd = 4;
            BlueprintManager.Instance.AddBlueprints(carIndex, amountToAdd);
            
            Assert.AreEqual(before + amountToAdd, BlueprintManager.Instance.GetBlueprints(carIndex));
        }

    }
}
