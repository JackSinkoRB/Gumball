using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SessionProgressBarRacerIcon : MonoBehaviour
    {

        [SerializeField] private Image icon;

        private SessionProgressBar progressBar;
        private AICar racer;

        private RectTransform rectTransform => transform as RectTransform;
        
        public void Initialise(SessionProgressBar progressBar, AICar racer)
        {
            this.progressBar = progressBar;
            this.racer = racer;

            RacerInfoProfile infoProfile = GameSessionManager.Instance.CurrentSession.CurrentRacers[racer].InfoProfile;
            if (infoProfile != null && infoProfile.Icon != null)
                icon.sprite = infoProfile.Icon;
        }

        private void LateUpdate()
        {
            GameSession currentSession = GameSessionManager.Instance.CurrentSession;
            if (currentSession.RaceDistanceMetres == 0)
                return;

            SplineTravelDistanceCalculator racersDistanceCalculator = racer.GetComponent<SplineTravelDistanceCalculator>();
            if (racersDistanceCalculator == null)
                return;
            
            float percent = Mathf.Clamp01(racersDistanceCalculator.DistanceInMap / currentSession.RaceDistanceMetres);

            RectTransform progressBarRect = progressBar.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = rectTransform.anchoredPosition.SetX(percent * progressBarRect.rect.width);
        }
        
    }
}
