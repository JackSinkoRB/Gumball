using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class WarehousePanel : AnimatedPanel
    {

        public void OnClickBackButton()
        {
            WarehouseSceneManager.Instance.ExitWarehouseScene();
        }
        
        public void OnClickUpgradeButton()
        {
            Hide();
            PanelManager.GetPanel<UpgradeWorkshopPanel>().Show();
        }
        
        public void OnClickCustomiseButton()
        {
            Hide();
            PanelManager.GetPanel<CustomiseWorkshopPanel>().Show();
        }
        
        public void OnClickBrowseButton()
        {
            Hide();
            PanelManager.GetPanel<PlayerStatsPanel>().Hide();
            
            PanelManager.GetPanel<SwapCarPanel>().Show();
        }

    }
}
