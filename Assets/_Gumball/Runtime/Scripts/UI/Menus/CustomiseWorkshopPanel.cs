using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CustomiseWorkshopPanel : AnimatedPanel
    {

        [Header("Debugging")]
        [SerializeField, ReadOnly] private WorkshopSubMenu[] subMenus;

        protected override void Initialise()
        {
            base.Initialise();

            subMenus = transform.GetComponentsInAllChildren<WorkshopSubMenu>().ToArray();
        }

        public void OnClickBackButton()
        {
            Hide();
            PanelManager.GetPanel<WarehousePanel>().Show();
        }

        protected override void OnShow()
        {
            base.OnShow();

            OpenSubMenu(null);
            
            //disable all the sub menus instantly in case they were left open to prevent popping
            foreach (WorkshopSubMenu subMenu in subMenus)
                subMenu.Hide(instant: true);
        }

        public void OpenSubMenu(WorkshopSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open
            
            //hide all other menus
            foreach (WorkshopSubMenu otherMenu in subMenus)
                otherMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }
        
    }
}
