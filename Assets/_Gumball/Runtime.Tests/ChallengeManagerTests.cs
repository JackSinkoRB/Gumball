using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ChallengeManagerTests : IPrebuildSetup, IPostBuildCleanup
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
            PersistentCooldown.IsRunningTests = true;
            DataManager.EnableTestProviders(true);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            isInitialised = false;
            PersistentCooldown.IsRunningTests = false;
            DataManager.EnableTestProviders(false);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }
        
        [TearDown]
        public void TearDown()
        {
            TimeUtils.SetTimeOffset(0);
        }

        private IEnumerator Initialise()
        {
            yield return DailyChallengeManager.LoadInstanceAsync();
            isInitialised = true;
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator DailyChallengePoolIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsNotNull(DailyChallengeManager.Instance.ChallengePool);
            Assert.Greater(DailyChallengeManager.Instance.ChallengePool.Length, 0);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator DailyChallengesAreAssignedOnStartup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            for (int slotIndex = 0; slotIndex < DailyChallengeManager.Instance.NumberOfChallenges; slotIndex++)
                Assert.IsNotNull(DailyChallengeManager.Instance.GetCurrentChallenge(slotIndex));
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator DailyChallengesCycleStarts()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //ensure the cycle has been created
            Assert.IsNotNull(DailyChallengeManager.Instance.ResetCycle);
        }

        [UnityTest]
        [Order(4)]
        public IEnumerator DailyChallengesResetAutomatically()
        {
            yield return new WaitUntil(() => isInitialised);

            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);
            
            Challenge[] previousChallengesInSlots = new Challenge[DailyChallengeManager.Instance.NumberOfChallenges];
            for (int slotIndex = 0; slotIndex < DailyChallengeManager.Instance.NumberOfChallenges; slotIndex++)
                previousChallengesInSlots[slotIndex] = DailyChallengeManager.Instance.GetCurrentChallenge(slotIndex);

            TimeUtils.SetTimeOffset(DailyChallengeManager.Instance.TimeBetweenReset.ToSeconds());
            yield return null; //wait for events to trigger
            
            for (int slotIndex = 0; slotIndex < DailyChallengeManager.Instance.NumberOfChallenges; slotIndex++)
                Assert.AreNotEqual(previousChallengesInSlots[slotIndex], DailyChallengeManager.Instance.GetCurrentChallenge(slotIndex), $"Equal at slot {slotIndex}");
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator DailyChallengesDontRepeat()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);

            Challenge[] previousChallengesInSlots = new Challenge[DailyChallengeManager.Instance.NumberOfChallenges];
            for (int slotIndex = 0; slotIndex < DailyChallengeManager.Instance.NumberOfChallenges; slotIndex++)
                previousChallengesInSlots[slotIndex] = DailyChallengeManager.Instance.GetCurrentChallenge(slotIndex);
            
            int[] challengesWithoutRepeats = DailyChallengeManager.Instance.GetChallengesWithoutRepeats();
            
            TimeUtils.SetTimeOffset(DailyChallengeManager.Instance.TimeBetweenReset.ToSeconds());
            yield return null; //wait for events to trigger

            foreach (Challenge previousChallenge in previousChallengesInSlots)
            {
                int previousChallengeIndex = DailyChallengeManager.Instance.ChallengePool.IndexOfItem(previousChallenge);
                Assert.IsFalse(challengesWithoutRepeats.Contains(previousChallengeIndex));
            }

            //do enough cycles so that the challenge becomes available again
            int items = 0;
            while (items <= DailyChallengeManager.Instance.ChallengesBetweenRepeats)
            {
                TimeUtils.SetTimeOffset(DailyChallengeManager.Instance.TimeBetweenReset.ToSeconds());
                yield return null; //wait for events to trigger
                items += DailyChallengeManager.Instance.NumberOfChallenges;
            }

            //ensure it is available again (or has already been selected)
            foreach (Challenge previousChallenge in previousChallengesInSlots)
            {
                int[] challengesWithoutRepeatsUpdated = DailyChallengeManager.Instance.GetChallengesWithoutRepeats();
                int previousChallengeIndex = DailyChallengeManager.Instance.ChallengePool.IndexOfItem(previousChallenge);
                bool isSpare = challengesWithoutRepeatsUpdated.Contains(previousChallengeIndex);
                bool isCurrent = GetCurrentChallenges().Contains(previousChallenge);
                Assert.IsTrue(isSpare || isCurrent);
            }
        }

        private Challenge[] GetCurrentChallenges()
        {
            Challenge[] currentChallengesInSlots = new Challenge[DailyChallengeManager.Instance.NumberOfChallenges];
            for (int slotIndex = 0; slotIndex < DailyChallengeManager.Instance.NumberOfChallenges; slotIndex++)
            {
                currentChallengesInSlots[slotIndex] = DailyChallengeManager.Instance.GetCurrentChallenge(slotIndex);
            }

            return currentChallengesInSlots;
        }

    }
}
