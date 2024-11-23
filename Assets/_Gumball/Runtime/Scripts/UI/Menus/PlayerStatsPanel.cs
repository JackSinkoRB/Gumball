using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class PlayerStatsPanel : AnimatedPanel
    {

        public void OnClickSettingsButton()
        {
            PanelManager.GetPanel<SettingsPanel>().Show();
        }
        
        public void OnClickShopItemButton()
        {
            PanelManager.GetPanel<StorePanel>().Show();
        }
        
    }
}
