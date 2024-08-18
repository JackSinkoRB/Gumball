using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettings
    {

        [Header("Drivetrain")]
        [Tooltip("The amount to contibute to the acceleration performance points for the gearbox.")]
        [SerializeField, PositiveValueOnly] private int gearboxAccelerationPerformancePoints;
        [Tooltip("The amount to contibute to the max speed performance points for the gearbox.")]
        [SerializeField, PositiveValueOnly] private int gearboxMaxSpeedPerformancePoints;
        
        [Header("Torque")]
        [Tooltip("The engine torque output (y axis - value) (in Newton metres) compared to the engine RPM (x axis - time), between the min and max RPM ranges (where x = 0 is minEngineRpm and x = 1 is maxEngineRpm)")]
        [SerializeField] private AnimationCurve minTorqueCurve;
        [Tooltip("The maximum amount of torque that is added to the peak torque values.")]
        [SerializeField] private CarPerformanceSettingFloatScalar additionalPeakTorque = new(2000);
        [Tooltip("The maximum percentage that the initial torque can be in comparison to the peak torque. A value of 1 means it can get to peak torque instantly. A value of 0 means the initial torque doesn't change.")]
        [SerializeField] private CarPerformanceSettingPercent minTorquePercentToPeak = new(0.8f);

        [Header("RPM range")]
        [SerializeField] private CarPerformanceSettingFloat engineRpmRangeMin = new(600, 1000);
        [SerializeField] private CarPerformanceSettingFloat engineRpmRangeMax = new(6000, 10000);

        [Tooltip("If RPM goes outside this range, it will try upshift/downshift to the desired RPM. For optimal power, the ideal range is where the torque is the highest.")]
        [SerializeField] private CarPerformanceSettingMinMaxFloat idealRPMRangeForGearChanges = new(new MinMaxFloat(3000, 6000), new MinMaxFloat(3000, 8000));

        [Header("Mass")]
        [SerializeField] private CarPerformanceSettingFloat rigidbodyMass = new(1300, 1000);

        [Header("Braking")]
        [SerializeField] private CarPerformanceSettingFloat brakeTorque = new(800, 1200);
        [SerializeField] private CarPerformanceSettingFloat handbrakeEaseOffDuration = new(1, 1);
        [SerializeField] private CarPerformanceSettingFloat handbrakeTorque = new(5000, 5000);
        
        [Header("Steering")]
        [Tooltip("The speed that the wheel collider turns.")]
        [SerializeField] private CarPerformanceSettingFloat steerSpeed = new(2.5f, 2.5f);
        [Tooltip("This allows for a different steer speed when the steering input has been released.")]
        [SerializeField] private CarPerformanceSettingFloat steerReleaseSpeed = new(15, 15);
        [Tooltip("The max angle the steering can turn (y) at the specified speed (x).")]
        [SerializeField] private CarPerformanceSettingAnimationCurve maxSteerAngle;
        
        [Header("Nos")]
        [Tooltip("How long (in seconds) does a full tank of NOS last?")]
        [SerializeField] private CarPerformanceSettingFloat nosDepletionRate = new(6, 3);
        [Tooltip("How long (in seconds) does it take to regenerate a full tank of NOS?")]
        [SerializeField] private CarPerformanceSettingFloat nosFillRate = new(60, 30);
        [Tooltip("How much additional torque added to the car when NOS is activated.")]
        [SerializeField] private CarPerformanceSettingFloat nosTorqueAddition = new(1000f, 2000f);

        public CarPerformanceSettingFloat EngineRpmRangeMin => engineRpmRangeMin;
        public CarPerformanceSettingFloat EngineRpmRangeMax => engineRpmRangeMax;
        public CarPerformanceSettingMinMaxFloat IdealRPMRangeForGearChanges => idealRPMRangeForGearChanges;
        public CarPerformanceSettingFloat RigidbodyMass => rigidbodyMass;
        public CarPerformanceSettingFloat BrakeTorque => brakeTorque;
        public CarPerformanceSettingFloat HandbrakeEaseOffDuration => handbrakeEaseOffDuration;
        public CarPerformanceSettingFloat HandbrakeTorque => handbrakeTorque;
        public CarPerformanceSettingFloat SteerSpeed => steerSpeed;
        public CarPerformanceSettingFloat SteerReleaseSpeed => steerReleaseSpeed;
        public CarPerformanceSettingAnimationCurve MaxSteerAngle => maxSteerAngle;
        public CarPerformanceSettingFloat NosDepletionRate => nosDepletionRate;
        public CarPerformanceSettingFloat NosFillRate => nosFillRate;
        public CarPerformanceSettingFloat NosTorqueAddition => nosTorqueAddition;

        private AnimationCurve torqueCurveCached;

        public float GetPeakTorque(CarPerformanceProfile profile)
        {
            CalculateTorqueCurve(profile);

            float maxValue = 0;
            
            foreach (Keyframe keyframe in torqueCurveCached.keys)
            {
                if (keyframe.value > maxValue)
                    maxValue = keyframe.value;
            }

            return maxValue;
        }
        
        public float GetStartingTorque(CarPerformanceProfile profile)
        {
            CalculateTorqueCurve(profile);

            if (torqueCurveCached.keys.Length == 0)
                return 0;
            
            return torqueCurveCached.keys[0].value;
        }
        
        public AnimationCurve CalculateTorqueCurve(CarPerformanceProfile profile, float additionalTorque = 0)
        {
            if (torqueCurveCached == null || torqueCurveCached.keys.Length != minTorqueCurve.keys.Length)
                torqueCurveCached = new AnimationCurve(minTorqueCurve.keys);
            
            float additionalPeakTorqueValue = additionalPeakTorque.GetValue(profile);
            
            //reset the curve to default
            for (int index = 0; index < torqueCurveCached.keys.Length; index++)
            {
                Keyframe keyframe = torqueCurveCached.keys[index];
                float originalValue = minTorqueCurve.keys[index].value;
                float desiredTorque;
                
                //ensure last key is 0
                bool isFirstKey = index == 0;
                bool isLastKey = index == torqueCurveCached.keys.Length - 1;
                if (isFirstKey)
                {
                    float nextKeyTorqueValue = minTorqueCurve.keys[index + 1].value + additionalPeakTorqueValue;
                    float difference = nextKeyTorqueValue - originalValue;
                    float maxAddition = difference * minTorquePercentToPeak.GetValue(profile);
                    desiredTorque = originalValue + (maxAddition * minTorquePercentToPeak.GetFinalWeight(profile)) + additionalTorque;
                }
                else if (isLastKey)
                {
                    desiredTorque = 0;
                }
                else
                {
                    //is peak key
                    desiredTorque = originalValue + additionalPeakTorqueValue + additionalTorque;
                }
                
                //move the key
                Keyframe newKeyframe = new Keyframe(keyframe.time, desiredTorque, keyframe.inTangent, keyframe.outTangent, keyframe.inWeight, keyframe.outWeight);
                torqueCurveCached.MoveKey(index, newKeyframe);
            }

            return torqueCurveCached;
        }

    }
}
