using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ChallengeUI : MonoBehaviour
    {

        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonLabel;
        [SerializeField] private AutosizeTextMeshPro descriptionLabel;

        private Challenge challenge;
        private Challenges challengeManager;
        private bool isUnclaimedChallenge;
        
        public void Initialise(Challenge challenge, Challenges challengeManager, bool isUnclaimedChallenge)
        {
            this.challenge = challenge;
            this.challengeManager = challengeManager;
            this.isUnclaimedChallenge = isUnclaimedChallenge;
            
            descriptionLabel.text = challenge.Description;
            descriptionLabel.Resize();
            
            UpdateClaimButton();
        }

        public void OnClickClaimButton()
        {
            CoroutineHelper.Instance.StartCoroutine(challenge.Rewards.GiveRewards());
            if (!isUnclaimedChallenge)
                challenge.SetClaimed(true);
            else
                OnClaimUnclaimedChallenge();

            UpdateClaimButton();
        }
        
        private void OnClaimUnclaimedChallenge()
        {
            gameObject.SetActive(false);
            
            //remove from unclaimed challenges
            challengeManager.RemoveUnclaimedChallenge(challengeManager.ChallengePool.IndexOfItem(challenge));
        }

        private void UpdateClaimButton()
        {
            if (challenge.IsClaimed)
            {
                descriptionLabel.fontStyle = FontStyles.Strikethrough;
                claimButtonLabel.text = "Claimed";
                
                claimButton.interactable = false;
                return;
            }

            descriptionLabel.fontStyle = FontStyles.Normal;
            claimButtonLabel.text = "Claim";

            if (isUnclaimedChallenge)
            {
                claimButton.interactable = true;
            }
            else
            {
                ChallengeTracker.Listener challengeListener = challenge.Tracker.GetListener(challenge.ChallengeID);
                claimButton.interactable = challengeListener.IsComplete;
            }
        }

    }
}
