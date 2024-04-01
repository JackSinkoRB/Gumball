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
        [SerializeField] private TextMeshProUGUI positionLabel;

        private RaceGameSession currentSession => (RaceGameSession) GameSessionManager.Instance.CurrentSession;
        
        private void LateUpdate()
        {
            GameSession session = GameSessionManager.Instance.CurrentSession;
            if (session == null || !session.InProgress || session is not RaceGameSession)
                return;
            
            UpdateDistanceLabel();
            UpdatePositionLabel();
        }
        
        private void UpdateDistanceLabel()
        {
            float playersDistanceTraveled = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>().DistanceTraveled;
            float distancePercent = Mathf.FloorToInt((playersDistanceTraveled / currentSession.RaceDistanceMetres) * 100f);
            distanceLabel.text = $"{distancePercent}%";
        }
        
        private void UpdatePositionLabel()
        {
            int currentRank = currentSession.GetRacePosition(WarehouseManager.Instance.CurrentCar);
            positionLabel.text = $"{currentRank} / {currentSession.CurrentRacers.Length}";
        }
        
    }
}
