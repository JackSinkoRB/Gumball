using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class RaceSessionPanel : GameSessionPanel
    {
        
        [SerializeField] private TextMeshProUGUI positionLabel;
        
        protected override void LateUpdate()
        {
            base.LateUpdate();
            
            UpdatePositionLabel();
        }

        private void UpdatePositionLabel()
        {
            RaceGameSession currentSession = (RaceGameSession)GameSessionManager.Instance.CurrentSession;
            int currentRank = currentSession.GetRacePosition(WarehouseManager.Instance.CurrentCar);
            positionLabel.text = $"{currentRank} / {currentSession.CurrentRacers.Count}";
        }

    }
}
