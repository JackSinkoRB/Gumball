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
        [SerializeField] private AutosizeTextMeshPro descriptionLabel;

        private Challenge challenge;
        
        public void Initialise(Challenge challenge)
        {
            this.challenge = challenge;
            
            descriptionLabel.text = challenge.Description;
            descriptionLabel.Resize();

            ChallengeTracker.Listener challengeListener = challenge.Tracker.GetListener(challenge.ChallengeID);
            claimButton.interactable = challengeListener.Progress >= 1;
        }

        public void OnClickClaimButton()
        {
            CoroutineHelper.Instance.StartCoroutine(challenge.Rewards.GiveRewards());
        }

    }
}
