using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class StorePanel : AnimatedPanel
    {
        
        [SerializeField] private SpecialsStoreMenu specialsMenu;
        [SerializeField] private ContentPacksStoreMenu contentPacksMenu;
        [SerializeField] private CarsStoreMenu carsMenu;
        [SerializeField] private PartsStoreMenu partsMenu;
        [SerializeField] private CurrencyStoreMenu currencyMenu;
        [Space(5)]
        [SerializeField] private Button specialsCategoryButton;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            OpenSubMenu(null);
            
            //disable the specials category if no PlayFab connection
            if (PlayFabManager.LoginStatus != PlayFabManager.ConnectionStatusType.SUCCESS)
                specialsCategoryButton.interactable = false;
        }
        
        public void OpenSubMenu(StoreSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open
            
            //hide all menus
            specialsMenu.Hide();
            contentPacksMenu.Hide();
            carsMenu.Hide();
            partsMenu.Hide();
            currencyMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }
        
#if UNITY_EDITOR
        [ButtonMethod]
        public void RefreshIAPProducts()
        {
            List<IAPProduct> storeProducts = new();
            foreach (StorePurchaseButton purchaseButton in transform.GetComponentsInAllChildren<StorePurchaseButton>())
            {
                if (purchaseButton.PurchaseData.CurrencyType == CurrencyType.REAL)
                    storeProducts.Add(purchaseButton.PurchaseData.Product);
            }
            
            IAPManager.Instance.SetStoreProducts(storeProducts);

            Debug.Log($"Found {storeProducts.Count} products in store panel and added them to the IAP product catalogue.");
        }
#endif
        
    }
}
