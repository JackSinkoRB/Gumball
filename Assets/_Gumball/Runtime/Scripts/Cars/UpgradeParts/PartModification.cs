using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class PartModification : MonoBehaviour
    {
        
        #region STATIC
        public static void SetCorePart(int carIndex, CorePart.PartType type, CorePart corePart)
        {
            DataManager.Cars.Set($"{GetSaveKeyFromIndex(carIndex)}.Core.{type.ToString()}", corePart == null ? null : corePart.ID);
        }
        
        public static CorePart GetCorePart(int carIndex, CorePart.PartType type)
        {
            string partID = DataManager.Cars.Get<string>($"{GetSaveKeyFromIndex(carIndex)}.Core.{type.ToString()}");
            return CorePartManager.GetPartByID(partID);
        }
        
        public static void SetSubPart(int carIndex, SubPart.SubPartType type, CorePart corePart)
        {
            DataManager.Cars.Set($"{GetSaveKeyFromIndex(carIndex)}.Sub.{type.ToString()}", corePart == null ? null : corePart.ID);
        }
        
        public static SubPart GetSubPart(int carIndex, SubPart.SubPartType type)
        {
            string partID = DataManager.Cars.Get<string>($"{GetSaveKeyFromIndex(carIndex)}.Sub.{type.ToString()}");
            return SubPartManager.GetPartByID(partID);
        }
        
        private static string GetSaveKeyFromIndex(int carIndex)
        {
            return $"{AICar.GetSaveKeyFromIndex(carIndex)}.Parts";
        }
        #endregion

        [SerializeField] private SubPartSlot[] subPartSlots;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;

        public SubPartSlot[] SubPartSlots => subPartSlots;
        
        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;

            InitialiseSubParts();
            ApplyModifiers();
        }

        public void ApplyModifiers()
        {
            carBelongsTo.SetPeakTorque(carBelongsTo.DefaultPeakTorque + GetTotalPeakTorqueModifiers());
        }

        public float GetTotalPeakTorqueModifiers()
        {
            float total = 0;
            
            CorePart currentEnginePart = GetCorePart(carBelongsTo.CarIndex, CorePart.PartType.ENGINE);
            if (currentEnginePart != null)
                total += currentEnginePart.PeakTorqueAddition;
            
            CorePart currentWheelsPart = GetCorePart(carBelongsTo.CarIndex, CorePart.PartType.WHEELS);
            if (currentWheelsPart != null)
                total += currentWheelsPart.PeakTorqueAddition;
            
            CorePart currentDrivetrainPart = GetCorePart(carBelongsTo.CarIndex, CorePart.PartType.DRIVETRAIN);
            if (currentDrivetrainPart != null)
                total += currentDrivetrainPart.PeakTorqueAddition;
            
            //TODO: loop over all sub parts
            
            return total;
        }

        private void InitialiseSubParts()
        {
            for (int id = 0; id < subPartSlots.Length; id++) //index is the ID
            {
                SubPartSlot slot = subPartSlots[id];
                
                slot.Initialise(carBelongsTo, id);
            }
        }

    }
}
