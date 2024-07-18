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

        public CarPerformanceProfile(float maxSpeed, float acceleration, float handling, float nos)
        {
            this.maxSpeed = maxSpeed;
            this.acceleration = acceleration;
            this.handling = handling;
            this.nos = nos;
        }

        /// <summary>
        /// Construct a performance profile from a players car depending on installed parts.
        /// </summary>
        public CarPerformanceProfile(int playerCarIndex)
        {
            //load parts
            CorePart[] allParts = CorePartManager.GetCoreParts(playerCarIndex);

            CarPerformanceProfileModifiers finalModifiers = new CarPerformanceProfileModifiers();
            
            foreach (CorePart corePart in allParts)
            {
                if (corePart == null)
                    continue; //no part applied

                CarPerformanceProfileModifiers totalModifiers = corePart.GetTotalModifiers();
                finalModifiers += totalModifiers;
            }

            maxSpeed = finalModifiers.MaxSpeed;
            acceleration = finalModifiers.Acceleration; 
            handling = finalModifiers.Handling;
            nos = finalModifiers.Nos;
        }

    }
}
