using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class SpeedCameraSprintRacerData
    {

        [Tooltip("The min/max distance before a speed camera zone the racer can start braking (chosen randomly).")]
        [SerializeField] private MinMaxFloat brakingDistanceRange = new(0, 25);
        [Tooltip("The min/max distance after a speed camera zone they can start accelerating (chosen randomly).")]
        [SerializeField] private MinMaxFloat accelerationDistanceRange = new(0, 25);

        public MinMaxFloat BrakingDistanceRange => brakingDistanceRange;
        public MinMaxFloat AccelerationDistanceRange => accelerationDistanceRange;

    }
}
