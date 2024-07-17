using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct CarPerformanceProfile
    {
        
        [SerializeField, Range(0, 1)] private float maxSpeed;
        [SerializeField, Range(0, 1)] private float acceleration;
        [SerializeField, Range(0, 1)] private float handling;
        [SerializeField, Range(0, 1)] private float nos;

        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;
        public float Handling => handling;
        public float Nos => nos;

        /// <summary>
        /// Construct a performance profile from a players car depending on installed parts.
        /// </summary>
        public CarPerformanceProfile(int playerCarIndex)
        {
            //load parts
            CorePart[] allParts = CorePartManager.GetCoreParts(playerCarIndex);

            maxSpeed = 0;
            acceleration = 0;
            handling = 0;
            nos = 0;

            
            //TODO: subpart modifiers

            
            foreach (CorePart corePart in allParts)
            {
                maxSpeed += corePart.PerformanceModifiers.MaxSpeed;
                acceleration += corePart.PerformanceModifiers.Acceleration;
                handling += corePart.PerformanceModifiers.Handling;
                nos += corePart.PerformanceModifiers.Nos;
            }
        }

    }
}
