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
        private SplineTravelDistanceCalculator playersDistanceCalculator => WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();

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
            float distancePercent = Mathf.FloorToInt((playersDistanceCalculator.DistanceTraveled / currentSession.RaceDistanceMetres) * 100f);
            distanceLabel.text = $"{distancePercent}%";
        }
        
        private void UpdatePositionLabel()
        {
            int currentRank = currentSession.GetRacePosition(WarehouseManager.Instance.CurrentCar);
            positionLabel.text = $"{currentRank} / {currentSession.CurrentRacers.Length}";
        }
        
    }
}
