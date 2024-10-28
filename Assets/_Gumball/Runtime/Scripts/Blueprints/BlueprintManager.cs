using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Blueprint Manager")]
    public class BlueprintManager : SingletonScriptable<BlueprintManager>
    {

        [Serializable]
        public struct Level
        {
            [SerializeField] private int blueprintsRequired;
            [SerializeField] private int standardCurrencyCostToUpgrade;

            public int BlueprintsRequired => blueprintsRequired;
            public int StandardCurrencyCostToUpgrade => standardCurrencyCostToUpgrade;
        }

        public delegate void OnValueChangeDelegate(int carIndex, int previousAmount, int newAmount);
        public static event OnValueChangeDelegate onBlueprintsChange;
        public static event OnValueChangeDelegate onLevelChange;
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            onBlueprintsChange = null;
        }
        
        [SerializeField] private List<Level> levels = new();

        [Header("Debugging")]
        [SerializeField, ReadOnly] private GenericDictionary<int, List<GameSession>> sessionsThatGiveCarBlueprintCache = new(); //car index, collection of sessions
        
        public List<Level> Levels => levels;

        public int MaxLevelIndex => levels.Count - 1;
        
        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();
            
#if UNITY_EDITOR
            CoroutineHelper.PerformAfterTrue(
                () => GumballEventManager.HasLoaded, 
                CacheSessionsThatGiveBlueprints);
#endif
        }

        public int GetBlueprints(int carIndex)
        {
            return DataManager.Cars.Get($"{GetSaveKey(carIndex)}.Amount", 0);
        }

        public void SetBlueprints(int carIndex, int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("Cannot set blueprints below 0");
                amount = 0;
            }

            int previous = GetBlueprints(carIndex);
            if (previous == amount)
                return; //no change
            
            DataManager.Cars.Set($"{GetSaveKey(carIndex)}.Amount", amount);
            
            onBlueprintsChange?.Invoke(carIndex, previous, amount);
        }
        
        public void AddBlueprints(int carIndex, int amount)
        {
            SetBlueprints(carIndex, GetBlueprints(carIndex) + amount);
        }
        
        public void TakeBlueprints(int carIndex, int amount)
        {
            int newAmount = GetBlueprints(carIndex) - amount;
            if (newAmount < 0)
            {
                newAmount = 0;
                Debug.LogError("Tried setting blueprints below 0 - this shouldn't happen.");
            }

            SetBlueprints(carIndex, newAmount);
        }
        
        public int GetLevelIndex(int carIndex)
        {
            bool isUnlocked = WarehouseManager.Instance.AllCarData[carIndex].IsUnlocked;
            int startingLevel = WarehouseManager.Instance.AllCarData[carIndex].StartingLevelIndex;
            return DataManager.Cars.Get($"{GetSaveKey(carIndex)}.LevelIndex", isUnlocked ? startingLevel : -1);
        }
        
        public void SetLevelIndex(int carIndex, int levelIndex)
        {
            if (levelIndex < -1)
            {
                Debug.LogError("Cannot set level below -1");
                levelIndex = -1;
            }

            int previous = GetLevelIndex(carIndex);
            if (previous == levelIndex)
                return; //no change
            
            DataManager.Cars.Set($"{GetSaveKey(carIndex)}.LevelIndex", levelIndex);
            
            onLevelChange?.Invoke(carIndex, previous, levelIndex);
        }

        public bool IsUnlocked(int carIndex)
        {
            return GetLevelIndex(carIndex) >= GetLevelToUnlock(carIndex);
        }

        public int GetLevelToUnlock(int carIndex)
        {
            return WarehouseManager.Instance.AllCarData[carIndex].StartingLevelIndex;
        }

        public int GetNextLevelIndex(int carIndex)
        {
            int unlockLevelIndex = WarehouseManager.Instance.AllCarData[carIndex].StartingLevelIndex;
            int currentLevelIndex = GetLevelIndex(carIndex);
            int nextLevelIndex = IsUnlocked(carIndex) ? currentLevelIndex + 1 : unlockLevelIndex;
            return nextLevelIndex > MaxLevelIndex ? -1 : nextLevelIndex;
        }
        
#if UNITY_EDITOR
        public void CacheSessionsThatGiveBlueprints()
        {
            int sessionsFoundWithBlueprints = 0;
            sessionsThatGiveCarBlueprintCache.Clear();

            //loop over each map and node to find the game sessions
            foreach (AssetReferenceGameObject mapReference in GumballEventManager.Instance.CurrentEvent.Maps)
            {
                GameSessionMap map = mapReference.editorAsset.GetComponent<GameSessionMap>();
                foreach (GameSessionNode node in map.Nodes)
                {
                    //cache the blueprint reward car
                    foreach (BlueprintReward blueprintReward in node.GameSession.Rewards.Blueprints)
                    {
                        List<GameSession> sessions = sessionsThatGiveCarBlueprintCache.ContainsKey(blueprintReward.CarIndex)
                            ? sessionsThatGiveCarBlueprintCache[blueprintReward.CarIndex]
                            : new List<GameSession>();

                        if (sessions.Contains(node.GameSession))
                            continue; //already exists on another node
                        
                        sessions.Add(node.GameSession);
                        sessionsFoundWithBlueprints++;
                        
                        sessionsThatGiveCarBlueprintCache[blueprintReward.CarIndex] = sessions;
                    }
                }
            }
            
            Debug.Log($"Caching sessions with blueprints: found {sessionsFoundWithBlueprints} sessions");
            
            //ensure values are saved
            EditorUtility.SetDirty(this);
        }
#endif
        
        public List<GameSession> GetSessionsThatGiveBlueprint(int carIndex)
        {
            return sessionsThatGiveCarBlueprintCache.ContainsKey(carIndex) ? sessionsThatGiveCarBlueprintCache[carIndex] : new List<GameSession>();
        }
        
        private string GetSaveKey(int carIndex)
        {
            return $"Blueprints.{carIndex}";
        }
        
    }
}
