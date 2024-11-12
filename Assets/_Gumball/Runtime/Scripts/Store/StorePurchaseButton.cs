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
        [SerializeField] private PurchaseData purchaseData;

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

        public PurchaseData PurchaseData => purchaseData;
        public string DisplayName => displayName;
        public string UniqueID => GetComponent<UniqueIDAssigner>().UniqueID;
        
        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        public void SetPurchaseData(PurchaseData purchaseData)
        {
            this.purchaseData = purchaseData;
            
            UpdateButtonUI();
        }

        private void Initialise()
        {
            isInitialised = true;

            UpdateButtonUI();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!EditorApplication.isUpdating && !Application.isPlaying)
                UpdateButtonUI();
        }
#endif
        
        public string GetPriceFormatted()
        {
            if (purchaseData.CurrencyType == CurrencyType.REAL)
            {
                if (!IAPManager.HasLoaded || IAPManager.Instance.StoreController == null)
                    return "N/A";
                
                Product storeProduct = IAPManager.Instance.StoreController.products.WithID(purchaseData.Product.ProductID);
                if (storeProduct == null)
                    return "N/A";
                
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
            if (purchaseData.CurrencyType == CurrencyType.STANDARD)
            {
                int finalPrice = (int)GetFinalPrice();
                if (!Currency.Standard.HasEnoughFunds(finalPrice))
                {
                    PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                    return;
                }
                
                Currency.Standard.TakeFunds(finalPrice);
                OnPurchaseSuccessful();
            } else if (purchaseData.CurrencyType == CurrencyType.PREMIUM)
            {
                int finalPrice = (int)GetFinalPrice();
                if (!Currency.Premium.HasEnoughFunds(finalPrice))
                {
                    PanelManager.GetPanel<InsufficientPremiumCurrencyPanel>().Show();
                    return;
                }
                
                Currency.Premium.TakeFunds(finalPrice);
                OnPurchaseSuccessful();
            } else if (purchaseData.CurrencyType == CurrencyType.REAL)
            {
                IAPManager.Instance.InitiatePurchase(purchaseData.Product.ProductID, new PurchaseHandler(OnPurchaseSuccessful, OnPurchaseFailed));
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
            if (purchaseData.CurrencyType == CurrencyType.REAL)
            {
                if (!IAPManager.HasLoaded || IAPManager.Instance.StoreController == null)
                    return 999;
                
                Product storeProduct = IAPManager.Instance.StoreController.products.WithID(purchaseData.Product.ProductID);
                return (float)storeProduct.metadata.localizedPrice;
            }
            else
            {
                int finalPrice = !purchaseData.IsSale ? purchaseData.Price : Mathf.RoundToInt(purchaseData.Price - (purchaseData.Price * purchaseData.DiscountPercent)); //round to int as non-real currencies don't have decimals
                return finalPrice;
            }
        }

        private float GetNonSalePrice()
        {
            if (!purchaseData.IsSale)
                return GetFinalPrice();
            
            if (purchaseData.CurrencyType == CurrencyType.REAL)
            {
                float finalPrice = GetFinalPrice();
                return finalPrice / (1 - purchaseData.DiscountPercent);
            }
            else
            {
                return purchaseData.Price;
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

            if (purchaseData.IsSale)
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
                standardCurrencySymbol.gameObject.SetActive(purchaseData.CurrencyType == CurrencyType.STANDARD);
            
            if (premiumCurrencySymbol != null)
                premiumCurrencySymbol.gameObject.SetActive(purchaseData.CurrencyType == CurrencyType.PREMIUM);
        }

    }
}
