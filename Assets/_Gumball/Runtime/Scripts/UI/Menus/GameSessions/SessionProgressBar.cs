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
        [SerializeField] private float interpolateSpeed = 1;

        public float InterpolateSpeed => interpolateSpeed;
        
        private void OnEnable()
        {
            progressBarFill.fillAmount = 0;
            
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
                
                SessionProgressBarRacerIcon instance = Instantiate(racerIconPrefab, transform).GetComponent<SessionProgressBarRacerIcon>();
                instance.Initialise(this, racer);
            }
        }

        private void UpdateForPlayer()
        {
            GameSession currentSession = GameSessionManager.Instance.CurrentSession;
            if (currentSession.RaceDistanceMetres == 0)
                return;

            SplineTravelDistanceCalculator playersDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (playersDistanceCalculator == null || playersDistanceCalculator.DistanceInMap < 0)
                return;
            
            float percent = Mathf.Clamp01(playersDistanceCalculator.DistanceInMap / currentSession.RaceDistanceMetres);
            progressBarFill.fillAmount = Mathf.Lerp(progressBarFill.fillAmount, percent, interpolateSpeed * Time.deltaTime);
        }

    }
}
