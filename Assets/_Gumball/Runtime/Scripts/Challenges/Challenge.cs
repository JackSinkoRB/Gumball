using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gumball
{
    [Serializable]
    public class Challenge
    {
        
#if UNITY_EDITOR
        public static void EnsureChallengesAreUnique(Challenge[] challenges, Object context)
        {
            HashSet<string> uniqueIDs = new HashSet<string>();
            bool isDirty = false;
            
            foreach (Challenge challenge in challenges)
            {
                while (challenge.uniqueID.IsNullOrEmpty() || uniqueIDs.Contains(challenge.UniqueID))
                {
                    isDirty = true;
                    challenge.AssignNewID();
                }

                uniqueIDs.Add(challenge.UniqueID);
            }
        
            if (isDirty)
                EditorUtility.SetDirty(context);
        }
#endif
        
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

        [SerializeField, ReadOnly] private string uniqueID = Guid.NewGuid().ToString();

        public string UniqueID => uniqueID;
        
        public bool IsClaimed
        {
            get => DataManager.GameSessions.Get($"Challenges.Listeners.{uniqueID}.Claimed", false);
            private set => DataManager.GameSessions.Set($"Challenges.Listeners.{uniqueID}.Claimed", value);
        }
        
        public void SetClaimed(bool isClaimed)
        {
            IsClaimed = isClaimed;
        }

        public void AssignNewID()
        {
            uniqueID = Guid.NewGuid().ToString();
            Debug.Log($"Assigned new ID for {description}");
        }
        
    }
}
