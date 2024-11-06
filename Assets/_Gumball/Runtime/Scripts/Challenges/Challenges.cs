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
        
        public bool HasClaimedMinorReward
        {
            get => DataManager.Player.Get($"Challenges.{id}.HasClaimedMinorReward", false);
            private set => DataManager.Player.Set($"Challenges.{id}.HasClaimedMinorReward", value);
        }

        public bool HasClaimedMajorReward
        {
            get => DataManager.Player.Get($"Challenges.{id}.HasClaimedMajorReward", false);
            private set => DataManager.Player.Set($"Challenges.{id}.HasClaimedMajorReward", value);
        }
        
        public int NumberOfChallenges => numberOfChallenges;
        public Challenge[] ChallengePool => challengePool;
        public SerializedTimeSpan TimeBetweenReset => timeBetweenReset;
        public int ChallengesBetweenRepeats => challengesBetweenRepeats;
        public PersistentCooldown ResetCycle { get; private set; }
        public float MinorRewardPercent => minorRewardPercent;
        public float MajorRewardPercent => majorRewardPercent;

        public List<int> UnclaimedChallengeIndices
        {
            get => DataManager.GameSessions.Get($"Challenges.{id}.Unclaimed", new List<int>());
            private set => DataManager.GameSessions.Set($"Challenges.{id}.Unclaimed", value);
        }

        public void Initialise()
        {
            //require internet connection
            PlayFabManager.onSuccessfulConnection += OnConnectToPlayFab;
        }

        private void OnConnectToPlayFab()
        {
            ResetCycle = new PersistentCooldown($"Challenges.{id}.ResetCycle", timeBetweenReset.ToSeconds(), true);
            ResetCycle.onCycleComplete += ResetChallenges;
            ResetCycle.Play();

            EnsureChallengesAreAssigned();
            
            StartTrackers();
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
                float progressPercent = challenge.Tracker.GetListener(challenge.ChallengeID).Progress;
                currentPercent += progressPercent;
            }

            return Mathf.Clamp01(currentPercent / maxPercent);
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
            bool isMinorRewardUnclaimed = !HasClaimedMinorReward && GetTotalProgressPercent() >= MinorRewardPercent;
            bool isMajorRewardUnclaimed = !HasClaimedMajorReward && GetTotalProgressPercent() >= MajorRewardPercent;

            if (!isMinorRewardUnclaimed && !isMajorRewardUnclaimed)
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
            
            if (isMinorRewardUnclaimed)
            {
                //give the reward
                yield return minorRewardsPool.GetRandom().GiveRewards();
            }
            
            if (isMajorRewardUnclaimed)
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
            Challenge previousChallenge = GetCurrentChallenge(slotIndex);
            if (previousChallenge != null)
            {
                if (!previousChallenge.IsClaimed && previousChallenge.Tracker.GetListener(previousChallenge.ChallengeID).IsComplete)
                {
                    //add to unclaimed challenges
                    AddUnclaimedChallenge(previousChallenge);
                }
                
                previousChallenge.SetClaimed(false); //reset
                previousChallenge.Tracker.StopListening(previousChallenge.ChallengeID);
            }

            DataManager.Player.Set($"Challenges.{id}.Current.{slotIndex}", challengeIndex);
            
            //add to previous challenges list
            List<int> previousChallengesTemp = previousChallenges;
            if (previousChallenges.Count >= challengesBetweenRepeats)
                previousChallengesTemp.RemoveAt(0);
            previousChallengesTemp.Add(challengeIndex);
            previousChallenges = previousChallengesTemp;

            //start tracker
            Challenge currentChallenge = challengePool[challengeIndex];
            currentChallenge.Tracker.StartListening(currentChallenge.ChallengeID, currentChallenge.Goal);
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
                currentChallenge.Tracker.StartListening(currentChallenge.ChallengeID, currentChallenge.Goal);
            }
        }

    }
}
