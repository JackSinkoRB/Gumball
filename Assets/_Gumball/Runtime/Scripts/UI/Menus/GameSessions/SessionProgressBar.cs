using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SessionProgressBar : MonoBehaviour
    {
        
        [SerializeField] private Image progressBarFill;
        [SerializeField] private SessionProgressBarRacerIcon racerIconPrefab;
        [SerializeField] private float interpolateSpeed = 1;
        [SerializeField] private Transform racerIconHolder;

        private Dictionary<AICar, SessionProgressBarRacerIcon> currentRacerIcons = new();
        
        private GameSession currentSession => GameSessionManager.Instance.CurrentSession;

        public Dictionary<AICar, SessionProgressBarRacerIcon> CurrentRacerIcons => currentRacerIcons;
        
        private void OnEnable()
        {
            progressBarFill.fillAmount = 0;
            
            SetupRacerIcons();
        }

        private void LateUpdate()
        {
            if (currentSession.RaceDistanceMetres == 0)
                return;
            
            UpdatePlayer();
            UpdateRacerIconPositions();
            UpdateRacerLayering();
        }
        
        /// <summary>
        /// Stops the racer icon from updating and greys it out.
        /// </summary>
        public void DisableRacerIcon(AICar racer)
        {
            if (racer.IsPlayer)
                return; //player doesn't have an icon
            
            SessionProgressBarRacerIcon icon = currentRacerIcons[racer];
            currentRacerIcons.Remove(racer);
            icon.Disable();
        }

        private void SetupRacerIcons()
        {
            foreach (AICar racer in GameSessionManager.Instance.CurrentSession.CurrentRacers.Keys)
            {
                if (racer.IsPlayer)
                    continue;
                
                SessionProgressBarRacerIcon instance = Instantiate(racerIconPrefab, racerIconHolder).GetComponent<SessionProgressBarRacerIcon>();
                instance.Initialise(racer);
                
                currentRacerIcons[racer] = instance;
            }
        }

        private void UpdatePlayer()
        {
            SplineTravelDistanceCalculator playersDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (playersDistanceCalculator == null || playersDistanceCalculator.DistanceInMap < 0)
                return;
            
            float percent = Mathf.Clamp01(playersDistanceCalculator.DistanceInMap / currentSession.RaceDistanceMetres);
            progressBarFill.fillAmount = Mathf.Lerp(progressBarFill.fillAmount, percent, interpolateSpeed * Time.deltaTime);
        }

        private void UpdateRacerIconPositions()
        {
            foreach (SessionProgressBarRacerIcon racerIcon in currentRacerIcons.Values)
            {
                SplineTravelDistanceCalculator racersDistanceCalculator = racerIcon.Racer.GetComponent<SplineTravelDistanceCalculator>();
                if (racersDistanceCalculator == null || racersDistanceCalculator.DistanceInMap < 0)
                    continue;
            
                float percent = Mathf.Clamp01(racersDistanceCalculator.DistanceInMap / currentSession.RaceDistanceMetres);
            
                RectTransform progressBarRect = GetComponent<RectTransform>();
                racerIcon.RectTransform.anchoredPosition = Vector2.Lerp(racerIcon.RectTransform.anchoredPosition, racerIcon.RectTransform.anchoredPosition.SetX(percent * progressBarRect.rect.width), interpolateSpeed * Time.deltaTime);
            }
        }
        
        private void UpdateRacerLayering()
        {
            if (GameSessionManager.Instance.CurrentSession is not RaceGameSession raceSession)
                return; //only race sessions have racer positions
            
            SplineTravelDistanceCalculator playersDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (playersDistanceCalculator == null || playersDistanceCalculator.DistanceInMap < 0)
                return;
            
            //sort the racer icons by their race position
            SessionProgressBarRacerIcon[] currentRacerIconsSorted = currentRacerIcons.Values.ToArray();
            Array.Sort(currentRacerIconsSorted, (racerIcon1, racerIcon2) => 
            {
                int racer1Position = raceSession.GetRacePosition(racerIcon1.Racer);
                int racer2Position = raceSession.GetRacePosition(racerIcon2.Racer);
                return racer1Position.CompareTo(racer2Position);
            });
            
            //set the sibling index based on the sorted order
            for (int i = 0; i < currentRacerIconsSorted.Length; i++)
            {
                int invertedIndex = currentRacerIconsSorted.Length - 1 - i;
                currentRacerIconsSorted[i].transform.SetSiblingIndex(invertedIndex);
            }
        }

    }
}
