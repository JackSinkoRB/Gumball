using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class ModifyWorkshopSubMenu : WorkshopSubMenu
    {

        [Header("Sub part menus")]
        [SerializeField] private SubPartsWorkshopSubMenu subPartMenu;

        public override void Show()
        {
            base.Show();
            
            OpenSubMenu(0);
        }

        public void OpenSubMenu(int partIndex)
        {
            OpenSubMenu((CorePart.PartType) partIndex);    
        }
        
        public void OpenSubMenu(CorePart.PartType type)
        {
            subPartMenu.Initialise(type);
            subPartMenu.Show();
        }
        
    }
}
