using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CustomiseWorkshopPanel : AnimatedPanel
    {
        
        [SerializeField] private PartsWorkshopMenu bodyKitMenu;
        [SerializeField] private BodyPaintWorkshopMenu bodyPaintMenu;
        [SerializeField] private LiveryWorkshopMenu liveryMenu;
        [SerializeField] private WheelPaintWorkshopMenu wheelPaintMenu;
        [SerializeField] private StanceWorkshopMenu stanceMenu;
        
        public void OnClickBackButton()
        {
            Hide();
            PanelManager.GetPanel<WarehousePanel>().Show();
        }

        protected override void OnShow()
        {
            base.OnShow();

            OpenSubMenu(null);
        }

        public void OpenSubMenu(WorkshopSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open
            
            //hide all other menus
            bodyKitMenu.Hide();
            bodyPaintMenu.Hide();
            liveryMenu.Hide();
            wheelPaintMenu.Hide();
            stanceMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }
        
    }
}
