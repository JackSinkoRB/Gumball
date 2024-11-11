using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SettingsPanel : AnimatedPanel
    {

        [Header("Debugging")]
        [SerializeField, ReadOnly] private SettingsSubMenu[] subMenus;
        
        protected override void Initialise()
        {
            base.Initialise();

            subMenus = transform.GetComponentsInAllChildren<SettingsSubMenu>().ToArray();
        }
        
        protected override void OnShow()
        {
            base.OnShow();
            
            //disable all the sub menus instantly in case they were left open to prevent popping
            foreach (SettingsSubMenu subMenu in subMenus)
                subMenu.Hide(instant: true);
            
            OpenSubMenu(0);
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
                subMenu.Show();
        }
        
        public void OpenSubMenu(int index) => OpenSubMenu(subMenus[index]);

    }
}
