using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class UpgradeWorkshopPanel : AnimatedPanel
    {
        
        [SerializeField] private VirtualButton cancelButton;
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
            {
                currentSubMenu.Hide();
                cancelButton.gameObject.SetActive(false); //disable cancel button if no menus open as it blocks
            }
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
            
            cancelButton.gameObject.SetActive(subMenu != null); //disable cancel button if no menus open as it blocks
        }

    }
}
