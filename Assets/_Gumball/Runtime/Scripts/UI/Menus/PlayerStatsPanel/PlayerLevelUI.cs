using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class PlayerLevelUI : MonoBehaviour
    {

        [SerializeField] private AutosizeTextMeshPro levelLabel;
        [SerializeField] private Image experienceBar;

        [SerializeField] private float progressBarFillStart = 0.1f;
        [SerializeField] private float progressBarFillEnd = 0.9f;

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

        private void RefreshLevelLabel()
        {
            levelLabel.text = $"{ExperienceManager.Level}";
            this.PerformAtEndOfFrame(levelLabel.Resize);
        }

        private void RefreshExperienceBar()
        {
            float endsPercent = 1 - (progressBarFillStart + progressBarFillEnd);
            experienceBar.fillAmount = progressBarFillStart + (endsPercent * ExperienceManager.GetPercentToNextLevel(ExperienceManager.TotalXP));
        }
        
    }
}
