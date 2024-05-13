using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SubPartsWorkshopSubMenu : WorkshopSubMenu
    {
        
        [SerializeField] private CorePart.PartType partType;
        [SerializeField] private Transform slotButtonsHolder; 
        [SerializeField] private GameObject slotButtonPrefab;

        public override void Show()
        {
            base.Show();
            
            SetupSlots();
        }
        
        public void OnClickSwapButton()
        {
            PanelManager.GetPanel<SwapCorePartPanel>().Show();
            PanelManager.GetPanel<SwapCorePartPanel>().Initialise(partType);
        }

        private void SetupSlots()
        {
            foreach (SubPartSlot slot in WarehouseManager.Instance.CurrentCar.PartModification.SubPartSlots)
            {
                if (slot.Type.GetCoreType() != partType)
                    continue;
                
                SubPartSlotButton button = slotButtonPrefab.GetSpareOrCreate<SubPartSlotButton>(slotButtonsHolder);
                button.transform.SetAsLastSibling();
                button.Initialise(slot);
            }
        }
        
    }
}
