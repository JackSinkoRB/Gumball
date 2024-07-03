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
            
            AsyncOperation loadBootScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.BootScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadBootScene.completed += OnSceneLoadComplete;
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
        
        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.PerformAfterTrue(() => GameLoaderSceneManager.HasLoaded, () => isInitialised = true);
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator GetLevelFromTotalXP()
        {
            yield return new WaitUntil(() => isInitialised);
            
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

        [UnityTest]
        [Order(2)]
        public IEnumerator AddXPChangesLevel()
        {
            yield return new WaitUntil(() => isInitialised);
            
            int previousLevel = ExperienceManager.LevelValue;
            Assert.AreEqual(1, previousLevel);
            
            ExperienceManager.AddXP(ExperienceManager.Instance.Levels[1].XPRequired);
            
            int newLevel = ExperienceManager.LevelValue;
            Assert.AreEqual(2, newLevel);
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator GetXPRequiredForLevel()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[0].XPRequired, ExperienceManager.GetXPRequiredForLevel(0));
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[0].XPRequired +
                            ExperienceManager.Instance.Levels[1].XPRequired, ExperienceManager.GetXPRequiredForLevel(1));

            Assert.AreEqual(ExperienceManager.Instance.Levels[0].XPRequired +
                            ExperienceManager.Instance.Levels[1].XPRequired +
                            ExperienceManager.Instance.Levels[2].XPRequired, ExperienceManager.GetXPRequiredForLevel(2));
        }

        [UnityTest]
        [Order(4)]
        public IEnumerator GetXPForNextLevel()
        {
            yield return new WaitUntil(() => isInitialised);
            
            ExperienceManager.SetLevel(0);
            
            int currentLevel = ExperienceManager.LevelValue;
            Assert.AreEqual(1, currentLevel);
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[1].XPRequired, ExperienceManager.RemainingXPForNextLevel);
            
            ExperienceManager.SetLevel(1);
            currentLevel = ExperienceManager.LevelValue;
            Assert.AreEqual(2, currentLevel);
            
            int xpForLevel3 = ExperienceManager.RemainingXPForNextLevel;
            Assert.AreEqual(ExperienceManager.Instance.Levels[2].XPRequired, xpForLevel3);

            //try with a difference
            const int difference = 5;
            ExperienceManager.SetTotalXP(ExperienceManager.GetXPRequiredForLevel(2) + difference);
            currentLevel = ExperienceManager.LevelValue;
            Assert.AreEqual(3, currentLevel);
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[3].XPRequired - difference, ExperienceManager.RemainingXPForNextLevel);
        }

        [UnityTest]
        [Order(5)]
        public IEnumerator RewardsGivenOnSingleLevelUp()
        {
            yield return new WaitUntil(() => isInitialised);
            
            ExperienceManager.SetLevel(0);

            int premiumCoinsBefore = 0; //TODO
            
            ExperienceManager.AddXP(ExperienceManager.RemainingXPForNextLevel);

            int premiumCoinsAfter = 0; //TODO
            int expectedPremiumCoinsAfter = premiumCoinsBefore + ExperienceManager.Instance.Levels[1].PremiumCurrencyReward;
            //TODO:
            //Assert.AreEqual(expectedPremiumCoinsAfter, premiumCoinsAfter);

            //TODO: check fuel is given
        }
        
        [UnityTest]
        [Order(6)]
        public IEnumerator RewardsGivenOnMultipleLevelUp()
        {
            yield return new WaitUntil(() => isInitialised);
            
            const int startingLevelIndex = 0;
            const int desiredLevelIndex = 3;
            
            ExperienceManager.SetLevel(startingLevelIndex);
            
            int premiumCoinsBefore = 0; //TODO
            
            int xpRequired = ExperienceManager.GetXPRequiredForLevel(desiredLevelIndex) - ExperienceManager.TotalXP;
            ExperienceManager.AddXP(xpRequired);

            int premiumCoinsAfter = 0; //TODO
            int expectedPremiumCoinsAfter = premiumCoinsBefore;
            for (int level = startingLevelIndex; level <= desiredLevelIndex; level++)
            {
                expectedPremiumCoinsAfter += ExperienceManager.Instance.Levels[level].PremiumCurrencyReward;
                
                //TODO: check fuel is given
            }

            //TODO:
            //Assert.AreEqual(expectedPremiumCoinsAfter, premiumCoinsAfter);
        }
        
    }
}
