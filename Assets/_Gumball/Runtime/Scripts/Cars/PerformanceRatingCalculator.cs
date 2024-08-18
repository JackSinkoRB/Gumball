using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class PerformanceRatingCalculator
    {

        public static int Calculate(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            int maxSpeedRating = CalculateMaxSpeedRating(settings, profile);
            int accelerationSpeedRating = CalculateAccelerationRating(settings, profile);
            int handlingRating = CalculateHandlingRating(settings, profile);
            int nosRating = CalculateNosRating(settings, profile);
            
            return maxSpeedRating + accelerationSpeedRating + handlingRating + nosRating;
        }

        public static int CalculateMaxSpeedRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float maxRpmValue = settings.EngineRpmRangeMax.GetValue(profile) * 0.005f;
            float idealRpmRangeMin = settings.IdealRPMRangeForGearChanges.GetValue(profile).Min * 0.005f;
            float idealRpmRangeMax = settings.IdealRPMRangeForGearChanges.GetValue(profile).Max * 0.005f;
            float mass = (5000 - settings.RigidbodyMass.GetValue(profile)) * 0.005f;
            
            return Mathf.CeilToInt(maxRpmValue + idealRpmRangeMin + idealRpmRangeMax + mass);
        }
        
        public static int CalculateAccelerationRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float torque = settings.GetPeakTorque(profile) * 0.1f;
            float minRpmValue = settings.EngineRpmRangeMin.GetValue(profile) * 0.005f;
            float startingTorque = settings.GetStartingTorque(profile) * 0.005f;
            float mass = (5000 - settings.RigidbodyMass.GetValue(profile)) * 0.005f;
            
            return Mathf.CeilToInt(torque + minRpmValue + startingTorque + mass);
        }
        
        public static int CalculateHandlingRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float brake = settings.BrakeTorque.GetValue(profile) * 0.01f;
            float handbrakeEaseOffDuration = settings.HandbrakeEaseOffDuration.GetValue(profile) * 7.5f;
            float handbrake = settings.HandbrakeTorque.GetValue(profile) * 0.001f;
            float steerSpeed = settings.SteerSpeed.GetValue(profile) * 5f;
            float steerReleaseSpeed = settings.SteerReleaseSpeed.GetValue(profile) * 0.25f;
            float maxSteerAngle = settings.MaxSteerAngle.GetValue(profile).keys[1].value * 0.5f; //just use the max speed value

            return Mathf.CeilToInt(brake + handbrakeEaseOffDuration + handbrake + steerSpeed + steerReleaseSpeed + maxSteerAngle);
        }
                
        public static int CalculateNosRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float depletionRate = settings.NosDepletionRate.GetValue(profile) * 3;
            float fillRate = settings.NosFillRate.GetValue(profile) * 0.2f;
            float torqueAddition = settings.NosTorqueAddition.GetValue(profile) * 0.02f;

            return Mathf.CeilToInt(depletionRate + fillRate + torqueAddition);
        }
        
    }
}
