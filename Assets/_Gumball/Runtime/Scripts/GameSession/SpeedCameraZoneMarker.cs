using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class SpeedCameraZoneMarker : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI distanceLabel;
        [SerializeField] private TextMeshProUGUI speedLabel;
        [SerializeField] private float heightAboveRoad = 3;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private float distanceAlongSpline;
        [SerializeField, ReadOnly] private float speedLimitKmh;

        public float HeightAboveRoad => heightAboveRoad;

        public void Initialise(float distanceAlongSpline, float speedLimitKmh)
        {
            this.distanceAlongSpline = distanceAlongSpline;
            this.speedLimitKmh = speedLimitKmh;
        }

        private void LateUpdate()
        {
            UpdateDistanceLabel();
            UpdateSpeedLabel();
        }

        private void UpdateDistanceLabel()
        {
            SplineTravelDistanceCalculator travelCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (travelCalculator == null)
                return;
            
            float playersDistanceAlongSpline = travelCalculator.DistanceInMap;
            
            float distance = Mathf.Max(0, distanceAlongSpline - playersDistanceAlongSpline);
            float distanceRounded = Mathf.RoundToInt(distance);
            distanceLabel.text = $"{distanceRounded}m";
        }
        
        private void UpdateSpeedLabel()
        {
            float speedLimitLocalised = UnitOfSpeedSetting.UseMiles ? SpeedUtils.FromKphToMph(speedLimitKmh) : speedLimitKmh;
            speedLabel.text = $"{Mathf.RoundToInt(speedLimitLocalised)}";
        }
        
    }
}
