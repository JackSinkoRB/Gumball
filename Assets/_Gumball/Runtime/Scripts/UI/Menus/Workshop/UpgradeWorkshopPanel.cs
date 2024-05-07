using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class UpgradeWorkshopPanel : AnimatedPanel
    {

        [Header("Sub part menus")]
        [SerializeField] private SubPartsWorkshopSubMenu engineSubPartMenu;
        [SerializeField] private SubPartsWorkshopSubMenu wheelsSubPartMenu;
        [SerializeField] private SubPartsWorkshopSubMenu drivetrainSubPartMenu;
        
        public void OnClickBackButton()
        {
            Hide();
            PanelManager.GetPanel<WorkshopSelectPanel>().Show();
        }
        
        public void OpenSubMenu(WorkshopSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open
            
            //hide all other menus
            engineSubPartMenu.Hide();
            wheelsSubPartMenu.Hide();
            drivetrainSubPartMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }

    }
}
