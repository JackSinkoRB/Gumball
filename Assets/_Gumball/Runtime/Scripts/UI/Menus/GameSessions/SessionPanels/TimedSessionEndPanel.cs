using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class TimedSessionEndPanel : SessionEndPanel
    {

        private TimedGameSession timedSession => GameSessionManager.Instance.CurrentSession as TimedGameSession;
        
        protected override void OnShowScorePanel()
        {
            string timeGoalUserFriendly = TimeSpan.FromSeconds(timedSession.TimeAllowedSeconds).ToPrettyString();
            PanelManager.GetPanel<SessionScorePanel>().PopulateMainObjective(timeGoalUserFriendly);
        }
        
    }
}
