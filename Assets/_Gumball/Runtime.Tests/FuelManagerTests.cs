using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class FuelManagerTests : IPrebuildSetup, IPostBuildCleanup
    {

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
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
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

        [Test]
        [Order(1)]
        public void StartsWithMaxFuel()
        {
            Assert.AreEqual(FuelManager.MaxFuel, FuelManager.CurrentFuel);
        }
        
        [Test]
        [Order(2)]
        public void SetFuel()
        {
            FuelManager.SetFuel(1);
            Assert.AreEqual(1, FuelManager.CurrentFuel);
            
            FuelManager.SetFuel(5);
            Assert.AreEqual(5, FuelManager.CurrentFuel);
            
            FuelManager.SetFuel(0);
            Assert.AreEqual(0, FuelManager.CurrentFuel);
        }
        
        [Test]
        [Order(3)]
        public void AddFuel()
        {
            FuelManager.SetFuel(1);
            FuelManager.AddFuel();
            Assert.AreEqual(2, FuelManager.CurrentFuel);
            
            FuelManager.SetFuel(5);
            FuelManager.AddFuel(2);
            Assert.AreEqual(7, FuelManager.CurrentFuel);
        }
        
        [Test]
        [Order(4)]
        public void TakeFuel()
        {
            FuelManager.SetFuel(1);
            FuelManager.TakeFuel();
            Assert.AreEqual(0, FuelManager.CurrentFuel);
            
            FuelManager.SetFuel(5);
            FuelManager.TakeFuel(2);
            Assert.AreEqual(3, FuelManager.CurrentFuel);
        }
        
        [Test]
        [Order(5)]
        public void HasFuel()
        {
            FuelManager.SetFuel(1);
            Assert.IsTrue(FuelManager.HasFuel());
            
            FuelManager.SetFuel(0);
            Assert.IsFalse(FuelManager.HasFuel());
            
            FuelManager.SetFuel(5);
            Assert.IsTrue(FuelManager.HasFuel());
        }
        
        [UnityTest]
        [Order(6)]
        public IEnumerator FuelRegeneratesAutomaticallySingle()
        {
            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);
            
            //ensure the cycle has been created
            Assert.IsNotNull(FuelManager.RegenerateCycle);

            FuelManager.SetFuel(0);
            
            FuelManager.RegenerateCycle.Restart();
            
            const long cycleDurationSeconds = FuelManager.MinutesBetweenFuelRegenerate * TimeUtils.SecondsInAMinute;
            TimeUtils.SetTimeOffset(cycleDurationSeconds);
            yield return null; //wait for events to trigger

            Assert.AreEqual(1, FuelManager.CurrentFuel);
        }
        
        [UnityTest]
        [Order(7)]
        public IEnumerator FuelRegeneratesAutomaticallyMultiple()
        {
            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);
            
            //ensure the cycle has been created
            Assert.IsNotNull(FuelManager.RegenerateCycle);

            FuelManager.SetFuel(0);
            
            FuelManager.RegenerateCycle.Restart();

            const long cycleDurationSeconds = FuelManager.MinutesBetweenFuelRegenerate * TimeUtils.SecondsInAMinute;
            const int cyclesToComplete = FuelManager.MaxFuel;
            TimeUtils.SetTimeOffset(cycleDurationSeconds * cyclesToComplete);
            yield return null; //wait for events to trigger

            Assert.AreEqual(cyclesToComplete, FuelManager.CurrentFuel);
        }
        
        [UnityTest]
        [Order(8)]
        public IEnumerator FuelDoesntRegenerateOverMax()
        {
            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);

            //ensure the cycle has been created
            Assert.IsNotNull(FuelManager.RegenerateCycle);

            FuelManager.SetFuel(FuelManager.MaxFuel - 1);
            
            FuelManager.RegenerateCycle.Restart();
            
            const long cycleDurationSeconds = FuelManager.MinutesBetweenFuelRegenerate * TimeUtils.SecondsInAMinute;
            const int cyclesToComplete = 2;
            TimeUtils.SetTimeOffset(cycleDurationSeconds * cyclesToComplete);
            yield return null; //wait for events to trigger

            Assert.AreEqual(FuelManager.MaxFuel, FuelManager.CurrentFuel);
        }
        
    }
}
