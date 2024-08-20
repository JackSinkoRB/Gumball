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

        [SerializeField] private MagneticScroll partsMagneticScroll;
        [SerializeField] private SwapCorePartInstallButton installButton;
        [SerializeField] private TextMeshProUGUI countLabel;
        [SerializeField] private SwapCorePartHeaderFilter headerFilter;
        [Space(5)]
        [SerializeField] private Transform optionButtonHolder;
        [SerializeField] private SwapCorePartOptionButton optionButtonPrefab;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType partType;
        [SerializeField, ReadOnly] private CorePart currentSelectedPart;

        public void Initialise(CorePart.PartType type)
        {
            partType = type;
            
            headerFilter.Select(WarehouseManager.Instance.CurrentCar.CarType);
            PopulateParts();
        }

        public void OnClickInstallButton()
        {
            bool isStockPart = currentSelectedPart == null;
            if (!isStockPart)
            {
                //take funds
                if (!Currency.Standard.HasEnoughFunds(currentSelectedPart.StandardCurrencyInstallCost))
                {
                    PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                    return;
                }

                Currency.Standard.TakeFunds(currentSelectedPart.StandardCurrencyInstallCost);
            }

            CorePartManager.InstallPartOnCar(partType, currentSelectedPart, WarehouseManager.Instance.CurrentCar.CarIndex);
            installButton.Initialise(partType, currentSelectedPart);

            //update the sub parts menu
            if (isStockPart)
                PanelManager.GetPanel<UpgradeWorkshopPanel>().OpenSubMenu(null);
            else
                PanelManager.GetPanel<UpgradeWorkshopPanel>().ModifySubMenu.OpenSubMenu(partType);
        }

        private void PopulateParts()
        {
            foreach (Transform child in optionButtonHolder)
                child.gameObject.Pool();

            //add the current part option as it doesn't show in spare parts
            CorePart currentPart = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, partType);
            CreatePartButtonInstance(currentPart);
            
            foreach (CorePart part in CorePartManager.GetSpareParts(partType, headerFilter.CurrentSelected))
                CreatePartButtonInstance(part);
        }
        
        private void CreatePartButtonInstance(CorePart part)
        {
            SwapCorePartOptionButton instance = optionButtonPrefab.gameObject.GetSpareOrCreate<SwapCorePartOptionButton>(optionButtonHolder);
            instance.Initialise(part);
            instance.transform.SetAsLastSibling();
        }

    }
}
