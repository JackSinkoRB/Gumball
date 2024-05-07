using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SubPartsWorkshopSubMenu : WorkshopSubMenu
    {
        
        [SerializeField] private CorePart.PartType partType;
        [SerializeField] private Button swapButton;
        
        public override void Show()
        {
            base.Show();
            
            //disable swap button interactivity if no spare parts
            swapButton.interactable = CorePartManager.GetSpareParts(partType).Count > 0;
        }

        public void OnClickSwapButton()
        {
            PanelManager.GetPanel<SwapCorePartPanel>().Show();
            PanelManager.GetPanel<SwapCorePartPanel>().Initialise(partType);
        }
        
    }
}
