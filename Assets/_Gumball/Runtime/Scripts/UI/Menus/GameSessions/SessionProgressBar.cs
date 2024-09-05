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
            SplineTravelDistanceCalculator playersDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (playersDistanceCalculator == null || playersDistanceCalculator.DistanceInMap < 0)
                return;
            
            //sort the racer icons by their distance from the player's distance
            SessionProgressBarRacerIcon[] currentRacerIconsSorted = currentRacerIcons.Values.ToArray();
            Array.Sort(currentRacerIconsSorted, (racerIcon1, racerIcon2) => 
            {
                SplineTravelDistanceCalculator racer1Dist = racerIcon1.Racer.GetComponent<SplineTravelDistanceCalculator>();
                SplineTravelDistanceCalculator racer2Dist = racerIcon2.Racer.GetComponent<SplineTravelDistanceCalculator>();

                if (racer1Dist == null || racer1Dist.DistanceInMap < 0) return 1;
                if (racer2Dist == null || racer2Dist.DistanceInMap < 0) return -1;

                float distanceDiff1 = Mathf.Abs(racer1Dist.DistanceInMap - playersDistanceCalculator.DistanceInMap);
                float distanceDiff2 = Mathf.Abs(racer2Dist.DistanceInMap - playersDistanceCalculator.DistanceInMap);

                return distanceDiff1.CompareTo(distanceDiff2);
            });
            
            //set the sibling index based on the sorted order
            for (int i = 0; i < currentRacerIconsSorted.Length; i++)
            {
                int invertedIndex = currentRacerIconsSorted.Length - 1 - i;
                currentRacerIconsSorted[i].transform.SetSiblingIndex(invertedIndex);
            }
        }

        public void RemoveRacerIcon(AICar racer)
        {
            if (racer.IsPlayer)
                return; //player doesn't have an icon
            
            SessionProgressBarRacerIcon icon = currentRacerIcons[racer];
            Destroy(icon.gameObject);
            currentRacerIcons.Remove(racer);
        }
        
    }
}
