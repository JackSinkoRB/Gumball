using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class SpeedCameraZone
    {
        
        [Tooltip("The distance (in metres) along the map where the speed camera is positioned.")]
        [SerializeField] private float position;
        [Tooltip("The distance before the position where it will start tracking speed.")]
        [SerializeField] private float zoneDistance = 25;
        [SerializeField] private float speedLimitKmh = 60;

        public float Position => position;
        public float SpeedLimitKmh => speedLimitKmh;
        
        public bool HasRacerPassedZone(AICar racer)
        {
            SplineTravelDistanceCalculator travelCalculator = racer.GetComponent<SplineTravelDistanceCalculator>();
            float zoneEnd = position;

            return travelCalculator.DistanceInMap > zoneEnd;
        }

        public bool IsRacerInZone(AICar racer)
        {
            SplineTravelDistanceCalculator travelCalculator = racer.GetComponent<SplineTravelDistanceCalculator>();
            float zoneStart = position - zoneDistance;
            float zoneEnd = position;

            return travelCalculator.DistanceInMap <= zoneEnd && travelCalculator.DistanceInMap >= zoneStart;
        }
        
    }
}
