using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class AirTimeSkillCheck : SkillCheck
    {
    
        public event Action onPerformLanding;
        
        [Header("Air time")]
        [SerializeField] private float minSpeedKmh = 50;

        [Header("Landing")]
        [SerializeField] private TextMeshProUGUI landingLabel;
        [SerializeField] private float landingLabelDuration = 1;
        [SerializeField] private float minTimeInAirForLandingPoints = 0.3f;
        [Tooltip("A nos bonus (measured in percent) to give when the player performs a landing.")]
        [SerializeField, Range(0, 1)] private float landingNosBonus = 0.1f;
        [SerializeField] private float landingPointBonus = 5;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isInAir;
        [SerializeField, ReadOnly] private float timeInAir;
        
        public TextMeshProUGUI LandingLabel => landingLabel;
        
        public override void CheckIfPerformed()
        {
            if (WarehouseManager.Instance.CurrentCar.SpeedKmh < minSpeedKmh)
            {
                OnStopAirTime();
                return;
            }
            
            if (WarehouseManager.Instance.CurrentCar.IsInAir && !isInAir)
            {
                OnStartAirTime();
                OnPerformed();
            } else if (!WarehouseManager.Instance.CurrentCar.IsInAir && isInAir)
            {
                OnStopAirTime();
            } else if (isInAir)
            {
                OnPerformed();
            }
        }
        
        private void OnStartAirTime()
        {
            if (isInAir)
                return; //already in air
            
            isInAir = true;
            timeInAir = 0;
            
            label.gameObject.SetActive(true);
        }
        
        protected override void OnPerformed()
        {
            base.OnPerformed();
            
            timeInAir += Time.deltaTime;
            float pointsGainedSinceStarted = pointBonus * timeInAir;

            label.text = $"Air time +{Mathf.CeilToInt(pointsGainedSinceStarted)}";
        }

        protected override float GetNosToAddWhenPerformed()
        {
            return nosBonus * Time.deltaTime;
        }

        protected override float GetPointsToAddWhenPerformed()
        {
            return pointBonus * Time.deltaTime;
        }

        private void OnStopAirTime()
        {
            if (!isInAir)
                return; //already landed
            
            isInAir = false;
            
            label.gameObject.SetActive(false);

            if (timeInAir < minTimeInAirForLandingPoints)
                return;

            OnPerformLanding();
        }

        private void OnPerformLanding()
        {
            SkillCheckManager.Instance.StartCoroutine(ShowLandingLabelIE());
            
            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(landingNosBonus);

            SkillCheckManager.Instance.AddPoints(landingPointBonus);
            
            onPerformLanding?.Invoke();
        }

        private IEnumerator ShowLandingLabelIE()
        {
            landingLabel.gameObject.SetActive(true);
            landingLabel.text = $"Landing +{landingPointBonus}";
            yield return new WaitForSeconds(landingLabelDuration);
            landingLabel.gameObject.SetActive(false);
        }
        
    }
}
