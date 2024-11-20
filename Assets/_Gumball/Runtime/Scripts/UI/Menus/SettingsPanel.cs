using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class SettingsPanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI subMenuTitle;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private SettingsSubMenu[] subMenus;
        
        private bool responsibleForVignette;
        
        protected override void Initialise()
        {
            base.Initialise();

            subMenus = transform.GetComponentsInAllChildren<SettingsSubMenu>().ToArray();
        }
        
        protected override void OnShow()
        {
            base.OnShow();

            if (PanelManager.PanelExists<VignetteBackgroundPanel>() && !PanelManager.GetPanel<VignetteBackgroundPanel>().IsShowing)
            {
                responsibleForVignette = true;
                PanelManager.GetPanel<VignetteBackgroundPanel>().Show();
                ScreenBlur.Show(true);
            }
            
            //disable all the sub menus instantly in case they were left open to prevent popping
            foreach (SettingsSubMenu subMenu in subMenus)
                subMenu.Hide(instant: true);
            
            OpenSubMenu(0);
        }

        protected override void OnHide()
        {
            foreach (SettingsSubMenu otherMenu in subMenus)
                otherMenu.Hide();
            
            base.OnHide();

            if (responsibleForVignette)
            {
                if (PanelManager.PanelExists<VignetteBackgroundPanel>() && PanelManager.GetPanel<VignetteBackgroundPanel>().IsShowing)
                    PanelManager.GetPanel<VignetteBackgroundPanel>().Hide();
                ScreenBlur.Show(false);
            }
        }

        public void OpenSubMenu(SettingsSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open
            
            //hide all other menus
            foreach (SettingsSubMenu otherMenu in subMenus)
                otherMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
            {
                subMenu.Show();
                subMenuTitle.text = subMenu.DisplayName;
            }
        }
        
        public void OpenSubMenu(int index) => OpenSubMenu(subMenus[index]);

    }
}
