using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Session completions")]
    public class SessionCompleteChallengeTracker : ChallengeTracker
    {

        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            GameSession.onSessionEnd += OnSessionEnd;
        }

        private void OnSessionEnd(GameSession session, GameSession.ProgressStatus progress)
        {
            if (progress == GameSession.ProgressStatus.COMPLETE)
                Track(1);
        }
        
    }
}
