using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SubPartsWorkshopSubMenu : WorkshopSubMenu
    {
        
        [SerializeField] private CorePart.PartType partType;
        [SerializeField] private Button swapButton;
        [SerializeField] private TextMeshProUGUI swapButtonLabel;

        public override void Show()
        {
            base.Show();
            
            //disable swap button interactivity if no spare parts
            swapButton.interactable = CorePartManager.GetSpareParts(partType).Count > 0;
            swapButtonLabel.text = swapButton.interactable ? "Swap" : "No spare parts";
        }

        public void OnClickSwapButton()
        {
            PanelManager.GetPanel<SwapCorePartPanel>().Show();
            PanelManager.GetPanel<SwapCorePartPanel>().Initialise(partType);
        }
        
    }
}
