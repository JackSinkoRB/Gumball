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
        
        private CorePart corePart => CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, corePartType);
        private bool isMaxLevel => corePart.CurrentLevelIndex >= corePart.Levels.Length - 1;

        public void Initialise(CorePart.PartType corePartType)
        {
            this.corePartType = corePartType;

            Refresh();
        }

        public void Refresh()
        {
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
            if (!corePart.AreAllSubPartsInstalled())
            {
                PanelManager.GetPanel<GenericMessagePanel>().Show();
                PanelManager.GetPanel<GenericMessagePanel>().Initialise("Upgrading requires all the sub parts to be installed.");
                return;
            }
            
            //take funds
            int nextLevelIndex = corePart.CurrentLevelIndex + 1;
            int upgradeCost = corePart.Levels[nextLevelIndex].StandardCurrencyCost;
            if (!Currency.Standard.HasEnoughFunds(upgradeCost))
            {
                PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                return;
            }

            int desiredLevel = nextLevelIndex + 1;
            PanelManager.GetPanel<ConfirmationPanel>().Initialise("Upgrade Core Part?", 
                $"Are you sure you'd like to upgrade this core part to level {desiredLevel}? Cars that are lower then level {desiredLevel} won't be able to use this part until they've been upgraded to level {desiredLevel}.\n\nIf the current car is a lower level then level {desiredLevel}, this part will be removed from the car.\n\nThis decision cannot be reversed.",
                () =>
                {
                    corePart.Upgrade();
                    Refresh();
                }, standardCurrencyCost: upgradeCost);
            PanelManager.GetPanel<ConfirmationPanel>().Show();
        }

        private void SetupSubPartSlots()
        {
            if (corePart == null || corePart.SubPartSlots.Length == 0)
            {
                Hide();
                return;
            }

            foreach (Transform child in slotButtonsGrid.transform)
                child.gameObject.Pool();

            foreach (SubPartSlot slot in corePart.SubPartSlots)
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
            int level = corePart == null ? 1 : corePart.CurrentLevelIndex + 1;
            int maxLevel = corePart == null ? 1 : corePart.Levels.Length;
            levelLabel.text = $"Level {level} / {maxLevel}";
        }

        private void SetUpgradeCostLabel()
        {
            upgradeCostLabel.text = corePart == null || isMaxLevel ? "N/A" : $"{corePart.Levels[corePart.CurrentLevelIndex + 1].StandardCurrencyCost}";
        }

        private void SetUpgradeButtonInteractable()
        {
            bool hasSubPartSlots = corePart != null && corePart.SubPartSlots.Length > 0;
            
            upgradeButton.interactable = hasSubPartSlots && !isMaxLevel;
        }

    }
}