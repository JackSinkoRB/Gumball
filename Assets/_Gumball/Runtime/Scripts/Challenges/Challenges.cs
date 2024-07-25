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
        
        private List<int> previousChallenges
        {
            get => DataManager.Player.Get($"Challenges.{id}.PreviouslyAssigned", new List<int>());
            set => DataManager.Player.Set($"Challenges.{id}.PreviouslyAssigned", value);
        }

        public int NumberOfChallenges => numberOfChallenges;
        public Challenge[] ChallengePool => challengePool;
        public SerializedTimeSpan TimeBetweenReset => timeBetweenReset;
        public int ChallengesBetweenRepeats => challengesBetweenRepeats;
        public PersistentCooldown ResetCycle { get; private set; }

        public void Initialise()
        {
            ResetCycle = new PersistentCooldown($"Challenges.{id}.ResetCycle", timeBetweenReset.ToSeconds());
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
            for (int slotIndex = 0; slotIndex < numberOfChallenges; slotIndex++)
            {
                SetCurrentChallenge(slotIndex, GetRandomChallengeIndex());
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
                previousChallenge.Tracker.StopListening(previousChallenge.ChallengeID);
            
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
