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
            levelLabel.text = $"{ExperienceManager.LevelValue}";
        }

        private void RefreshExperienceBar()
        {
            int currentLevelIndex = ExperienceManager.LevelValue;
            int nextLevelIndex = currentLevelIndex + 1;

            int totalXPForCurrentLevel = ExperienceManager.GetXPRequiredForLevel(currentLevelIndex);
            int totalXPForNextLevel = ExperienceManager.GetXPRequiredForLevel(nextLevelIndex);

            int xpRequiredForLevelUp = totalXPForNextLevel - totalXPForCurrentLevel;
            int xpGainedSinceLastLevel = ExperienceManager.TotalXP - totalXPForCurrentLevel;
            
            experienceBar.fillAmount = Mathf.Clamp01((float)xpGainedSinceLastLevel / xpRequiredForLevelUp);
        }
        
    }
}
