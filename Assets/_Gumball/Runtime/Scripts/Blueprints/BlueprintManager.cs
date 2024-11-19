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

        public delegate void OnValueChangeDelegate(string carGUID, int previousAmount, int newAmount);
        public static event OnValueChangeDelegate onBlueprintsChange;
        public static event OnValueChangeDelegate onLevelChange;
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            onBlueprintsChange = null;
        }
        
        [SerializeField] private List<Level> levels = new();

        [Header("Debugging")]
        [SerializeField, ReadOnly] private GenericDictionary<string, List<GameSession>> sessionsThatGiveCarBlueprintCache = new(); //car GUID, collection of sessions
        
        public List<Level> Levels => levels;

        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();
            
#if UNITY_EDITOR
            CoroutineHelper.PerformAfterTrue(
                () => GumballEventManager.HasLoaded, 
                CacheSessionsThatGiveBlueprints);
#endif
        }

        public int GetBlueprints(string carGUID)
        {
            return DataManager.Cars.Get($"{GetSaveKey(carGUID)}.Amount", 0);
        }

        public void SetBlueprints(string carGUID, int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("Cannot set blueprints below 0");
                amount = 0;
            }

            int previous = GetBlueprints(carGUID);
            if (previous == amount)
                return; //no change
            
            DataManager.Cars.Set($"{GetSaveKey(carGUID)}.Amount", amount);
            
            onBlueprintsChange?.Invoke(carGUID, previous, amount);
        }
        
        public void AddBlueprints(string carGUID, int amount)
        {
            SetBlueprints(carGUID, GetBlueprints(carGUID) + amount);
        }
        
        public void TakeBlueprints(string carGUID, int amount)
        {
            int newAmount = GetBlueprints(carGUID) - amount;
            if (newAmount < 0)
            {
                newAmount = 0;
                Debug.LogError("Tried setting blueprints below 0 - this shouldn't happen.");
            }

            SetBlueprints(carGUID, newAmount);
        }
        
        public int GetLevelIndex(string carGUID)
        {
            int startingLevelIndex = WarehouseManager.Instance.GetCarDataFromGUID(carGUID).StartingLevelIndex;
            return DataManager.Cars.Get($"{GetSaveKey(carGUID)}.LevelIndex", startingLevelIndex);
        }
        
        public void SetLevelIndex(string carGUID, int levelIndex)
        {
            if (levelIndex < -1)
            {
                Debug.LogError("Cannot set level below -1");
                levelIndex = -1;
            }

            int previous = GetLevelIndex(carGUID);
            if (previous == levelIndex)
                return; //no change
            
            DataManager.Cars.Set($"{GetSaveKey(carGUID)}.LevelIndex", levelIndex);
            
            onLevelChange?.Invoke(carGUID, previous, levelIndex);
        }

        public bool IsUnlocked(string carGUID)
        {
            return GetLevelIndex(carGUID) >= GetLevelToUnlock(carGUID);
        }

        public int GetLevelToUnlock(string carGUID)
        {
            return WarehouseManager.Instance.GetCarDataFromGUID(carGUID).StartingLevelIndex;
        }

        public int GetNextLevelIndex(string carGUID)
        {
            int unlockLevelIndex = WarehouseManager.Instance.GetCarDataFromGUID(carGUID).StartingLevelIndex;
            int currentLevelIndex = GetLevelIndex(carGUID);
            int nextLevelIndex = IsUnlocked(carGUID) ? currentLevelIndex + 1 : unlockLevelIndex;
            return nextLevelIndex > WarehouseManager.Instance.GetCarDataFromGUID(carGUID).MaxLevelIndex ? -1 : nextLevelIndex;
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
                        if (blueprintReward.CarGUID == null)
                            return; //not initialised
                        
                        List<GameSession> sessions = sessionsThatGiveCarBlueprintCache.ContainsKey(blueprintReward.CarGUID)
                            ? sessionsThatGiveCarBlueprintCache[blueprintReward.CarGUID]
                            : new List<GameSession>();

                        if (sessions.Contains(node.GameSession))
                            continue; //already exists on another node
                        
                        sessions.Add(node.GameSession);
                        sessionsFoundWithBlueprints++;
                        
                        sessionsThatGiveCarBlueprintCache[blueprintReward.CarGUID] = sessions;
                    }
                }
            }
            
            Debug.Log($"Caching sessions with blueprints: found {sessionsFoundWithBlueprints} sessions");
            
            //ensure values are saved
            EditorUtility.SetDirty(this);
        }
#endif
        
        public List<GameSession> GetSessionsThatGiveBlueprint(string carGUID)
        {
            return sessionsThatGiveCarBlueprintCache.ContainsKey(carGUID) ? sessionsThatGiveCarBlueprintCache[carGUID] : new List<GameSession>();
        }
        
        private string GetSaveKey(string carGUID)
        {
            return $"Blueprints.{carGUID}";
        }
        
    }
}
