using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class CurrencyTests : IPrebuildSetup, IPostBuildCleanup
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

        [Test]
        [Order(1)]
        public void StartWithCorrectFunds()
        {
            Assert.AreEqual(Currency.Premium.StartingFunds, Currency.Premium.Funds);
            Assert.AreEqual(Currency.Standard.StartingFunds, Currency.Standard.Funds);
        }
        
        [Test]
        [Order(2)]
        public void SetFunds()
        {
            const int amount = 10;
            Currency.Premium.SetFunds(amount);
            
            Assert.AreEqual(amount, Currency.Premium.Funds);
        }
        
        [Test]
        [Order(3)]
        public void AddFunds()
        {
            const int fundsBefore = 5;
            Currency.Premium.SetFunds(fundsBefore);
            
            const int amountToAdd = 4;
            Currency.Premium.AddFunds(amountToAdd);
            
            Assert.AreEqual(fundsBefore + amountToAdd, Currency.Premium.Funds);
        }
        
        [Test]
        [Order(4)]
        public void TakeFunds()
        {
            const int fundsBefore = 5;
            Currency.Premium.SetFunds(fundsBefore);
            
            const int amountToTake = 3;
            Currency.Premium.TakeFunds(amountToTake);
            
            Assert.AreEqual(fundsBefore - amountToTake, Currency.Premium.Funds);
        }
        
        [Test]
        [Order(5)]
        public void HasEnoughFunds()
        {
            const int amount = 10;
            Currency.Premium.SetFunds(amount);
            
            //just enough
            Assert.IsTrue(Currency.Premium.HasEnoughFunds(amount));
            
            //has less
            Assert.IsFalse(Currency.Premium.HasEnoughFunds(amount + 1));
            
            //has more
            Assert.IsTrue(Currency.Premium.HasEnoughFunds(amount - 1));
        }
        
    }
}
