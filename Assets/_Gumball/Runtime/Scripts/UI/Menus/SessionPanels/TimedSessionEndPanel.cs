using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class TimedSessionEndPanel : SessionEndPanel
    {
        
        private TimedGameSession timedSession => GameSessionManager.Instance.CurrentSession as TimedGameSession;

        protected override void OnShow()
        {
            base.OnShow();
            
            SetVictory(timedSession.TimeRemainingSeconds > 0);
            ShowPosition(false);
        }
        
    }
}
