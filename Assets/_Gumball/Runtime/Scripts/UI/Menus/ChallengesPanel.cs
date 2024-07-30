using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class ChallengesPanel : AnimatedPanel
    {

        [SerializeField] private ChallengesSubMenu dailyMenu;
        [SerializeField] private ChallengesSubMenu weeklyMenu;
        [SerializeField] private ChallengesSubMenu dailyLoginMenu;

        private ChallengesSubMenu defaultMenu => dailyLoginMenu;
        
        protected override void OnShow()
        {
            base.OnShow();

            OpenSubMenu(defaultMenu);
        }
        
        public void OpenSubMenu(ChallengesSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open
            
            //hide all menus
            dailyMenu.Hide();
            weeklyMenu.Hide();
            dailyLoginMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }
        
    }
}
