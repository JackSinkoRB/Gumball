using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Blueprint Manager")]
    public class BlueprintManager : SingletonScriptable<BlueprintManager>
    {

        [SerializeField] private List<int> blueprintsRequiredForEachLevel = new();

        public int GetBlueprints(int carIndex)
        {
            return DataManager.Cars.Get(GetSaveKey(carIndex), 0);
        }
        
        public void SetBlueprints(int carIndex, int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("Cannot set blueprints below 0");
                amount = 0;
            }
            
            DataManager.Cars.Set(GetSaveKey(carIndex), amount);
        }

        public void AddBlueprints(int carIndex, int amount)
        {
            SetBlueprints(carIndex, GetBlueprints(carIndex) + amount);
        }

        public bool IsUnlocked(int carIndex)
        {
            return GetLevelIndex(carIndex) >= 0;
        }
        
        public int GetLevelIndex(int carIndex)
        {
            return GetLevelIndexFromBlueprints(GetBlueprints(carIndex));
        }
        
        /// <returns>The current level, or 0 if not enough enough to be unlocked.</returns>
        public int GetLevelIndexFromBlueprints(int blueprints)
        {
            for (int count = 0; count < blueprintsRequiredForEachLevel.Count; count++)
            {
                int levelIndex = count - 1;
                int nextLevelBlueprints = blueprintsRequiredForEachLevel[count];
                if (nextLevelBlueprints > blueprints)
                    return levelIndex;
            }

            int maxLevelIndex = blueprintsRequiredForEachLevel.Count - 1;
            return maxLevelIndex;
        }

        private string GetSaveKey(int carIndex)
        {
            return $"Blueprints.{carIndex}";
        }
        
    }
}
