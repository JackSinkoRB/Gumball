using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class TimedSessionPanel : GameSessionPanel
    {

        /// <summary>
        /// The time (in seconds) remaining for it to be considered time nearly out.
        /// </summary>
        private const float timeNearlyOutTime = 10;
        
        [SerializeField] private TextMeshProUGUI timerLabel;
        [SerializeField] private Color timeNearlyOutTimerLabelColor = Color.red;
        
        private Color defaultTimerLabelColor;
        
        protected override void Initialise()
        {
            base.Initialise();
            
            defaultTimerLabelColor = timerLabel.color;
        }

        protected virtual void LateUpdate()
        {
            UpdateTimerLabel();
        }

        private void UpdateTimerLabel()
        {
            TimedGameSession currentSession = (TimedGameSession) GameSessionManager.Instance.CurrentSession;
            
            timerLabel.text = TimeSpan.FromSeconds(currentSession.TimeRemainingSeconds).ToPrettyString(currentSession.TimeRemainingSeconds < timeNearlyOutTime, precise: false);
            timerLabel.color = currentSession.TimeRemainingSeconds > timeNearlyOutTime ? defaultTimerLabelColor : timeNearlyOutTimerLabelColor;
        }

    }
}
