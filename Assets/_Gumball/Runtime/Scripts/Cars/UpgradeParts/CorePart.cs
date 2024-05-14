using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
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

        [SerializeField] private PartType type;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        [Header("SubParts")]
        [SerializeField] private SubPartSlot[] subPartSlots;
        [SerializeField] private CorePartLevel[] levels;
        
        [Header("Modifiers")]
        [Tooltip("The amount of peak torque to add to the car.")]
        [SerializeField] private float peakTorqueAddition;
        
        public string SaveKey => $"{type.ToString()}-{name}-{ID}";

        public PartType Type => type;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public SubPartSlot[] SubPartSlots => subPartSlots;
        public float PeakTorqueAddition => peakTorqueAddition;
        
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

        public bool IsAppliedToCar => CarBelongsToIndex != -1;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (displayName.IsNullOrEmpty())
                displayName = name;

            foreach (CorePartLevel level in levels)
            {
                level.SetupInspector(this);
            }
        }
#endif

        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
        }

        public void ApplyToCar(int carIndex)
        {
            if (IsAppliedToCar)
            {
                Debug.LogError($"Trying to apply core part {name} to carIndex {carIndex}, but it is already applied to {CarBelongsToIndex}");
                return;
            }

            CarBelongsToIndex = carIndex;

            ApplySubPartsToCar();
        }

        public void RemoveFromCar()
        {
            if (!IsAppliedToCar)
            {
                Debug.LogWarning($"Trying to remove core part {name} from car, but it is not applied to a car.");
                return;
            }

            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == CarBelongsToIndex;
            if (isAttachedToCurrentCar)
            {
                //update the modifiers
                WarehouseManager.Instance.CurrentCar.PartModification.ApplyModifiers();
            }
            
            CarBelongsToIndex = -1;
        }

        private void ApplySubPartsToCar()
        {
            for (int saveKeyID = 0; saveKeyID < subPartSlots.Length; saveKeyID++) //index is the ID
            {
                SubPartSlot slot = subPartSlots[saveKeyID];
                
                slot.Initialise(this, saveKeyID);
            }
        }
        
    }
}
