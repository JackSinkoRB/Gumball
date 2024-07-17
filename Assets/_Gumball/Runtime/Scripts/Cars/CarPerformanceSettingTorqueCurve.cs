using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettingTorqueCurve : CarPerformanceSetting
    {
        
        [Space]
        [Tooltip("The engine torque output (y) (in Newton metres) compared to the engine RPM (x), between the min and max RPM ranges (where x = 0 is minEngineRpm)")]
        [SerializeField] private AnimationCurve minTorqueCurve;
        [Tooltip("The maximum amount of torque that is added to the peak torque values.")]
        [SerializeField] private float maxPeakTorqueToAdd = 1000;
        [Tooltip("The maximum percentage that the initial torque can be in comparison to the peak torque. A value of 1 means it can get to peak torque instantly. A value of 0 means the initial torque doesn't change.")]
        [SerializeField, Range(0, 1)] private float maxInitialTorquePercentComparedToPeak = 0.8f;

        private AnimationCurve resultCached;
        private float lastKnownWeight = -1;
        private float lastKnownTemporaryAdditionalTorque;

        private float temporaryAdditionalTorque;
        
        public void SetTemporaryAdditionalTorque(float torque)
        {
            temporaryAdditionalTorque = torque;
        }
        
        public AnimationCurve GetValue(CarPerformanceProfile profile)
        {
            float finalWeight = GetFinalWeight(profile);

            if (resultCached != null && lastKnownWeight.Approximately(finalWeight) && lastKnownTemporaryAdditionalTorque.Approximately(temporaryAdditionalTorque))
                return resultCached; //nothing has changed, use the cache

            lastKnownWeight = finalWeight;
            lastKnownTemporaryAdditionalTorque = temporaryAdditionalTorque;
            
            resultCached = new AnimationCurve(minTorqueCurve.keys);
            
            AddAdditionalPeakTorque(finalWeight);
            SetInitialTorqueValue(finalWeight);

            return resultCached;
        }

        private void AddAdditionalPeakTorque(float finalWeight)
        {
            float additionalPeakTorque = Mathf.Lerp(0, maxPeakTorqueToAdd, finalWeight);
            
            //loop over the middle keys in the resultCached curve and add the additional peak torque
            for (int peakTorqueKeyIndex = 1; peakTorqueKeyIndex < resultCached.keys.Length - 1; peakTorqueKeyIndex++) //just the middle keys
            {
                Keyframe currentKeyframe = resultCached.keys[peakTorqueKeyIndex];
                
                float newTorque = currentKeyframe.value + additionalPeakTorque + temporaryAdditionalTorque;
                Keyframe newKeyframe = new Keyframe(currentKeyframe.time, newTorque, currentKeyframe.inTangent, currentKeyframe.outTangent, currentKeyframe.inWeight, currentKeyframe.outWeight);
                
                resultCached.MoveKey(peakTorqueKeyIndex, newKeyframe);
            }
        }

        private void SetInitialTorqueValue(float finalWeight)
        {
            const int initialKeyIndex = 0;
            
            float minValue = minTorqueCurve.keys[initialKeyIndex].value;
            float difference = GetLowestPeakTorqueValue() - minValue;
            float maxAddition = difference * maxInitialTorquePercentComparedToPeak;
            float desiredValue = minValue + (maxAddition * finalWeight) + temporaryAdditionalTorque;
            
            Keyframe currentKeyframe = resultCached.keys[initialKeyIndex];
            Keyframe newKeyframe = new Keyframe(currentKeyframe.time, desiredValue, currentKeyframe.inTangent, currentKeyframe.outTangent, currentKeyframe.inWeight, currentKeyframe.outWeight);
            
            resultCached.MoveKey(initialKeyIndex, newKeyframe);
        }

        private float GetLowestPeakTorqueValue()
        {
            //loop over the middle keys in the resultCached curve (already had additional peak torque added) and get the lowest key value
            float lowestValue = Mathf.Infinity;
            for (int peakTorqueKeyIndex = 1; peakTorqueKeyIndex < resultCached.keys.Length - 1; peakTorqueKeyIndex++) //just the middle keys
            {
                float torqueValue = resultCached.keys[peakTorqueKeyIndex].value;
                if (torqueValue < lowestValue)
                    lowestValue = torqueValue;
            }

            return lowestValue;
        }
        
    }
}
