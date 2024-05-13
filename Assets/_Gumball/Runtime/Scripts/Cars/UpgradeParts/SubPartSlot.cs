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
        [SerializeField] private Sprite icon;
        
        private AICar carBelongsTo;
        private int id;

        private string saveKey => $"{carBelongsTo.SaveKey}.SubPartSlot.{id}";
        
        public SubPart.SubPartType Type => type;
        public Sprite Icon => icon;
        
        public SubPart CurrentPart
        {
            get => SubPartManager.GetPartByID(DataManager.Cars.Get<string>($"{saveKey}.CurrentPartID", null));
            private set => DataManager.Cars.Set($"{saveKey}.CurrentPartID", value.ID);
        }
        
        public void Initialise(AICar carBelongsTo, int id)
        {
            this.carBelongsTo = carBelongsTo;
            this.id = id;
        }

    }
}
