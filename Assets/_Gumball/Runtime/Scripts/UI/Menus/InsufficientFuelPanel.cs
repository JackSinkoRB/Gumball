using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class InsufficientFuelPanel : AnimatedPanel
    {

        [SerializeField] private StorePurchaseButton replenishButton;
        
        public void OnClickBuyButton()
        {
            Hide();
            replenishButton.OnConfirmPurchase();
        }
        
    }
}
