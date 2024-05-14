using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapSubPartPanel : AnimatedPanel
    {

        [SerializeField] private Button installButton;
        [SerializeField] private TextMeshProUGUI installButtonLabel;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private SubPartSlot slot;

        private SubPart sparePart;

        public void Initialise(SubPartSlot slot)
        {
            this.slot = slot;

            UpdateInstallButton();
        }

        public void OnClickInstallButton()
        {
            if (slot.CurrentSubPart != null)
                slot.UninstallSubPart();
            else
                slot.InstallSubPart(sparePart);
            
            UpdateInstallButton();
        }
        
        private void UpdateInstallButton()
        {
            if (slot.CurrentSubPart != null)
            {
                //show uninstall
                installButton.interactable = true;
                installButtonLabel.text = "Uninstall";
                return;
            }
            
            sparePart = SubPartManager.GetSpareSubPart(slot.Type, slot.Rarity);
            if (sparePart == null)
            {
                //show not available
                installButton.interactable = false;
                installButtonLabel.text = "Not available";
                return;
            }
            
            installButton.interactable = true;
            installButtonLabel.text = "Install";
        }
        
    }
}
