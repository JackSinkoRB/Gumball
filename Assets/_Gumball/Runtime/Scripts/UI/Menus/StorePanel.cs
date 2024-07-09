using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class StorePanel : AnimatedPanel
    {
        
        [SerializeField] private CurrencyStoreMenu currencyMenu;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            OpenSubMenu(null);
        }
        
        public void OpenSubMenu(StoreSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open
            
            //hide all menus
            currencyMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }
        
#if UNITY_EDITOR
        [ButtonMethod]
        public void RefreshIAPProducts()
        {
            IAPManager.Instance.ClearProducts();
            int realProducts = 0;
            foreach (StorePurchaseButton purchaseButton in transform.GetComponentsInAllChildren<StorePurchaseButton>())
            {
                if (purchaseButton.CurrencyType == CurrencyType.REAL)
                {
                    IAPManager.Instance.AddProduct(purchaseButton.Product);
                    realProducts++;
                }
            }
            
            Debug.Log($"Found {realProducts} products in store panel and added them to the IAP product catalogue.");
        }
#endif
        
    }
}
