using System;
using System.Collections;
using System.Collections.Generic;
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

        public float currentSteering;
        private float lastShiftTime = -1;
        
        // cached Drivetrain reference
        public Drivetrain drivetrain;

        // How long the car takes to shift gears
        public float shiftSpeed = 0.8f;


        // These values determine how fast throttle value is changed when the accelerate keys are pressed or released.
        // Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.
        // There are different values for when the wheels have full traction and when there are spinning, to implement 
        // traction control schemes.

        // How long it takes to fully engage the throttle
        public float throttleTime = 1.0f;

        // How long it takes to fully engage the throttle 
        // when the wheels are spinning (and traction control is disabled)
        public float throttleTimeTraction = 10.0f;

        // How long it takes to fully release the throttle
        public float throttleReleaseTime = 0.5f;

        // How long it takes to fully release the throttle 
        // when the wheels are spinning.
        public float throttleReleaseTimeTraction = 0.1f;

        // Turn traction control on or off
        public bool tractionControl;

        // Turn ABS control on or off
        public bool absControl;

        // These values determine how fast steering value is changed when the steering keys are pressed or released.
        // Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.

        // How long it takes to fully turn the steering wheel from center to full lock
        public float maxSteerSpeed = 2f;
        public float minSteerSpeed = 0.5f;
        [Tooltip("Using Km/h")]
        public float speedForMinSteerSpeed = 150;

        // How long it takes to fully turn the steering wheel from full lock to center
        public float steerReleaseSpeed = 2f;

        [ReadOnly, SerializeField] private Rigidbody rigidBody;
        private bool reversing;
        private bool clutchIn;
        [Tooltip("The speed that the rigidbody is moving (in m/s)")]
        [ReadOnly, SerializeField] private float speed;

        public CarCustomisation Customisation => customisation;

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
        }


        private void CalculateSpeed()
        {
            speed = transform.InverseTransformDirection(rigidBody.velocity).z;
        }

        private void CalculateThrottle()
        {
            
        }

        private void CalculateHandbrake()
        {
            if (InputManager.Handbrake.IsPressed())
                Debug.Log("Handbrake on!");
        }
        
        private void Update()
        {
            CalculateSpeed();
            CalculateSteering();
            CalculateThrottle();
            CalculateHandbrake();

            if (InputManager.Accelerate.IsPressed())
            {
                if (drivetrain.slipRatio < 0.10f)
                    throttle += Time.deltaTime / throttleTime;
                else if (!tractionControl)
                    throttle += Time.deltaTime / throttleTimeTraction;
                else
                    throttle -= Time.deltaTime / throttleReleaseTime;

                if (throttleInput < 0)
                    throttleInput = 0;
                throttleInput += Time.deltaTime / throttleTime;
            }
            else
            {
                if (drivetrain.slipRatio < 0.2f)
                    throttle -= Time.deltaTime / throttleReleaseTime;
                else
                    throttle -= Time.deltaTime / throttleReleaseTimeTraction;
            }

            throttle = Mathf.Clamp01(throttle);

            if (InputManager.Decelerate.IsPressed())
            {
                if (drivetrain.slipRatio < 0.2f)
                    brake += Time.deltaTime / throttleTime;
                else
                    brake += Time.deltaTime / throttleTimeTraction;
                throttle = 0;
                throttleInput -= Time.deltaTime / throttleTime;
            }
            else
            {
                if (drivetrain.slipRatio < 0.2f)
                    brake -= Time.deltaTime / throttleReleaseTime;
                else
                    brake -= Time.deltaTime / throttleReleaseTimeTraction;
            }

            if (!InputManager.Decelerate.IsPressed() && !InputManager.Accelerate.IsPressed())
            {
                throttleInput = 0;
            }

            brake = Mathf.Clamp01(brake);
            throttleInput = Mathf.Clamp(throttleInput, -1, 1);

            customisation.SetBrakeLights(brake > 0);

            if (InputManager.Handbrake.IsPressed() || clutchIn)
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

            if (drivetrain.gear <= 2 && speed <= 0)
            {
                if (throttleInput < -0.1f)
                {
                    if (!reversing)
                    {
                        reversing = true;
                        drivetrain.gear = 0;
                    }

                    if (speed > 0)
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
                    reversing = false;
                    drivetrain.gear = 2;
                    throttle = throttleInput;
                    brake = 0;
                }
            }

            if (drivetrain.gear == 0 && throttleInput >= 0.1f)
            {
                if (reversing)
                {
                    if (speed < -0.5f)
                    {
                        brake = 1;
                        throttle = 0;
                    }
                    else
                    {
                        reversing = false;
                        drivetrain.gear = 2;
                        throttle = throttleInput;
                        brake = 0;
                    }
                }
            }
            
            if (drivetrain.gear == 0)
                drivetrain.throttle = throttle;
            else
                drivetrain.throttle = throttle * shiftThrottleFactor;

            drivetrain.throttleInput = throttleInput;

            if (InputManager.ShiftUp.WasPressedThisFrame())
            {
                ShiftUp();
            }

            if (InputManager.ShiftDown.WasPressedThisFrame())
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
                if (absControl)
                {
                    brakeToApply *= 1 - (slipRatio / highestSlipRatio);
                }
                
                wheel.brake = brakeToApply;
                wheel.handbrake = InputManager.Handbrake.IsPressed() ? 1 : 0;
                wheel.steering = currentSteering;
                
                if (brake != 0)
                    Debug.Log("Braking: " + brakeToApply + " - steer: " + currentSteering + " - slip: " + slipRatio);
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

        private void CalculateSteering()
        {
            float finalSteerSpeed = 0;
            if (InputManager.SteeringInput == 0)
                finalSteerSpeed = steerReleaseSpeed;
            else
            {
                float speedPercent = Mathf.Clamp01(rigidBody.velocity.magnitude / SpeedUtils.FromKmh(speedForMinSteerSpeed));
                float difference = maxSteerSpeed - minSteerSpeed;
                finalSteerSpeed = minSteerSpeed + ((1-speedPercent) * difference);
            }
            
            currentSteering = Mathf.Lerp(currentSteering, InputManager.SteeringInput, Time.deltaTime * finalSteerSpeed);
            Debug.Log("Steering input = " + InputManager.SteeringInput + " - Speed = " + SpeedUtils.ToKmh(rigidBody.velocity.magnitude) + "km/h - Actual steer speed = " + finalSteerSpeed);
        }
    }
}