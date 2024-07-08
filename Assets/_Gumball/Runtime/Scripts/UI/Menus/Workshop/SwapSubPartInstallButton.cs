using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapSubPartInstallButton : MonoBehaviour
    {
        
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Color installColor = Color.blue;
        [SerializeField] private Color uninstallColor = Color.red;
        [SerializeField] private GameObject costHolder;
        [SerializeField] private TextMeshProUGUI costLabel;
        
        private SubPartSlot slot;
        
        private Button button => GetComponent<Button>();
        
        public void Initialise(SubPartSlot slot)
        {
            this.slot = slot;
            
            if (slot.CurrentSubPart != null)
            {
                //show uninstall
                button.interactable = true;
                label.alignment = TextAlignmentOptions.Center;
                label.text = "Uninstall";
                button.image.color = uninstallColor;
                costHolder.SetActive(false);
                return;
            }
            
            SubPart sparePart = SubPartManager.GetSpareSubPart(slot.Type, slot.Rarity);
            if (sparePart == null)
            {
                //show not available
                button.interactable = false;
                label.alignment = TextAlignmentOptions.Center;
                label.text = "Not available";
                button.image.color = installColor;
                costHolder.SetActive(false);
                return;
            }
            
            button.interactable = true;
            label.alignment = TextAlignmentOptions.Right;
            label.text = "Install";
            button.image.color = installColor;
            costHolder.SetActive(true);
            costLabel.text = sparePart.StandardCurrencyInstallCost.ToString();
        }

        public void OnClickButton()
        {
            if (slot.CurrentSubPart != null)
                slot.UninstallSubPart();
            else
            {
                SubPart sparePart = SubPartManager.GetSpareSubPart(slot.Type, slot.Rarity);
                
                //take funds
                if (!Currency.Standard.HasEnoughFunds(sparePart.StandardCurrencyInstallCost))
                {
                    PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                    return;
                }

                Currency.Standard.TakeFunds(sparePart.StandardCurrencyInstallCost);
                
                slot.InstallSubPart(sparePart);
            }

            Initialise(slot);
        }
        
    }
}