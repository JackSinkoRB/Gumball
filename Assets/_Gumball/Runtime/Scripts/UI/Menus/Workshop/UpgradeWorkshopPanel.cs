using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class UpgradeWorkshopPanel : AnimatedPanel
    {
        
        [Header("Sub part menus")]
        [SerializeField] private ModifyWorkshopSubMenu modifySubMenu;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private WorkshopSubMenu currentSubMenu;

        public ModifyWorkshopSubMenu ModifySubMenu => modifySubMenu;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            OpenSubMenu(null);
        }
        
        public void OnClickBackButton()
        {
            Hide();
            PanelManager.GetPanel<WarehousePanel>().Show();
        }

        public void OnClickCancelButton()
        {
            if (currentSubMenu != null)
                currentSubMenu.Hide();
        }
        
        public void OpenSubMenu(WorkshopSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open

            currentSubMenu = subMenu;
            
            //hide all other menus
            modifySubMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }

    }
}
