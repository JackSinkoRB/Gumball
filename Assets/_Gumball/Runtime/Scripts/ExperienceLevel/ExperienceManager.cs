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
        
        public static int LevelValue => GetLevelIndexFromTotalXP(TotalXP) + 1; //add 1 as using index
        public static PlayerLevel Level => GetLevelFromTotalXP(TotalXP);
        
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
            int previousLevel = LevelValue;
            TotalXP = xp;
            int newLevel = LevelValue;
            int newXP = TotalXP;
            
            onXPChange?.Invoke(previousXP, newXP);
            
            if (newLevel != previousLevel)
            {
                onLevelChange?.Invoke(previousLevel, newLevel);

                if (newLevel > previousLevel)
                    OnLevelUp(previousLevel, newLevel);
            }
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

            throw new ArgumentOutOfRangeException(nameof(levelIndex));
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
        
        private static void OnLevelUp(int previousLevel, int newLevel)
        {
            CoroutineHelper.Instance.StartCoroutine(OnLevelUpIE(previousLevel, newLevel));
        }

        private static IEnumerator OnLevelUpIE(int previousLevel, int newLevel)
        {
            //give rewards for all the levels in between
            for (int level = previousLevel + 1; level <= newLevel; level++)
            {
                int levelIndex = level - 1;
                yield return Instance.levels[levelIndex].GiveRewards();
            }
        }
        
    }
}
