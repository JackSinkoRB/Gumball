using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class PurchaseConfirmationPanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI descriptionLabel;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private StorePurchaseButton purchaseButton;
        
        public void Initialise(StorePurchaseButton purchaseButton)
        {
            this.purchaseButton = purchaseButton;
            
            descriptionLabel.text = $"Are you sure you want to purchase {purchaseButton.DisplayName} for {purchaseButton.GetPriceFormatted()}?";
        }

        public void OnClickConfirmButton()
        {
            Hide();
            purchaseButton.OnConfirmPurchase();
        }
        
    }
}
