using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ChallengeManagerTests : IPrebuildSetup, IPostBuildCleanup
    {

        private bool isInitialised;

        private Challenges dailyChallenges => ChallengeManager.Instance.Daily;
        private Challenges weeklyChallenges => ChallengeManager.Instance.Weekly;

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
            yield return ChallengeManager.LoadInstanceAsync();
            isInitialised = true;
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator DailyChallengePoolIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsNotNull(dailyChallenges.ChallengePool);
            Assert.Greater(dailyChallenges.ChallengePool.Length, 0);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator DailyChallengesAreAssignedOnStartup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            for (int slotIndex = 0; slotIndex < dailyChallenges.NumberOfChallenges; slotIndex++)
                Assert.IsNotNull(dailyChallenges.GetCurrentChallenge(slotIndex));
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator DailyChallengesCycleStarts()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //ensure the cycle has been created
            Assert.IsNotNull(dailyChallenges.ResetCycle);
        }

        [UnityTest]
        [Order(4)]
        public IEnumerator EnsureTheresEnoughDailyChallenges()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.Greater(dailyChallenges.ChallengePool.Length, dailyChallenges.NumberOfChallenges + dailyChallenges.ChallengesBetweenRepeats - 2);
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator EnsureTheresEnoughWeeklyChallenges()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.Greater(weeklyChallenges.ChallengePool.Length, weeklyChallenges.NumberOfChallenges + weeklyChallenges.ChallengesBetweenRepeats - 2);
        }

        [UnityTest]
        [Order(6)]
        public IEnumerator DailyChallengesResetAutomatically()
        {
            yield return new WaitUntil(() => isInitialised);

            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);
            
            Challenge[] previousChallengesInSlots = new Challenge[dailyChallenges.NumberOfChallenges];
            for (int slotIndex = 0; slotIndex < dailyChallenges.NumberOfChallenges; slotIndex++)
                previousChallengesInSlots[slotIndex] = dailyChallenges.GetCurrentChallenge(slotIndex);

            TimeUtils.SetTimeOffset(dailyChallenges.TimeBetweenReset.ToSeconds());
            yield return null; //wait for events to trigger
            
            for (int slotIndex = 0; slotIndex < dailyChallenges.NumberOfChallenges; slotIndex++)
                Assert.AreNotEqual(previousChallengesInSlots[slotIndex], dailyChallenges.GetCurrentChallenge(slotIndex), $"Equal at slot {slotIndex}");
        }
        
        [UnityTest]
        [Order(7)]
        public IEnumerator DailyChallengesDontRepeat()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(0, TimeUtils.TimeOffsetSeconds);

            Challenge[] previousChallengesInSlots = new Challenge[dailyChallenges.NumberOfChallenges];
            for (int slotIndex = 0; slotIndex < dailyChallenges.NumberOfChallenges; slotIndex++)
                previousChallengesInSlots[slotIndex] = dailyChallenges.GetCurrentChallenge(slotIndex);
            
            int[] challengesWithoutRepeats = dailyChallenges.GetChallengesWithoutRepeats();
            
            TimeUtils.SetTimeOffset(dailyChallenges.TimeBetweenReset.ToSeconds());
            yield return null; //wait for events to trigger

            foreach (Challenge previousChallenge in previousChallengesInSlots)
            {
                int previousChallengeIndex = dailyChallenges.ChallengePool.IndexOfItem(previousChallenge);
                Assert.IsFalse(challengesWithoutRepeats.Contains(previousChallengeIndex));
            }

            //do enough cycles so that the challenge becomes available again
            int items = 0;
            while (items <= dailyChallenges.ChallengesBetweenRepeats)
            {
                TimeUtils.SetTimeOffset(dailyChallenges.TimeBetweenReset.ToSeconds());
                yield return null; //wait for events to trigger
                items += dailyChallenges.NumberOfChallenges;
            }

            //ensure it is available again (or has already been selected)
            foreach (Challenge previousChallenge in previousChallengesInSlots)
            {
                int[] challengesWithoutRepeatsUpdated = dailyChallenges.GetChallengesWithoutRepeats();
                int previousChallengeIndex = dailyChallenges.ChallengePool.IndexOfItem(previousChallenge);
                bool isSpare = challengesWithoutRepeatsUpdated.Contains(previousChallengeIndex);
                bool isCurrent = GetCurrentChallenges().Contains(previousChallenge);
                Assert.IsTrue(isSpare || isCurrent);
            }
        }
        
        [UnityTest]
        [Order(8)]
        public IEnumerator DailyChallengeListenersAreActiveOnLoad()
        {
            yield return new WaitUntil(() => isInitialised);
            
            //ensure the cycle has been created
            for (int slotIndex = 0; slotIndex < dailyChallenges.NumberOfChallenges; slotIndex++)
            {
                Challenge challenge = dailyChallenges.GetCurrentChallenge(slotIndex);
                Assert.IsNotNull(challenge.Tracker.GetListener(challenge.ChallengeID));
            }
        }
        
        [UnityTest]
        [Order(9)]
        public IEnumerator DailyChallengeListenersAreActiveAfterChanging()
        {
            yield return new WaitUntil(() => isInitialised);
            
            TimeUtils.SetTimeOffset(dailyChallenges.TimeBetweenReset.ToSeconds());
            yield return null; //wait for events to trigger
            
            //ensure the cycle has been created
            for (int slotIndex = 0; slotIndex < dailyChallenges.NumberOfChallenges; slotIndex++)
            {
                Challenge challenge = dailyChallenges.GetCurrentChallenge(slotIndex);
                Assert.IsNotNull(challenge.Tracker.GetListener(challenge.ChallengeID));
            }
        }

        private Challenge[] GetCurrentChallenges()
        {
            Challenge[] currentChallengesInSlots = new Challenge[dailyChallenges.NumberOfChallenges];
            for (int slotIndex = 0; slotIndex < dailyChallenges.NumberOfChallenges; slotIndex++)
            {
                currentChallengesInSlots[slotIndex] = dailyChallenges.GetCurrentChallenge(slotIndex);
            }

            return currentChallengesInSlots;
        }

    }
}
