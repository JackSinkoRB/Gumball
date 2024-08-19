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
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType partType;
        [SerializeField, ReadOnly] private CorePart currentSelectedPart;

        public void Initialise(CorePart.PartType type)
        {
            partType = type;
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            
            //add the current part option as it doesn't show in spare parts
            CorePart currentPart = CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, type);
            ScrollItem currentScrollItem = CreateScrollItem(type, currentPart);
            scrollItems.Add(currentScrollItem);
            
            foreach (CorePart part in CorePartManager.GetSpareParts(type))
            {
                ScrollItem scrollItem = CreateScrollItem(type, part);
                scrollItems.Add(scrollItem);
            }

            partsMagneticScroll.SetItems(scrollItems);
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

        private ScrollItem CreateScrollItem(CorePart.PartType type, CorePart part)
        {
            ScrollItem scrollItem = new ScrollItem();

            scrollItem.onLoad += () =>
            {
                CorePartScrollIcon scrollIcon = (CorePartScrollIcon)scrollItem.CurrentIcon;
                scrollIcon.Initialise(part);
            };

            scrollItem.onSelect += () =>
            {
                countLabel.text = $"{partsMagneticScroll.Items.IndexOf(scrollItem) + 1} / {partsMagneticScroll.Items.Count}";
                currentSelectedPart = part;
                
                //populate details
                installButton.Initialise(type, part);
            };

            return scrollItem;
        }

    }
}
