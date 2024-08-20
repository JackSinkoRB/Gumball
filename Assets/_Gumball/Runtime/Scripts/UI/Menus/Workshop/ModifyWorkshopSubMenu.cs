using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ModifyWorkshopSubMenu : WorkshopSubMenu
    {

        [Header("Sub part menus")]
        [SerializeField] private SubPartsWorkshopSubMenu subPartMenu;

        [SerializeField] private List<ModifyCorePartButton> corePartButtons = new();
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private int selectedCorePartIndex = -1;

        public int SelectedCorePartIndex => selectedCorePartIndex;
        
        public override void Show()
        {
            base.Show();
            
            OpenSubMenu(0);
        }

        public void OpenSubMenu(int partIndex)
        {
            //deselect old buttons
            foreach (ModifyCorePartButton corePartButton in corePartButtons)
                corePartButton.Deselect();

            //select new button
            selectedCorePartIndex = partIndex;
            corePartButtons[partIndex].Select();
            
            OpenSubMenu((CorePart.PartType) partIndex);    
        }
        
        public void OpenSubMenu(CorePart.PartType type)
        {
            subPartMenu.Initialise(type);
            subPartMenu.Show();
        }
        
    }
}
