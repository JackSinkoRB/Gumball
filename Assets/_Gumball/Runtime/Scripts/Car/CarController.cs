using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{

// This class is repsonsible for controlling inputs to the car.
[RequireComponent(typeof(Drivetrain))]
public class CarController : MonoBehaviour {

    // Add all wheels of the car here, so brake and steering forces can be applied to them.
    public Wheel[] wheels;
    public bool CarEnabled;
    // A transform object which marks the car's center of gravity.
    // Cars with a higher CoG tend to tilt more in corners.
    // The further the CoG is towards the rear of the car, the more the car tends to oversteer. 
    // If this is not set, the center of mass is calculated from the colliders.
    public Transform centerOfMass;
    public TyreCompound tyreCompound = TyreCompound.Street;
    public CarCustomisation customisation;
    public enum TyreCompound
    {
        Economy, //Low Forward & Sideways
        Street, //Low Forward, OK Sideways
        Sport, //OK Forward, OK Sideways
        Drag //OK Forward, Low Sideways
    }
    public int Offtrack;

    // A factor applied to the car's inertia tensor. 
    // Unity calculates the inertia tensor based on the car's collider shape.
    // This factor lets you scale the tensor, in order to make the car more or less dynamic.
    // A higher inertia makes the car change direction slower, which can make it easier to respond to.
    public float inertiaFactor = 1.5f;

    // current input state
    [HideInInspector]
    public float brake;
    [HideInInspector]
    float throttle;
    float throttleInput;
    [HideInInspector]
    public float steering;
    float lastShiftTime = -1;
    [HideInInspector]
    public float handbrake;

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
    public bool tractionControl = false;

    // Turn ABS control on or off
    public bool absControl = false;

    // These values determine how fast steering value is changed when the steering keys are pressed or released.
    // Getting these right is important to make the car controllable, as keyboard input does not allow analogue input.

    // How long it takes to fully turn the steering wheel from center to full lock
    public float steerTime = 1.2f;
    // This is added to steerTime per m/s of velocity, so steering is slower when the car is moving faster.
    public float veloSteerTime = 0.1f;
    public float veloHighSteerTime = 0.1f;
    // How long it takes to fully turn the steering wheel from full lock to center
    public float steerReleaseTime = 0.6f;
    // This is added to steerReleaseTime per m/s of velocity, so steering is slower when the car is moving faster.
    public float veloSteerReleaseTime = 0f;
    // When detecting a situation where the player tries to counter steer to correct an oversteer situation,
    // steering speed will be multiplied by the difference between optimal and current steering times this 
    // factor, to make the correction easier.
    public float steerCorrectionFactor = 4.0f;

    internal bool driftingNow = false;                                      // Currently drifting?
    internal float driftAngle = 0f;

    public Rigidbody rb;
    private bool gearShifted = false;
    private bool gearShiftedFlag = false;
    private bool reversing;
    public float rbSpeed;

    public float controllerThrottle = 0;
    public float controllerBrake = 0;
    public float controllerSteer = 0;

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

    // Initialize
    void Start() {
        rb = GetComponent<Rigidbody>();
        if (centerOfMass != null)
            rb.centerOfMass = centerOfMass.localPosition;

        rb.inertiaTensor *= inertiaFactor;
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].controller = this;
        }

    }


    void Update()
    {


        rbSpeed = transform.InverseTransformDirection(rb.velocity).z;
        // Steering
        Vector3 carDir = transform.forward;
        float fVelo = rb.velocity.magnitude;
        Vector3 veloDir = rb.velocity * (1 / fVelo);
        float angle = -Mathf.Asin(Mathf.Clamp(Vector3.Cross(veloDir, carDir).y, -1, 1));
        float optimalSteering = angle / (wheels[0].maxSteeringAngle * Mathf.Deg2Rad);
        if (fVelo < 1)
            optimalSteering = 0;

        float steerInput = 0;
        //steerInput = -0.600000023841858f * rb.angularVelocity.y; //Counter Steering
        if(controllerSteer !=0)
        {
            steerInput = controllerSteer;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow) || left)
                steerInput = -1;
            if (Input.GetKey(KeyCode.RightArrow) || right)
                steerInput = 1;
        }

        float absDifference = Mathf.Abs(steerInput - steering);
        if(steerInput!=0)
        {
            if (rbSpeed * 3.6f > 200)
            {
                steering = Mathf.Lerp(steering, steerInput, Time.deltaTime * veloHighSteerTime * absDifference);
            }
            else
            {
                steering = Mathf.Lerp(steering, steerInput, Time.deltaTime * (rbSpeed * 3.6f > 100 ? veloSteerTime : steerTime)* absDifference);
            }
                
        }
        else
        {
            
            steering = Mathf.Lerp(steering, steerInput, Time.deltaTime * (rbSpeed * 3.6f > 100 ? veloSteerReleaseTime : steerReleaseTime));
        }
        
        /*
        if (steerInput < steering)
        {
            float steerSpeed = (steering > 0) ? (1 / (steerReleaseTime + veloSteerReleaseTime * fVelo)) : (1 / (steerTime + veloSteerTime * fVelo));
            if (steering > optimalSteering)
                steerSpeed *= 1 + (steering - optimalSteering) * steerCorrectionFactor;
            steering -= steerSpeed * Time.deltaTime;
            if (steerInput > steering)
                steering = steerInput;
        }
        else if (steerInput > steering)
        {
            float steerSpeed = (steering < 0) ? (1 / (steerReleaseTime + veloSteerReleaseTime * fVelo)) : (1 / (steerTime + veloSteerTime * fVelo));
            if (steering < optimalSteering)
                steerSpeed *= 1 + (optimalSteering - steering) * steerCorrectionFactor;
            steering += steerSpeed * Time.deltaTime;
            if (steerInput < steering)
                steering = steerInput;
        }*/

        bool accelKey = false;
        if (Input.GetKey(KeyCode.UpArrow) || accel) { accelKey = true; }
        bool brakeKey = false;
        if (Input.GetKey(KeyCode.DownArrow) || brakes) { brakeKey = true; };
        /*
        if (drivetrain.automatic && drivetrain.gear == 0)
        {
            accelKey = Input.GetKey(KeyCode.DownArrow);
            brakeKey = Input.GetKey(KeyCode.UpArrow);
        }*/

        if (Input.GetKey(KeyCode.LeftShift))
        {
            throttle += Time.deltaTime / throttleTime;
            throttleInput += Time.deltaTime / throttleTime;
        }
        if(controllerThrottle>0)
        {
            throttle = controllerThrottle;
            throttleInput = controllerThrottle;
        }
        else if (accelKey)
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

        if (controllerBrake > 0)
        {
            throttle = 0;
            throttleInput = -controllerBrake;
            brake = controllerBrake;
        }
        else if (brakeKey)
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
        if(!brakeKey&&!accelKey&&controllerThrottle == 0&& controllerBrake==0)
        {
            throttleInput = 0;
        }
        brake = Mathf.Clamp01(brake);
        throttleInput = Mathf.Clamp(throttleInput, -1, 1);

        customisation.SetBrakeLights(brake > 0);
        // Handbrake
        handbrake = (handbrakes||Input.GetKey(KeyCode.Space)) ? 1f : 0f;
        if (handbrake == 1 || clutchin) { drivetrain.clutch = 1;  }
        else { drivetrain.clutch = 0; }
        // Gear shifting
        float shiftThrottleFactor = Mathf.Clamp01((Time.time - lastShiftTime) / shiftSpeed);

        
        //auto reverse
        //rbSpeed

        if (drivetrain.gear <= 2 && rbSpeed <= 0)
        {
            if (throttleInput < -0.1f)
            {
                if (!reversing)
                {
                    reversing = true;
                    drivetrain.gear = 0;
                }

                if (rbSpeed > 0)
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
                brake =0;
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
                if (rbSpeed < -0.5f)
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

        //
        if (drivetrain.gear == 0)
            drivetrain.throttle = throttle;
        else
            drivetrain.throttle = throttle * shiftThrottleFactor;

        drivetrain.throttleInput = throttleInput;

        if (Input.GetKeyDown(KeyCode.A)) // remove this to input gather
        {
            lastShiftTime = Time.time;
            drivetrain.ShiftUp();
            //status.sfxController.PlaySFX("SHIFT");
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            lastShiftTime = Time.time;
            drivetrain.ShiftDown();
            //status.sfxController.PlaySFX("SHIFT");
        }

        //play gear shift sound
        if (gearShifted && gearShiftedFlag && drivetrain.gear != 1)
        {
            gearShifted = false;
            gearShiftedFlag = false;
        }
        // Apply inputs
        foreach (Wheel w in wheels)
        {
            if(absControl)
            {
                float absSlipRatio = Mathf.Abs(w.slipRatio);
                brake = absSlipRatio > 0.5f ? 1-absSlipRatio / 1 : brake;
                
            }
            debugBrk = brake;
            debugAbs = Mathf.Abs(w.slipRatio);
            w.brake = (Input.GetKey(KeyCode.DownArrow)  || brakes ||brake>0) ? brake : 0;
            w.handbrake = handbrake;
            w.steering = steering;
            if (w.slipRatio < -1) { w.angularVelocity = 0; }
        }

        // Reset Car position and rotation in case it rolls over
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            transform.rotation = Quaternion.Euler(0, transform.localRotation.y, 0);
        }

        


    }
    public float debugAbs;
    public float debugBrk;
    private void FixedUpdate()
    {
        DriftVariables();
    }

    private void DriftVariables()
    {

        //float mydriftValue = Vector3.Dot(rb.velocity, transform.forward);
        //float mydriftAngle = Mathf.Acos(mydriftValue) * Mathf.Rad2Deg;
        float mydriftAngle = Vector3.SignedAngle(transform.forward, rb.velocity, Vector3.up);

        float slip = wheels[2].slipVelo;

        if (rb.velocity.magnitude > 1f && driftingNow)
            driftAngle = slip * 1f;
        else
            driftAngle = 0f;

        if (Mathf.Abs(slip) > 5)
        {
            driftingNow = true;


        }
        else
        {
            driftingNow = false;


        }

    }

    public bool left;
    public bool right;
    public bool accel;
    public bool brakes;
    public bool handbrakes;
    public bool clutchin;
    /*
    public void InputLeft()
    {
        this.left = true;
    }

     public void InputRight()
    {
        this.right = true;
    }
    */
    public void setgearPlus()
    {
        this.lastShiftTime = Time.time;
        this.drivetrain.ShiftUp();
        //status.sfxController.PlaySFX("SHIFT");
    }

    public void setgearMinus()
    {
        this.lastShiftTime = Time.time;
        this.drivetrain.ShiftDown();
        //status.sfxController.PlaySFX("SHIFT");
    }

    /// <summary>
    /// Force vehicle to be reset at a certain point
    /// </summary>
    public void OverrideVehicleState(Vector3 newPos, Quaternion newRot)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        drivetrain.rpm = 0;
        drivetrain.gear = 2;

        //reset wheels so they dont spin now we have reset
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].angularVelocity = 0;
        }
        transform.SetPositionAndRotation(newPos, newRot);
    }

    /*
    public void InputaccelButton()
    {
        this.accel = true;
    }

    public void InputbrakeButton()
    {
        this.brakes = true;
    }
    

    public void InputaccelButtonOff()
    {
        this.accel = false;
    }

    public void InputbrakeButtonOff()
    {
        this.brakes = false;
    }
    */
}

}
