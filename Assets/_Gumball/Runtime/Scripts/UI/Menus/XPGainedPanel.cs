using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class XPGainedPanel : AnimatedPanel
    {

        [SerializeField] private PlayerLevelUI playerLevelUI;
        [SerializeField] private TextMeshProUGUI xpGainedLabel;
        
        private Tween experienceLabelTween;

        public void Initialise(int previousTotalXP, int newTotalXP)
        {
            playerLevelUI.TweenExperienceBar(previousTotalXP, newTotalXP);
            TweenExperienceLabel(newTotalXP - previousTotalXP);
        }

        private void TweenExperienceLabel(int xpGained)
        {
            experienceLabelTween?.Kill();
            xpGainedLabel.text = "0";
            experienceLabelTween = DOTween.To(() => int.Parse(xpGainedLabel.text), value => xpGainedLabel.text = $"+{value}", xpGained, playerLevelUI.ExperienceBarTweenDuration)
                .SetEase(playerLevelUI.ExperienceBarTweenEase);
        }

    }
}