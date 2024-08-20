using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
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
        [SerializeField] private SwapCorePartHeaderFilterInfo infoHeaderFilter;
        [Space(5)]
        [SerializeField] private Transform optionButtonHolder;
        [SerializeField] private SwapCorePartOptionButton optionButtonPrefab;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType partType;
        [SerializeField, ReadOnly] private SwapCorePartOptionButton selectedOption;
        [SerializeField, ReadOnly] private List<SwapCorePartOptionButton> partOptions = new();

        public void Initialise(CorePart.PartType type)
        {
            partType = type;

            this.PerformAtEndOfFrame(() =>
            {
                headerFilter.Select(WarehouseManager.Instance.CurrentCar.CarType);
                infoHeaderFilter.Select(0);
                SelectPartOption(null);
            });
        }

        public void SelectPartOption(SwapCorePartOptionButton option)
        {
            if (selectedOption != null)
                selectedOption.OnDeselect();
            
            selectedOption = option;
            if (selectedOption != null)
                selectedOption.OnSelect();
            
            installButton.Initialise(partType, selectedOption == null ? null : selectedOption.CorePart);
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

            CorePartManager.InstallPartOnCar(partType, selectedOption.CorePart, WarehouseManager.Instance.CurrentCar.CarIndex);
            installButton.Initialise(partType, selectedOption.CorePart);

            //update the sub parts menu
            if (isStockPart)
                PanelManager.GetPanel<UpgradeWorkshopPanel>().OpenSubMenu(null);
            else
                PanelManager.GetPanel<UpgradeWorkshopPanel>().ModifySubMenu.OpenSubMenu(partType);
        }

        public void PopulateParts()
        {
            partOptions.Clear();
            
            foreach (Transform child in optionButtonHolder)
                child.gameObject.Pool();

            //add the current part option as it doesn't show in spare parts
            CorePart currentPart = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, partType);
            if (currentPart != null && currentPart.CarType == headerFilter.CurrentSelected)
                CreatePartButtonInstance(currentPart);

            foreach (CorePart part in CorePartManager.GetSpareParts(partType, headerFilter.CurrentSelected))
                CreatePartButtonInstance(part);
        }

        private void CreatePartButtonInstance(CorePart part)
        {
            SwapCorePartOptionButton instance = optionButtonPrefab.gameObject.GetSpareOrCreate<SwapCorePartOptionButton>(optionButtonHolder);
            instance.Initialise(part);
            instance.transform.SetAsLastSibling();
            
            partOptions.Add(instance);
        }
        
    }
}
