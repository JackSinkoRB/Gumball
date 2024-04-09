using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class TimedSessionPanel : AnimatedPanel
    {

        /// <summary>
        /// The time (in seconds) remaining for it to be considered time nearly out.
        /// </summary>
        private const float timeNearlyOutTime = 10;
        
        [SerializeField] private TextMeshProUGUI timerLabel;
        [SerializeField] private Color timeNearlyOutTimerLabelColor = Color.red;
        [SerializeField] private TextMeshProUGUI distanceLabel;

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

            UpdateDistanceLabel();
            UpdateTimerLabel();
        }
        
        private void UpdateDistanceLabel()
        {
            float distancePercent = Mathf.FloorToInt((playersDistanceCalculator.DistanceTraveled / currentSession.RaceDistanceMetres) * 100f);
            distanceLabel.text = $"{distancePercent}%";
        }
        
        private void UpdateTimerLabel()
        {
            const float timeRemainingForMs = 10; //if timer goes below this value, show the milliseconds
            timerLabel.text = TimeSpan.FromSeconds(currentSession.TimeRemainingSeconds).ToPrettyString(currentSession.TimeRemainingSeconds < timeRemainingForMs, precise: false);

            timerLabel.color = currentSession.TimeRemainingSeconds > timeNearlyOutTime ? defaultTimerLabelColor : timeNearlyOutTimerLabelColor;
        }

    }
}
