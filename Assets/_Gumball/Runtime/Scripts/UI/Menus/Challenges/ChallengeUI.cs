using System.Collections;
using System.Collections.Generic;
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
        
        public void Initialise(Challenge challenge)
        {
            this.challenge = challenge;
            
            descriptionLabel.text = challenge.Description;
            descriptionLabel.Resize();
            
            UpdateClaimButton();
        }

        public void OnClickClaimButton()
        {
            CoroutineHelper.Instance.StartCoroutine(challenge.Rewards.GiveRewards());
            challenge.SetClaimed(true);

            UpdateClaimButton();
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
            
            ChallengeTracker.Listener challengeListener = challenge.Tracker.GetListener(challenge.ChallengeID);
            claimButton.interactable = challengeListener.Progress >= 1;
        }

    }
}
