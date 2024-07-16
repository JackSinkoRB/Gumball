using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MyBox;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(UniqueIDAssigner))]
    public class StorePurchaseButton : MonoBehaviour
    {

        [Header("Item")]
        [SerializeField] private string displayName = "some item"; 
        [SerializeField] private CurrencyType currencyType;

        /// <summary>
        /// This should match the ids of the products in your Google/Apple stores.
        /// </summary>
        [SerializeField, ConditionalField(nameof(currencyType), true)]
        private IAPProduct product;
        [SerializeField, ConditionalField(nameof(currencyType)), PositiveValueOnly]
        private int price = 100;
        
        [Header("Sale Item")]
        [SerializeField] private bool isSale;
        [Tooltip("The discount percent to take from the price. For example: 0.2 means take 20% off the original price. For real items, this is added to the localised price to 'fake' a sale.")]
        [SerializeField, ConditionalField(nameof(isSale)), Range(0, 1)]
        private float discountPercent = 0.2f;

        [Header("Actions")]
        [SerializeField] private UnityEvent onSuccessfulPurchase;

        [Header("UI")]
        [SerializeField] private bool updatesUI = true;
        [SerializeField, ConditionalField(nameof(updatesUI))] private AutosizeTextMeshPro priceLabel;
        [SerializeField, ConditionalField(nameof(updatesUI))] private Image standardCurrencySymbol;
        [SerializeField, ConditionalField(nameof(updatesUI))] private Image premiumCurrencySymbol;
        [Space(5)]
        [SerializeField, ConditionalField(nameof(updatesUI))] private GameObject saleHolder;
        [SerializeField, ConditionalField(nameof(updatesUI))] private AutosizeTextMeshPro salePercentLabel;
        
        private bool isInitialised;

        public string DisplayName => displayName;
        public CurrencyType CurrencyType => currencyType;
        public IAPProduct Product => product;
        public string UniqueID => GetComponent<UniqueIDAssigner>().UniqueID;
        
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!EditorApplication.isUpdating)
                UpdateButtonUI();
        }
#endif
        
        public string GetPriceFormatted()
        {
            if (currencyType == CurrencyType.REAL)
            {
                if (!IAPManager.HasLoaded || IAPManager.Instance.StoreController == null)
                    return "N/A";
                
                Product storeProduct = IAPManager.Instance.StoreController.products.WithID(product.ProductID);
                return storeProduct.metadata.localizedPriceString; //include the localised price symbol ($ etc.)
            }
            else
            {
                return $"{GetFinalPrice()}";
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
                int finalPrice = (int)GetFinalPrice();
                if (!Currency.Standard.HasEnoughFunds(finalPrice))
                {
                    PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                    return;
                }
                
                Currency.Standard.TakeFunds(finalPrice);
                OnPurchaseSuccessful();
            } else if (currencyType == CurrencyType.PREMIUM)
            {
                int finalPrice = (int)GetFinalPrice();
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
        
        private float GetFinalPrice()
        {
            if (currencyType == CurrencyType.REAL)
            {
                if (!IAPManager.HasLoaded || IAPManager.Instance.StoreController == null)
                    return 999;
                
                Product storeProduct = IAPManager.Instance.StoreController.products.WithID(product.ProductID);
                return (float)storeProduct.metadata.localizedPrice;
            }
            else
            {
                int finalPrice = !isSale ? price : Mathf.RoundToInt(price - (price * discountPercent)); //round to int as non-real currencies don't have decimals
                return finalPrice;
            }
        }

        private float GetNonSalePrice()
        {
            if (!isSale)
                return GetFinalPrice();
            
            if (currencyType == CurrencyType.REAL)
            {
                float finalPrice = GetFinalPrice();
                return finalPrice / (1 - discountPercent);
            }
            else
            {
                return price;
            }
        }
        
        private void UpdateButtonUI()
        {
            if (!updatesUI)
                return;
            
            if (priceLabel != null)
            {
                priceLabel.text = GetPriceFormatted();
                this.PerformAtEndOfFrame(priceLabel.Resize);
            }

            if (isSale)
            {
                saleHolder.gameObject.SetActive(true);
                float salePercent = 1 - (GetFinalPrice() / GetNonSalePrice());
                salePercentLabel.text = $"-{Mathf.RoundToInt(salePercent * 100)}%";
                this.PerformAtEndOfFrame(salePercentLabel.Resize);
            }
            else
            {
                saleHolder.gameObject.SetActive(false);
            }

            if (standardCurrencySymbol != null)
                standardCurrencySymbol.gameObject.SetActive(currencyType == CurrencyType.STANDARD);
            
            if (premiumCurrencySymbol != null)
                premiumCurrencySymbol.gameObject.SetActive(currencyType == CurrencyType.PREMIUM);
        }

    }
}
