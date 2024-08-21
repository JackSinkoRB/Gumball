using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SubPartsWorkshopSubMenu : WorkshopSubMenu
    {
        
        [SerializeField] private Transform slotButtonsHolder; 
        [SerializeField] private GameObject slotButtonPrefab;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType corePartType;

        public void Initialise(CorePart.PartType corePartType)
        {
            this.corePartType = corePartType;

            SetupSubPartSlots();
        }
        
        public void OnClickSwapButton()
        {
            PanelManager.GetPanel<SwapCorePartPanel>().Show();
            PanelManager.GetPanel<SwapCorePartPanel>().Initialise(corePartType);
        }

        private void SetupSubPartSlots()
        {
            CorePart currentCarCorePart = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, corePartType);
            if (currentCarCorePart == null || currentCarCorePart.SubPartSlots.Length == 0)
            {
                Hide();
                return;
            }

            foreach (Transform child in slotButtonsHolder)
                child.gameObject.Pool();

            foreach (SubPartSlot slot in currentCarCorePart.SubPartSlots)
            {
                if (slot.Type.GetCoreType() != corePartType)
                    continue;
                
                SubPartSlotButton button = slotButtonPrefab.GetSpareOrCreate<SubPartSlotButton>(slotButtonsHolder);
                button.transform.SetAsLastSibling();
                button.Initialise(slot);
            }
        }
        
    }
}