using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ChallengesPanel : AnimatedPanel
    {

        [SerializeField] private ChallengesHeaderFilter header;
        [SerializeField] private ChallengesSubMenu dailyMenu;
        [SerializeField] private ChallengesSubMenu weeklyMenu;
        [SerializeField] private ChallengesSubMenu dailyLoginMenu;

        private ChallengesSubMenu defaultMenu => dailyLoginMenu;

        public ChallengesHeaderFilter Header => header;
        
        protected override void OnShow()
        {
            base.OnShow();

            PanelManager.GetPanel<MainMenuPanel>().Hide();
            OpenSubMenu(defaultMenu);

            this.PerformAtEndOfFrame(Canvas.ForceUpdateCanvases);
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            PanelManager.GetPanel<MainMenuPanel>().Show();
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
