using System;
using System.Collections;
using System.Collections.Generic;
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

        public enum Rating
        {
            STREET,
            SUPERCAR
        }

        [SerializeField] private PartType type;
        [SerializeField] private Rating rating;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        [Header("Modifiers")]
        [Tooltip("The amount of peak torque to add to the car.")]
        [SerializeField] private float peakTorqueAddition;
        
        private string saveKey => $"{type.ToString()}-{name}-{ID}";

        public PartType Type => type;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        
        public bool IsUnlocked
        {
            get => DataManager.Cars.Get($"Parts.Core.{saveKey}.IsUnlocked", false);
            private set => DataManager.Cars.Set($"Parts.Core.{saveKey}.IsUnlocked", value);
        }
        
        public int CarBelongsToIndex
        {
            get => DataManager.Cars.Get($"Parts.Core.{saveKey}.CarBelongsToIndex", -1);
            private set => DataManager.Cars.Set($"Parts.Core.{saveKey}.CarBelongsToIndex", value);
        }

        public bool IsAppliedToCar => CarBelongsToIndex != -1;

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

            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == CarBelongsToIndex;
            if (isAttachedToCurrentCar)
            {
                //update the modifiers
                WarehouseManager.Instance.CurrentCar.PartModification.ApplyModifiers();
            }
            
            CarBelongsToIndex = -1;
        }
        
        /// <returns>Returns the total peak torque modifier of the part and all sub parts.</returns>
        public float GetPeakTorqueModifier()
        {
            float total = peakTorqueAddition;
            
            //TODO: loop over sub parts
            
            return total;
        }

    }
}
