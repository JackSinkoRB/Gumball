using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/IAP Manager")]
    public class IAPManager : SingletonScriptable<IAPManager>, IDetailedStoreListener
    {

        public enum InitialisationStatusType
        {
            LOADING,
            SUCCESS,
            ERROR
        }
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<IAPProduct> allProducts = new();

        private IStoreController storeController;

        public InitialisationStatusType InitialisationStatus { get; private set; }
        
        public void Initialise()
        {
            InitialisationStatus = InitialisationStatusType.LOADING;
            GlobalLoggers.StoreLogger.Log("Initialising store.");
            
            InitialiseProducts();
        }

        public void ClearProducts()
        {
            allProducts.Clear();
        }

        public void AddProduct(IAPProduct product)
        {
            allProducts.Add(product);
        }
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            
            InitialisationStatus = InitialisationStatusType.SUCCESS;
            GlobalLoggers.StoreLogger.Log("Store initialisation successful.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, null);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            InitialisationStatus = InitialisationStatusType.ERROR;
            
            string additionalErrorMessage = message != null ? $" - {message}" : "";
            Debug.LogError($"Store initialisation error: {error}{additionalErrorMessage}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            throw new NotImplementedException();
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            
        }
        
        private void InitialiseProducts()
        {
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (IAPProduct product in allProducts)
            {
                builder.AddProduct(product.ProductID, product.Type);
            }
            
            UnityPurchasing.Initialize(this, builder);
        }
        
    }
}
