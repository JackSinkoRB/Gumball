using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Experience Manager")]
    public class ExperienceManager : SingletonScriptable<ExperienceManager>
    {

        #region SCRIPTABLE SETTINGS

        [SerializeField] private PlayerLevel[] levels;

        public PlayerLevel[] Levels => levels;
        
        #endregion
        
        public delegate void OnLevelChangeDelegate(int previousLevel, int newLevel);
        public static OnLevelChangeDelegate onLevelChange;

        [RuntimeInitializeOnLoadMethod]
        public static void RuntimeInitialise()
        {
            onLevelChange = null;
        }
        
        public static int TotalXP
        {
            get => DataManager.Player.Get("Experience.TotalXP", 0);
            private set => DataManager.Player.Set("Experience.TotalXP", value);
        }

        public static int Level => GetLevelIndexFromTotalXP(TotalXP) + 1; //add 1 as using index
        
        /// <summary>
        /// Overrides the player's total XP with the value.
        /// </summary>
        public static void SetTotalXP(int xp)
        {
            int previousLevel = Level;
            TotalXP = xp;
            int newLevel = Level;
            
            if (newLevel != previousLevel)
                onLevelChange?.Invoke(previousLevel, newLevel);
        }

        public static void AddXP(int xp)
        {
            int newTotalXP = TotalXP + xp;
            SetTotalXP(newTotalXP);
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
        
    }
}
