using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class Challenges
    {

        [SerializeField] private string id = "Daily";
        [SerializeField] private Challenge[] challengePool;
        [Space(5)]
        [SerializeField] private int numberOfChallenges = 3;
        [SerializeField] private SerializedTimeSpan timeBetweenReset = new(1, 0, 0, 0);
        [Tooltip("How many challenges need to be assigned before a challenge can be reassigned? This prevents players getting the same challenges repeatedly.")]
        [SerializeField] private int challengesBetweenRepeats = 10;

        [Header("Intermittent rewards")]
        [SerializeField, Range(0,1)] private float minorRewardPercent = 0.5f;
        [SerializeField] private Rewards[] minorRewardsPool;
        [Space(5)]
        [SerializeField, Range(0,1)] private float majorRewardPercent = 0.8f;
        [SerializeField] private Rewards[] majorRewardsPool;

        private List<int> previousChallenges
        {
            get => DataManager.Player.Get($"Challenges.{id}.PreviouslyAssigned", new List<int>());
            set => DataManager.Player.Set($"Challenges.{id}.PreviouslyAssigned", value);
        }
        
        private bool HasClaimedMinorReward
        {
            get => DataManager.Player.Get($"Challenges.{id}.HasClaimedMinorReward", false);
            set => DataManager.Player.Set($"Challenges.{id}.HasClaimedMinorReward", value);
        }

        private bool HasClaimedMajorReward
        {
            get => DataManager.Player.Get($"Challenges.{id}.HasClaimedMajorReward", false);
            set => DataManager.Player.Set($"Challenges.{id}.HasClaimedMajorReward", value);
        }
        
        public bool IsInitialised { get; private set; }
        public int NumberOfChallenges => numberOfChallenges;
        public Challenge[] ChallengePool => challengePool;
        public SerializedTimeSpan TimeBetweenReset => timeBetweenReset;
        public int ChallengesBetweenRepeats => challengesBetweenRepeats;
        public PersistentCooldown ResetCycle { get; private set; }
        public bool IsMinorRewardReadyToBeClaimed => !HasClaimedMinorReward && GetTotalProgressPercent() >= minorRewardPercent; 
        public bool IsMajorRewardReadyToBeClaimed => !HasClaimedMajorReward && GetTotalProgressPercent() >= majorRewardPercent; 

        public List<int> UnclaimedChallengeIndices
        {
            get => DataManager.GameSessions.Get($"Challenges.{id}.Unclaimed", new List<int>());
            private set => DataManager.GameSessions.Set($"Challenges.{id}.Unclaimed", value);
        }

        public void Initialise()
        {
            ResetCycle = new PersistentCooldown($"Challenges.{id}.ResetCycle", timeBetweenReset.ToSeconds(), true);
            ResetCycle.onCycleComplete += ResetChallenges;
            ResetCycle.Play();

            EnsureChallengesAreAssigned();
            
            StartTrackers();

            IsInitialised = true;
            GlobalLoggers.ChallengesLogger.Log($"Initialised {id} challenges.");
        }
        
        public Challenge GetCurrentChallenge(int slotIndex)
        {
            string key = $"Challenges.{id}.Current.{slotIndex}";
            if (!DataManager.Player.HasKey(key))
                return null;
            
            int challengeIndex = DataManager.Player.Get<int>(key);
            if (challengeIndex == -1)
                return null; //not enough challenges
            
            return challengePool[challengeIndex];
        }
        
        public float GetTotalProgressPercent()
        {
            float currentPercent = 0;
            float maxPercent = NumberOfChallenges;
            
            for (int slotIndex = 0; slotIndex < NumberOfChallenges; slotIndex++)
            {
                Challenge challenge = GetCurrentChallenge(slotIndex);
                if (challenge == null)
                {
                    maxPercent--;
                    continue;
                }

                ChallengeTracker.Listener listener = challenge.Tracker.GetListener(challenge.UniqueID);
                currentPercent += listener.Progress;
            }

            return Mathf.Clamp01(currentPercent / maxPercent);
        }

        public bool AreRewardsReadyToBeClaimed()
        {
            //check unclaimed challenges
            if (UnclaimedChallengeIndices.Count > 0)
                return true;
            
            //check minor/major rewards
            if (IsMinorRewardReadyToBeClaimed || IsMajorRewardReadyToBeClaimed)
                return true;
            
            //check current slots
            for (int slotIndex = 0; slotIndex < numberOfChallenges; slotIndex++)
            {
                Challenge currentChallenge = GetCurrentChallenge(slotIndex);
                if (currentChallenge == null)
                    continue;

                ChallengeTracker.Listener tracker = currentChallenge.Tracker.GetListener(currentChallenge.UniqueID);
                if (tracker.IsComplete && !currentChallenge.IsClaimed)
                    return true;
            }

            return false;
        }
        
        public int[] GetChallengesWithoutRepeats()
        {
            if (previousChallenges.Count >= challengePool.Length)
                return Array.Empty<int>(); //not enough challenges

            int[] challengesWithoutRepeats = new int[challengePool.Length - previousChallenges.Count];

            int indexCount = 0;
            for (int challengeIndex = 0; challengeIndex < challengePool.Length; challengeIndex++)
            {
                if (previousChallenges.Contains(challengeIndex))
                    continue; //is repeat

                challengesWithoutRepeats[indexCount] = challengeIndex;
                indexCount++;
            }
            
            return challengesWithoutRepeats;
        }
        
        public void ClaimMinorReward()
        {
            HasClaimedMinorReward = true;
            CoroutineHelper.Instance.StartCoroutine(minorRewardsPool.GetRandom().GiveRewards());
        }

        public void ClaimMajorReward()
        {
            HasClaimedMajorReward = true;
            CoroutineHelper.Instance.StartCoroutine(majorRewardsPool.GetRandom().GiveRewards());
        }

        private void EnsureChallengesAreAssigned()
        {
            for (int slotIndex = 0; slotIndex < numberOfChallenges; slotIndex++)
            {
                if (GetCurrentChallenge(slotIndex) == null)
                    SetCurrentChallenge(slotIndex, GetRandomChallengeIndex());
            }
        }

        private void ResetChallenges()
        {
            GlobalLoggers.ChallengesLogger.Log($"Resetting {id} challenges.");
            
            //give unclaimed intermittent rewards before resetting
            CoroutineHelper.Instance.StartCoroutine(GiveUnclaimedIntermittentRewards());

            for (int slotIndex = 0; slotIndex < numberOfChallenges; slotIndex++)
            {
                SetCurrentChallenge(slotIndex, GetRandomChallengeIndex());
            }
            
            //reset intermittent rewards claiming
            HasClaimedMinorReward = false;
            HasClaimedMajorReward = false;
        }

        private IEnumerator GiveUnclaimedIntermittentRewards()
        {
            if (!IsMinorRewardReadyToBeClaimed && !IsMajorRewardReadyToBeClaimed)
                yield break;
            
            yield return new WaitUntil(() => PanelManager.PanelExists<MainMenuPanel>()
                                             && PanelManager.GetPanel<MainMenuPanel>().IsShowing
                                             && !PanelManager.GetPanel<MainMenuPanel>().IsTransitioning);

            if (PanelManager.PanelExists<GenericMessagePanel>())
            {
                PanelManager.GetPanel<GenericMessagePanel>().Show();
                PanelManager.GetPanel<GenericMessagePanel>().Initialise("You have unclaimed challenge rewards!");
            
                yield return new WaitUntil(() => !PanelManager.PanelExists<GenericMessagePanel>() || !PanelManager.GetPanel<GenericMessagePanel>().IsShowing);
            }
            
            if (IsMinorRewardReadyToBeClaimed)
            {
                //give the reward
                yield return minorRewardsPool.GetRandom().GiveRewards();
            }
            
            if (IsMajorRewardReadyToBeClaimed)
            {
                //give the reward
                yield return majorRewardsPool.GetRandom().GiveRewards();
            }
        }

        private int GetRandomChallengeIndex()
        {
            int[] spareChallenges = GetChallengesWithoutRepeats();
            if (spareChallenges.Length == 0)
                return -1;
            
            return spareChallenges.GetRandom();
        }

        private void SetCurrentChallenge(int slotIndex, int challengeIndex)
        {
            //end the previous tracker
            EndCurrentChallenge(slotIndex);

            DataManager.Player.Set($"Challenges.{id}.Current.{slotIndex}", challengeIndex);
            
            //add to previous challenges list
            List<int> previousChallengesTemp = previousChallenges;
            if (previousChallenges.Count >= challengesBetweenRepeats)
                previousChallengesTemp.RemoveAt(0);
            previousChallengesTemp.Add(challengeIndex);
            previousChallenges = previousChallengesTemp;

            //start tracker
            Challenge currentChallenge = challengePool[challengeIndex];
            currentChallenge.Tracker.StartListening(currentChallenge.UniqueID, currentChallenge.Goal);
        }
        
        private void EndCurrentChallenge(int slotIndex)
        {
            Challenge currentChallenge = GetCurrentChallenge(slotIndex);
            if (currentChallenge == null)
                return;
            
            currentChallenge.SetClaimed(false); //reset
                
            ChallengeTracker.Listener previousChallengeListener = currentChallenge.Tracker.GetListener(currentChallenge.UniqueID);
            if (!currentChallenge.IsClaimed && previousChallengeListener.IsComplete)
            {
                //add to unclaimed challenges
                AddUnclaimedChallenge(currentChallenge);
            }

            currentChallenge.Tracker.StopListening(currentChallenge.UniqueID);
        }

        private void AddUnclaimedChallenge(Challenge challenge)
        {
            List<int> unclaimedChallengesTemp = new List<int>(UnclaimedChallengeIndices);
            unclaimedChallengesTemp.Add(challengePool.IndexOfItem(challenge));
            UnclaimedChallengeIndices = unclaimedChallengesTemp;
        }
        
        public void RemoveUnclaimedChallenge(int index)
        {
            List<int> unclaimedChallengesTemp = new List<int>(UnclaimedChallengeIndices);
            unclaimedChallengesTemp.Remove(index);
            UnclaimedChallengeIndices = unclaimedChallengesTemp;
        }

        private void StartTrackers()
        {
            for (int slotIndex = 0; slotIndex < numberOfChallenges; slotIndex++)
            {
                Challenge currentChallenge = GetCurrentChallenge(slotIndex);
                currentChallenge.Tracker.StartListening(currentChallenge.UniqueID, currentChallenge.Goal);
            }
        }

    }
}
