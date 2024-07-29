using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class Challenge
    {
        
        [SerializeField] private string description = "Description of challenge";
        [SerializeField] private ChallengeTracker tracker;
        [SerializeField] private int goal;
        [SerializeField] private Rewards rewards;
        
        public string Description => description;
        public ChallengeTracker Tracker => tracker;
        public int Goal => goal;
        public Rewards Rewards => rewards;
        
        public string ChallengeID => $"{description}-{tracker.GetType()}-{goal}";

        public bool IsClaimed
        {
            get => DataManager.GameSessions.Get($"Challenges.Listeners.{ChallengeID}.Claimed", false);
            private set => DataManager.GameSessions.Set($"Challenges.Listeners.{ChallengeID}.Claimed", value);
        }
        
        public void SetClaimed(bool isClaimed)
        {
            IsClaimed = isClaimed;
        }
        
    }
}
