using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class PausePanelDriving : AnimatedPanel
    {

        public void OnClickSettingsButton()
        {
            Hide(true);
            PanelManager.GetPanel<SettingsPanel>().Show();
        }

        public override void OnAddToStack()
        {
            base.OnAddToStack();
            
            Time.timeScale = 0;
        }

        public override void OnRemoveFromStack()
        {
            base.OnRemoveFromStack();
            
            Time.timeScale = 1;
        }
        
    }
}
