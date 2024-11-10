using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SubPartSlotButton : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image icon;

        [SerializeField] private Color partInstalledIconColor;
        [SerializeField] private Color noPartInstalledIconColor;
        
        private SubPartSlot slot;

        public void Initialise(SubPartSlot slot)
        {
            this.slot = slot;
            
            label.text = slot.Type.ToFriendlyString();
            icon.sprite = slot.Icon;

            icon.color = slot.CurrentSubPart == null ? noPartInstalledIconColor : partInstalledIconColor;
        }

        public void OnClick()
        {
            PanelManager.GetPanel<ModifyWorkshopSubMenu>().Hide();
            
            PanelManager.GetPanel<SwapSubPartPanel>().Show();
            PanelManager.GetPanel<SwapSubPartPanel>().Initialise(slot);
        }
        
    }
}
