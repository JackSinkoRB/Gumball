using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Gumball
{
    public class StorePurchaseButton : MonoBehaviour
    {

        [SerializeField] private CurrencyType currencyType;

        /// <summary>
        /// This should match the ids of the products in your Google/Apple stores.
        /// </summary>
        [SerializeField, ConditionalField(nameof(currencyType), compareValues: nameof(CurrencyType.REAL))]
        private IAPProduct product;

        [SerializeField, ConditionalField(nameof(currencyType), compareValues: new object[] { nameof(CurrencyType.PREMIUM), nameof(CurrencyType.STANDARD) }), PositiveValueOnly]
        private int price = 100;
        
        [SerializeField, ConditionalField(nameof(currencyType), compareValues: new object[] { nameof(CurrencyType.PREMIUM), nameof(CurrencyType.STANDARD) }), Range(0, 1)]
        private float discountPercent;

        private float discountedPrice => price - (price * discountPercent);

        private bool isInitialised;
        
        public CurrencyType CurrencyType => currencyType;
        public IAPProduct Product => product;
        
        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        private void Initialise()
        {
            isInitialised = true;

            UpdateButtonUI();
        }

        public void OnClickButton()
        {
            PanelManager.GetPanel<PurchaseConfirmationPanel>().Show();
            PanelManager.GetPanel<PurchaseConfirmationPanel>().Initialise(this);
        }

        public void OnConfirmPurchase()
        {
            //TODO
            //handle each currency
            // - return if successful
            //gives the items
        }

        private void UpdateButtonUI()
        {
            //TODO - change the button layout depending on currency type
        }
        
    }
}
