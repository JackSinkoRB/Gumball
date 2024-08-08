using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class PlayerStatsPanel : AnimatedPanel
    {
        
        public void OnClickDriveButton()
        {
            MapSceneManager.LoadMapScene();
        }

        public void OnClickSettingsButton()
        {
            PanelManager.GetPanel<SettingsPanel>().Show();
        }
        
    }
}
