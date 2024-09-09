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

        [SerializeField] private AutosizeTextMeshPro levelLabel;
        [SerializeField] private TextMeshProUGUI xpGainedLabel;
        [Space(5)]
        [SerializeField] private Image experienceBar;
        [SerializeField] private float experienceBarTweenDuration;
        [SerializeField] private Ease experienceBarTweenEase;
        
        private Tween experienceBarTween;
        private Tween experienceLabelTween;

        public void Initialise(int previousTotalXP, int newTotalXP)
        {
            TweenExperienceBar(previousTotalXP, newTotalXP);
            TweenExperienceLabel(newTotalXP - previousTotalXP);
        }

        private void TweenExperienceLabel(int xpGained)
        {
            experienceLabelTween?.Kill();
            xpGainedLabel.text = "0";
            experienceLabelTween = DOTween.To(() => int.Parse(xpGainedLabel.text), value => xpGainedLabel.text = $"+{value}", xpGained, experienceBarTweenDuration)
                .SetEase(experienceBarTweenEase);
        }
        
        private void TweenExperienceBar(int previousTotalXP, int newTotalXP)
        {
            levelLabel.text = $"{ExperienceManager.GetLevelIndexFromTotalXP(previousTotalXP) + 1}";

            int previousLevel = ExperienceManager.GetLevelIndexFromTotalXP(previousTotalXP);
            int newLevel = ExperienceManager.GetLevelIndexFromTotalXP(newTotalXP);
            
            float startingFill = ExperienceManager.GetPercentToNextLevel(Mathf.Max(0, previousTotalXP));
            float desiredFill = previousLevel == newLevel ? ExperienceManager.GetPercentToNextLevel(Mathf.Max(0, newTotalXP)) : 1; //if increasing level, just tween to full
            
            experienceBar.fillAmount = startingFill;

            experienceBarTween?.Kill();
            experienceBarTween = experienceBar.DOFillAmount(desiredFill, experienceBarTweenDuration).SetEase(experienceBarTweenEase);
        }

    }
}