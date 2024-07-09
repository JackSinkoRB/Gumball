using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace Gumball
{
    public class StorePurchaseButton : MonoBehaviour
    {

        [Header("Item")]
        [SerializeField] private string displayName = "some item"; 
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

        [SerializeField] private UnityEvent onSuccessfulPurchase;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI priceLabel;
        [SerializeField] private Image standardCurrencySymbol;
        [SerializeField] private Image premiumCurrencySymbol;
        
        private int finalPrice => Mathf.RoundToInt(price - (price * discountPercent));

        private bool isInitialised;

        public string DisplayName => displayName;
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

        private void OnValidate()
        {
            UpdateButtonUI();
        }

        public string GetPriceFormatted()
        {
            if (currencyType == CurrencyType.REAL)
            {
                if (!IAPManager.HasLoaded || IAPManager.Instance.StoreController == null)
                    return "N/A";
                
                Product storeProduct = IAPManager.Instance.StoreController.products.WithID(product.ProductID);
                return storeProduct.metadata.localizedPriceString; //include the localised price icon
            }
            else
            {
                return $"{finalPrice}";
            }
        }
        
        public void OnClickButton()
        {
            PanelManager.GetPanel<PurchaseConfirmationPanel>().Show();
            PanelManager.GetPanel<PurchaseConfirmationPanel>().Initialise(this);
        }

        public void OnConfirmPurchase()
        {
            if (currencyType == CurrencyType.STANDARD)
            {
                if (!Currency.Standard.HasEnoughFunds(finalPrice))
                {
                    PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                    return;
                }
                
                Currency.Standard.TakeFunds(finalPrice);
                OnPurchaseSuccessful();
            } else if (currencyType == CurrencyType.PREMIUM)
            {
                if (!Currency.Premium.HasEnoughFunds(finalPrice))
                {
                    PanelManager.GetPanel<InsufficientPremiumCurrencyPanel>().Show();
                    return;
                }
                
                Currency.Premium.TakeFunds(finalPrice);
                OnPurchaseSuccessful();
            } else if (currencyType == CurrencyType.REAL)
            {
                IAPManager.Instance.InitiatePurchase(product.ProductID, new PurchaseHandler(OnPurchaseSuccessful, OnPurchaseFailed));
            }
        }

        private void OnPurchaseSuccessful()
        {
            onSuccessfulPurchase?.Invoke();
        }

        private void OnPurchaseFailed()
        {
            const string userMessage = "There was an issue with the purchase.\nPlease try again.";
            PanelManager.GetPanel<GenericMessagePanel>().Show();
            PanelManager.GetPanel<GenericMessagePanel>().Initialise(userMessage);
        }
        
        private void UpdateButtonUI()
        {
            if (priceLabel != null)
                priceLabel.text = GetPriceFormatted();
            
            if (standardCurrencySymbol != null)
                standardCurrencySymbol.gameObject.SetActive(currencyType == CurrencyType.STANDARD);
            
            if (premiumCurrencySymbol != null)
                premiumCurrencySymbol.gameObject.SetActive(currencyType == CurrencyType.PREMIUM);
        }

    }
}
