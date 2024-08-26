using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class ChallengeChallengesSubMenu : ChallengesSubMenu
    {

        [Header("Challenges")]
        [SerializeField] private ChallengeUI challengeUIPrefab;
        [SerializeField] private Transform challengeUIHolder;

        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI timerLabel;

        [Header("Intermittent rewards")]
        [SerializeField] private Image totalProgressFill;
        [SerializeField] private Button minorRewardButton;
        [SerializeField] private Button majorRewardButton;
        
        protected abstract Challenges GetChallengeManager();

        protected override void OnShow()
        {
            base.OnShow();
            
            SetupChallengeItems();
            UpdateTimerLabel();
            UpdateIntermittentRewards();
        }
        
        public void OnClickMinorReward()
        {
            GetChallengeManager().ClaimMinorReward();
            UpdateIntermittentRewards();
        }

        public void OnClickMajorReward()
        {
            GetChallengeManager().ClaimMajorReward();
            UpdateIntermittentRewards();
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

        private void UpdateTimerLabel()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(GetChallengeManager().ResetCycle.SecondsRemainingInCurrentCycle);
            string timeFormatted = timeSpan.ToPrettyStringMaxUnitOnly();
            timerLabel.text = $"Resets in {timeFormatted}";
        }
        
        private void UpdateIntermittentRewards()
        {
            float totalProgressPercent = GetChallengeManager().GetTotalProgressPercent();
            totalProgressFill.fillAmount = totalProgressPercent;

            minorRewardButton.interactable = !GetChallengeManager().HasClaimedMinorReward && totalProgressPercent >= GetChallengeManager().MinorRewardPercent;
            majorRewardButton.interactable = !GetChallengeManager().HasClaimedMajorReward && totalProgressPercent >= GetChallengeManager().MajorRewardPercent;
        }

    }
}
