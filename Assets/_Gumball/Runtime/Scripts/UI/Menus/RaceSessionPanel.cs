using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class RaceSessionPanel : AnimatedPanel
    {
        
        [SerializeField] private TextMeshProUGUI distanceLabel;

        private RaceGameSession currentSession => (RaceGameSession) GameSessionManager.Instance.CurrentSession;
        
        private void LateUpdate()
        {
            GameSession session = GameSessionManager.Instance.CurrentSession;
            if (session == null || !session.InProgress || session is not RaceGameSession)
                return;
            
            UpdateDistanceLabel();
        }
        
        private void UpdateDistanceLabel()
        {
            float distancePercent = Mathf.FloorToInt((currentSession.SplineDistanceTraveled / currentSession.RaceDistanceMetres) * 100f);
            distanceLabel.text = $"{distancePercent}%";
        }
        
    }
}
