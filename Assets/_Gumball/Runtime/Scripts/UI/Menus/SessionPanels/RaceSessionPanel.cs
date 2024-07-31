using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class RaceSessionPanel : GameSessionPanel
    {
        
        [SerializeField] private TextMeshProUGUI positionLabel;
        [SerializeField] private Image progressBarFill;

        private RaceGameSession currentSession => (RaceGameSession) GameSessionManager.Instance.CurrentSession;
        private SplineTravelDistanceCalculator playersDistanceCalculator => WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();

        private void LateUpdate()
        {
            GameSession session = GameSessionManager.Instance.CurrentSession;
            if (session == null || !session.InProgress || session is not RaceGameSession)
                return;
            
            UpdatePositionLabel();
            UpdateProgressBar();
        }

        private void UpdatePositionLabel()
        {
            int currentRank = currentSession.GetRacePosition(WarehouseManager.Instance.CurrentCar);
            positionLabel.text = $"{currentRank} / {currentSession.CurrentRacers.Count}";
        }

        private void UpdateProgressBar()
        {
            if (currentSession.RaceDistanceMetres == 0)
                return;
            
            float percent = playersDistanceCalculator.DistanceTraveled / currentSession.RaceDistanceMetres;
            progressBarFill.fillAmount = Mathf.Clamp01(percent);
        }
        
    }
}
