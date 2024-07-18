using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SubPartsWorkshopSubMenu : WorkshopSubMenu
    {
        
        [SerializeField] private CorePart.PartType corePartType;
        [SerializeField] private Transform slotButtonsHolder; 
        [SerializeField] private GameObject slotButtonPrefab;

        public override void Show()
        {
            base.Show();
            
            SetupSubPartSlots();
        }

        public void OnClickCorePartButton()
        {
            CorePart currentCarCorePart = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, corePartType);
            if (currentCarCorePart == null)
            {
                PanelManager.GetPanel<UpgradeWorkshopPanel>().OpenSubMenu(null);
                OnClickSwapButton();
                return;
            }

            PanelManager.GetPanel<UpgradeWorkshopPanel>().OpenSubMenu(this);
        }
        
        public void OnClickSwapButton()
        {
            PanelManager.GetPanel<SwapCorePartPanel>().Show();
            PanelManager.GetPanel<SwapCorePartPanel>().Initialise(corePartType);
        }

        private void SetupSubPartSlots()
        {
            CorePart currentCarCorePart = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, corePartType);
            if (currentCarCorePart == null)
                return;
            
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
