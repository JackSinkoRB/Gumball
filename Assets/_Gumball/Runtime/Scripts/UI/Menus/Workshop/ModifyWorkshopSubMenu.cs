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

        public override void OnAddToPanelLookup()
        {
            base.OnAddToPanelLookup();
            
            OpenSubMenu(0);
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            OpenSubMenu(selectedCorePartIndex);
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
