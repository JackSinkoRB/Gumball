using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Experience Manager")]
    public class ExperienceManager : SingletonScriptable<ExperienceManager>
    {

        #region SCRIPTABLE SETTINGS
        [SerializeField] private PlayerLevel[] levels;

        public PlayerLevel[] Levels => levels;

        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            CheckToUnlockUnlockables();
        }
        
        private void CheckToUnlockUnlockables()
        {
            foreach (PlayerLevel level in levels)
            {
                if (TotalXP < level.XPRequired)
                    break; //not yet unlocked
                
                foreach (Unlockable unlockable in level.Rewards.Unlockables)
                {
                    unlockable.Unlock();
                }
            }
        }
        
#if UNITY_EDITOR
        /// <remarks>Only for testing.</remarks>
        [ButtonMethod]
        public void TestLevelUp()
        {
            AddXP(RemainingXPForNextLevel);
        }
#endif
        #endregion
        
        public delegate void OnLevelChangeDelegate(int previousLevel, int newLevel);
        public static OnLevelChangeDelegate onLevelChange;
        
        public delegate void OnXPChangeDelegate(int previousXP, int newXP);
        public static OnXPChangeDelegate onXPChange;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RuntimeInitialise()
        {
            onLevelChange = null;
            onXPChange = null;
        }
        
        public static int TotalXP
        {
            get => DataManager.Player.Get("Experience.TotalXP", 0);
            private set => DataManager.Player.Set("Experience.TotalXP", value);
        }

        public static int RemainingXPForNextLevel => GetRemainingXPForNextLevel(TotalXP);
        
        public static int Level => GetLevelIndexFromTotalXP(TotalXP) + 1; //add 1 as using index
        public static PlayerLevel PlayerLevel => GetLevelFromTotalXP(TotalXP);
        public static bool IsMaxLevel => Level >= Instance.levels.Length;
        
        public static void SetLevel(int level)
        {
            SetTotalXP(GetXPRequiredForLevel(level));
        }
        
        /// <summary>
        /// Overrides the player's total XP with the value.
        /// </summary>
        public static void SetTotalXP(int xp)
        {
            if (TotalXP == xp)
                return; //nothing changed
            
            int previousXP = TotalXP;
            int previousLevel = Level;
            TotalXP = xp;
            int newLevel = Level;
            int newXP = TotalXP;
            
            onXPChange?.Invoke(previousXP, newXP);
            
            if (newLevel != previousLevel)
                onLevelChange?.Invoke(previousLevel, newLevel);
        }

        public static void AddXP(int xp)
        {
            int newTotalXP = TotalXP + xp;
            SetTotalXP(newTotalXP);
        }

        public static int GetXPRequiredForLevel(int levelIndex)
        {
            int totalXPOfLevel = 0;
            for (int index = 0; index < Instance.levels.Length; index++)
            {
                totalXPOfLevel += Instance.levels[index].XPRequired;
                if (index == levelIndex)
                    return totalXPOfLevel;
            }

            throw new ArgumentOutOfRangeException(nameof(levelIndex), $"Level {levelIndex} is not an existing level.");
        }

        public static float GetPercentToNextLevel(int totalXP)
        {
            int currentLevelIndex = GetLevelIndexFromTotalXP(totalXP);
            int nextLevelIndex = currentLevelIndex + 1;

            if (nextLevelIndex >= Instance.levels.Length)
                return 0; //is max level
            
            int totalXPForCurrentLevel = GetXPRequiredForLevel(currentLevelIndex);
            int totalXPForNextLevel = GetXPRequiredForLevel(nextLevelIndex);
            
            int xpRequiredForLevelUp = totalXPForNextLevel - totalXPForCurrentLevel;
            int xpGainedSinceLastLevel = totalXP - totalXPForCurrentLevel;
            
            return Mathf.Clamp01((float)xpGainedSinceLastLevel / xpRequiredForLevelUp);
        }
        
        public static int GetRemainingXPForNextLevel(int currentTotalXP)
        {
            int currentLevelIndex = GetLevelIndexFromTotalXP(currentTotalXP);
            int nextLevelIndex = currentLevelIndex + 1;

            int totalXPForCurrentLevel = GetXPRequiredForLevel(currentLevelIndex);
            int totalXPForNextLevel = GetXPRequiredForLevel(nextLevelIndex);
            
            int xpGainedSinceLastLevel = currentTotalXP - totalXPForCurrentLevel;
            
            return totalXPForNextLevel - totalXPForCurrentLevel - xpGainedSinceLastLevel;
        }
        
        public static PlayerLevel GetLevelFromTotalXP(int totalXP)
        {
            int index = GetLevelIndexFromTotalXP(totalXP);
            return Instance.levels[index]; //if totalXP exceeds all levels, return the max level
        }

        public static PlayerLevel GetLevelFromIndex(int index)
        {
            return Instance.levels[index];
        }
        
        public static int GetLevelIndexFromTotalXP(int totalXP)
        {
            int totalXPOfLevel = 0;
            for (int currentLevel = 0; currentLevel < Instance.levels.Length; currentLevel++)
            {
                totalXPOfLevel += Instance.levels[currentLevel].XPRequired;
                if (totalXPOfLevel > totalXP)
                    return currentLevel - 1;
            }
            
            return Instance.levels.Length - 1; //if totalXP exceeds all levels, return the max level
        }

    }
}
