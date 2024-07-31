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
        [SerializeField] private Image progressBarFill;
        
        private Color defaultTimerLabelColor;
        
        private TimedGameSession currentSession => (TimedGameSession) GameSessionManager.Instance.CurrentSession;
        private SplineTravelDistanceCalculator playersDistanceCalculator => WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();

        protected override void Initialise()
        {
            base.Initialise();
            
            defaultTimerLabelColor = timerLabel.color;
        }

        private void LateUpdate()
        {
            GameSession session = GameSessionManager.Instance.CurrentSession;
            if (session == null || !session.InProgress || session is not TimedGameSession)
                return;

            UpdateTimerLabel();
            UpdateProgressBar();
        }

        private void UpdateTimerLabel()
        {
            timerLabel.text = TimeSpan.FromSeconds(currentSession.TimeRemainingSeconds).ToPrettyString(currentSession.TimeRemainingSeconds < timeNearlyOutTime, precise: false);

            timerLabel.color = currentSession.TimeRemainingSeconds > timeNearlyOutTime ? defaultTimerLabelColor : timeNearlyOutTimerLabelColor;
        }
        
        private void UpdateProgressBar()
        {
            float percent = playersDistanceCalculator.DistanceTraveled / currentSession.RaceDistanceMetres;
            progressBarFill.fillAmount = Mathf.Clamp01(percent);
        }
        
    }
}
