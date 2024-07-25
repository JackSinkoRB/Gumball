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

        public void Initialise(Challenge challenge)
        {
            descriptionLabel.text = challenge.Description;
            descriptionLabel.Resize();

            claimButton.interactable = challenge.Tracker.GetListener(challenge.ChallengeID).Progress >= 1;
        }

    }
}
