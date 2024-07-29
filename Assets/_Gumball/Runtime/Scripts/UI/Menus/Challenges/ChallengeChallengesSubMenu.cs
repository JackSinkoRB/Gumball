using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public abstract class ChallengeChallengesSubMenu : ChallengesSubMenu
    {

        [Space(5)]
        [SerializeField] private ChallengeUI challengeUIPrefab;
        [SerializeField] private Transform challengeUIHolder;

        protected abstract Challenges GetChallengeManager();
        
        public override void Show()
        {
            base.Show();
            
            SetupChallengeItems();
        }

        private void SetupChallengeItems()
        {
            //pool previous
            foreach (Transform child in challengeUIHolder)
                child.gameObject.Pool();
            
            //show current slots
            for (int slotIndex = 0; slotIndex < GetChallengeManager().NumberOfChallenges; slotIndex++)
            {
                Challenge currentChallenge = GetChallengeManager().GetCurrentChallenge(slotIndex);
                if (currentChallenge == null)
                    continue;

                ChallengeUI challengeUI = challengeUIPrefab.gameObject.GetSpareOrCreate<ChallengeUI>(challengeUIHolder);
                
                //put unclaimed challenges on top
                ChallengeTracker.Listener tracker = currentChallenge.Tracker.GetListener(currentChallenge.ChallengeID);
                if (tracker.IsComplete
                    && !currentChallenge.IsClaimed)
                    challengeUI.transform.SetAsFirstSibling();
                else challengeUI.transform.SetAsLastSibling();
                
                challengeUI.Initialise(currentChallenge, GetChallengeManager(), false);
            }
            
            //show unclaimed challenges
            foreach (int unclaimedChallengeIndex in GetChallengeManager().UnclaimedChallengeIndices)
            {
                Challenge unclaimedChallenge = GetChallengeManager().ChallengePool[unclaimedChallengeIndex];
                
                ChallengeUI challengeUI = challengeUIPrefab.gameObject.GetSpareOrCreate<ChallengeUI>(challengeUIHolder);
                challengeUI.transform.SetAsFirstSibling();
                challengeUI.Initialise(unclaimedChallenge, GetChallengeManager(), true);
            }
        }
        
    }
}
