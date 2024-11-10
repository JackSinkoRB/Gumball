using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Parts/Core Part")]
    public class CorePart : UniqueScriptableObject
    {

        public enum PartType
        {
            ENGINE,
            WHEELS,
            DRIVETRAIN
        }

        [Header("Details")]
        [SerializeField] private PartType type;
        [SerializeField] private CarType carType;
        [SerializeField] private string displayName;
        [SerializeField] private string description = "This is the description for the core part.";
        [SerializeField] private Sprite icon;
        
        [Header("Cost")]
        [Tooltip("This is the cost to install the core part on a car.")]
        [SerializeField] private int standardCurrencyInstallCost = 500;
        
        [Header("SubParts")]
        [SerializeField] private SubPartSlot[] subPartSlots = Array.Empty<SubPartSlot>();
        [SerializeField] private CorePartLevel[] levels;

        [Header("Modifiers")]
        [SerializeField] private CarPerformanceProfileModifiers performanceModifiers;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<GameSession> sessionsThatGiveReward = new();
        
        public string SaveKey => $"{type.ToString()}-{name}-{ID}";

        public PartType Type => type;
        public CarType CarType => carType;
        public string DisplayName => displayName.IsNullOrEmpty() ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int StandardCurrencyInstallCost => standardCurrencyInstallCost;
        public SubPartSlot[] SubPartSlots => subPartSlots;
        public ReadOnlyCollection<GameSession> SessionsThatGiveReward => sessionsThatGiveReward.AsReadOnly();
        public CorePartLevel[] Levels => levels;
        
        public bool IsUnlocked
        {
            get => DataManager.Cars.Get($"Parts.Core.{SaveKey}.IsUnlocked", false);
            private set => DataManager.Cars.Set($"Parts.Core.{SaveKey}.IsUnlocked", value);
        }
        
        public int CarBelongsToIndex
        {
            get => DataManager.Cars.Get($"Parts.Core.{SaveKey}.CarBelongsToIndex", -1);
            private set => DataManager.Cars.Set($"Parts.Core.{SaveKey}.CarBelongsToIndex", value);
        }

        public int CurrentLevelIndex
        {
            get => DataManager.Cars.Get($"Parts.Core.{SaveKey}.LevelIndex", 0);
            private set => DataManager.Cars.Set($"Parts.Core.{SaveKey}.LevelIndex", value);
        }

        public bool IsAppliedToCar => CarBelongsToIndex != -1;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (displayName.IsNullOrEmpty())
                displayName = name;

            if (levels != null)
            {
                foreach (CorePartLevel level in levels)
                {
                    level.SetupInspector(this);
                }
            }

            CheckRewardsAreStillTracked();
        }
        
        private void CheckRewardsAreStillTracked()
        {
            for (int index = sessionsThatGiveReward.Count - 1; index >= 0; index--)
            {
                GameSession session = sessionsThatGiveReward[index];
                if (session == null || !session.Rewards.CoreParts.Contains(this))
                    UntrackAsReward(session);
            }
        }

        public void TrackAsReward(GameSession session)
        {
            if (sessionsThatGiveReward.Contains(session))
                return; //already tracked
            
            sessionsThatGiveReward.Add(session);
            
            EditorUtility.SetDirty(this);
        }
        
        public void UntrackAsReward(GameSession session)
        {
            if (!sessionsThatGiveReward.Contains(session))
                return; //already not tracked
            
            sessionsThatGiveReward.Remove(session);
            
            EditorUtility.SetDirty(this);
        }
#endif

        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
        }
        
        public void Upgrade()
        {
            //TODO
            //consume all sub parts

            CurrentLevelIndex++;
        }

        public void ApplyToCar(int carIndex)
        {
            if (IsAppliedToCar)
            {
                Debug.LogError($"Trying to apply core part {name} to carIndex {carIndex}, but it is already applied to {CarBelongsToIndex}");
                return;
            }

            CarBelongsToIndex = carIndex;
        }

        public void RemoveFromCar()
        {
            if (!IsAppliedToCar)
            {
                Debug.LogWarning($"Trying to remove core part {name} from car, but it is not applied to a car.");
                return;
            }

            //update the cars performance profile if it's the active car
            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == CarBelongsToIndex;
            if (isAttachedToCurrentCar)
                WarehouseManager.Instance.CurrentCar.SetPerformanceProfile(new CarPerformanceProfile(CarBelongsToIndex));
            
            CarBelongsToIndex = -1;
        }
        
        public CarPerformanceProfileModifiers GetTotalModifiers()
        {
            CarPerformanceProfileModifiers subPartModifiers = new CarPerformanceProfileModifiers();
            if (subPartSlots == null)
                return subPartModifiers;
            
            foreach (SubPartSlot slot in subPartSlots)
            {
                SubPart subPart = slot.CurrentSubPart;
                if (subPart == null)
                    continue; //nothing applied
                
                subPartModifiers += subPart.CorePartModifiers;
            }
            
            return subPartModifiers * performanceModifiers;
        }

        public void InitialiseSubPartSlots()
        {
            if (subPartSlots == null)
                return;
            
            for (int saveKeyID = 0; saveKeyID < subPartSlots.Length; saveKeyID++) //index is the ID
            {
                SubPartSlot slot = subPartSlots[saveKeyID];
                
                slot.Initialise(this, saveKeyID);
            }
        }

        public bool AreAllSubPartsInstalled()
        {
            if (subPartSlots.Length == 0)
                return false; //no sub part slots, so no sub parts installed
            
            foreach (SubPartSlot slot in subPartSlots)
            {
                if (slot.CurrentSubPart == null)
                    return false;
            }

            return true;
        }
        
    }
}
