using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class PlayerLevelUI : MonoBehaviour
    {

        [SerializeField] private AutosizeTextMeshPro levelLabel;
        [Space(5)]
        [SerializeField] private float progressBarFillStart = 0.1f;
        [SerializeField] private float progressBarFillEnd = 0.9f;
        [Space(5)]
        [SerializeField] private Image experienceBar;
        [SerializeField] private float experienceBarTweenDuration;
        [SerializeField] private Ease experienceBarTweenEase;

        public float ExperienceBarTweenDuration => experienceBarTweenDuration;
        public Ease ExperienceBarTweenEase => experienceBarTweenEase;
        
        private Tween experienceBarTween;
        
        private void OnEnable()
        {
            Refresh();

            ExperienceManager.onXPChange += OnXPChange;
            ExperienceManager.onLevelChange += OnLevelChange;
        }

        private void OnDisable()
        {
            ExperienceManager.onXPChange -= OnXPChange;
            ExperienceManager.onLevelChange -= OnLevelChange;
        }
        
        private void OnXPChange(int previousXP, int newXP)
        {
            RefreshExperienceBar();
        }

        private void OnLevelChange(int previousLevel, int newLevel)
        {
            RefreshLevelLabel();
        }

        public void Refresh()
        {
            RefreshLevelLabel();
            RefreshExperienceBar();
        }

        public void TweenExperienceBar(int previousTotalXP, int newTotalXP)
        {
            levelLabel.text = $"{ExperienceManager.GetLevelIndexFromTotalXP(previousTotalXP) + 1}";

            int previousLevel = ExperienceManager.GetLevelIndexFromTotalXP(previousTotalXP);
            int newLevel = ExperienceManager.GetLevelIndexFromTotalXP(newTotalXP);
            
            float startingFill = GetBarFillAmount(Mathf.Max(0, previousTotalXP));
            float desiredFill = previousLevel == newLevel ? GetBarFillAmount(Mathf.Max(0, newTotalXP)) : 1; //if increasing level, just tween to full
            
            experienceBar.fillAmount = startingFill;

            experienceBarTween?.Kill();
            experienceBarTween = experienceBar.DOFillAmount(desiredFill, experienceBarTweenDuration).SetEase(experienceBarTweenEase);
        }
        
        private float GetBarFillAmount(int totalXP)
        {
            float difference = progressBarFillEnd - progressBarFillStart;
            return progressBarFillStart + (difference * ExperienceManager.GetPercentToNextLevel(totalXP));
        }
        
        private void RefreshLevelLabel()
        {
            levelLabel.text = $"{ExperienceManager.Level}";
            this.PerformAtEndOfFrame(levelLabel.Resize);
        }

        private void RefreshExperienceBar()
        {
            experienceBar.fillAmount = GetBarFillAmount(ExperienceManager.TotalXP);
        }
        
    }
}
