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
        
        [Header("Torque")]
        [SerializeField] private CarPerformanceSettingTorqueCurve torqueCurve;
        
        [Header("RPM range")]
        [SerializeField] private CarPerformanceSettingMinMaxFloat engineRpmRange = new(new MinMaxFloat(1000, 7000), new MinMaxFloat(1000, 9000));
        [Tooltip("If RPM goes outside this range, it will try upshift/downshift to the desired RPM. For optimal power, the ideal range is where the torque is the highest.")]
        [SerializeField] private CarPerformanceSettingMinMaxFloat idealRPMRangeForGearChanges = new(new MinMaxFloat(3000, 6000), new MinMaxFloat(3000, 8000));

        [Header("Gear ratios")]
        [SerializeField] private CarPerformanceSettingFloatArray gearRatios = new(new[] { -1.5f, 2.66f, 1.78f, 1.3f, 1, 0.7f, 0.5f }, new[] { -1.5f, 2.66f, 1.78f, 1.3f, 1, 0.7f, 0.5f });
        [SerializeField] private CarPerformanceSettingFloat finalGearRatio = new(3.42f, 3.42f);

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
        
        [Header("Nos")]
        [Tooltip("How long (in seconds) does a full tank of NOS last?")]
        [SerializeField] private CarPerformanceSettingFloat nosDepletionRate = new(6, 3);
        [Tooltip("How long (in seconds) does it take to regenerate a full tank of NOS?")]
        [SerializeField] private CarPerformanceSettingFloat nosFillRate = new(60, 30);
        [Tooltip("How much additional torque added to the car when NOS is activated.")]
        [SerializeField] private CarPerformanceSettingFloat nosTorqueAddition = new(1000f, 2000f);

        public CarPerformanceSettingTorqueCurve TorqueCurve => torqueCurve;
        public CarPerformanceSettingMinMaxFloat EngineRpmRange => engineRpmRange;
        public CarPerformanceSettingMinMaxFloat IdealRPMRangeForGearChanges => idealRPMRangeForGearChanges;
        public CarPerformanceSettingFloatArray GearRatios => gearRatios;
        public CarPerformanceSettingFloat FinalGearRatio => finalGearRatio;
        public CarPerformanceSettingFloat RigidbodyMass => rigidbodyMass;
        public CarPerformanceSettingFloat BrakeTorque => brakeTorque;
        public CarPerformanceSettingFloat HandbrakeEaseOffDuration => handbrakeEaseOffDuration;
        public CarPerformanceSettingFloat HandbrakeTorque => handbrakeTorque;
        public CarPerformanceSettingFloat SteerSpeed => steerSpeed;
        public CarPerformanceSettingFloat SteerReleaseSpeed => steerReleaseSpeed;
        public CarPerformanceSettingFloat NosDepletionRate => nosDepletionRate;
        public CarPerformanceSettingFloat NosFillRate => nosFillRate;
        public CarPerformanceSettingFloat NosTorqueAddition => nosTorqueAddition;
        
    }
}
