using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ExperienceManagerTests : BaseRuntimeTests
    {

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
            ExperienceManager.SetLevel(0);
            
            int currentLevel = ExperienceManager.Level;
            Assert.AreEqual(1, currentLevel);
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[1].XPRequired, ExperienceManager.RemainingXPForNextLevel);
            
            ExperienceManager.SetLevel(1);
            currentLevel = ExperienceManager.Level;
            Assert.AreEqual(2, currentLevel);
            
            int xpForLevel3 = ExperienceManager.RemainingXPForNextLevel;
            Assert.AreEqual(ExperienceManager.Instance.Levels[2].XPRequired, xpForLevel3);

            //try with a difference
            const int difference = 5;
            ExperienceManager.SetTotalXP(ExperienceManager.GetXPRequiredForLevel(2) + difference);
            currentLevel = ExperienceManager.Level;
            Assert.AreEqual(3, currentLevel);
            
            Assert.AreEqual(ExperienceManager.Instance.Levels[3].XPRequired - difference, ExperienceManager.RemainingXPForNextLevel);
        }

        [UnityTest]
        [Order(5)]
        public IEnumerator GivePremiumCurrencyOnLevelUp()
        {
            //initialise reward
            const int levelToTest = 1;
            const int premiumCurrencyToGive = 10;
            int previousPremiumCurrencyReward = ExperienceManager.Instance.Levels[levelToTest].Rewards.PremiumCurrency;
            ExperienceManager.Instance.Levels[levelToTest].Rewards.SetPremiumCurrencyReward(premiumCurrencyToGive);
            
            ExperienceManager.SetLevel(0);

            int premiumFundsBefore = Currency.Premium.Funds;
            
            Rewards xpAsReward = new Rewards(xp: ExperienceManager.RemainingXPForNextLevel);
            yield return xpAsReward.GiveRewards(false);

            int expectedPremiumCoinsAfter = premiumFundsBefore + premiumCurrencyToGive;

            Assert.AreEqual(expectedPremiumCoinsAfter, Currency.Premium.Funds);

            //reset
            ExperienceManager.Instance.Levels[levelToTest].Rewards.SetPremiumCurrencyReward(previousPremiumCurrencyReward);
        }
        
        [UnityTest]
        [Order(6)]
        public IEnumerator GivePremiumCurrencyOnMultipleLevelUp()
        {
            const int startingLevelIndex = 0;
            const int desiredLevelIndex = 3;
            
            ExperienceManager.SetLevel(startingLevelIndex);
            
            int premiumFundsBefore = Currency.Premium.Funds;
            
            int xpRequired = ExperienceManager.GetXPRequiredForLevel(desiredLevelIndex) - ExperienceManager.TotalXP;
            Rewards xpAsReward = new Rewards(xp: xpRequired);
            yield return xpAsReward.GiveRewards(false);

            int expectedPremiumFundsAfter = premiumFundsBefore;
            for (int level = startingLevelIndex; level <= desiredLevelIndex; level++)
            {
                expectedPremiumFundsAfter += ExperienceManager.Instance.Levels[level].Rewards.PremiumCurrency;
            }

            Assert.AreEqual(expectedPremiumFundsAfter, Currency.Premium.Funds);
        }
        
        [UnityTest]
        [Order(7)]
        public IEnumerator GiveFuelOnLevelUp()
        {
            yield return FuelManager.LoadInstanceAsync();
            
            //initialise reward
            const int levelToTest = 1;
            bool previousFuelReward = ExperienceManager.Instance.Levels[levelToTest].Rewards.FuelRefill;
            ExperienceManager.Instance.Levels[levelToTest].Rewards.SetFuelRefillReward(true);
            
            FuelManager.Instance.SetFuel(0);
            ExperienceManager.SetLevel(0);

            Rewards xpAsReward = new Rewards(xp: ExperienceManager.RemainingXPForNextLevel);
            yield return xpAsReward.GiveRewards(false);
            
            Assert.AreEqual(FuelManager.Instance.MaxFuel, FuelManager.Instance.CurrentFuel);

            //reset
            ExperienceManager.Instance.Levels[levelToTest].Rewards.SetFuelRefillReward(previousFuelReward);
        }
        
    }
}
