using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class LevelUpWorkshopSubMenu : WorkshopSubMenu
    {

        [SerializeField] private MultiImageButton purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonLabel;
        [SerializeField] private Transform purchaseButtonCostHolder;
        [SerializeField] private TextMeshProUGUI purchaseButtonCostLabel;
        
        [SerializeField] private OpenBlueprintOption openBlueprintOptionPrefab;
        [SerializeField] private Transform openBlueprintOptionHolder;

        protected override void OnShow()
        {
            base.OnShow();

            PopulateOpenBlueprints();

            UpdatePurchaseButton();
        }

        public void OnClickPurchaseButton()
        {
            int carIndex = WarehouseManager.Instance.CurrentCar.CarIndex;
            int nextLevelIndex = BlueprintManager.Instance.GetNextLevelIndex(carIndex);
            
            //take funds
            int standardCurrencyCost = BlueprintManager.Instance.Levels[nextLevelIndex].StandardCurrencyCostToUpgrade;
            if (!Currency.Standard.HasEnoughFunds(standardCurrencyCost))
            {
                PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                return;
            }

            Currency.Standard.TakeFunds(standardCurrencyCost);
            
            //take the blueprints
            int blueprintsCost = BlueprintManager.Instance.Levels[nextLevelIndex].BlueprintsRequired;
            BlueprintManager.Instance.TakeBlueprints(carIndex, blueprintsCost);
            
            //level up the car
            BlueprintManager.Instance.SetLevelIndex(carIndex, nextLevelIndex);

            UpdatePurchaseButton();
        }

        private void PopulateOpenBlueprints()
        {
            foreach (Transform child in openBlueprintOptionHolder)
                child.gameObject.Pool();
            
            //get the sessions that give the CarIndex blueprint as a reward
            foreach (GameSession session in BlueprintManager.Instance.GetSessionsThatGiveBlueprint(WarehouseManager.Instance.CurrentCar.CarIndex))
            {
                OpenBlueprintOption instance = openBlueprintOptionPrefab.gameObject.GetSpareOrCreate<OpenBlueprintOption>(openBlueprintOptionHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(session.GetModeIcon(), session.DisplayName);
            }
        }

        private void UpdatePurchaseButton()
        {
            int carIndex = WarehouseManager.Instance.CurrentCar.CarIndex;
            int nextLevelIndex = BlueprintManager.Instance.GetNextLevelIndex(carIndex);
            bool isMaxLevel = nextLevelIndex > BlueprintManager.Instance.MaxLevelIndex;
            
            purchaseButtonCostHolder.gameObject.SetActive(!isMaxLevel);
            
            purchaseButtonLabel.text = isMaxLevel ? "Max Level" : $"Level {nextLevelIndex + 1}";
            if (isMaxLevel)
            {
                purchaseButton.interactable = false;
                return;
            }

            int blueprints = BlueprintManager.Instance.GetBlueprints(carIndex);
            int requiredBlueprints = BlueprintManager.Instance.Levels[nextLevelIndex].BlueprintsRequired;
            bool hasEnoughBlueprints = blueprints >= requiredBlueprints;
            
            purchaseButton.interactable = hasEnoughBlueprints;

            int requiredStandardCurrency = BlueprintManager.Instance.Levels[nextLevelIndex].StandardCurrencyCostToUpgrade;
            purchaseButtonCostLabel.text = $"{requiredStandardCurrency}";
        }
        
    }
}
