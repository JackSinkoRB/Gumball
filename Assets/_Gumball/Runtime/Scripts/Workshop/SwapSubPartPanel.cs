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

        [Header("Install button")]
        [SerializeField] private Button installButton;
        [SerializeField] private TextMeshProUGUI installButtonLabel;
        [SerializeField] private Color installButtonColorInstall = Color.blue;
        [SerializeField] private Color installButtonColorUninstall = Color.red;

        [Header("Events")]
        [SerializeField] private Transform eventHolder;
        [SerializeField] private EventScrollItem eventPrefab;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private SubPartSlot slot;

        private SubPart sparePart;

        public void Initialise(SubPartSlot slot)
        {
            this.slot = slot;

            UpdateEvents();
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

        private void UpdateEvents()
        {
            //for all parts that match the slot, show all the game sessions that reward it
            foreach (SubPart matchingPart in SubPartManager.GetSubParts(slot.Type, slot.Rarity))
            {
                foreach (GameSession session in matchingPart.SessionsThatGiveReward)
                {
                    EventScrollItem eventScrollItem = eventPrefab.gameObject.GetSpareOrCreate<EventScrollItem>(eventHolder);
                    eventScrollItem.transform.SetAsLastSibling();
                    eventScrollItem.Initialise(session, matchingPart);
                }
            }
        }
        
        private void UpdateInstallButton()
        {
            if (slot.CurrentSubPart != null)
            {
                //show uninstall
                installButton.interactable = true;
                installButtonLabel.text = "Uninstall";
                installButton.image.color = installButtonColorUninstall;
                return;
            }
            
            sparePart = SubPartManager.GetSpareSubPart(slot.Type, slot.Rarity);
            if (sparePart == null)
            {
                //show not available
                installButton.interactable = false;
                installButtonLabel.text = "Not available";
                installButton.image.color = installButtonColorInstall;
                return;
            }
            
            installButton.interactable = true;
            installButtonLabel.text = "Install";
            installButton.image.color = installButtonColorInstall;
        }
        
    }
}
