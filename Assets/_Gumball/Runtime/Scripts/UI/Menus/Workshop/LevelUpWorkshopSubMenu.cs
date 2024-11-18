using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class LevelUpWorkshopSubMenu : WorkshopSubMenu
    {

        [SerializeField] private MultiImageButton purchaseButton;
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private TextMeshProUGUI costLabel;
        
        [SerializeField] private OpenBlueprintOption openBlueprintOptionPrefab;
        [SerializeField] private Transform openBlueprintOptionHolder;

        protected override void OnShow()
        {
            base.OnShow();

            PopulateOpenBlueprints();
            
            Refresh();
        }

        public void OnClickPurchaseButton()
        {
            string carGUID = WarehouseManager.Instance.CurrentCar.CarGUID;
            int nextLevelIndex = BlueprintManager.Instance.GetNextLevelIndex(carGUID);
            
            int blueprints = BlueprintManager.Instance.GetBlueprints(carGUID);
            int requiredBlueprints = BlueprintManager.Instance.Levels[nextLevelIndex].BlueprintsRequired;
            bool hasEnoughBlueprints = blueprints >= requiredBlueprints;
            if (!hasEnoughBlueprints)
            {
                int remainingBlueprints = requiredBlueprints - blueprints;
                PanelManager.GetPanel<GenericMessagePanel>().Initialise($"Not enough blueprints! You require <b>{remainingBlueprints} more</b> blueprints to upgrade.");
                PanelManager.GetPanel<GenericMessagePanel>().Show();
                return;
            }
            
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
            BlueprintManager.Instance.TakeBlueprints(carGUID, blueprintsCost);
            
            //level up the car
            BlueprintManager.Instance.SetLevelIndex(carGUID, nextLevelIndex);

            Refresh();
        }
        
        private void Refresh()
        {
            UpdateLevelLabel();
            UpdatePurchaseButton();
        }

        private void UpdateLevelLabel()
        {
            string carIndex = WarehouseManager.Instance.CurrentCar.CarGUID;
            int currentLevel = BlueprintManager.Instance.GetLevelIndex(carIndex) + 1;
            int maxLevel = BlueprintManager.Instance.MaxLevelIndex + 1;
            
            levelLabel.text = $"Level {currentLevel} / {maxLevel}";
        }

        private void PopulateOpenBlueprints()
        {
            foreach (Transform child in openBlueprintOptionHolder)
                child.gameObject.Pool();
            
            //get the sessions that give the CarIndex blueprint as a reward
            foreach (GameSession session in BlueprintManager.Instance.GetSessionsThatGiveBlueprint(WarehouseManager.Instance.CurrentCar.CarGUID))
            {
                OpenBlueprintOption instance = openBlueprintOptionPrefab.gameObject.GetSpareOrCreate<OpenBlueprintOption>(openBlueprintOptionHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(session.GetModeIcon(), session.DisplayName);
            }
        }

        private void UpdatePurchaseButton()
        {
            string carGUID = WarehouseManager.Instance.CurrentCar.CarGUID;
            int nextLevelIndex = BlueprintManager.Instance.GetNextLevelIndex(carGUID);
            bool isMaxLevel = nextLevelIndex > BlueprintManager.Instance.MaxLevelIndex;
            
            purchaseButton.interactable = !isMaxLevel;

            costLabel.text = isMaxLevel ? "N/A" : BlueprintManager.Instance.Levels[nextLevelIndex].StandardCurrencyCostToUpgrade.ToString();
        }
        
    }
}
