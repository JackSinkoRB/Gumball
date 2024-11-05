using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public abstract class DailyLoginRewardNode : MonoBehaviour
    {

        public void OnClickButton()
        {
            GiveRewards();
            
            DailyLoginManager.Instance.IncreaseCurrentDayNumber();
            PanelManager.GetPanel<DailyLoginChallengesSubMenu>().RefreshNodes();
        }

        protected abstract void GiveRewards();

    }
}
