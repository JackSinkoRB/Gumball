using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Daily Challenge Manager")]
    public class DailyChallengeManager : SingletonScriptable<DailyChallengeManager>
    {

        [SerializeField] private Challenge[] challengePool;
        [Space(5)]
        [SerializeField] private int numberOfChallenges = 3;
        [SerializeField] private SerializedTimeSpan timeBetweenReset = new(1, 0, 0, 0);
        [Tooltip("How many challenges need to be assigned before a challenge can be reassigned? This prevents players getting the same challenges repeatedly.")]
        [SerializeField] private int challengesBetweenRepeats = 10;
        
        private List<int> previousChallenges
        {
            get => DataManager.Player.Get($"Challenges.PreviouslyAssigned", new List<int>());
            set => DataManager.Player.Set($"Challenges.PreviouslyAssigned", value);
        }

        public int NumberOfChallenges => numberOfChallenges;
        public Challenge[] ChallengePool => challengePool;
        public SerializedTimeSpan TimeBetweenReset => timeBetweenReset;
        public int ChallengesBetweenRepeats => challengesBetweenRepeats;
        public PersistentCooldown ResetCycle { get; private set; }

        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();
            
            ResetCycle = new PersistentCooldown($"ChallengeReset.Daily", timeBetweenReset.ToSeconds());
            ResetCycle.onCycleComplete += ResetChallenges;
            ResetCycle.Play();

            EnsureChallengesAreAssigned();
        }
        
        public Challenge GetCurrentChallenge(int slotIndex)
        {
            string key = $"Challenges.Current.{slotIndex}";
            if (!DataManager.Player.HasKey(key))
                return null;
            
            int challengeIndex = DataManager.Player.Get<int>(key);
            return challengePool[challengeIndex];
        }
        
        public int[] GetChallengesWithoutRepeats()
        {
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
            return GetChallengesWithoutRepeats().GetRandom();
        }

        private void SetCurrentChallenge(int slotIndex, int challengeIndex)
        {
            DataManager.Player.Set($"Challenges.Current.{slotIndex}", challengeIndex);
            
            //add to previous challenges list
            List<int> previousChallengesTemp = previousChallenges;
            if (previousChallenges.Count >= challengesBetweenRepeats)
                previousChallengesTemp.RemoveAt(0);
            previousChallengesTemp.Add(challengeIndex);
            previousChallenges = previousChallengesTemp;
        }
        
    }
}
