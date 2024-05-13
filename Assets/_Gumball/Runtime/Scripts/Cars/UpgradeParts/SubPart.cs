using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Parts/Sub Part")]
    public class SubPart : UniqueScriptableObject
    {

        public enum SubPartType
        {
            ENGINE_ECU,
            ENGINE_INTAKE,
            ENGINE_EXHAUST,
            ENGINE_NOS,
            ENGINE_TURBO,
            WHEELS_TYRES,
            WHEELS_BRAKES,
            WHEELS_COILOVERS,
            DRIVETRAIN_GEARBOX,
            DRIVETRAIN_CLUTCH,
            DRIVETRAIN_DIFFERENTIAL
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
