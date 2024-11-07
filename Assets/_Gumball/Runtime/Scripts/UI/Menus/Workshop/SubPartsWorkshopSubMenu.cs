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
        
        [SerializeField] private GridLayoutWithScreenSize slotButtonsGrid; 
        [SerializeField] private GameObject slotButtonPrefab;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostLabel;
        [SerializeField] private TextMeshProUGUI levelLabel;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType corePartType;

        public void Initialise(CorePart.PartType corePartType)
        {
            this.corePartType = corePartType;

            SetupSubPartSlots();
            SetLevelLabel();
            SetUpgradeCostLabel();
            SetUpgradeButtonInteractable();
        }
        
        public void OnClickSwapButton()
        {
            PanelManager.GetPanel<SwapCorePartPanel>().Show();
            PanelManager.GetPanel<SwapCorePartPanel>().Initialise(corePartType);
        }

        public void OnClickUpgradeButton()
        {
            bool hasAllSubPartsInstalled = false; //TODO
            if (!hasAllSubPartsInstalled)
            {
                PanelManager.GetPanel<GenericMessagePanel>().Show();
                PanelManager.GetPanel<GenericMessagePanel>().Initialise("Upgrading requires all the sub parts to be installed.");
            }
        }

        private void SetupSubPartSlots()
        {
            CorePart currentCarCorePart = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, corePartType);
            if (currentCarCorePart == null || currentCarCorePart.SubPartSlots.Length == 0)
            {
                Hide();
                return;
            }

            foreach (Transform child in slotButtonsGrid.transform)
                child.gameObject.Pool();

            foreach (SubPartSlot slot in currentCarCorePart.SubPartSlots)
            {
                if (slot.Type.GetCoreType() != corePartType)
                    continue;
                
                SubPartSlotButton button = slotButtonPrefab.GetSpareOrCreate<SubPartSlotButton>(slotButtonsGrid.transform);
                button.transform.SetAsLastSibling();
                button.Initialise(slot);
            }
            
            slotButtonsGrid.Resize();
        }

        private void SetLevelLabel()
        {
            //TODO
            levelLabel.text = "Level NA / NA";
        }

        private void SetUpgradeCostLabel()
        {
            //TODO
            upgradeCostLabel.text = "N/A";
        }

        private void SetUpgradeButtonInteractable()
        {
            //TODO
            upgradeButton.interactable = false;
        }

    }
}