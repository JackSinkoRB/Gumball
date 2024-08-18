using System;
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

        private SubPartsWorkshopSubMenu currentSubPartMenu;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            OpenSubMenu(null);
        }
        
        public void OnClickBackButton()
        {
            Hide();
            PanelManager.GetPanel<WarehousePanel>().Show();
        }

        public void OnClickCancelButton()
        {
            if (currentSubPartMenu != null)
                currentSubPartMenu.Hide();
        }
        
        public void OpenSubMenu(SubPartsWorkshopSubMenu subMenu)
        {
            if (subMenu != null && subMenu.IsShowing)
                return; //already open

            currentSubPartMenu = subMenu;
            
            //hide all other menus
            engineSubPartMenu.Hide();
            wheelsSubPartMenu.Hide();
            drivetrainSubPartMenu.Hide();
            
            //just show this menu
            if (subMenu != null)
                subMenu.Show();
        }

        public void OpenSubMenu(CorePart.PartType type)
        {
            switch (type)
            {
                case CorePart.PartType.ENGINE:
                    OpenSubMenu(engineSubPartMenu);
                    break;
                case CorePart.PartType.WHEELS:
                    OpenSubMenu(wheelsSubPartMenu);
                    break;
                case CorePart.PartType.DRIVETRAIN:
                    OpenSubMenu(drivetrainSubPartMenu);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
