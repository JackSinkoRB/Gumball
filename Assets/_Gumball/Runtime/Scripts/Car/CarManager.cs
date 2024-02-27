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

        private static readonly int BrakeShaderID = Shader.PropertyToID("_Brake");

        [Tooltip("A transform object which marks the car's center of gravity." +
                 "\nCars with a higher CoG tend to tilt more in corners." +
                 "\nThe further the CoG is towards the rear of the car, the more the car tends to oversteer." +
                 "\nIf this is not set, the center of mass is calculated from the colliders.")]
        [SerializeField] private Transform centerOfMass;

        [SerializeField] private GameObject colliders;
        
        [Tooltip("A factor applied to the car's inertia tensor." +
                 "\nUnity calculates the inertia tensor based on the car's collider shape." +
                 "\nThis factor lets you scale the tensor, in order to make the car more or less dynamic." +
                 "\nA higher inertia makes the car change direction slower, which can make it easier to respond to.")]
        [SerializeField] private float inertiaFactor = 1.5f;

        [Header("Braking")]
        [SerializeField] private Material brakeLightsMaterial;
        [ReadOnly] public float brake;
        
        private float throttle;
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

        [Header("Driving assists")]
        [Tooltip("Prevents the powered wheels from applying more torque if they are slipping.")]
        [SerializeField] private bool hasTractionControl = true;

        [Tooltip("Prevents the wheels from locking up when braking which can prevent turning while braking.")]
        [SerializeField] private bool hasAbsControl = true;
        
        [Tooltip("Detects oversteer and understeer, and applies the brakes to the opposite corner wheel to correct it.")]
        [SerializeField] private bool hasStabilityControl = true;
        [Tooltip("The max speed for the stabilityControlBrakingModifier check.")]
        [ConditionalField(nameof(hasStabilityControl)), SerializeField]
        private float stabilityControlBrakingModifierMax = 250;

        [Tooltip("The amount of braking to apply for stability control, depending on the current speed of the car.")]
        [ConditionalField(nameof(hasStabilityControl)), SerializeField]
        private AnimationCurve stabilityControlBrakingModifier;
        [Tooltip("When the car is slipping, how much of the torque should be balanced between the slipping wheels?")]
        [SerializeField, Range(0, 1)] private float stabilityControlTorqueModifier = 0.5f;
        
        /// <summary>
        /// The amount of slip required to trigger traction control.
        /// </summary>
        private const float tractionControlSlipTrigger = 0.2f;
        
        [Header("Steering")]
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float maxSteerSpeed = 2;
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float minSteerSpeed = 0.35f;
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float maxSteerReleaseSpeed = 10;
        [Tooltip("Higher value means it's faster to turn the steering wheel.")]
        [SerializeField] private float minSteerReleaseSpeed = 35;
        [Tooltip("The speed (in km/h) that the car must be going to be at minSteerSpeed.")]
        [SerializeField] private float speedForMinSteerSpeed = 250;

        [ReadOnly, SerializeField] private Rigidbody rigidBody;
        public Rigidbody Rigidbody => rigidBody;
        
        private bool isReversing;
        private bool clutchIn;

        public GameObject Colliders => colliders;
        
        /// <summary>
        /// The speed that the rigidbody is moving (in m/s).
        /// </summary>
        public float Speed { get; private set; }

        public bool HasTractionControl => hasTractionControl;
        public bool HasStabilityControl => hasStabilityControl;
        public bool StabilityControlOn { get; private set; }
        public float TractionControlSlipTrigger => tractionControlSlipTrigger;
        public bool IsBraking => brake > 0 && !isReversing;

        // Used by SoundController to get average slip velo of all wheels for skid sounds.
        public float slipVelo
        {
            get
            {
                float val = 0.0f;
                foreach (Wheel w in wheelManager.Wheels)
                    val += w.slipVelo / wheelManager.Wheels.Length;
                return val;
            }
        }

        [Header("New")]
        [SerializeField] private CarWheelsManager wheelManager;
        [SerializeField] private CarIKManager avatarIKManager;

        [SerializeField, ReadOnly] private int carIndex;
        [SerializeField, ReadOnly] private int id;

        public CarWheelsManager WheelManager => wheelManager;
        public CarIKManager AvatarIKManager => avatarIKManager;
        public int CarIndex => carIndex;
        public int ID => id;
        public string SaveKey => $"CarData.{carIndex}.{id}";
        
        public IEnumerator Initialise(int carIndex, int id)
        {
            this.carIndex = carIndex;
            this.id = id;
            
            yield return wheelManager.Initialise();
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
            foreach (Wheel wheel in wheelManager.Wheels)
            {
                wheel.manager = this;
            }

            SetGearboxFromSettings();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!Rigidbody.isKinematic)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }

            transform.position = position;
            transform.rotation = rotation;
        }
        
        /// <summary>
        /// Gets the wheel's slip percentage, in comparison to another wheel.
        /// </summary>
        /// <returns>2 float values that add up to 1.</returns>
        public (float, float) GetSlipPercentage(bool isRear)
        {
            Wheel leftWheel = isRear ? wheelManager.RearLeftWheel : wheelManager.FrontLeftWheel;
            Wheel rightWheel = isRear ? wheelManager.RearRightWheel : wheelManager.FrontRightWheel;
            
            float leftSlipRatio = Mathf.Abs(leftWheel.slipRatio);
            float rightSlipRatio = Mathf.Abs(rightWheel.slipRatio);
            float combined = leftSlipRatio + rightSlipRatio;
                
            float leftPercent = leftSlipRatio / combined;
            float rightPercent = rightSlipRatio / combined;

            return (leftPercent, rightPercent);
        }

        public void StabilityControlCheck()
        {
            //reset values:
            StabilityControlOn = false;
            wheelManager.FrontLeftWheel.stabilityControlBraking = 0;
            wheelManager.FrontRightWheel.stabilityControlBraking = 0;
            wheelManager.RearLeftWheel.stabilityControlBraking = 0;
            wheelManager.RearRightWheel.stabilityControlBraking = 0;
            
            if (!HasStabilityControl)
                return;
            
            //get the averages:
            float frontSlipAngleAverage = 0;
            float rearSlipAngleAverage = 0;
            foreach (Wheel wheel in wheelManager.Wheels)
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

            //reset in case there's no under or oversteer
            wheelManager.RearLeftWheel.SetDriveTorqueSlipModifier(0);
            wheelManager.RearRightWheel.SetDriveTorqueSlipModifier(0);
            wheelManager.FrontLeftWheel.SetDriveTorqueSlipModifier(0);
            wheelManager.FrontRightWheel.SetDriveTorqueSlipModifier(0);
            
            if (hasUndersteer)
            {
                ApplyStabilityControl(false);
            }
            
            if (hasOversteer)
            {
                ApplyStabilityControl(true);
            }
        }

        private void ApplyStabilityControl(bool isRear)
        {
            StabilityControlOn = true;

            Wheel leftWheel = isRear ? wheelManager.RearLeftWheel : wheelManager.FrontLeftWheel;
            Wheel rightWheel = isRear ? wheelManager.RearRightWheel : wheelManager.FrontRightWheel;
            
            //get the slip ratios of the rear 2 wheels
            //apply brakes to front wheels depending on the percentage
            var (leftPercent, rightPercent) = GetSlipPercentage(isRear);

            //slow down the wheel that is spinning more by applying brakes
            float speedModifier = Mathf.Clamp01(Speed / stabilityControlBrakingModifierMax);
            leftWheel.stabilityControlBraking = leftPercent * stabilityControlBrakingModifier.Evaluate(speedModifier);
            rightWheel.stabilityControlBraking = rightPercent * stabilityControlBrakingModifier.Evaluate(speedModifier);
                
            //balance the wheel's torque (acts like a differential would)
            leftWheel.SetDriveTorqueSlipModifier(GetDriveTorqueSlipModifier(leftPercent));
            rightWheel.SetDriveTorqueSlipModifier(GetDriveTorqueSlipModifier(rightPercent));
        }

        private float GetDriveTorqueSlipModifier(float slipPercent)
        {
            //if slipPercent = 0.5, value = 0
            //if slipPercent = 1, value = 1
            //if slipPercent = 0, value = -1
            return Mathf.Lerp(1, -1, slipPercent) * stabilityControlTorqueModifier;
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

        private void FixedUpdate()
        {
            CalculateSteering();
        }

        private void Update()
        {
            CalculateSpeed();
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

            SetBrakeLights(brake > 0);

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
        
        private void SetBrakeLights(bool isOn)
        {
            brakeLightsMaterial.SetFloat(BrakeShaderID, isOn ? 1 : 0);
        }

        /// <summary>
        /// Get the slip ratio of the wheel with the highest slip ratio.
        /// </summary>
        private float GetHighestSlipRatio()
        {
            float highest = 0;
            foreach (Wheel wheel in wheelManager.Wheels)
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
            
            foreach (Wheel wheel in wheelManager.Wheels)
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