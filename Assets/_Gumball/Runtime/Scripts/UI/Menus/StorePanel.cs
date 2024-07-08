using System.Collections;
using System.Collections.Generic;
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
        
    }
}
