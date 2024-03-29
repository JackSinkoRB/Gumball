using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class AICar : MonoBehaviour
    {

        public event Action onDisable;

        private enum WheelConfiguration
        {
            REAR_WHEEL_DRIVE,
            FRONT_WHEEL_DRIVE,
            ALL_WHEEL_DRIVE
        }
        
        [Header("Player car")]
        [SerializeField] private bool canBeDrivenByPlayer;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField] private CarIKManager avatarIKManager;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField] private SteeringWheel steeringWheel;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isPlayerDrivingEnabled;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField, ReadOnly] private int carIndex;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField, ReadOnly] private int id;
        
        public CarIKManager AvatarIKManager => avatarIKManager;
        public SteeringWheel SteeringWheel => steeringWheel;
        public int CarIndex => carIndex;
        public int ID => id;
        public string SaveKey => $"CarData.{carIndex}.{id}";

        [Header("Wheels")]
        [SerializeField, InitializationField] private WheelConfiguration wheelConfiguration;
        [SerializeField, InitializationField] private Transform[] frontWheelMeshes;
        [SerializeField, InitializationField] private Transform[] rearWheelMeshes;
        [SerializeField, InitializationField] private WheelCollider[] frontWheelColliders;
        [SerializeField, InitializationField] private WheelCollider[] rearWheelColliders;
        [Space(5)]
        private WheelCollider[] poweredWheels;
        private Transform[] allWheelMeshes;
        private WheelCollider[] allWheelColliders;
        
        [Header("Auto drive")]
        [SerializeField] private bool autoDrive;
        
        [Header("Max speed")]
        [Tooltip("Does the car obey the current chunks speed limit?")]
        [SerializeField] private bool obeySpeedLimit = true;
        [SerializeField, ConditionalField(nameof(obeySpeedLimit), true)] private float maxSpeed = 200;
        
        [Header("Lanes")]
        [Tooltip("Does the car try to take the optimal race line, or does it stay in a single (random) lane?")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private bool useRacingLine;
        [ConditionalField(new[]{ nameof(useRacingLine), nameof(autoDrive) }, new[]{ true, false }), SerializeField, ReadOnly] private float currentLaneDistance;
        [ConditionalField(nameof(autoDrive), nameof(useRacingLine)), SerializeField] private float racingLineOffset;

        [Header("Drag")]
        [SerializeField] private float dragWhenAccelerating;
        [SerializeField] private float dragWhenIdle = 0.15f;
        [SerializeField] private float dragWhenBraking = 1;
        [SerializeField] private float dragWhenHandbraking = 0.5f;
        [SerializeField] private float angularDragWhenHandbraking = 6;
        
        [Header("Movement")]
        [SerializeField, ReadOnly] private float speed;
        [SerializeField, ReadOnly] private float desiredSpeed;
        private bool wasMovingLastFrame;
        private const float stationarySpeed = 2;
        private bool isStationary => speed < stationarySpeed && !isAccelerating;

        [Header("Engine & Transmission")]
        [Tooltip("The engine torque output (y) compared to the engine RPM (x), between the min and max RPM ranges (where x = 0 is minEngineRpm)")]
        [SerializeField] private AnimationCurve torqueCurve;
        [SerializeField] private float[] gearRatios = { -1.5f, 2.66f, 1.78f, 1.3f, 1, 0.7f, 0.5f };
        [SerializeField] private float finalGearRatio = 3.42f;
        [SerializeField] private MinMaxFloat engineRpmRange = new(1000, 8000);
        [Space(5)]
        [SerializeField] private MinMaxFloat idealRPMRangeForGearChanges = new(3000, 6000);
        [Space(5)]
        [SerializeField, ReadOnly] private int currentGear;
        [SerializeField, ReadOnly] private bool isAccelerating;
        [SerializeField, ReadOnly] private float engineRpm;

        public delegate void OnGearChangedDelegate(int previousGear, int currentGear);
        public event OnGearChangedDelegate onGearChanged;
        
        private bool wasAcceleratingLastFrame;
        public bool IsAutomaticTransmission => autoDrive || GearboxSetting.Setting == GearboxSetting.GearboxOption.AUTOMATIC;
        public int CurrentGear => currentGear;
        public int NumberOfGears => gearRatios.Length;
        public float EngineRpm => engineRpm;
        
        [Header("Reversing")]
        [SerializeField] private float maxReverseSpeed = 25;
        [SerializeField, ReadOnly] private bool isReversing;
        
        [Header("Steering")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private MinMaxFloat movementTargetDistance = new(5, 10);
        [Tooltip("At less than or equal to 'min' km/h, the movementTargetDistance is min.\n" +
                 "At greater than or equal to 'max' km/h, the movementTargetDistance is max.")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private MinMaxFloat movementTargetDistanceSpeedFactors = new(20, 90);
        [Tooltip("The speed that the wheel collider turns if not auto driving.")]
        [ConditionalField(nameof(autoDrive), true), SerializeField] private float steerSpeed = 2.5f;
        [Tooltip("The speed that the wheel collider turns if auto driving.")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private float autoDriveSteerSpeed = 10;
        [Tooltip("This allows for a different steer speed when the steering input has been released.")]
        [ConditionalField(nameof(autoDrive), true), SerializeField] private float releaseSpeed = 15;
        [Tooltip("The speed that the wheel mesh is interpolated to the desired steer angle. This is different to the steer speed of the wheel collider.")]
        [SerializeField] private float visualSteerSpeed = 5;
        [ConditionalField(nameof(autoDrive)), SerializeField] private float autoDriveMaxSteerAngle = 65;
        [ConditionalField(nameof(autoDrive), true), SerializeField] private AnimationCurve maxSteerAngleCurve;
        [Space(5)]
        [SerializeField, ReadOnly] private float desiredSteerAngle;
        [SerializeField, ReadOnly] private float visualSteerAngle;
        
        [Header("Braking")]
        [SerializeField] private float brakeTorque = 1000;
        [Space(5)]
        [Tooltip("When the angle is supplied (x axis), the y axis represents the desired speed.")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private AnimationCurve cornerBrakingCurve;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isBraking;
        [ConditionalField(nameof(autoDrive)), SerializeField, ReadOnly] private float corneringSpeed = Mathf.Infinity;
        [SerializeField, ReadOnly] private float speedToBrakeTo;
        
        private bool wasBrakingLastFrame;
        
        [Header("Handbrake")]
        [SerializeField] private float handbrakeEaseOffDuration = 1f;
        [SerializeField] private float handbrakeTorque = 5000;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isHandbrakeEngaged;
        
        private float defaultRearWheelStiffness = -1;
        private float defaultAngularDrag;
        private readonly Sequence[] handbrakeEaseOffTweens = new Sequence[2];
        
        [Header("Collisions")]
        [SerializeField] private GameObject colliders;
        [SerializeField] private float collisionRecoverDuration = 1;
        [Space(5)]
        [SerializeField, ReadOnly] private bool inCollision;
        
        private float timeOfLastCollision = -Mathf.Infinity;
        
        public GameObject Colliders => colliders;
        
        [Header("Obstacle detection")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private bool brakeForObstacles = true;
        [ConditionalField(nameof(brakeForObstacles), nameof(autoDrive)), SerializeField] private ObstacleRaycast brakeForObstaclesRaycast;
        [Tooltip("If speed is at min of speedForBrakingRaycastLength, the brakingRaycastLength is at min, and vice versa.")]
        [ConditionalField(nameof(brakeForObstacles), nameof(autoDrive)), SerializeField] private MinMaxFloat brakingRaycastLength = new(8, 25);
        [Tooltip("If speed is at min of speedForBrakingRaycastLength, the brakingRaycastLength is at min, and vice versa.")]
        [ConditionalField(nameof(brakeForObstacles), nameof(autoDrive)), SerializeField] private MinMaxFloat speedForBrakingRaycastLength = new(10, 60);
        
        [Header("Obstacle avoidance")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private bool useObstacleAvoidance;
        [Tooltip("The speed the car should brake to if all the directions are blocked (exlcuding the 'when blocked' layers).")]
        [ConditionalField(nameof(useObstacleAvoidance), nameof(autoDrive)), SerializeField] private float speedToBrakeToIfBlocked = 50;
        [ConditionalField(nameof(useObstacleAvoidance), nameof(autoDrive)), SerializeField] private ObstacleRaycastLayer[] obstacleAvoidanceRaycastLayers;
        [ConditionalField(nameof(useObstacleAvoidance), nameof(autoDrive)), SerializeField] private ObstacleRaycastLayer obstacleAvoidanceRaycastLayerWhenBlocked;
        [ConditionalField(nameof(useObstacleAvoidance), nameof(autoDrive)), SerializeField, ReadOnly] private int currentLayerIndex;
        
        private bool allDirectionsAreBlocked;

        [Header("Debugging")]
        [SerializeField] private bool debug;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isInitialised;
        [Space(5)]
        [SerializeField, ReadOnly] private Chunk currentChunkCached;
        [SerializeField, ReadOnly] private bool isFrozen;

        private Vector3 targetPosition;
        private int lastFrameChunkWasCached = -1;
        private (Chunk, Vector3, Quaternion)? targetPos;
        
        private float timeSinceCollision => Time.time - timeOfLastCollision;
        private bool recoveringFromCollision => inCollision || timeSinceCollision < collisionRecoverDuration;
        private bool faceForward => useRacingLine || currentChunkCached.TrafficManager.GetLaneDirection(CurrentLaneDistance) == ChunkTrafficManager.LaneDirection.FORWARD;

        public Rigidbody Rigidbody => GetComponent<Rigidbody>();
        public float DesiredSpeed => desiredSpeed;
        public float Speed => speed;
        public float MaxSpeed => isReversing ? maxReverseSpeed : (obeySpeedLimit ? CurrentChunk.TrafficManager.SpeedLimitKmh : maxSpeed);
        public float CurrentLaneDistance => currentLaneDistance;
        
        /// <returns>The chunk the player is on, else null if it can't be found.</returns>
        public Chunk CurrentChunk
        {
            get
            {
                if (lastFrameChunkWasCached != Time.frameCount)
                {
                    lastFrameChunkWasCached = Time.frameCount;

                    Chunk previousChunk = currentChunkCached;
                    
                    //raycast down to terrain
                    currentChunkCached = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.ChunkDetector))
                        ? hitDown.transform.parent.GetComponent<Chunk>()
                        : null;
                    
                    if (currentChunkCached != previousChunk)
                        OnChangeChunk(previousChunk, currentChunkCached);
                }

                return currentChunkCached;
            }
        }
        
        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();

            if (canBeDrivenByPlayer)
                GearboxSetting.onSettingChanged += OnGearboxSettingChanged;
            
            //reset steering
            visualSteerAngle = 0;
            desiredSteerAngle = 0;
            
            InitialiseGearbox();
        }

        private void OnDisable()
        {
            onDisable?.Invoke();
            onDisable = null; //reset listener
            
            //reset for pooled objects:
            Unfreeze();
            isAccelerating = false;
            wasAcceleratingLastFrame = false;

            if (canBeDrivenByPlayer)
                GearboxSetting.onSettingChanged -= OnGearboxSettingChanged;
            
            onGearChanged = null; //reset listeners
            
            if (InputManager.ExistsRuntime && canBeDrivenByPlayer)
            {
                InputManager.Instance.CarInput.ShiftUp.onPressed -= ShiftUp;
                InputManager.Instance.CarInput.ShiftDown.onPressed -= ShiftDown;
            }
        }

        private void Initialise()
        {
            isInitialised = true;

            defaultAngularDrag = Rigidbody.angularDrag;
            
            gameObject.layer = canBeDrivenByPlayer ? (int)LayersAndTags.Layer.PlayerVehicle : (int)LayersAndTags.Layer.TrafficCar;
            
            OnChangeChunk(null, CurrentChunk);

            CacheAllWheelMeshes();
            CacheAllWheelColliders();
            CachePoweredWheels();
        }
        
        public void InitialisePlayerCar(int carIndex, int id)
        {
            this.carIndex = carIndex;
            this.id = id;
            
            SetAutoDrive(false);
        }
        
        public void SetAutoDrive(bool autoDrive)
        {
            this.autoDrive = autoDrive;
        }

        public void SetRacingLineOffset(float offset)
        {
            racingLineOffset = offset;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!Rigidbody.isKinematic)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }
            
            //move the transform AND the rigidbody, so physics calculations are updated instantly too
            transform.position = position;
            transform.rotation = rotation;
            Rigidbody.position = position;
            Rigidbody.rotation = rotation;
            
            //reset steer angle
            visualSteerAngle = 0;
            desiredSteerAngle = 0;
            
            foreach (WheelCollider wheelCollider in allWheelColliders)
            {
                wheelCollider.motorTorque = 0;
                wheelCollider.rotationSpeed = 0;
                wheelCollider.steerAngle = 0;
            }

            Move(); //force update

            GlobalLoggers.AICarLogger.Log($"Teleported {gameObject.name} to {position}.");
        }

        public void SetLaneDistance(float laneDistance)
        {
            currentLaneDistance = laneDistance;
        }
        
        private void FixedUpdate()
        {
            if (!isInitialised)
                return;
            
            if (CurrentChunk == null && autoDrive)
            {
                //current chunk may have despawned
                Despawn();
                return;
            }
            
            if (!isFrozen)
            {
                Move();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool hitACar = LayersAndTags.AllCarLayers.ContainsLayer(collision.gameObject.layer);
            if (!hitACar)
                return;

            GlobalLoggers.AICarLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            timeOfLastCollision = Time.time; //reset collision time

            inCollision = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            bool hitACar = LayersAndTags.AllCarLayers.ContainsLayer(collision.gameObject.layer);
            if (!hitACar)
                return;
            
            timeOfLastCollision = Time.time; //reset collision time
            inCollision = false;
        }

        private void OnChunkCachedBecomeInaccessible()
        {
            Freeze();
        }

        private void OnChunkCachedBecomeAccessible()
        {
            Unfreeze();
        }
        
        private void Freeze()
        {
            isFrozen = true;

            Rigidbody.velocity = Vector3.zero;
            Rigidbody.isKinematic = true;
        }

        private void Unfreeze()
        {
            isFrozen = false;
            
            Rigidbody.isKinematic = false;
        }
        
        private void OnChangeDesiredSpeed(float newSpeed)
        {
            desiredSpeed = newSpeed;
            
            if (debug) GlobalLoggers.AICarLogger.Log($"Changed desired speed to {desiredSpeed}");
        }

        private void Move()
        {
            if (autoDrive)
            {
                targetPos = GetPositionAhead(GetMovementTargetDistance(Speed));
                if (targetPos == null)
                {
                    Despawn();
                    return;
                }

                targetPosition = targetPos.Value.Item2;
            }

            speed = SpeedUtils.ToKmh(Rigidbody.velocity.magnitude);

            CheckIfPlayerDriving();
            
            if (useObstacleAvoidance && autoDrive)
                TryAvoidObstacles();
            
            UpdateDesiredSpeed();
            
            if (!autoDrive || !recoveringFromCollision) //don't update steering angle in collision
                CalculateSteerAngle();
            
            if (autoDrive)
                CheckForCorner();
            
            UpdateBrakingValues();
            DoBrakeEvents();

            CheckForHandbrake();
            CheckToReverse();

            UpdateCurrentGear();
            
            CheckToAccelerate();
            DoAccelerationEvents();
            
            ApplySteering();
            
            UpdateWheelMeshes();

            CalculateEngineRPM();

            UpdateDrag();
            
            //debug directions:
            Debug.DrawRay(transform.position, Rigidbody.velocity, Color.green);
            
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
        }

        private void UpdateDrag()
        {
            if (isHandbrakeEngaged)
            {
                Sequence handbrakeEaseOffTween = handbrakeEaseOffTweens[0];
                if (handbrakeEaseOffTween != null && handbrakeEaseOffTween.IsPlaying())
                    return;
                
                Rigidbody.drag = dragWhenHandbraking;
            }
            else if (isBraking)
                Rigidbody.drag = dragWhenBraking;
            else if (isAccelerating)
                Rigidbody.drag = dragWhenAccelerating;
            else
                Rigidbody.drag = dragWhenIdle;
        }
        
        private void UpdateCurrentGear()
        {
            if (!IsAutomaticTransmission)
                return;

            if (isReversing)
                return;

            //check whether to shift up or down
            if (engineRpm < idealRPMRangeForGearChanges.Min && currentGear > 1)
            {
                ShiftDown();
            } else if (engineRpm > idealRPMRangeForGearChanges.Max && currentGear < NumberOfGears - 1)
            {
                ShiftUp();
            }
        }

        /// <summary>
        /// Calculate the engine RPM from the average rpm of the powered wheels, taken in account the gear ratios.
        /// </summary>
        private void CalculateEngineRPM()
        {
            float sumOfPoweredWheelRPM = 0;
            foreach (WheelCollider poweredWheel in poweredWheels)
            {
                sumOfPoweredWheelRPM += poweredWheel.rpm;
            }
            
            float averagePoweredWheelRPM = sumOfPoweredWheelRPM / poweredWheels.Length;

            float engineRpmUnclamped = engineRpmRange.Min + averagePoweredWheelRPM * gearRatios[currentGear] * finalGearRatio;
            engineRpm = engineRpmRange.Clamp(engineRpmUnclamped);
        }
        
        private void UpdateDesiredSpeed()
        {
            if (!MaxSpeed.Approximately(DesiredSpeed))
                OnChangeDesiredSpeed(MaxSpeed);
        }

        private void CheckToAccelerate()
        {
            //don't accelerate if braking
            if (isBraking && speed > stationarySpeed)
                isAccelerating = false;
            //don't accelerate if in collision (auto drive only)
            else if (autoDrive && recoveringFromCollision)
                isAccelerating = false;
            //can't accelerate if above desired speed
            if (speed > desiredSpeed)
                isAccelerating = false;

            //check to accelerate
            else
            {
                isAccelerating = autoDrive || (!autoDrive && InputManager.Instance.CarInput.Accelerate.IsPressed) || (!autoDrive && isReversing);
                
                //if accelerating from reverse, change the gear
                if (InputManager.Instance.CarInput.Accelerate.IsPressed && currentGear == 0)
                    ChangeGear(1);
            }
        }

        private void ChangeGear(int newGear)
        {
            int previousGear = currentGear;
            currentGear = newGear;
            
            onGearChanged?.Invoke(previousGear, currentGear);
        }

        private void DoAccelerationEvents()
        {
            if (wasAcceleratingLastFrame && !isAccelerating)
                OnStopAccelerating();

            if (!wasAcceleratingLastFrame && isAccelerating)
                OnStartAccelerating();
            
            if (isAccelerating)
                OnAccelerate();
            
            wasAcceleratingLastFrame = isAccelerating;
        }
        
        private void CalculateSteerAngle()
        {
            float speedPercent = Mathf.Clamp01(speed / MaxSpeed);
            float maxSteerAngle = autoDrive ? autoDriveMaxSteerAngle : maxSteerAngleCurve.Evaluate(speedPercent);

            if (autoDrive)
            {
                Vector3 directionToTarget = targetPosition - Rigidbody.position;
                float angle = Mathf.Clamp(-Vector2.SignedAngle(Rigidbody.velocity.FlattenAsVector2(), directionToTarget.FlattenAsVector2()), -maxSteerAngle, maxSteerAngle);
                desiredSteerAngle = Mathf.LerpAngle(desiredSteerAngle, angle, autoDriveSteerSpeed * Time.deltaTime);
            }
            else
            {
                float actualSteerSpeed = InputManager.Instance.CarInput.SteeringInput.Approximately(0) ? releaseSpeed : steerSpeed;

                float angle = InputManager.Instance.CarInput.SteeringInput * maxSteerAngle;
                desiredSteerAngle = Mathf.LerpAngle(desiredSteerAngle, angle, actualSteerSpeed * Time.deltaTime);
            }
            
            //set the visual steer angle (same for all front wheels)
            const float minSpeedVisualSteerModifier = 20;
            float speedModifier = Mathf.Clamp01(speed / minSpeedVisualSteerModifier); //adjust for low speed
            visualSteerAngle = Mathf.LerpAngle(visualSteerAngle, desiredSteerAngle, visualSteerSpeed * speedModifier * Time.deltaTime);
        }

        private void UpdateBrakingValues()
        {
            //reset for check
            isBraking = false;
            speedToBrakeTo = Mathf.Infinity;

            if (autoDrive && inCollision)
                return;

            if (isReversing)
                return;

            //is speeding over the desired speed? 
            const float speedingLeeway = 10; //the amount the player can speed past the desired speed before needing to brake
            if (autoDrive && speed > desiredSpeed + speedingLeeway)
            {
                speedToBrakeTo = desiredSpeed;
                isBraking = true;
            }

            //brake around corners
            if (autoDrive && speed > corneringSpeed && corneringSpeed < speedToBrakeTo)
            {
                speedToBrakeTo = corneringSpeed;
                isBraking = true;
            }

            if (autoDrive && brakeForObstacles)
            {
                float speedPercent = (speed - speedForBrakingRaycastLength.Min) / (speedForBrakingRaycastLength.Max - speedForBrakingRaycastLength.Min);
                float raycastLength = brakingRaycastLength.Min + ((brakingRaycastLength.Max - brakingRaycastLength.Min) * speedPercent);
                brakeForObstaclesRaycast.SetRaycastLength(raycastLength);
                
                brakeForObstaclesRaycast.DoRaycast(transform, targetPosition);
                if (brakeForObstaclesRaycast.IsBlocked)
                {
                    speedToBrakeTo = 0;
                    isBraking = true;
                }
            }
            
            if (autoDrive && useObstacleAvoidance && allDirectionsAreBlocked)
            {
                if (speed > speedToBrakeToIfBlocked && speedToBrakeToIfBlocked < speedToBrakeTo)
                {
                    speedToBrakeTo = speedToBrakeToIfBlocked;
                    isBraking = true;
                }
            }
            
            if (!autoDrive && InputManager.Instance.CarInput.Brake.IsPressed)
            {
                speedToBrakeTo = 0;
                isBraking = true;
            }

            //brake if stationary
            if (isStationary)
            {
                speedToBrakeTo = 0;
                isBraking = true;
            }
        }
        
        private void DoBrakeEvents()
        {
            if (wasBrakingLastFrame && !isBraking)
                OnStopBraking();
            
            if (!wasBrakingLastFrame && isBraking)
                OnStartBraking();
            
            if (isBraking)
                OnBrake();

            wasBrakingLastFrame = isBraking;
        }

        private void CheckToReverse()
        {
            if (autoDrive)
                //TODO: might want to handle cases if car is completely blocked ahead and reverse out
                return;
            
            if (isReversing && !InputManager.Instance.CarInput.Brake.IsPressed)
                OnStopReversing();
            if (!isReversing && (isStationary || speed < 1 || currentGear == 0) && InputManager.Instance.CarInput.Brake.IsPressed)
                OnStartReversing();
        }
        
        private void CheckForHandbrake()
        {
            if (autoDrive)
                return;
            
            if (isHandbrakeEngaged && !InputManager.Instance.CarInput.Handbrake.IsPressed)
                OnHandbrakeDisengage();
            if (!isHandbrakeEngaged && InputManager.Instance.CarInput.Handbrake.IsPressed)
                OnHandbrakeEngage();
        }

        private void OnHandbrakeEngage()
        {
            isHandbrakeEngaged = true;
            
            Rigidbody.angularDrag = angularDragWhenHandbraking;

            for (int index = 0; index < rearWheelColliders.Length; index++)
            {
                WheelCollider wheelCollider = rearWheelColliders[index];
                handbrakeEaseOffTweens[index]?.Kill();

                WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = 0;

                wheelCollider.sidewaysFriction = sidewaysFriction;

                wheelCollider.brakeTorque = handbrakeTorque;
            }
        }

        private void OnHandbrakeDisengage()
        {
            isHandbrakeEngaged = false;

            for (int index = 0; index < rearWheelColliders.Length; index++)
            {
                WheelCollider wheelCollider = rearWheelColliders[index];
                
                WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;

                wheelCollider.brakeTorque = 0;
                
                handbrakeEaseOffTweens[index]?.Kill();
                handbrakeEaseOffTweens[index] = DOTween.Sequence()
                    .Join(DOTween.To(() => Rigidbody.angularDrag, x => Rigidbody.angularDrag = x, defaultAngularDrag, handbrakeEaseOffDuration))
                    .Join(DOTween.To(() => sidewaysFriction.stiffness, 
                        x => sidewaysFriction.stiffness = x, defaultRearWheelStiffness, handbrakeEaseOffDuration)
                    .OnUpdate(() => wheelCollider.sidewaysFriction = sidewaysFriction));
            }
        }
        
        private void OnStartReversing()
        {
            isReversing = true;
            ChangeGear(0);
        }

        private void OnStopReversing()
        {
            isReversing = false;
        }
        
        private void TryAvoidObstacles()
        {
            if (!autoDrive)
                return;
            
            //get the angle with the least angle UP TO the current layer
            float leastAngle = Mathf.Infinity;
            int leastAngleLayer = default;
            Vector3 leastAngleOffset = default;

            allDirectionsAreBlocked = true;
            
            for (int index = 0; index < obstacleAvoidanceRaycastLayers.Length; index++)
            {
                if (index > currentLayerIndex)
                    break;
                
                ObstacleRaycastLayer layer = obstacleAvoidanceRaycastLayers[index];
                ObstacleRaycast raycast = layer.GetUnblockedRaycastWithLeastAngle(transform, targetPosition);

                bool areAllBlocked = raycast == null;
                if (!areAllBlocked)
                {
                    if (raycast.Angle > 20 && !allDirectionsAreBlocked)
                    {
                        if (index == currentLayerIndex)
                            currentLayerIndex++;
                        continue;
                    }
                    
                    allDirectionsAreBlocked = false;
                    
                    if (raycast.Angle > leastAngle)
                        continue;

                    leastAngle = raycast.Angle;
                    leastAngleLayer = index;
                    leastAngleOffset = raycast.OffsetVector;
                }
                else
                {
                    //check the next layer as all are blocked
                    if (index == currentLayerIndex)
                        currentLayerIndex++;
                }
            }

            if (allDirectionsAreBlocked)
            {
                //try with directions relative to car
                ObstacleRaycast raycast = obstacleAvoidanceRaycastLayerWhenBlocked.GetUnblockedRaycastWithLeastAngle(transform, transform.position + transform.forward * 10);
                if (raycast == null)
                    return;

                targetPosition = transform.position + (transform.forward * raycast.RaycastLength) + raycast.OffsetVector;
                currentLayerIndex = obstacleAvoidanceRaycastLayers.Length;
            }
            else
            {
                targetPosition += leastAngleOffset;
                currentLayerIndex = leastAngleLayer;
            }
        }
        
        private void OnStartBraking()
        {
            Rigidbody.drag = dragWhenBraking;
            
            foreach (WheelCollider wheelCollider in allWheelColliders)
            {
                wheelCollider.brakeTorque = brakeTorque;
            }
        }
        
        private void OnBrake()
        {
            
        }

        private void OnStopBraking()
        {
            Rigidbody.drag = isAccelerating ? 0 : dragWhenIdle;
            
            foreach (WheelCollider wheelCollider in allWheelColliders)
            {
                wheelCollider.brakeTorque = 0;
            }
        }
        
        private void CheckForCorner()
        {
            const float visionDistance = 25f;
            var targetPos = GetPositionAhead(visionDistance);
            if (targetPos == null)
                return;
            
            var (chunk, targetPosition, targetRotation) = targetPos.Value;
            Vector3 directionToTarget = targetPosition - transform.position;

            float angleAhead = Vector2.Angle(transform.forward.FlattenAsVector2(), directionToTarget.FlattenAsVector2());
            float angleAheadToBrakeTo = Mathf.Max(Mathf.Abs(desiredSteerAngle), angleAhead);
            
            corneringSpeed = cornerBrakingCurve.Evaluate(angleAheadToBrakeTo);
        }

        private void OnStartAccelerating()
        {
            Rigidbody.drag = 0;
        }

        private void OnStopAccelerating()
        {
            foreach (WheelCollider wheelCollider in poweredWheels)
            {
                wheelCollider.motorTorque = 0;
            }
            
            Rigidbody.drag = isBraking ? dragWhenBraking : dragWhenIdle;
        }
        
        private void OnAccelerate()
        {
            ApplyTorqueToPoweredWheels();
        }

        private void ApplyTorqueToPoweredWheels()
        {
            //calculate the current engine torque from the engine RPM
            float engineRpmPercent = (engineRpm - engineRpmRange.Min) / engineRpmRange.Difference;
            float engineTorque = torqueCurve.Evaluate(engineRpmPercent);
            
            foreach (WheelCollider poweredWheel in poweredWheels)
            {
                //distribute the engine torque to the wheels based on gear ratios
                float engineTorqueDistributed = engineTorque / poweredWheels.Length; //TODO: might want to distribute this unevenly - eg. give more torque to the wheel with more traction
                float wheelTorque = engineTorqueDistributed * gearRatios[currentGear] * finalGearRatio;
            
                //apply to the wheels
                poweredWheel.motorTorque = wheelTorque;
            }
        }
        
        private void ApplySteering()
        {
            foreach (WheelCollider frontWheel in frontWheelColliders)
            {
                if (Mathf.Abs(desiredSteerAngle) < 1)
                    frontWheel.steerAngle = 0;
                else
                {
                    frontWheel.steerAngle = desiredSteerAngle;
                }
            }
            
            if (steeringWheel != null)
                steeringWheel.UpdateSteeringAmount(desiredSteerAngle);
        }

        /// <summary>
        /// Update all the wheel meshes to match the wheel colliders.
        /// </summary>
        private void UpdateWheelMeshes()
        {
            //do rear wheels first as the front wheels require their rotation
            for (int count = 0; count < rearWheelMeshes.Length; count++)
            {
                Transform rearWheelMesh = rearWheelMeshes[count];
                WheelCollider rearWheelCollider = rearWheelColliders[count];
                
                rearWheelCollider.GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);
                rearWheelMesh.position = wheelPosition;
                rearWheelMesh.rotation = wheelRotation;
            }

            for (int count = 0; count < frontWheelMeshes.Length; count++)
            {
                Transform frontWheelMesh = frontWheelMeshes[count];
                WheelCollider frontWheelCollider = frontWheelColliders[count];
                
                frontWheelCollider.GetWorldPose(out Vector3 wheelPosition, out _);
                frontWheelMesh.position = wheelPosition;

                //rotation is the same as the rear wheel, but with interpolated steer speed
                Transform rearWheelRotation = rearWheelMeshes[count];
                frontWheelMesh.rotation = rearWheelRotation.rotation;
                
                //set the steer amount
                Transform steerPivot = frontWheelMesh.parent;
                steerPivot.Rotate(Vector3.up, visualSteerAngle);
            }
        }
        
        private float GetMovementTargetDistance(float speedToCheck)
        {
            speedToCheck = Mathf.Clamp(speedToCheck, movementTargetDistanceSpeedFactors.Min, movementTargetDistanceSpeedFactors.Max);
            float percentage = (speedToCheck - movementTargetDistanceSpeedFactors.Min) / (movementTargetDistanceSpeedFactors.Max - movementTargetDistanceSpeedFactors.Min);
            float resultDistance = Mathf.Lerp(movementTargetDistance.Min, movementTargetDistance.Max, percentage);
            return resultDistance;
        }
        
        private void Despawn()
        {
            gameObject.Pool();
            GlobalLoggers.AICarLogger.Log($"Despawned at {transform.position}");
        }

        private void CacheAllWheelMeshes()
        {
            int indexCount = 0;
            allWheelMeshes = new Transform[frontWheelMeshes.Length + rearWheelMeshes.Length];
            foreach (Transform wheelMesh in frontWheelMeshes)
            {
                allWheelMeshes[indexCount] = wheelMesh;
                indexCount++;
            }
            foreach (Transform wheelMesh in rearWheelMeshes)
            {
                allWheelMeshes[indexCount] = wheelMesh;
                indexCount++;
            }
        }

        private void CacheAllWheelColliders()
        {
            int indexCount = 0;
            allWheelColliders = new WheelCollider[frontWheelColliders.Length + rearWheelColliders.Length];
            foreach (WheelCollider wheelCollider in frontWheelColliders)
            {
                wheelCollider.gameObject.AddComponent<WheelColliderData>();
                
                allWheelColliders[indexCount] = wheelCollider;
                indexCount++;
            }
            foreach (WheelCollider wheelCollider in rearWheelColliders)
            {
                if (defaultRearWheelStiffness < 0)
                    defaultRearWheelStiffness = wheelCollider.sidewaysFriction.stiffness;
                        
                wheelCollider.gameObject.AddComponent<WheelColliderData>();

                allWheelColliders[indexCount] = wheelCollider;
                indexCount++;
            }
        }
        
        private void CachePoweredWheels()
        {
            int numberOfPoweredWheels = wheelConfiguration == WheelConfiguration.ALL_WHEEL_DRIVE ? 4 : 2;
            poweredWheels = new WheelCollider[numberOfPoweredWheels];

            int indexCount = 0;
            foreach (WheelCollider wheelCollider in frontWheelColliders)
            {
                if (wheelConfiguration != WheelConfiguration.FRONT_WHEEL_DRIVE && wheelConfiguration != WheelConfiguration.ALL_WHEEL_DRIVE)
                    continue;
                
                poweredWheels[indexCount] = wheelCollider;
                indexCount++;
            }
            foreach (WheelCollider wheelCollider in rearWheelColliders)
            {
                if (wheelConfiguration != WheelConfiguration.REAR_WHEEL_DRIVE && wheelConfiguration != WheelConfiguration.ALL_WHEEL_DRIVE)
                    continue;
                
                poweredWheels[indexCount] = wheelCollider;
                indexCount++;
            }
        }

        /// <summary>
        /// Get the next desired position and rotation relative to the sample on the next chunk's spline.
        /// </summary>
        /// <returns>The spline sample's position and rotation, or null if no more loaded chunks in the desired direction.</returns>
        private (Chunk, Vector3, Quaternion)? GetPositionAhead(float distance)
        {
            if (CurrentChunk == null)
                return null;
            
            if (CurrentChunk.TrafficManager == null)
            {
                Debug.LogWarning($"A traffic car is on the chunk {CurrentChunk.gameObject.name}, but it doesn't have a traffic manager.");
                return null;
            }

            (SplineSample, Chunk)? splineSampleAhead = GetSplineSampleAhead(distance);
            if (splineSampleAhead == null)
                return null; //no more chunks loaded

            if (useRacingLine)
            {
                Vector3 offset = splineSampleAhead.Value.Item1.right * racingLineOffset;
                return (splineSampleAhead.Value.Item2, splineSampleAhead.Value.Item1.position + offset, splineSampleAhead.Value.Item1.rotation);
            }
            else
            {
                var (position, rotation) = CurrentChunk.TrafficManager.GetLanePosition(splineSampleAhead.Value.Item1, CurrentLaneDistance);
                return (splineSampleAhead.Value.Item2, position, rotation);
            }

            return null;
        }

        /// <summary>
        /// Gets the spline sample that is 'distance' metres away from the closest sample.
        /// </summary>
        private (SplineSample, Chunk)? GetSplineSampleAhead(float desiredDistance)
        {
            if (CurrentChunk.TrafficManager == null)
                return null; //no traffic manager

            float desiredDistanceSqr = desiredDistance * desiredDistance;

            Chunk chunkToUse = CurrentChunk;
            int chunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunkToUse);
            
            bool isChunkLoaded = chunkIndex >= 0;
            if (!isChunkLoaded)
                return null; //current chunk isn't loaded

            SampleCollection sampleCollection = useRacingLine ? CurrentChunk.TrafficManager.RacingLine.SampleCollection : CurrentChunk.SplineSampleCollection;
            
            //get the closest sample, then get the next, and next, until it is X distance away from the closest
            int closestSplineIndex = sampleCollection.GetClosestSampleIndexOnSpline(Rigidbody.position).Item1;
            SplineSample closestSample = sampleCollection.samples[closestSplineIndex];

            SplineSample? previousSample = null;
            float previousDistanceOffset = 0;
            int offset = faceForward ? 1 : -1;
            while (true)
            {
                int sampleIndex = closestSplineIndex + offset;
                
                //check if it goes past the current chunk
                if (sampleIndex >= sampleCollection.samples.Length || sampleIndex < 0)
                {
                    //get the next chunk
                    chunkIndex = faceForward ? chunkIndex + 1 : chunkIndex - 1;
                    
                    LoadedChunkData? loadedChunkData = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(chunkIndex);
                    if (loadedChunkData == null)
                    {
                        //no more loaded chunks
                        return null;
                    }
                    
                    Chunk newChunk = loadedChunkData.Value.Chunk;
                    chunkToUse = newChunk;
                    if (newChunk.TrafficManager == null)
                        return null; //no traffic manager

                    sampleCollection = useRacingLine ? chunkToUse.TrafficManager.RacingLine.SampleCollection : chunkToUse.SplineSampleCollection;
                    
                    //reset the values
                    previousSample = null;
                    closestSplineIndex = sampleCollection.GetClosestSampleIndexOnSpline(Rigidbody.position).Item1;
                    closestSample = sampleCollection.samples[closestSplineIndex];
                    offset = faceForward ? 1 : -1;
                    continue;
                }
                
                SplineSample sample = sampleCollection.samples[closestSplineIndex + offset];
                float distanceToSampleSqr = Vector3.SqrMagnitude(sample.position - closestSample.position);
                float distanceOffset = Mathf.Abs(desiredDistanceSqr - distanceToSampleSqr);
                
                bool isFurtherAway = previousSample != null && distanceOffset > previousDistanceOffset;
                if (isFurtherAway)
                    return (previousSample.Value, chunkToUse);
                
                previousDistanceOffset = distanceOffset;
                previousSample = sample;
                
                offset = faceForward ? offset + 1 : offset - 1;
            }
        }
        
        private void OnChangeChunk(Chunk previous, Chunk current)
        {
            if (previous != null)
            {
                previous.onBecomeAccessible -= OnChunkCachedBecomeAccessible;
                previous.onBecomeInaccessible -= OnChunkCachedBecomeInaccessible;
            }

            if (current != null)
            {
                current.onBecomeAccessible += OnChunkCachedBecomeAccessible;
                current.onBecomeInaccessible += OnChunkCachedBecomeInaccessible;
            }
        }
        
        private void InitialiseGearbox()
        {
            ChangeGear(1);
            
            if (canBeDrivenByPlayer)
                OnGearboxSettingChanged(GearboxSetting.Setting);
        }

        private void OnGearboxSettingChanged(GearboxSetting.GearboxOption newValue)
        {
            InputManager.Instance.CarInput.ShiftUp.onPressed -= ShiftUp;
            InputManager.Instance.CarInput.ShiftDown.onPressed -= ShiftDown;
            
            if (newValue == GearboxSetting.GearboxOption.MANUAL)
            {
                InputManager.Instance.CarInput.ShiftUp.onPressed += ShiftUp;
                InputManager.Instance.CarInput.ShiftDown.onPressed += ShiftDown;
            }
        }
        
        private void ShiftUp()
        {
            if (currentGear >= NumberOfGears - 1)
                return; //at max gear;
            
            currentGear++;

            onGearChanged?.Invoke(currentGear - 1, currentGear);
        }

        private void ShiftDown()
        {
            if (currentGear <= 1)
                return; //at lowest gear (excluding reverse)
            
            currentGear--;
            
            onGearChanged?.Invoke(currentGear + 1, currentGear);
        }
        
        private void CheckIfPlayerDriving()
        {
            bool playerDrivingPreviouslyEnabled = isPlayerDrivingEnabled;
            bool playerDrivingShouldBeEnabled = canBeDrivenByPlayer && !autoDrive && WarehouseManager.Instance.CurrentCar == this;

            if (playerDrivingPreviouslyEnabled && !playerDrivingShouldBeEnabled)
            {
                OnPlayerDrivingDisabled();
            } else if (!playerDrivingPreviouslyEnabled && playerDrivingShouldBeEnabled)
            {
                OnPlayerDrivingEnabled();
            }
        }
        
        private void OnPlayerDrivingEnabled()
        {
            isPlayerDrivingEnabled = true;
        }

        private void OnPlayerDrivingDisabled()
        {
            isPlayerDrivingEnabled = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 carCentreOfMassWorld = Rigidbody.transform.TransformPoint(Rigidbody.centerOfMass);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(carCentreOfMassWorld, 0.5f);
        }
#endif
        
    }
}
