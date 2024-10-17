using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class RaceSessionPanel : TimedSessionPanel
    {
        
        [SerializeField] private TextMeshProUGUI positionLabel;

        protected virtual int numberOfRacers => GameSessionManager.Instance.CurrentSession.CurrentRacers.Count;
        
        protected override void LateUpdate()
        {
            base.LateUpdate();
            
            UpdatePositionLabel();
        }

        private void UpdatePositionLabel()
        {
            RaceGameSession currentSession = (RaceGameSession)GameSessionManager.Instance.CurrentSession;

            if (!currentSession.InProgress)
                return;
            
            int currentRank = currentSession.GetRacePosition(WarehouseManager.Instance.CurrentCar);
            positionLabel.text = $"<size=62>{currentRank}</size>/{numberOfRacers}";
        }

    }
}
