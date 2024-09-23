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
        [SerializeField] private AirTimeSkillCheckUI airTimeUI;
        [SerializeField] private float minSpeedKmh = 50;

        [Header("Landing")]
        [SerializeField] private float minTimeInAirForLandingPoints = 0.3f;
        [Tooltip("A nos bonus (measured in percent) to give when the player performs a landing.")]
        [SerializeField, Range(0, 1)] private float landingNosBonus = 0.1f;
        [SerializeField] private float landingPointBonus = 5;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isInAir;
        [SerializeField, ReadOnly] private float timeInAir;
        
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
            
            airTimeUI.Show(0);
        }
        
        protected override void OnPerformed()
        {
            base.OnPerformed();
            
            timeInAir += Time.deltaTime;
            float pointsGainedSinceStarted = pointBonus * timeInAir;

            airTimeUI.PointBonusLabel.text = $"+{Mathf.CeilToInt(pointsGainedSinceStarted)}";
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
            
            airTimeUI.Hide();

            if (timeInAir >= minTimeInAirForLandingPoints)
                OnPerformLanding();
        }

        private void OnPerformLanding()
        {
            //TODO: show landing UI
            
            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(landingNosBonus);

            SkillCheckManager.Instance.AddPoints(landingPointBonus);
            
            onPerformLanding?.Invoke();
        }

    }
}
