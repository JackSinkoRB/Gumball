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
        [SerializeField] private Button installButton;
        [SerializeField] private TextMeshProUGUI installButtonLabel;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType partType;
        [SerializeField, ReadOnly] private CorePart currentSelectedPart;

        public void Initialise(CorePart.PartType type)
        {
            partType = type;
            
            bool hasNonStockPart = false;
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            //add the stock option
            ScrollItem stockScrollItem = CreateScrollItem(type, null);
            scrollItems.Add(stockScrollItem);

            //add the current part option as it doesn't show in spare parts
            CorePart currentPart = PartModification.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, type);
            if (currentPart != null)
            {
                ScrollItem currentScrollItem = CreateScrollItem(type, currentPart);
                scrollItems.Add(currentScrollItem);
                hasNonStockPart = true;
            }

            foreach (CorePart part in CorePartManager.GetSpareParts(type))
            {
                ScrollItem scrollItem = CreateScrollItem(type, part);
                scrollItems.Add(scrollItem);
            }

            partsMagneticScroll.SetItems(scrollItems, hasNonStockPart ? 1 : 0);
        }

        public void OnClickInstallButton()
        {
            CorePartManager.InstallPartOnCurrentCar(partType, currentSelectedPart, WarehouseManager.Instance.CurrentCar.CarIndex);
            UpdateInstallButton(partType, currentSelectedPart);
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
                currentSelectedPart = part;
                
                //populate details
                UpdateInstallButton(type, part);
            };

            return scrollItem;
        }

        private void UpdateInstallButton(CorePart.PartType type, CorePart part)
        {
            bool isSelected = PartModification.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, type) == part;
            installButton.interactable = !isSelected;
            installButtonLabel.text = isSelected ? "Installed" : "Install";
        }
        
    }
}
