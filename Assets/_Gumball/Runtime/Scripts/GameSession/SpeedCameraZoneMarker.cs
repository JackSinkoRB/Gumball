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
        [SerializeField] private float heightAboveRoad = 3;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private float distanceAlongSpline;
        
        public float HeightAboveRoad => heightAboveRoad;

        public void Initialise(float distanceAlongSpline)
        {
            this.distanceAlongSpline = distanceAlongSpline;
        }

        private void LateUpdate()
        {
            UpdateDistanceLabel();
        }

        private void UpdateDistanceLabel()
        {
            SplineTravelDistanceCalculator travelCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            float playersDistanceAlongSpline = travelCalculator.DistanceInMap;
            
            float distance = Mathf.Max(0, distanceAlongSpline - playersDistanceAlongSpline);
            float distanceRounded = Mathf.RoundToInt(distance);
            distanceLabel.text = $"{distanceRounded}m";
        }
        
    }
}
