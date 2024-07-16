using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Gumball.Editor.Tests
{
    public class FuelManagerTests
    {

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
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

    }
}
