using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ExperienceManagerTests : IPrebuildSetup, IPostBuildCleanup
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
        public void GetLevelFromTotalXP()
        {
            Assert.AreEqual(0, ExperienceManager.GetLevelIndexFromTotalXP(0));
            
            //just not enough experience
            int totalXP = ExperienceManager.Instance.Levels[0].XPRequired + ExperienceManager.Instance.Levels[1].XPRequired - 1;
            Assert.AreEqual(0,  ExperienceManager.GetLevelIndexFromTotalXP(totalXP));
            
            //exact experience
            totalXP = ExperienceManager.Instance.Levels[0].XPRequired + ExperienceManager.Instance.Levels[1].XPRequired;
            Assert.AreEqual(1,  ExperienceManager.GetLevelIndexFromTotalXP(totalXP));

            //more experience
            totalXP = ExperienceManager.Instance.Levels[0].XPRequired + ExperienceManager.Instance.Levels[1].XPRequired + 1;
            Assert.AreEqual(1,  ExperienceManager.GetLevelIndexFromTotalXP(totalXP));
            
            //level 2
            totalXP = ExperienceManager.Instance.Levels[0].XPRequired + ExperienceManager.Instance.Levels[1].XPRequired + ExperienceManager.Instance.Levels[2].XPRequired;
            Assert.AreEqual(2,  ExperienceManager.GetLevelIndexFromTotalXP(totalXP));
            
            //level 3
            totalXP = ExperienceManager.Instance.Levels[0].XPRequired + ExperienceManager.Instance.Levels[1].XPRequired + ExperienceManager.Instance.Levels[2].XPRequired + ExperienceManager.Instance.Levels[3].XPRequired;
            Assert.AreEqual(3,  ExperienceManager.GetLevelIndexFromTotalXP(totalXP));
        }

        [Test]
        [Order(2)]
        public void AddXPChangesLevel()
        {
            int previousLevel = ExperienceManager.Level;
            Assert.AreEqual(1, previousLevel);
            
            ExperienceManager.AddXP(ExperienceManager.Instance.Levels[1].XPRequired);
            
            int newLevel = ExperienceManager.Level;
            Assert.AreEqual(2, newLevel);
        }

        [Test]
        [Order(3)]
        public void GetXPRequiredForLevel()
        {
            Assert.AreEqual(ExperienceManager.Instance.Levels[0].XPRequired, ExperienceManager.GetXPRequiredForLevel(0));
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[0].XPRequired +
                            ExperienceManager.Instance.Levels[1].XPRequired, ExperienceManager.GetXPRequiredForLevel(1));

            Assert.AreEqual(ExperienceManager.Instance.Levels[0].XPRequired +
                            ExperienceManager.Instance.Levels[1].XPRequired +
                            ExperienceManager.Instance.Levels[2].XPRequired, ExperienceManager.GetXPRequiredForLevel(2));
        }

        [Test]
        [Order(4)]
        public void GetXPForNextLevel()
        {
            ExperienceManager.SetTotalXP(ExperienceManager.GetXPRequiredForLevel(0));
            
            int currentLevel = ExperienceManager.Level;
            Assert.AreEqual(1, currentLevel);
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[1].XPRequired, ExperienceManager.XPForNextLevel);
            
            ExperienceManager.SetTotalXP(ExperienceManager.GetXPRequiredForLevel(1));
            currentLevel = ExperienceManager.Level;
            Assert.AreEqual(2, currentLevel);
            
            int xpForLevel3 = ExperienceManager.XPForNextLevel;
            Assert.AreEqual(ExperienceManager.Instance.Levels[2].XPRequired, xpForLevel3);

            //try with a difference
            const int difference = 5;
            ExperienceManager.SetTotalXP(ExperienceManager.GetXPRequiredForLevel(2) + difference);
            currentLevel = ExperienceManager.Level;
            Assert.AreEqual(3, currentLevel);
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[3].XPRequired - difference, ExperienceManager.XPForNextLevel);
        }
        
    }
}
