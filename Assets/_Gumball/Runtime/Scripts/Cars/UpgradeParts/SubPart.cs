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

    public static class SubPartTypeHelper
    {
        public static CorePart.PartType GetCoreType(this SubPart.SubPartType subPartType)
        {
            string subPartTypeName = subPartType.ToString();
            int underscoreIndex = subPartTypeName.IndexOf('_');
            string corePartTypeName = subPartTypeName.Substring(0, underscoreIndex);
            CorePart.PartType corePartType = Enum.Parse<CorePart.PartType>(corePartTypeName);
            return corePartType;
        }

        public static string ToFriendlyString(this SubPart.SubPartType subPartType)
        {
            string subPartTypeName = subPartType.ToString();
            int underscoreIndex = subPartTypeName.IndexOf('_');
            string name = subPartTypeName.Substring(underscoreIndex + 1);
            return name;
        }
    }
    
    [CreateAssetMenu(menuName = "Gumball/Parts/Sub Part")]
    public class SubPart : UniqueScriptableObject
    {

        public enum SubPartType
        {
            ENGINE_ECU,
            ENGINE_Intake,
            ENGINE_Exhaust,
            ENGINE_NOS,
            ENGINE_Turbo,
            WHEELS_Tyres,
            WHEELS_Brakes,
            WHEELS_Coilovers,
            DRIVETRAIN_Gearbox,
            DRIVETRAIN_Clutch,
            DRIVETRAIN_Differential
        }

        public enum SubPartRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Exotic,
            Legendary
        }

        [Header("Details")]
        [SerializeField] private SubPartType type;
        [SerializeField] private SubPartRarity rarity;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        [Header("Cost")]
        [Tooltip("This is the cost to install the sub part on a core part.")]
        [SerializeField] private int standardCurrencyInstallCost = 50;
        
        [Header("Modifiers")]
        [Tooltip("How much does this sub part contribute to the core parts total modifier?")]
        [SerializeField] private CarPerformanceProfileModifiers corePartModifiers;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<GameSession> sessionsThatGiveReward = new();
        
        private string saveKey => $"{type.ToString()}-{name}-{ID}";

        public SubPartType Type => type;
        public SubPartRarity Rarity => rarity;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int StandardCurrencyInstallCost => standardCurrencyInstallCost;
        public CarPerformanceProfileModifiers CorePartModifiers => corePartModifiers;
        public ReadOnlyCollection<GameSession> SessionsThatGiveReward => sessionsThatGiveReward.AsReadOnly();
        
        public bool IsUnlocked
        {
            get => DataManager.Cars.Get($"Parts.Sub.{saveKey}.IsUnlocked", false);
            private set => DataManager.Cars.Set($"Parts.Sub.{saveKey}.IsUnlocked", value);
        }
        
        public CorePart CorePartBelongsTo
        {
            get => CorePartManager.GetPartByID(DataManager.Cars.Get<string>($"Parts.Sub.{saveKey}.CorePartBelongsTo", null));
            private set => DataManager.Cars.Set($"Parts.Sub.{saveKey}.CorePartBelongsTo", value == null ? null : value.ID);
        }
        
        public bool IsConsumed
        {
            get => DataManager.Cars.Get($"Parts.Sub.{saveKey}.IsConsumed", false);
            private set => DataManager.Cars.Set($"Parts.Sub.{saveKey}.IsConsumed", value);
        }

        public bool IsAppliedToCorePart => CorePartBelongsTo != null;
            
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (displayName.IsNullOrEmpty())
                displayName = name;

            CheckRewardsAreStillTracked();
        }

        private void CheckRewardsAreStillTracked()
        {
            for (int index = sessionsThatGiveReward.Count - 1; index >= 0; index--)
            {
                GameSession session = sessionsThatGiveReward[index];
                if (session == null || !session.Rewards.SubParts.Contains(this))
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

        public void SetConsumed(bool consumed)
        {
            IsConsumed = consumed;
        }
        
        public void ApplyToCorePart(CorePart corePart)
        {
            if (IsAppliedToCorePart)
            {
                Debug.LogError($"Trying to apply sub part {name} to core part {corePart.name}, but it is already applied to {CorePartBelongsTo.name}");
                return;
            }

            CorePartBelongsTo = corePart;
        }
        
        public void RemoveFromCorePart()
        {
            if (!IsAppliedToCorePart)
            {
                Debug.LogWarning($"Trying to remove sub part {name} from core part, but it is not applied to a core part.");
                return;
            }

            //update the cars performance profile if it's the active car
            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == CorePartBelongsTo.CarBelongsToIndex;
            if (isAttachedToCurrentCar)
                WarehouseManager.Instance.CurrentCar.SetPerformanceProfile(new CarPerformanceProfile(CorePartBelongsTo.CarBelongsToIndex));
            
            CorePartBelongsTo = null;
        }
        
    }
}
