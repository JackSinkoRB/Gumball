using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SessionProgressBar : MonoBehaviour
    {
        
        [SerializeField] private Image progressBarFill;
        [SerializeField] private SessionProgressBarRacerIcon racerIconPrefab;
        [SerializeField] private Transform racerIconHolder;
        
        private void OnEnable()
        {
            SetupRacerIcons();
        }

        private void LateUpdate()
        {
            UpdateForPlayer();
        }

        private void SetupRacerIcons()
        {
            foreach (AICar racer in GameSessionManager.Instance.CurrentSession.CurrentRacers.Keys)
            {
                if (racer.IsPlayer)
                    continue;
                
                SessionProgressBarRacerIcon instance = Instantiate(racerIconPrefab, racerIconHolder).GetComponent<SessionProgressBarRacerIcon>();
                instance.Initialise(this, racer);
            }
        }

        private void UpdateForPlayer()
        {
            GameSession currentSession = GameSessionManager.Instance.CurrentSession;
            if (currentSession.RaceDistanceMetres == 0)
                return;

            SplineTravelDistanceCalculator playersDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (playersDistanceCalculator == null)
                return;
            
            float percent = Mathf.Clamp01(playersDistanceCalculator.DistanceInMap / currentSession.RaceDistanceMetres);
            progressBarFill.fillAmount = percent;
        }

    }
}
