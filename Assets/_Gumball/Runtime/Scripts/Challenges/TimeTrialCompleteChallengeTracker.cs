using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Time trial completions")]
    public class TimeTrialCompleteChallengeTracker : ChallengeTracker
    {

        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            GameSession.onSessionEnd += OnSessionEnd;
        }

        private void OnSessionEnd(GameSession session, GameSession.ProgressStatus progress)
        {
            if (session is TimedGameSession && progress == GameSession.ProgressStatus.COMPLETE)
                Track(1);
        }
        
    }
}
