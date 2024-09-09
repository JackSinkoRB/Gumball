using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class Challenge : ISerializationCallbackReceiver
    {
        
        [SerializeField] private string description = "Description of challenge";
        [SerializeField] private Sprite icon;
        [SerializeField] private ChallengeTracker tracker;
        [SerializeField] private int goal;
        [SerializeField] private Rewards rewards;

        public string Description => description;
        public Sprite Icon => icon;
        public ChallengeTracker Tracker => tracker;
        public int Goal => goal;
        public Rewards Rewards => rewards;

        [SerializeField, ReadOnly] private string uniqueID;
        
        public string ChallengeID => $"{description}-{tracker.GetType()}-{uniqueID}-{goal}";

        public bool IsClaimed
        {
            get => DataManager.GameSessions.Get($"Challenges.Listeners.{ChallengeID}.Claimed", false);
            private set => DataManager.GameSessions.Set($"Challenges.Listeners.{ChallengeID}.Claimed", value);
        }

        public void OnBeforeSerialize()
        {
            if (uniqueID.IsNullOrEmpty())
                uniqueID = Guid.NewGuid().ToString();
        }

        public void OnAfterDeserialize()
        {
            
        }

        public void SetClaimed(bool isClaimed)
        {
            IsClaimed = isClaimed;
        }
        
    }
}
