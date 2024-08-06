using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class GameSessionPanel : AnimatedPanel
    {

        [Tooltip("Optional: progress bar")]
        [SerializeField] private Image progressBarFill;

        protected virtual void LateUpdate()
        {
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            if (progressBarFill == null)
                return;
            
            GameSession currentSession = GameSessionManager.Instance.CurrentSession;
            if (currentSession.RaceDistanceMetres == 0)
                return;

            SplineTravelDistanceCalculator playersDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (playersDistanceCalculator == null)
                return;
            
            float percent = playersDistanceCalculator.DistanceInMap / currentSession.RaceDistanceMetres;
            progressBarFill.fillAmount = Mathf.Clamp01(percent);
        }
        
    }
}
