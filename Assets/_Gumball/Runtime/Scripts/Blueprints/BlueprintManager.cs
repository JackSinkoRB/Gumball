using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEditor.Build;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Blueprint Manager")]
    public class BlueprintManager : SingletonScriptable<BlueprintManager>
#if UNITY_EDITOR
        , IPreprocessBuildWithReport
#endif
    {

        public delegate void OnValueChangeDelegate(int carIndex, int previousAmount, int newAmount);
        public static event OnValueChangeDelegate onBlueprintsChange;
        public static event OnValueChangeDelegate onLevelChange;
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            onBlueprintsChange = null;
        }
        
        [SerializeField] private List<int> blueprintsRequiredForEachLevel = new();

        private readonly GenericDictionary<int, List<GameSession>> sessionsThatGiveCarBlueprintCache = new(); //car index, collection of sessions
        
        public List<int> BlueprintsRequiredForEachLevel => blueprintsRequiredForEachLevel;

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

        public int GetNextLevel(int carIndex)
        {
            int unlockLevelIndex = WarehouseManager.Instance.AllCarData[carIndex].StartingLevelIndex;
            int currentLevelIndex = GetLevelIndex(carIndex);
            int nextLevelIndex = IsUnlocked(carIndex) ? currentLevelIndex + 1 : unlockLevelIndex;
            return nextLevelIndex;
        }

        public int GetBlueprintsRequiredForLevel(int levelIndex)
        {
            return blueprintsRequiredForEachLevel[levelIndex];
        }
        
#if UNITY_EDITOR
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            CacheSessionsThatGiveBlueprints();
        }
        
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
            
            Debug.Log($"Found {sessionsFoundWithBlueprints} sessions with blueprints.");
            
            //ensure values are saved
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
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
