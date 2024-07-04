using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class XPGainedPanel : AnimatedPanel
    {

        [SerializeField] private AutosizeTextMeshPro levelLabel;
        [Space(5)]
        [SerializeField] private Image experienceBar;
        [SerializeField] private float experienceBarTweenDuration;
        [SerializeField] private Ease experienceBarTweenEase;

        public void TweenExperienceBar(int previousTotalXP, int newTotalXP)
        {
            levelLabel.text = $"{ExperienceManager.GetLevelIndexFromTotalXP(previousTotalXP) + 1}";

            int previousLevel = ExperienceManager.GetLevelIndexFromTotalXP(previousTotalXP);
            int newLevel = ExperienceManager.GetLevelIndexFromTotalXP(newTotalXP);
            
            float startingFill = ExperienceManager.GetPercentToNextLevel(Mathf.Max(0, previousTotalXP));
            float desiredFill = previousLevel == newLevel ? ExperienceManager.GetPercentToNextLevel(Mathf.Max(0, newTotalXP)) : 1; //if increasing level, just tween to full
            
            experienceBar.fillAmount = startingFill;
            experienceBar.DOFillAmount(desiredFill, experienceBarTweenDuration).SetEase(experienceBarTweenEase);
        }

    }
}