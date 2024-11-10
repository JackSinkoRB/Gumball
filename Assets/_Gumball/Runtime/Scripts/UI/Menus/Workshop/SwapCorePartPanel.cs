using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapCorePartPanel : AnimatedPanel
    {

        [SerializeField] private SwapCorePartInstallButton installButton;
        [SerializeField] private SwapCorePartHeaderFilter headerFilter;
        [Space(5)]
        [SerializeField] private Transform optionButtonHolder;
        [SerializeField] private SwapCorePartOptionButton optionButtonPrefab;
        
        [Header("Properties")]
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private PerformanceRatingSlider maxSpeedSlider;
        [SerializeField] private PerformanceRatingSlider accelerationSlider;
        [SerializeField] private PerformanceRatingSlider handlingSlider;
        [SerializeField] private PerformanceRatingSlider nosSlider;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType partType;
        [SerializeField, ReadOnly] private SwapCorePartOptionButton selectedOption;
        [SerializeField, ReadOnly] private List<SwapCorePartOptionButton> partOptions = new();

        private AICar currentCar => WarehouseManager.Instance.CurrentCar;
        
        public SwapCorePartOptionButton SelectedOption => selectedOption;

        public void Initialise(CorePart.PartType type)
        {
            partType = type;

            this.PerformAtEndOfFrame(() =>
            {
                headerFilter.Select(currentCar.CarType);
                SelectPartOption(null);
            });
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            PanelManager.GetPanel<PaintStripeBackgroundPanel>().Show();
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            if (PanelManager.PanelExists<PaintStripeBackgroundPanel>())
                PanelManager.GetPanel<PaintStripeBackgroundPanel>().Hide();

            PanelManager.GetPanel<ModifyWorkshopSubMenu>().Show();
        }

        public void SelectPartOption(SwapCorePartOptionButton option)
        {
            if (option == null)
                option = GetCurrentInstalledOption(); //try select the current installed option
            
            if (selectedOption != null)
                selectedOption.OnDeselect();
            
            selectedOption = option;
            if (selectedOption != null)
                selectedOption.OnSelect();
            
            installButton.Initialise(partType, selectedOption == null ? null : selectedOption.CorePart);
            
            UpdateDescription();
            UpdatePerformanceRatingSliders();
        }
        
        public void OnClickInstallButton()
        {
            bool isStockPart = selectedOption.CorePart == null;
            if (!isStockPart)
            {
                //take funds
                if (!Currency.Standard.HasEnoughFunds(selectedOption.CorePart.StandardCurrencyInstallCost))
                {
                    PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                    return;
                }

                Currency.Standard.TakeFunds(selectedOption.CorePart.StandardCurrencyInstallCost);
            }

            CorePartManager.InstallPartOnCar(partType, selectedOption.CorePart, currentCar.CarIndex);
            installButton.Initialise(partType, selectedOption.CorePart);

            //update the sub parts menu
            if (isStockPart)
                PanelManager.GetPanel<UpgradeWorkshopPanel>().OpenSubMenu(null);
            else
                PanelManager.GetPanel<UpgradeWorkshopPanel>().ModifySubMenu.OpenSubMenu(partType);
            
            UpdatePerformanceRatingSliders();
        }

        public void PopulateParts()
        {
            partOptions.Clear();
            
            foreach (Transform child in optionButtonHolder)
                child.gameObject.Pool();

            //add the current part option as it doesn't show in spare parts
            CorePart currentPart = CorePartManager.GetCorePart(currentCar.CarIndex, partType);
            if (currentPart != null && currentPart.CarType == headerFilter.CurrentSelected)
                CreatePartButtonInstance(currentPart);

            foreach (CorePart part in CorePartManager.GetSpareParts(partType, headerFilter.CurrentSelected))
                CreatePartButtonInstance(part);
        }
        
        private void UpdateDescription()
        {
            descriptionLabel.text = selectedOption == null || selectedOption.CorePart == null ? "" : selectedOption.CorePart.Description;
        }
        
        private void UpdatePerformanceRatingSliders()
        {
            maxSpeedSlider.gameObject.SetActive(selectedOption != null);
            accelerationSlider.gameObject.SetActive(selectedOption != null);
            handlingSlider.gameObject.SetActive(selectedOption != null);
            nosSlider.gameObject.SetActive(selectedOption != null);
            
            if (selectedOption == null)
                return;
            
            //create a profile with the specific core part
            Dictionary<CorePart.PartType, CorePart> allParts = CorePartManager.GetCoreParts(currentCar.CarIndex);
            allParts[selectedOption.CorePart.Type] = selectedOption.CorePart;

            CarPerformanceProfile profileWithPart = new CarPerformanceProfile(allParts.Values);

            CarPerformanceProfile currentProfile = new CarPerformanceProfile(currentCar.CarIndex);
            maxSpeedSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
            accelerationSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
            handlingSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
            nosSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
        }

        private void CreatePartButtonInstance(CorePart part)
        {
            SwapCorePartOptionButton instance = optionButtonPrefab.gameObject.GetSpareOrCreate<SwapCorePartOptionButton>(optionButtonHolder);
            instance.Initialise(part);
            instance.transform.SetAsLastSibling();
            
            partOptions.Add(instance);
        }
        
        private SwapCorePartOptionButton GetCurrentInstalledOption()
        {
            if (headerFilter.CurrentSelected != currentCar.CarType)
                return null; //category not showing
            
            foreach (SwapCorePartOptionButton optionButton in partOptions)
            {
                CorePart currentInstalledPart = CorePartManager.GetCorePart(currentCar.CarIndex, partType);
                if (optionButton.CorePart == currentInstalledPart)
                    return optionButton;
            }

            return null;
        }
        
    }
}
