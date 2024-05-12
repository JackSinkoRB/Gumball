using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class GameSessionEndPanel : AnimatedPanel
    {
        
        private RewardPanel rewardPanel => PanelManager.GetPanel<RewardPanel>();
        
        public void OnClickExitButton()
        {
            if (rewardPanel.PendingRewards > 0)
                rewardPanel.Show();
            
            this.PerformAfterTrue(() => !rewardPanel.IsShowing, MainSceneManager.LoadMainScene);
        }
        
    }
}
