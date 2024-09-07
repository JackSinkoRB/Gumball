using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SessionRewardPanel : RewardPanel
    {

        public void OnClickRetryButton()
        {
            GameSessionManager.Instance.RestartCurrentSession();
        }
        
    }
}
