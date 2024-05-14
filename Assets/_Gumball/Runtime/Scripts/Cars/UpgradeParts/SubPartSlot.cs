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
        [SerializeField] private Sprite icon;
        
        [SerializeField, HideInInspector] private CorePart corePartBelongsTo;
        [SerializeField, HideInInspector] private int saveKeyID;

        private string saveKey => $"{corePartBelongsTo.SaveKey}.SubPartSlot.{saveKeyID}";
        
        public SubPart.SubPartType Type => type;
        public SubPart.SubPartRarity Rarity => rarity;
        public Sprite Icon => icon;
        
        public SubPart CurrentSubPart
        {
            get => SubPartManager.GetPartByID(DataManager.Cars.Get<string>($"{saveKey}.CurrentPartID", null));
            private set => DataManager.Cars.Set($"{saveKey}.CurrentPartID", value.ID);
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
            
            //update the cars modifiers
            bool isAttachedToCurrentCar = WarehouseManager.Instance.CurrentCar != null && WarehouseManager.Instance.CurrentCar.CarIndex == corePartBelongsTo.CarBelongsToIndex;
            if (isAttachedToCurrentCar)
                WarehouseManager.Instance.CurrentCar.PartModification.ApplyModifiers();
        }
        
    }
}
