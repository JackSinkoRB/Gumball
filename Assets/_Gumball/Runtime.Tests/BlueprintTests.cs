using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class BlueprintTests : IPrebuildSetup, IPostBuildCleanup
    {

        private const int carIndex = 0;
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
            PersistentCooldown.IsRunningTests = true;
            DecalEditor.IsRunningTests = true;
            DataManager.EnableTestProviders(true);

            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            PersistentCooldown.IsRunningTests = false;
            DataManager.EnableTestProviders(false);
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
        
        [UnityTest]
        [Order(4)]
        public IEnumerator GetLevel()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //start with no level
            Assert.AreEqual(-1, BlueprintManager.Instance.GetLevelIndex(carIndex));
            
            const int blueprints1 = 10;
            BlueprintManager.Instance.SetBlueprints(carIndex, blueprints1);
            Assert.AreEqual(0, BlueprintManager.Instance.GetLevelIndex(carIndex));

            const int blueprints2 = 35;
            BlueprintManager.Instance.SetBlueprints(carIndex, blueprints2);
            Assert.AreEqual(2, BlueprintManager.Instance.GetLevelIndex(carIndex));

            //max level
            const int blueprints3 = int.MaxValue;
            BlueprintManager.Instance.SetBlueprints(carIndex, blueprints3);
            Assert.AreEqual(BlueprintManager.Instance.BlueprintsRequiredForEachLevel.Count - 1, BlueprintManager.Instance.GetLevelIndex(carIndex));
        }

    }
}
