using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WorkshopPanel : AnimatedPanel
    {
        
        [SerializeField] private PartsWorkshopMenu bodyKitMenu;
        [SerializeField] private WorkshopSubMenu paintMenu;
        [SerializeField] private WorkshopSubMenu liveryMenu;
        [SerializeField] private WorkshopSubMenu stanceMenu;
        
        public void OnClickBackButton()
        {
            MainSceneManager.LoadMainScene();
        }

        public void OpenSubMenu(WorkshopSubMenu subMenu)
        {
            if (subMenu.IsShowing)
                return; //already open
            
            //hide all other menus
            bodyKitMenu.Hide();
            paintMenu.Hide();
            liveryMenu.Hide();
            stanceMenu.Hide();
            
            //just show this menu
            subMenu.Show();
        }
        
    }
}
