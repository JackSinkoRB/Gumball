using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gumball
{
    // This class simulates a car's engine and drivetrain, generating
    // torque, and applying the torque to the wheels.
    public class Drivetrain : MonoBehaviour
    {
        
        public delegate void OnGearChangedDelegate(int previousGear, int newGear);
        public static event OnGearChangedDelegate onGearChanged;
        
        // All the wheels the drivetrain should power
        public Wheel[] poweredWheels;

        // The gear ratios, including neutral (0) and reverse (negative) gears
        public float[] gearRatios;

        // The final drive ratio, which is multiplied to each gear ratio
        public float finalDriveRatio = 3.23f;

        // The engine's torque curve characteristics. Since actual curves are often hard to come by,
        // we approximate the torque curve from these values instead.

        // powerband RPM range
        public float minRPM = 800;
        public float maxRPM = 6400;

        // engine's maximal torque (in Nm) and RPM.
        public float maxTorque = 664;
        public float torqueRPM = 4000;

        // engine's maximal power (in Watts) and RPM.
        public float maxPower = 317000;
        public float powerRPM = 5000;

        // engine inertia (how fast the engine spins up), in kg * m^2
        public float engineInertia = 0.3f;

        // engine's friction coefficients - these cause the engine to slow down, and cause engine braking.

        // constant friction coefficient
        public float engineBaseFriction = 25f;

        // linear friction coefficient (higher friction when engine spins higher)
        public float engineRPMFriction = 0.02f;

        // Engine orientation (typically either Vector3.forward or Vector3.right). 
        // This determines how the car body moves as the engine revs up.	
        public Vector3 engineOrientation = Vector3.right;

        // Coefficient determining how muchg torque is transfered between the wheels when they move at 
        // different speeds, to simulate differential locking.
        public float differentialLockCoefficient = 0;

        // inputs
        // engine throttle
        public float throttle = 0;

        // engine throttle without traction control (used for automatic gear shifting)
        public float throttleInput = 0;

        //clutch
        public float clutch;
        private float clutchTorque;

        private int clutchshock = 0;

        // shift gears automatically?
        public bool automatic = true;

        private Rigidbody _rb;

        public int Gear { get; private set; } = 2; //start in first
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialise()
        {
            onGearChanged = null;
        }
        
        public void SetGear(int newGear)
        {
            if (newGear == Gear)
                return;
            
            int previousGear = Gear;
            Gear = newGear;
            
            onGearChanged?.Invoke(previousGear, newGear);
        }
        
        public float rpm;
        public float slipRatio = 0.0f;
        public float engineAngularVelo;

        public Turbocharger turbo;
        public bool enableTurbo = false;

        float Sqr(float x)
        {
            return x * x;
        }

        public bool backFireEnabled;
        private float backfireTracker = 0f;
        private float lastEngFricTorq = 0f;
        public float launchPower = 100;
        public float reverseLaunchPower = 50;
        public float launchCutoff = 2500;
        public float launchAssist = 0;

        private bool inReverse => Gear == 0;

        public bool TractionControlOn => WarehouseManager.Instance.CurrentCar.HasTractionControl && 
                                         Mathf.Abs(slipRatio) > WarehouseManager.Instance.CurrentCar.TractionControlSlipTrigger;

        // Calculate engine torque for current rpm and throttle values.
        /*
        float CalcEngineTorque() {
            float result;
            if (rpm < torqueRPM)
                result = maxTorque * (-Sqr(rpm / torqueRPM - 1) + 1);
            else {
                float maxPowerTorque = maxPower / (powerRPM * 2 * Mathf.PI / 60);
                float aproxFactor = (maxTorque - maxPowerTorque) / (2 * torqueRPM * powerRPM - Sqr(powerRPM) - Sqr(torqueRPM));
                float torque = aproxFactor * Sqr(rpm - torqueRPM) + maxTorque;
                result = torque > 0 ? torque : 0;
            }
            if (rpm > maxRPM) {
                result *= 1 - ((rpm - maxRPM) * 0.006f);
                if (result < 0)
                    result = 0;
            }
            if (rpm < 0)
                result = 0;
            return result;
        }*/

        float CalcEngineTorque()
        {
            float result;

            if (rpm < torqueRPM)
            {
                result = maxTorque * (-Sqr(rpm / torqueRPM - 1) + 1);
            }
            else
            {
                float maxPowerTorque = maxPower / (powerRPM * 2 * Mathf.PI / 60);
                float aproxFactor = (maxTorque - maxPowerTorque) /
                                    (2 * torqueRPM * powerRPM - Sqr(powerRPM) - Sqr(torqueRPM));
                float torque;

                if (rpm > torqueRPM)
                {
                    torque = maxTorque; // Set torque to maximum torque beyond torqueRPM
                }
                else
                {
                    torque = aproxFactor * Sqr(rpm - torqueRPM) + maxTorque;
                }

                result = torque > 0 ? torque : 0;
            }

            if (rpm > maxRPM)
            {
                result *= 1 - ((rpm - maxRPM) * 0.006f);
                if (result < 0)
                    result = 0;
            }

            if (rpm < 0)
                result = 0;

            return result;
        }

        void Awake()
        {
            AudioSource[] aud = GetComponents<AudioSource>();
            for (int i = aud.Length - 1; i >= 0; i--)
            {
                Destroy(aud[i]);
            }
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }
        
        void FixedUpdate()
        {
            float ratio = gearRatios[Gear] * finalDriveRatio;
            float inertia = engineInertia * Sqr(ratio);
            float engineFrictionTorque = engineBaseFriction + rpm * engineRPMFriction;
            float engineTorque = (CalcEngineTorque() + Mathf.Abs(engineFrictionTorque)) * throttle;
            float lap = (launchCutoff + minRPM - rpm) /
                        launchCutoff; //value 1 to 0 up to the launch cutoff. Add 800 to offset idle rpm

            if (throttle > 0 && (inReverse || Gear == 2)) //in first gear
            {
                if (rpm > launchCutoff)
                {
                    lap = 0;
                }

                float desiredLaunchPower = inReverse ? -Mathf.Abs(reverseLaunchPower) : launchPower;
                launchAssist = maxTorque * (_rb.mass / 100) * desiredLaunchPower * lap * Time.deltaTime;
            }
            else
            {
                launchAssist = 0;
            }


            if (backFireEnabled)
            {
                //backfire calculation
                if (engineFrictionTorque > lastEngFricTorq)
                {
                    //increasing
                    backfireTracker -= Mathf.Abs(engineFrictionTorque - lastEngFricTorq);
                    backfireTracker = Mathf.Clamp(backfireTracker, 0f, backfireTracker);
                }
                else if (engineFrictionTorque < lastEngFricTorq)
                {
                    // decreasing
                    backfireTracker += Mathf.Abs(engineFrictionTorque - lastEngFricTorq);
                }

                //if (backfireTracker > 32f) {
                if (backfireTracker > 16f)
                {
                    //TODO: get reference inside Drivetrain to particle controller (doesn't require player status)
                    //playerStatus.particleController.Trigger_ExhaustFlame();
                    backfireTracker = 0f;
                }

                lastEngFricTorq = engineFrictionTorque;
            }

            slipRatio = 0.0f;

            // TURBO //
            float turboPower = 1f;
            if (enableTurbo)
            {
                float temp = turbo.CalculateTorque((rpm / powerRPM), throttle, maxRPM);
                turboPower = 1f + temp;
            }

            if (engineAngularVelo < -1)
            {
                engineAngularVelo = -1;
            }

            if (engineAngularVelo < 1000 && throttle > 0)
            {
                engineAngularVelo += 1000 * Time.deltaTime;
            }

            turbo.angularVelocity = engineAngularVelo;


            if (ratio == 0 || (clutch == 1))
            {
                // Neutral gear - just rev up engine
                float engineAngularAcceleration =
                    (engineTorque - engineFrictionTorque) / engineInertia / 1.5f; //Experimental
                engineAngularVelo += engineAngularAcceleration * Time.deltaTime;

                //Still apply friction to drivetrain, ignoring engine torque.
                float drivetrainFraction = 1.0f / poweredWheels.Length;
                foreach (Wheel w in poweredWheels)
                {
                    w.drivetrainInertia = inertia * drivetrainFraction;
                    w.driveFrictionTorque = engineFrictionTorque * Mathf.Abs(ratio) * drivetrainFraction;
                    w.driveTorque = ratio * drivetrainFraction;
                    slipRatio += w.slipRatio * drivetrainFraction;
                }

                clutchshock = 1;
            }
            else
            {
                float drivetrainFraction = 1.0f / poweredWheels.Length;
                float averageAngularVelo = 0;
                foreach (Wheel wheel in poweredWheels)
                    averageAngularVelo += wheel.angularVelocity * drivetrainFraction;

                float engineAngularAcceleration = (engineTorque - engineFrictionTorque) / engineInertia;
                // Apply torque to wheels
                foreach (Wheel wheel in poweredWheels)
                {
                    float lockingTorque = (averageAngularVelo - wheel.angularVelocity) * differentialLockCoefficient;
                    wheel.drivetrainInertia = inertia * drivetrainFraction;
                    wheel.driveFrictionTorque = engineFrictionTorque * Mathf.Abs(ratio) * drivetrainFraction;
                    wheel.driveTorque = (launchAssist + engineTorque * ratio * drivetrainFraction + lockingTorque) *
                                    Mathf.Clamp(clutchshock * ratio * 10, 1, 50); //We need to limit this somehow
                    slipRatio += wheel.slipRatio * drivetrainFraction;
                }

                engineAngularVelo = averageAngularVelo * ratio;
                clutchshock = 0;
            }

            engineAngularVelo = Mathf.Clamp(engineAngularVelo, 0, 1000); // prevent over force

            // update state
            slipRatio *= Mathf.Sign(ratio);

            if (TractionControlOn)
            {
                float highestSlipRatio = GetHighestSlipRatio();
                foreach (Wheel wheel in poweredWheels)
                {
                    //limit the power depending on which wheel is slipping the most
                    wheel.driveTorque *= 1 - (slipRatio / highestSlipRatio);
                }
            }

            WarehouseManager.Instance.CurrentCar.StabilityControlCheck();

            rpm = engineAngularVelo * (60.0f / (2 * Mathf.PI));
            rpm = Mathf.Clamp(rpm, minRPM, maxRPM + minRPM); //limit excess rpm

            // very simple simulation of clutch - just pretend we are at a higher rpm.
            
            // Automatic gear shifting. Bases shift points on throttle input and rpm.
            
            if (Input.GetKeyDown(KeyCode.X))
            {
                automatic = !automatic;
            }

            if (automatic)
            {
                if (rpm >= powerRPM + minRPM && throttle > 0)
                {
                    ShiftUp();
                }
                else if (Gear > 2)
                {
                    if (rpm <= 4000 && throttle > 0 || throttle < 0.1f && rpm < 2000)
                    {
                        sampleRpm = rpm + rpm * (gearRatios[Gear] / gearRatios[Gear - 1]);


                        if (Gear > 2 && sampleRpm + minRPM <= powerRPM && sampleRpm > minRPM)
                        {
                            ShiftDown();
                        }
                    }
                }
            }
            
            //automatically set into reverse (even for manual)
            if (throttleInput < 0 && rpm <= minRPM && Gear >= 2)
            {
                SetGear(0); 
            }
        }

        /// <summary>
        /// Get the slip ratio of the wheel with the highest slip ratio.
        /// </summary>
        private float GetHighestSlipRatio()
        {
            float highest = 0;
            foreach (Wheel wheel in poweredWheels)
            {
                float ratio = Mathf.Abs(wheel.slipRatio);
                if (ratio > highest)
                    highest = ratio;
            }
            return highest;
        }
        
        public float sampleRpm;

        public void ShiftUp()
        {
            if (Gear < gearRatios.Length - 1)
                SetGear(Gear + 1);
        }

        public void ShiftDown()
        {
            if (Gear > 1) //can't go into reverse (0) because it's automatic when holding brake
                SetGear(Gear - 1);
        }
    }

    [System.Serializable]
    public class Turbocharger
    {
        public float angularVelocity = 0f;
        public float prevAngularVelocity = 0f;
        public float brakingCoeff = 0.92f;

        public float rpm = 0f;
        private float rpmNormalized = 0f;

        public float boost = 2f;
        public float pressure = 0f;
        [HideInInspector] public float prev_pressure = 0f;

        [HideInInspector] public float prev_throttle;

        public bool isSteeringWheelNo2 = false;


        public float CalculateTorque(float engineRpmNormalized, float throttle, float maxRPM)
        {
            float inertia = 0.01f;

            float angularAcceleration = throttle * engineRpmNormalized * 300f / inertia;

            angularVelocity += angularAcceleration * Time.deltaTime;

            angularVelocity += -brakingCoeff * rpmNormalized * 60f;

            if (angularVelocity < -1)
            {
                angularVelocity = -1;
            }

            rpm = angularVelocity * 60f; // rpm

            rpmNormalized = rpm / maxRPM;
            prev_pressure = pressure;
            pressure = rpmNormalized * boost * throttle + (isSteeringWheelNo2 ? 3.5f : 0f);

            CalculateBlowOff(throttle, prev_pressure);

            //WhistleSource.pitch = engineRpmNormalized;
            //WhistleSource.volume = rpmNormalized * 0.2f;


            return pressure * boost * 0.1f;
        }

        public void CalculateBlowOff(float throttle, float pressure)
        {
            prevAngularVelocity = angularVelocity;
        }
    }
}