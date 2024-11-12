using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class FuelManagerTests : BaseRuntimeTests
    {

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
            yield return FuelManager.LoadInstanceAsync();
            
            isInitialised = true;
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator StartsWithMaxFuel()
        {
            yield return new WaitUntil(() => isInitialised);

            Assert.AreEqual(FuelManager.Instance.MaxFuel, FuelManager.Instance.CurrentFuel);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator SetFuel()
        {
            yield return new WaitUntil(() => isInitialised);

            FuelManager.Instance.SetFuel(1);
            Assert.AreEqual(1, FuelManager.Instance.CurrentFuel);
            
            FuelManager.Instance.SetFuel(5);
            Assert.AreEqual(5, FuelManager.Instance.CurrentFuel);
            
            FuelManager.Instance.SetFuel(0);
            Assert.AreEqual(0, FuelManager.Instance.CurrentFuel);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator AddFuel()
        {
            yield return new WaitUntil(() => isInitialised);

            FuelManager.Instance.SetFuel(1);
            FuelManager.Instance.AddFuel();
            Assert.AreEqual(2, FuelManager.Instance.CurrentFuel);
            
            FuelManager.Instance.SetFuel(5);
            FuelManager.Instance.AddFuel(2);
            Assert.AreEqual(7, FuelManager.Instance.CurrentFuel);
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator TakeFuel()
        {
            yield return new WaitUntil(() => isInitialised);
            
            FuelManager.Instance.SetFuel(1);
            FuelManager.Instance.TakeFuel();
            Assert.AreEqual(0, FuelManager.Instance.CurrentFuel);
            
            FuelManager.Instance.SetFuel(5);
            FuelManager.Instance.TakeFuel(2);
            Assert.AreEqual(3, FuelManager.Instance.CurrentFuel);
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator HasFuel()
        {
            yield return new WaitUntil(() => isInitialised);

            FuelManager.Instance.SetFuel(1);
            Assert.IsTrue(FuelManager.Instance.HasFuel());
            
            FuelManager.Instance.SetFuel(0);
            Assert.IsFalse(FuelManager.Instance.HasFuel());
            
            FuelManager.Instance.SetFuel(5);
            Assert.IsTrue(FuelManager.Instance.HasFuel());
        }
        
        [UnityTest]
        [Order(6)]
        public IEnumerator FuelRegeneratesAutomaticallySingle()
        {
            yield return new WaitUntil(() => isInitialised);

            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);
            
            //ensure the cycle has been created
            Assert.IsNotNull(FuelManager.Instance.RegenerateCycle);

            FuelManager.Instance.SetFuel(0);
            
            FuelManager.Instance.RegenerateCycle.Restart();
            
            TimeUtils.SetTimeOffset(FuelManager.Instance.TimeBetweenFuelRegenerate.ToSeconds());
            yield return null; //wait for events to trigger

            Assert.AreEqual(1, FuelManager.Instance.CurrentFuel);
        }
        
        [UnityTest]
        [Order(7)]
        public IEnumerator FuelRegeneratesAutomaticallyMultiple()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);
            
            //ensure the cycle has been created
            Assert.IsNotNull(FuelManager.Instance.RegenerateCycle);

            FuelManager.Instance.SetFuel(0);
            
            FuelManager.Instance.RegenerateCycle.Restart();

            int cyclesToComplete = FuelManager.Instance.MaxFuel;
            TimeUtils.SetTimeOffset(FuelManager.Instance.TimeBetweenFuelRegenerate.ToSeconds() * cyclesToComplete);
            yield return null; //wait for events to trigger

            Assert.AreEqual(cyclesToComplete, FuelManager.Instance.CurrentFuel);
        }
        
        [UnityTest]
        [Order(8)]
        public IEnumerator FuelDoesntRegenerateOverMax()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);

            //ensure the cycle has been created
            Assert.IsNotNull(FuelManager.Instance.RegenerateCycle);

            FuelManager.Instance.SetFuel(FuelManager.Instance.MaxFuel - 1);
            
            FuelManager.Instance.RegenerateCycle.Restart();
            
            const int cyclesToComplete = 2;
            TimeUtils.SetTimeOffset(FuelManager.Instance.TimeBetweenFuelRegenerate.ToSeconds() * cyclesToComplete);
            yield return null; //wait for events to trigger

            Assert.AreEqual(FuelManager.Instance.MaxFuel, FuelManager.Instance.CurrentFuel);
        }
        
    }
}
