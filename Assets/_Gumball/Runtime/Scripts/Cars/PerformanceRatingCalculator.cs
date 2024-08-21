using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct PerformanceRatingCalculator
    {

        public enum Component
        {
            MAX_SPEED,
            ACCELERATION,
            HANDLING,
            NOS
        }
        
        public static PerformanceRatingCalculator GetCalculator(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            PerformanceRatingCalculator calculator = new PerformanceRatingCalculator();
            calculator.Calculate(settings, profile);
            return calculator;
        }

        [SerializeField, ReadOnly] private int totalRating;
        [SerializeField, ReadOnly] private int maxSpeedRating;
        [SerializeField, ReadOnly] private int accelerationSpeedRating;
        [SerializeField, ReadOnly] private int handlingRating;
        [SerializeField, ReadOnly] private int nosRating;

        public int TotalRating => totalRating;
        public int MaxSpeedRating => maxSpeedRating;
        public int AccelerationSpeedRating => accelerationSpeedRating;
        public int HandlingRating => handlingRating;
        public int NosRating => nosRating;
        
        public void Calculate(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            maxSpeedRating = GetMaxSpeedRating(settings, profile);
            accelerationSpeedRating = GetAccelerationRating(settings, profile);
            handlingRating = GetHandlingRating(settings, profile);
            nosRating = GetNosRating(settings, profile);

            totalRating = maxSpeedRating + accelerationSpeedRating + handlingRating + nosRating;
        }
        
        public int GetRating(Component component)
        {
            return component switch
            {
                Component.MAX_SPEED => maxSpeedRating,
                Component.ACCELERATION => accelerationSpeedRating,
                Component.HANDLING => handlingRating,
                Component.NOS => nosRating,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private int GetMaxSpeedRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float maxRpmValue = settings.EngineRpmRangeMax.GetValue(profile) * 0.005f;
            float idealRpmRangeMin = settings.IdealRPMRangeForGearChanges.GetValue(profile).Min * 0.005f;
            float idealRpmRangeMax = settings.IdealRPMRangeForGearChanges.GetValue(profile).Max * 0.005f;
            float mass = (5000 - settings.RigidbodyMass.GetValue(profile)) * 0.005f;
            
            return Mathf.CeilToInt(maxRpmValue + idealRpmRangeMin + idealRpmRangeMax + mass);
        }
        
        private int GetAccelerationRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float torque = settings.GetPeakTorque(profile) * 0.1f;
            float minRpmValue = settings.EngineRpmRangeMin.GetValue(profile) * 0.005f;
            float startingTorque = settings.GetStartingTorque(profile) * 0.005f;
            float mass = (5000 - settings.RigidbodyMass.GetValue(profile)) * 0.005f;
            
            return Mathf.CeilToInt(torque + minRpmValue + startingTorque + mass);
        }
        
        private int GetHandlingRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float brake = settings.BrakeTorque.GetValue(profile) * 0.01f;
            float handbrakeEaseOffDuration = settings.HandbrakeEaseOffDuration.GetValue(profile) * 7.5f;
            float handbrake = settings.HandbrakeTorque.GetValue(profile) * 0.001f;
            float steerSpeed = settings.SteerSpeed.GetValue(profile) * 5f;
            float steerReleaseSpeed = settings.SteerReleaseSpeed.GetValue(profile) * 0.25f;
            float maxSteerAngle = settings.MaxSteerAngle.GetValue(profile).keys.Length < 1 ? 0
                : settings.MaxSteerAngle.GetValue(profile).keys[1].value * 0.5f; //just use the max speed value

            return Mathf.CeilToInt(brake + handbrakeEaseOffDuration + handbrake + steerSpeed + steerReleaseSpeed + maxSteerAngle);
        }
        
        private int GetNosRating(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            float depletionRate = settings.NosDepletionRate.GetValue(profile) * 3;
            float fillRate = settings.NosFillRate.GetValue(profile) * 0.2f;
            float torqueAddition = settings.NosTorqueAddition.GetValue(profile) * 0.02f;

            return Mathf.CeilToInt(depletionRate + fillRate + torqueAddition);
        }
        
    }
}
