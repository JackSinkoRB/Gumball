using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WorkshopSelectPanel : AnimatedPanel
    {
        
        public void OnClickBackButton()
        {
            WorkshopSceneManager.Instance.ExitWorkshopScene();
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
        
    }
}
