using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
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

        [SerializeField] private SubPartType type;
        [SerializeField] private SubPartRarity rarity;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        private string saveKey => $"{type.ToString()}-{name}-{ID}";

        public SubPartType Type => type;
        public SubPartRarity Rarity => rarity;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        
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

        public bool IsAppliedToCorePart => CorePartBelongsTo != null;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (displayName.IsNullOrEmpty())
                displayName = name;
        }
        
        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
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

            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == CorePartBelongsTo.CarBelongsToIndex;
            if (isAttachedToCurrentCar)
            {
                //update the modifiers
                WarehouseManager.Instance.CurrentCar.PartModification.ApplyModifiers();
            }
            
            CorePartBelongsTo = null;
        }
        
    }
}
