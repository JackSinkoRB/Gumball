using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Drivetrain))]
    public class CarManager : MonoBehaviour
    {
        public enum TyreCompound
        {
            Economy, //Low Forward & Sideways
            Street, //Low Forward, OK Sideways
            Sport, //OK Forward, OK Sideways
            Drag //OK Forward, Low Sideways
        }
        
        [Tooltip("Add all wheels of the car here, so brake and steering forces can be applied to them.")]
        [SerializeField] private Wheel[] wheels;
        [SerializeField] private CarCustomisation customisation;

        [Tooltip("A transform object which marks the car's center of gravity." +
                 "\nCars with a higher CoG tend to tilt more in corners." +
                 "\nThe further the CoG is towards the rear of the car, the more the car tends to oversteer." +
                 "\nIf this is not set, the center of mass is calculated from the colliders.")]
        [SerializeField] private Transform centerOfMass;
        
        [SerializeField] private TyreCompound tyreCompound = TyreCompound.Street;

        [Tooltip("A factor applied to the car's inertia tensor." +
                 "\nUnity calculates the inertia tensor based on the car's collider shape." +
                 "\nThis factor lets you scale the tensor, in order to make the car more or less dynamic." +
                 "\nA higher inertia makes the car change direction slower, which can make it easier to respond to.")]
        [SerializeField] private float inertiaFactor = 1.5f;

        [ReadOnly] public float brake;
        [HideInInspector] private float throttle;
        private float throttleInput;

        public float CurrentSteering { get; private set; }
        private float lastShiftTime = -1;
        
        // cached Drivetrain reference
        public Drivetrain drivetrain;

        // How long the car takes to shift gears
        public float shiftSpeed = 0.8f;
        
        [Header("Throttle")]
        [Tooltip("How long it takes to fully engage the throttle")]
        [SerializeField] private float throttleTime = 0.5f;
        
        [Tooltip("How long it takes to fully engage the throttle when the wheels are spinning (and traction control is disabled) (should be quicker than normal).")]
        [ConditionalField(nameof(hasTractionControl))]
        [SerializeField] private float throttleTimeNoTraction = 0.1f;
        
        [Tooltip("How long it takes to fully engage the throttle")]
        [SerializeField] private float throttleReleaseTime = 0.5f;

        [Tooltip("How long it takes to fully release the throttle when the wheels are spinning.")]
        [SerializeField] private float throttleReleaseTimeNoTraction = 0.1f;

        [Tooltip("Prevents the powered wheels from applying more torque if they are slipping.")]
        [SerializeField] private bool hasTractionControl = true;

        [Tooltip("Prevents the wheels from locking up when braking which can prevent turning while braking.")]
        [SerializeField] private bool hasAbsControl = true;
        
        [Tooltip("Detects oversteer and understeer, and applies the brakes to the opposite corner wheel to correct it.")]
        [SerializeField] private bool hasStabilityControl = true;
        
        /// <summary>
        /// The amount of slip required to trigger traction control.
        /// </summary>
        private const float tractionControlSlipTrigger = 0.2f;

        // These values determine how fast steering value is changed when the steering keys are pressed or released.
        // Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.

        [Header("Steering")]
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float maxSteerSpeed = 2.5f;
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float minSteerSpeed = 0.5f;
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float maxSteerReleaseSpeed = 10f;
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float minSteerReleaseSpeed = 3f;
        [Tooltip("The speed (in km/h) that the car must be going to be at minSteerSpeed.")]
        [SerializeField] private float speedForMinSteerSpeed = 200;

        [ReadOnly, SerializeField] private Rigidbody rigidBody;
        private bool isReversing;
        private bool clutchIn;
        /// <summary>
        /// The speed that the rigidbody is moving (in m/s).
        /// </summary>
        public float Speed { get; private set; }

        public bool HasTractionControl => hasTractionControl;
        public bool HasStabilityControl => hasStabilityControl;
        public bool StabilityControlOn { get; private set; }
        public float TractionControlSlipTrigger => tractionControlSlipTrigger;
        public bool IsBraking => brake > 0 && !isReversing; 
        public CarCustomisation Customisation => customisation;
        
        public Wheel[] Wheels => wheels;
        public Wheel FrontLeftWheel => wheels.First(w => !w.isRear && w.isLeft);
        public Wheel FrontRightWheel => wheels.First(w => !w.isRear && !w.isLeft);
        public Wheel RearLeftWheel => wheels.First(w => w.isRear && w.isLeft);
        public Wheel RearRightWheel => wheels.First(w => w.isRear && !w.isLeft);
        
        // Used by SoundController to get average slip velo of all wheels for skid sounds.
        public float slipVelo
        {
            get
            {
                float val = 0.0f;
                foreach (Wheel w in wheels)
                    val += w.slipVelo / wheels.Length;
                return val;
            }
        }

        private void OnEnable()
        {
            SettingsManager.Instance.onGearboxSettingChanged += OnGearboxSettingChanged;
        }
        
        private void OnDisable()
        {
            SettingsManager.Instance.onGearboxSettingChanged -= OnGearboxSettingChanged;
        }

        private void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            if (centerOfMass != null)
                rigidBody.centerOfMass = centerOfMass.localPosition;

            rigidBody.inertiaTensor *= inertiaFactor;
            foreach (Wheel wheel in wheels)
            {
                wheel.manager = this;
            }

            SetGearboxFromSettings();
        }

        public void StabilityControlCheck()
        {
            //reset values:
            StabilityControlOn = false;
            FrontLeftWheel.stabilityControlBraking = 0;
            FrontRightWheel.stabilityControlBraking = 0;
            RearLeftWheel.stabilityControlBraking = 0;
            RearRightWheel.stabilityControlBraking = 0;
            
            if (!PlayerCarManager.Instance.CurrentCar.HasStabilityControl)
                return;
            
            //get the averages:
            float frontSlipAngleAverage = 0;
            float rearSlipAngleAverage = 0;
            foreach (Wheel wheel in wheels)
            {
                if (wheel.isRear)
                {
                    rearSlipAngleAverage += wheel.SlipAngle;
                }
                else
                {
                    frontSlipAngleAverage += wheel.SlipAngle;
                }
            }
            frontSlipAngleAverage /= 2;
            rearSlipAngleAverage /= 2;
            
            bool hasUndersteer = Mathf.Abs(frontSlipAngleAverage) > 0.1f && Mathf.Abs(frontSlipAngleAverage) > Mathf.Abs(rearSlipAngleAverage);
            bool hasOversteer = Mathf.Abs(rearSlipAngleAverage) > 0.1f && Mathf.Abs(rearSlipAngleAverage) > Mathf.Abs(frontSlipAngleAverage);

            if (hasUndersteer)
            {
                StabilityControlOn = true;
                
                //get the slip ratios of the front 2 wheels
                //apply brakes to rear wheels depending on the percentage
                float leftSlipRatio = Mathf.Abs(FrontLeftWheel.slipRatio);
                float rightSlipRatio = Mathf.Abs(FrontRightWheel.slipRatio);
                float combined = leftSlipRatio + rightSlipRatio;
                
                float leftPercent = leftSlipRatio / combined;
                float rightPercent = rightSlipRatio / combined;

                RearLeftWheel.stabilityControlBraking = leftPercent;
                RearRightWheel.stabilityControlBraking = rightPercent;
            }
            
            if (hasOversteer)
            {
                StabilityControlOn = true;
                
                //get the slip ratios of the rear 2 wheels
                //apply brakes to front wheels depending on the percentage
                float leftSlipRatio = Mathf.Abs(RearLeftWheel.slipRatio);
                float rightSlipRatio = Mathf.Abs(RearRightWheel.slipRatio);
                float combined = leftSlipRatio + rightSlipRatio;
                
                float leftPercent = leftSlipRatio / combined;
                float rightPercent = rightSlipRatio / combined;

                FrontLeftWheel.stabilityControlBraking = rightPercent;
                FrontRightWheel.stabilityControlBraking = leftPercent;
            }
        }

        private void CalculateSpeed()
        {
            Speed = transform.InverseTransformDirection(rigidBody.velocity).z;
        }

        private void CalculateThrottle()
        {
            
        }

        private void CalculateHandbrake()
        {
            
        }
        
        private void Update()
        {
            CalculateSpeed();
            CalculateSteering();
            CalculateThrottle();
            CalculateHandbrake();

            if (InputManager.Accelerate.IsPressed)
            {
                //if car is slipping, and there's no traction control, increase the throttle response time
                if (drivetrain.slipRatio < tractionControlSlipTrigger)
                    throttle += Time.deltaTime / throttleTime;
                else if (hasTractionControl)
                    throttle -= Time.deltaTime / throttleReleaseTime;
                else
                    throttle += Time.deltaTime / throttleTimeNoTraction;

                if (throttleInput < 0)
                    throttleInput = 0;
                throttleInput += Time.deltaTime / throttleTime;
            }
            else
            {
                if (drivetrain.slipRatio < tractionControlSlipTrigger)
                    throttle -= Time.deltaTime / throttleReleaseTime;
                else
                    throttle -= Time.deltaTime / throttleReleaseTimeNoTraction;
            }

            throttle = Mathf.Clamp01(throttle);

            if (InputManager.Decelerate.IsPressed)
            {
                if (drivetrain.slipRatio < tractionControlSlipTrigger)
                    brake += Time.deltaTime / throttleTime;
                else
                    brake += Time.deltaTime / throttleTimeNoTraction;
                throttle = 0;
                throttleInput -= Time.deltaTime / throttleTime;
            }
            else
            {
                if (drivetrain.slipRatio < tractionControlSlipTrigger)
                    brake -= Time.deltaTime / throttleReleaseTime;
                else
                    brake -= Time.deltaTime / throttleReleaseTimeNoTraction;
            }

            if (!InputManager.Decelerate.IsPressed && !InputManager.Accelerate.IsPressed)
            {
                throttleInput = 0;
            }

            brake = Mathf.Clamp01(brake);
            throttleInput = Mathf.Clamp(throttleInput, -1, 1);

            customisation.SetBrakeLights(brake > 0);

            if (InputManager.Handbrake.IsPressed || clutchIn)
            {
                drivetrain.clutch = 1;
            }
            else
            {
                drivetrain.clutch = 0;
            }

            // Gear shifting
            float shiftThrottleFactor = Mathf.Clamp01((Time.time - lastShiftTime) / shiftSpeed);


            //auto reverse
            //rbSpeed

            if (drivetrain.Gear <= 2 && Speed <= 0)
            {
                if (throttleInput < -0.1f)
                {
                    if (!isReversing)
                    {
                        isReversing = true;
                        drivetrain.SetGear(0);
                    }

                    if (Speed > 0)
                    {
                        brake = 1;
                        throttle = 0;
                    }
                    else
                    {
                        throttle = 1f;
                        brake = 0;
                    }
                }
                else if (throttleInput == 0)
                {
                    throttle = 0f;
                    brake = 0;
                }
                else
                {
                    isReversing = false;
                    drivetrain.SetGear(2);
                    throttle = throttleInput;
                    brake = 0;
                }
            }

            if (drivetrain.Gear == 0 && throttleInput >= 0.1f)
            {
                if (isReversing)
                {
                    if (Speed < -0.5f)
                    {
                        brake = 1;
                        throttle = 0;
                    }
                    else
                    {
                        isReversing = false;
                        drivetrain.SetGear(2);
                        throttle = throttleInput;
                        brake = 0;
                    }
                }
            }
            
            if (drivetrain.Gear == 0)
                drivetrain.throttle = throttle;
            else
                drivetrain.throttle = throttle * shiftThrottleFactor;

            drivetrain.throttleInput = throttleInput;

            if (InputManager.ShiftUp.WasPressedThisFrame)
            {
                ShiftUp();
            }

            if (InputManager.ShiftDown.WasPressedThisFrame)
            {
                ShiftDown();
            }

            ApplyToWheels();
        }

        /// <summary>
        /// Get the slip ratio of the wheel with the highest slip ratio.
        /// </summary>
        private float GetHighestSlipRatio()
        {
            float highest = 0;
            foreach (Wheel wheel in wheels)
            {
                float slipRatio = Mathf.Abs(wheel.slipRatio);
                if (slipRatio > highest)
                    highest = slipRatio;
            }
            return highest;
        }
        
        /// <summary>
        /// Applies the data to the wheels, such as braking and steering.
        /// </summary>
        private void ApplyToWheels()
        {
            float highestSlipRatio = GetHighestSlipRatio();
            
            foreach (Wheel wheel in wheels)
            {
                float brakeToApply = brake;
                
                float slipRatio = Mathf.Abs(wheel.slipRatio);
                if (hasAbsControl)
                {
                    brakeToApply *= 1 - (slipRatio / highestSlipRatio);
                }
                
                wheel.brakePedal = brakeToApply;
                wheel.handbrake = InputManager.Handbrake.IsPressed ? 1 : 0;
                wheel.steering = CurrentSteering;
            }
        }
        
        public void SetClutchIn(bool clutchEngaged = true)
        {
            clutchIn = clutchEngaged;
        }
        
        public void ShiftUp()
        {
            lastShiftTime = Time.time;
            drivetrain.ShiftUp();
        }

        public void ShiftDown()
        {
            lastShiftTime = Time.time;
            drivetrain.ShiftDown();
        }

        public float CurrentSteerSpeed { get; private set; }

        private void CalculateSteering()
        {
            float speedPercent = Mathf.Clamp01(rigidBody.velocity.magnitude / SpeedUtils.FromKmh(speedForMinSteerSpeed));
            float desiredSteering = InputManager.SteeringInput;
            bool isCorrecting = false;
            
            if (InputManager.SteeringInput == 0)
            {
                float difference = maxSteerReleaseSpeed - minSteerReleaseSpeed;
                CurrentSteerSpeed = minSteerReleaseSpeed + ((1-speedPercent) * difference);;
            } else if (InputManager.SteeringInput > 0.01f && CurrentSteering < -0.01f
                       || InputManager.SteeringInput < -0.01f && CurrentSteering > 0.01f)
            {
                float maxCorrection = (maxSteerSpeed + maxSteerReleaseSpeed) / 2;
                float minCorrection = (minSteerSpeed + minSteerReleaseSpeed) / 2;

                float difference = maxCorrection - minCorrection;
                CurrentSteerSpeed = minCorrection + ((1-speedPercent) * difference);

                isCorrecting = true;
            }
            else
            {
                float difference = maxSteerSpeed - minSteerSpeed;
                CurrentSteerSpeed = minSteerSpeed + ((1-speedPercent) * difference);
            }
            
            
            CurrentSteering = Mathf.Lerp(CurrentSteering, desiredSteering, Time.deltaTime * CurrentSteerSpeed);
            if (isCorrecting)
                CurrentSteering = Mathf.Clamp(CurrentSteering, 0, -Mathf.Sign(InputManager.SteeringInput) * 1);
        }
        
        private void SetGearboxFromSettings()
        {
            drivetrain.automatic = SettingsManager.GearboxSetting == 0;
        }
        
        private void OnGearboxSettingChanged(int newValue)
        {
            drivetrain.automatic = newValue == 0;
        }
    }
}