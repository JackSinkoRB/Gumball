using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// A sub part slot is added to a PartModification component to allow the player to change that part.
    /// </summary>
    [Serializable]
    public class SubPartSlot
    {
        
        [SerializeField] private SubPart.SubPartType type;
        [SerializeField] private SubPart.SubPartRarity rarity;
        [SerializeField] private string displayName = "Missing name";
        [SerializeField] private string description = "This is the description for the sub part.";
        [SerializeField] private Sprite icon;
        
        [SerializeField, HideInInspector] private CorePart corePartBelongsTo;
        [SerializeField, HideInInspector] private int saveKeyID;

        private string saveKey => $"{corePartBelongsTo.SaveKey}.SubPartSlot.{saveKeyID}";
        
        public SubPart.SubPartType Type => type;
        public SubPart.SubPartRarity Rarity => rarity;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        
        public SubPart CurrentSubPart
        {
            get => SubPartManager.GetPartByID(DataManager.Cars.Get<string>($"{saveKey}.CurrentPartID", null));
            private set => DataManager.Cars.Set($"{saveKey}.CurrentPartID", value == null ? null : value.ID);
        }
        
        public void Initialise(CorePart corePartBelongsTo, int saveKeyID)
        {
            this.corePartBelongsTo = corePartBelongsTo;
            this.saveKeyID = saveKeyID;
        }

        public void InstallSubPart(SubPart part)
        {
            //if there is already a part applied, make sure to unapply it from the core part
            if (CurrentSubPart != null)
                CurrentSubPart.RemoveFromCorePart();
            
            //apply to slot
            CurrentSubPart = part;
            
            //apply to sub part
            part.ApplyToCorePart(corePartBelongsTo);
            
            //update the cars performance profile if it's the active car
            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == corePartBelongsTo.CarBelongsToGUID;
            if (isAttachedToCurrentCar)
                WarehouseManager.Instance.CurrentCar.SetPerformanceProfile(new CarPerformanceProfile(corePartBelongsTo.CarBelongsToGUID));
        }

        public void UninstallSubPart()
        {
            if (CurrentSubPart == null)
            {
                Debug.LogError($"Tried uninstalling a sub part, but there is no sub part attached to {corePartBelongsTo.name}.");
                return;
            }
            
            //remove from core part
            CurrentSubPart.RemoveFromCorePart();
            
            //apply to slot
            CurrentSubPart = null;
            
            //update the cars performance profile if it's the active car
            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == corePartBelongsTo.CarBelongsToGUID;
            if (isAttachedToCurrentCar)
                WarehouseManager.Instance.CurrentCar.SetPerformanceProfile(new CarPerformanceProfile(corePartBelongsTo.CarBelongsToGUID));
        }
        
    }
}
