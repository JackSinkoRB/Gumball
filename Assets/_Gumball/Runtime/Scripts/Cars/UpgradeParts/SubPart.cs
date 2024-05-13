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

        [SerializeField] private SubPartType type;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        private string saveKey => $"{type.ToString()}-{name}-{ID}";

        public SubPartType Type => type;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        
        public bool IsUnlocked
        {
            get => DataManager.Cars.Get($"Parts.Core.{saveKey}.IsUnlocked", false);
            private set => DataManager.Cars.Set($"Parts.Core.{saveKey}.IsUnlocked", value);
        }
        
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
        
    }
}
