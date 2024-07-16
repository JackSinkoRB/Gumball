using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

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
        
        private static readonly Dictionary<string, PurchaseHandler> purchasesInProgress = new(); //string is product ID

#if UNITY_EDITOR
        public static bool IsRunningTests;
#endif
        
        [SerializeField] private bool useStoreKitTesting;
        [Tooltip("Manually register products here that aren't included in the store.")]
        [SerializeField] private List<IAPProduct> nonStoreProducts = new();
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<IAPProduct> storeProducts = new();

        private CrossPlatformValidator purchaseValidator;
        
        public InitialisationStatusType InitialisationStatus { get; private set; }
        public IStoreController StoreController { get; private set; }
        
        //the CrossPlatform validator only supports the GooglePlayStore and Apple's App Stores.
        private bool IsCurrentStoreSupportedByValidator => StandardPurchasingModule.Instance().appStore is AppStore.GooglePlay or AppStore.AppleAppStore or AppStore.MacAppStore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
#if UNITY_EDITOR
            IsRunningTests = false;
#endif
            
            purchasesInProgress.Clear();
        }
        
        public void Initialise()
        {
            InitialisationStatus = InitialisationStatusType.LOADING;
            GlobalLoggers.StoreLogger.Log("Initialising store.");
            
#if UNITY_EDITOR
            //allows simulating failed IAP transactions in the Editor
            StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
            
#if !UNITY_EDITOR
            //the CrossPlatform validator only supports Google Play and Apple App Store
            if (IsCurrentStoreSupportedByValidator)
            {
				byte[] appleTangleData = useStoreKitTesting ? AppleStoreKitTestTangle.Data() : AppleTangle.Data();
				purchaseValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), appleTangleData, Application.identifier);
            }
#endif

            InitialiseProducts();

            //check for timeout
            const float timeoutSeconds = 10;
            CoroutineHelper.Instance.PerformAfterDelay(timeoutSeconds, () =>
            {
                if (InitialisationStatus == InitialisationStatusType.LOADING)
                {
                    InitialisationStatus = InitialisationStatusType.ERROR;
                    Debug.LogError("Timeout waiting to initialise UnityPurchasing.");
                }
            });
        }
        
        private void InitialiseProducts()
        {
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (IAPProduct product in storeProducts)
                builder.AddProduct(product.ProductID, product.Type);

            foreach (IAPProduct product in nonStoreProducts)
                builder.AddProduct(product.ProductID, product.Type);
            
            UnityPurchasing.Initialize(this, builder);
        }
        
#if UNITY_EDITOR
        public void SetStoreProducts(List<IAPProduct> products)
        {
            storeProducts = products;
            EditorUtility.SetDirty(this);
        }
#endif

        public void InitiatePurchase(string productID, PurchaseHandler handler)
        {
            try
            {
                GlobalLoggers.StoreLogger.Log($"Initiating purchase for {productID}.");

                if (purchasesInProgress.ContainsKey(productID))
                {
                    Debug.LogError("Could not initiate purchase as a purchase initiation for this item is already in progress.");
                    handler.onFail?.Invoke();
                    return;
                }

                purchasesInProgress[productID] = handler;

                StoreController.InitiatePurchase(productID);
            }
            catch (Exception e)
            {
                purchasesInProgress.Remove(productID);
                handler.onFail?.Invoke();
                throw new Exception("There was an error while initiating a purchase.", e);
            }
        }
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            StoreController = controller;
            
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
#if UNITY_EDITOR
            if (!IsRunningTests)
#endif
                Debug.LogError($"Store initialisation error: {error}{additionalErrorMessage}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Product product = purchaseEvent.purchasedProduct;
            string productID = product.definition.id;
            PurchaseHandler handler = purchasesInProgress[productID];
            
            if (!IsPurchaseValid(product))
            {
                purchasesInProgress.Remove(productID);
                handler.onFail?.Invoke();
                return PurchaseProcessingResult.Complete;
            }
            
            purchasesInProgress.Remove(productID);
            handler.onSuccess?.Invoke();            

            return PurchaseProcessingResult.Complete;
        }
        
        private bool IsPurchaseValid(Product product)
        {
            //if the validator doesn't support the current store, we assume the purchase is valid
            if (IsCurrentStoreSupportedByValidator)
            {
                try
                {
                    purchaseValidator.Validate(product.receipt);
                }
                catch (IAPSecurityException reason) //if the purchase is deemed invalid, the validator throws an IAPSecurityException.
                {
                    GlobalLoggers.StoreLogger.Log($"Invalid receipt: {reason}");
                    return false;
                }
            }

            return true;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string productID = product.definition.id;
            GlobalLoggers.StoreLogger.Log($"Store purchase failed: {productID} - {failureReason}");
            
            PurchaseHandler handler = purchasesInProgress[productID];
            handler.onFail?.Invoke();

            purchasesInProgress.Remove(productID);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            string productID = product.definition.id;
            GlobalLoggers.StoreLogger.Log($"Purchase failed - Product: '{productID}'," +
                                          $" Purchase failure reason: {failureDescription.reason}," +
                                          $" Purchase failure details: {failureDescription.message}");

            PurchaseHandler handler = purchasesInProgress[productID];
            handler.onFail?.Invoke();
            
            purchasesInProgress.Remove(productID);
        }

    }
}
