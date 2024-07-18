using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public abstract class CarPerformanceSetting
    {
        
        [Header("Category weightings")]
        [SerializeField, Range(0, 1)] protected float maxSpeedWeight;
        [SerializeField, Range(0, 1)] protected float accelerationWeight;
        [SerializeField, Range(0, 1)] protected float handlingWeight;
        [SerializeField, Range(0, 1)] protected float nosWeight;

        public void SetWeights(float maxSpeed, float acceleration, float handling, float nos)
        {
            maxSpeedWeight = maxSpeed;
            accelerationWeight = acceleration;
            handlingWeight = handling;
            nosWeight = nos;
        }
        
        public float GetFinalWeight(CarPerformanceProfile profile)
        {
            //calculate the sum of weights to normalize them
            float totalWeight = maxSpeedWeight + accelerationWeight + handlingWeight + nosWeight;

            if (totalWeight == 0)
                return 0;
            
            //normalize the weights
            float normalizedMaxSpeedWeight = maxSpeedWeight / totalWeight;
            float normalizedAccelerationWeight = accelerationWeight / totalWeight;
            float normalizedHandlingWeight = handlingWeight / totalWeight;
            float normalizedNosWeight = nosWeight / totalWeight;

            //calculate the weighted average of the profile values
            float finalWeight = 
                profile.MaxSpeed * normalizedMaxSpeedWeight +
                profile.Acceleration * normalizedAccelerationWeight +
                profile.Handling * normalizedHandlingWeight +
                profile.Nos * normalizedNosWeight;

            return finalWeight;
        }
        
    }
}
