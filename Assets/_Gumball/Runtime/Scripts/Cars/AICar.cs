using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dreamteck.Splines;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody), typeof(CarSimulation))]
    public class AICar : MonoBehaviour
    {

        public static string GetSaveKeyFromIndex(int carIndex)
        {
            return $"CarData.{carIndex}";
        }
        
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
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField] private Transform cockpitCameraTarget;
        
        [Space(5)]
        [SerializeField, ReadOnly] private bool isPlayerCar;
        [SerializeField, ReadOnly] private bool isPlayerDrivingEnabled;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField, ReadOnly] private int carIndex;

        public bool IsPlayerCar => isPlayerCar;
        public CarIKManager AvatarIKManager => avatarIKManager;
        public SteeringWheel SteeringWheel => steeringWheel;
        public int CarIndex => carIndex;
        public string SaveKey => GetSaveKeyFromIndex(carIndex);
        
        public Transform CockpitCameraTarget => cockpitCameraTarget;

        [Header("Customisation")]
        [SerializeField] private CarPartManager carPartManager;
        [SerializeField] private BodyPaintModification bodyPaintModification;
        [SerializeField] private PartModification partModification;
        [Tooltip("This gets added on initialise for every player car.")]
        [SerializeField, ReadOnly] private NosManager nosManager;

        public CarPartManager CarPartManager => carPartManager;
        public BodyPaintModification BodyPaintModification => bodyPaintModification;
        public PartModification PartModification => partModification;
        public NosManager NosManager => nosManager;
        
        [Header("Sizing")]
        [SerializeField] private Vector3 frontOfCarPosition = new(0, 1, 2);
        [SerializeField] private float carWidth = 2;
        
        [Header("Wheels")]
        [SerializeField, InitializationField] private WheelConfiguration wheelConfiguration;
        [SerializeField, InitializationField] private WheelMesh[] frontWheelMeshes;
        [SerializeField, InitializationField] private WheelMesh[] rearWheelMeshes;
        [SerializeField, InitializationField] private WheelCollider[] frontWheelColliders;
        [SerializeField, InitializationField] private WheelCollider[] rearWheelColliders;
        [Space(5)]
        private WheelCollider[] poweredWheels;
        private WheelMesh[] allWheelMeshesCached;
        private WheelCollider[] allWheelCollidersCached;

        public WheelMesh[] FrontWheelMeshes => frontWheelMeshes;
        public WheelMesh[] RearWheelMeshes => rearWheelMeshes;
        public WheelCollider[] FrontWheelColliders => frontWheelColliders;
        public WheelCollider[] RearWheelColliders => rearWheelColliders;

        public WheelMesh[] AllWheelMeshes
        {
            get
            {
                if (allWheelMeshesCached == null || allWheelMeshesCached.Length == 0 || allWheelMeshesCached[0] == null)
                    CacheAllWheelMeshes();
                return allWheelMeshesCached; 
            }
        }
        
        public WheelCollider[] AllWheelColliders
        {
            get
            {
                if (allWheelCollidersCached == null || allWheelCollidersCached.Length == 0 || allWheelCollidersCached[0] == null)
                    CacheAllWheelColliders();
                return allWheelCollidersCached;
            }
        }

        [Header("Auto drive")]
        [SerializeField] private bool autoDrive;
        
        [Header("Speed limit")]
        [Tooltip("Does the car obey the current chunks speed limit?")]
        [SerializeField] private bool obeySpeedLimit = true;
        [Tooltip("A speed limit that overrides the max speed to be changed at runtime.")]
        [SerializeField] private float tempSpeedLimit = -1f;
        
        [Header("Lanes")]
        [Tooltip("Does the car try to take the optimal race line, or does it stay in a single (random) lane?")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private bool useRacingLine;
        [ConditionalField(new[]{ nameof(useRacingLine), nameof(autoDrive) }, new[]{ true, false }), SerializeField, ReadOnly] private float currentLaneDistance;
        [ConditionalField(new[]{ nameof(useRacingLine), nameof(autoDrive) }, new[]{ true, false }), SerializeField, ReadOnly] private ChunkTrafficManager.LaneDirection currentLaneDirection;
        [ConditionalField(nameof(autoDrive), nameof(useRacingLine)), SerializeField] private float racingLineOffset;
        [ConditionalField(nameof(autoDrive)), SerializeField] private float targetPositionOffset; //show in inspector
        
        private Tween currentRacingLineOffsetTween;

        public float CurrentLaneDistance => currentLaneDistance;
        
        [Header("Drag")]
        [SerializeField] private float dragWhenAccelerating;
        [SerializeField] private float dragWhenIdle = 0.15f;
        [SerializeField] private float dragWhenBraking = 1;
        [SerializeField] private float dragWhenHandbraking = 0.5f;
        [SerializeField] private float angularDragWhenHandbraking = 6;
        
        [Header("Movement")]
        [SerializeField, ReadOnly] private float speed;
        private bool wasMovingLastFrame;
        private const float stationarySpeed = 2;
        
        public bool IsStationary => speed < stationarySpeed && !isAccelerating;

        [Header("Engine & Drivetrain")]
        [Tooltip("The engine torque output (y) (in Newton metres) compared to the engine RPM (x), between the min and max RPM ranges (where x = 0 is minEngineRpm)")]
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
        public AnimationCurve TorqueCurve => torqueCurve;
        public bool IsAccelerating => isAccelerating;
        
        [Header("Reversing")]
        [SerializeField] private float maxReverseSpeed = 25;
        [SerializeField, ReadOnly] private bool isReversing;
        
        public bool IsReversing => isReversing;
        
        [Header("Steering")]
        [Tooltip("The speed that the wheel collider turns if not auto driving.")]
        [ConditionalField(nameof(autoDrive), true), SerializeField] private float steerSpeed = 2.5f;
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
        
        /// <summary>
        /// The time that the autodriving car looks ahead for curves to brake.
        /// </summary>
        private const float brakingReactionTime = 0.95f;
        
        private bool wasBrakingLastFrame;

        public bool IsBraking => isBraking;
        
        [Header("Handbrake")]
        [SerializeField] private float handbrakeEaseOffDuration = 1f;
        [SerializeField] private float handbrakeTorque = 5000;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isHandbrakeEngaged;
        
        private float defaultRearWheelStiffness = -1;
        private float defaultAngularDrag;
        private readonly Sequence[] handbrakeEaseOffTweens = new Sequence[2];

        public bool IsHandbrakeEngaged => isHandbrakeEngaged;
        
        [Header("Collisions")]
        [SerializeField] private GameObject colliders;
        [SerializeField] private float collisionRecoverDuration = 1;
        [Space(5)]
        [SerializeField, ReadOnly] private bool inCollision;
        [SerializeField, ReadOnly] private int numberOfCollisions;
        [SerializeField, ReadOnly] private bool isPushingAnotherRacer;
        [SerializeField, ReadOnly] private List<AICar> racersCollidingWith = new();
        
        public event Action<Collision> onCollisionEnter;
        
        private float timeOfLastCollision = -Mathf.Infinity;
        private BoxCollider movementPathCollider;
        
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
        [ConditionalField(nameof(autoDrive)), SerializeField] private LayerMask obstacleLayers;
        [Tooltip("The speed the car should brake to if all the directions are blocked (exlcuding the 'when blocked' layers).")]
        [ConditionalField(nameof(useObstacleAvoidance), nameof(autoDrive)), SerializeField] private float speedToBrakeToIfBlocked = 50;

        /// <summary>
        /// The time that the autodriving car looks ahead for curves.
        /// </summary>
        private const float predictedPositionReactionTime = 0.65f;
        
        private readonly RaycastHit[] blockagesTemp = new RaycastHit[10];
        private bool allDirectionsAreBlocked;

        [Header("Value modification")]
        [SerializeField, ReadOnly] private float defaultPeakTorque;
        
        private int[] peakTorqueKeys;

        public float DefaultPeakTorque => defaultPeakTorque;
        private Keyframe peakTorqueKey => torqueCurve.keys[peakTorqueKeys[^1]];
        public float PeakTorque => peakTorqueKey.value;
        public float Horsepower => DynoUtils.CalculateHorsepower(DynoUtils.ConvertNewtonMetresToFootPounds(peakTorqueKey.value), peakTorqueKey.time);
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isInitialised;
        [Space(5)]
        [SerializeField, ReadOnly] private Chunk currentChunkCached;
        [SerializeField, ReadOnly] private bool isFrozen;

        private Vector3 targetPosition;
        private int lastFrameChunkWasCached = -1;
        private (Chunk, Vector3, Quaternion, SplineSample)? targetPos;
        private readonly RaycastHit[] groundedHitsCached = new RaycastHit[1];
        
        private float timeSinceCollision => Time.time - timeOfLastCollision;
        private bool recoveringFromCollision => collisionRecoverDuration > 0 && (inCollision || timeSinceCollision < collisionRecoverDuration);
        private bool faceForward => useRacingLine || currentLaneDirection == ChunkTrafficManager.LaneDirection.FORWARD;
        private bool isRacer => gameObject.layer == (int)LayersAndTags.Layer.RacerCar;
        private bool isPlayer => gameObject.layer == (int)LayersAndTags.Layer.PlayerCar;

        public Rigidbody Rigidbody => GetComponent<Rigidbody>();
        public float Speed => speed;
        public float DesiredSpeed => tempSpeedLimit >= 0 ? tempSpeedLimit : (isReversing ? maxReverseSpeed : (obeySpeedLimit && CurrentChunk != null ? CurrentChunk.TrafficManager.SpeedLimitKmh : Mathf.Infinity));
        
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
            racersCollidingWith.Clear();
            tempSpeedLimit = -1f; //clear the temp speed limit
            
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
            
            OnChangeChunk(null, CurrentChunk);
            
            CachePoweredWheels();
            CachePeakTorque();
        }

        public void InitialiseAsPlayer(int carIndex)
        {
            isPlayerCar = true;
            
            this.carIndex = carIndex;
            
            gameObject.layer = (int)LayersAndTags.Layer.PlayerCar;
            colliders.layer = (int)LayersAndTags.Layer.PlayerCar;
            
            SetAutoDrive(false);
            
            if (carPartManager != null)
                carPartManager.Initialise(this);
            
            if (bodyPaintModification != null)
                bodyPaintModification.Initialise(this);

            if (partModification != null)
                partModification.Initialise(this);

            nosManager = transform.GetOrAddComponent<NosManager>();
            nosManager.Initialise(this);
            
            foreach (WheelMesh wheelMesh in AllWheelMeshes)
            {
                WheelPaintModification wheelPaintModification = wheelMesh.GetComponent<WheelPaintModification>();
                if (wheelPaintModification != null)
                    wheelPaintModification.Initialise(this);
            }
            
            InitialiseWheelStance();
        }

        public void InitialiseAsRacer()
        {
            gameObject.layer = (int)LayersAndTags.Layer.RacerCar;
            colliders.layer = (int)LayersAndTags.Layer.RacerCar;
            
            SetAutoDrive(true);
            
            InitialiseWheelStance();
        }
        
        public void InitialiseAsTraffic()
        {
            gameObject.layer = (int)LayersAndTags.Layer.TrafficCar;
            colliders.layer = (int)LayersAndTags.Layer.TrafficCar;
            
            SetAutoDrive(true);
        }
        
        public void SetAutoDrive(bool autoDrive)
        {
            this.autoDrive = autoDrive;
        }

        public void SetSpeed(float speedKmh)
        {
            Rigidbody.velocity = SpeedUtils.FromKmh(speedKmh) * transform.forward;
        }
        
        public void SetTemporarySpeedLimit(float speedKmh)
        {
            tempSpeedLimit = speedKmh;
        }

        /// <summary>
        /// Movement should only be called in FixedUpdate, but this can be called manually if simulating.
        /// </summary>
        public void SimulateMovement()
        {
            Move();
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

            SetGrounded();
            
            //reset steer angle
            visualSteerAngle = 0;
            desiredSteerAngle = 0;
            
            foreach (WheelCollider wheelCollider in AllWheelColliders)
            {
                wheelCollider.motorTorque = 0;
                wheelCollider.rotationSpeed = 0;
                wheelCollider.steerAngle = 0;
            }
            
            UpdateWheelMeshes(); //force update

            GlobalLoggers.AICarLogger.Log($"Teleported {gameObject.name} to {position}.");
        }

        public void SetGrounded()
        {
            int numberOfHitsDown = Physics.RaycastNonAlloc(transform.position.OffsetY(10000), Vector3.down, groundedHitsCached, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.Ground));

            if (numberOfHitsDown == 0)
            {
                Debug.LogWarning($"Could not ground car {gameObject.name} because there is no ground above or below.");
                return;
            }

            Vector3 offset = groundedHitsCached[0].point - transform.position;

            transform.position += offset;
            Rigidbody.position += offset;
            GlobalLoggers.AICarLogger.Log($"Grounded {gameObject.name} - moved {offset}");
            
            //check to apply ride height - currently only for player cars
            if (isPlayerCar)
            {
                float rideHeight = DataManager.Cars.Get<float>($"{SaveKey}.RideHeight");
                transform.position = transform.position.OffsetY(rideHeight);
                Rigidbody.position = Rigidbody.position.OffsetY(rideHeight);

                GlobalLoggers.AICarLogger.Log($"Applied {rideHeight} ride height to {gameObject.name}");
            }
        }

        public void SetLaneDistance(float laneDistance, ChunkTrafficManager.LaneDirection direction)
        {
            currentLaneDistance = laneDistance;
            currentLaneDirection = direction;
        }
        
        public void SetPeakTorque(float peakTorque)
        {
            foreach (int peakTorqueKeyIndex in peakTorqueKeys)
            {
                Keyframe currentKeyframe = torqueCurve.keys[peakTorqueKeyIndex];
                Keyframe newKeyframe = new Keyframe(currentKeyframe.time, peakTorque, currentKeyframe.inTangent, currentKeyframe.outTangent, currentKeyframe.inWeight, currentKeyframe.outWeight);
                torqueCurve.MoveKey(peakTorqueKeyIndex, newKeyframe);
            }
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
            onCollisionEnter?.Invoke(collision);
            
            AICar car = collision.gameObject.GetComponent<AICar>();
            if (car == null)
                return;

            CheckIfCollidedWithRacer(car, true);

            GlobalLoggers.AICarLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            timeOfLastCollision = Time.time; //reset collision time

            numberOfCollisions++;
            inCollision = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            AICar car = collision.gameObject.GetComponent<AICar>();
            if (car == null)
                return;

            CheckIfCollidedWithRacer(car, false);
            
            GlobalLoggers.AICarLogger.Log($"{gameObject.name} stopped colliding with {collision.gameObject.name}");
            
            timeOfLastCollision = Time.time; //reset collision time

            numberOfCollisions--;
            if (numberOfCollisions == 0)
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

        public void SetRacingLineOffset(float offset, float interpolationDuration = 0)
        {
            currentRacingLineOffsetTween?.Kill();

            if (interpolationDuration == 0)
            {
                racingLineOffset = offset;
                return;
            }

            currentRacingLineOffsetTween = DOTween.To(() => racingLineOffset, x => racingLineOffset = x, offset, interpolationDuration); 
        }
        
        private void Move()
        {
            speed = SpeedUtils.ToKmh(Rigidbody.velocity.magnitude);
            TryCreateMovementPathCollider();
            
            if (autoDrive)
            {
                targetPos = GetPositionAhead(GetMovementTargetDistance());
                if (targetPos == null)
                {
                    Despawn();
                    return;
                }

                targetPosition = targetPos.Value.Item2;

                //check to add racing line offset
                if (useRacingLine)
                {
                    SplineSample targetPosSplineSample = targetPos.Value.Item4;
                    Vector3 racingLineOffsetVector = transform.right * racingLineOffset;
                    targetPosition += racingLineOffsetVector;
                }
            }
            else
            {
                targetPosition = transform.position + (transform.forward * 100);
            }
            
            CheckIfPlayerDriving();
            
            if (useObstacleAvoidance && autoDrive)
                TryAvoidObstacles();
            
            if (!autoDrive || !recoveringFromCollision) //don't update steering angle in collision
                CalculateSteerAngle();
            
            if (autoDrive)
                CheckForCorner();

            CheckIfPushingAnotherRacer();
            
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

            UpdateMovementPathCollider();
            
            //debug directions:
            Debug.DrawLine(transform.position, targetPosition, 
                isBraking ? Color.red : (isAccelerating ? Color.green : Color.white));
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

        /// <summary>
        /// Check if colliding with another racer or player and is behind it.
        /// </summary>
        private void CheckIfPushingAnotherRacer()
        {
            if (!isPlayer && !isRacer)
                return;
            
            //check if colliding with another racer or player
            //then check if it is behind it

            isPushingAnotherRacer = false;

            HashSet<AICar> racersAlreadyChecked = new();
            
            foreach (AICar racerCollidingWith in racersCollidingWith)
            {
                //since the racer may be in the list multiple times (multiple colliders colliding), only do the check once per racer
                if (racersAlreadyChecked.Contains(racerCollidingWith))
                    continue;

                racersAlreadyChecked.Add(racerCollidingWith);
                
                //check if racer is ahead or behind
                if (IsPositionAhead(racerCollidingWith.transform.position + racerCollidingWith.frontOfCarPosition))
                {
                    isPushingAnotherRacer = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Checks if the specified position is ahead of the player (in terms of the direction to the target position).
        /// </summary>
        private bool IsPositionAhead(Vector3 position)
        {
            Vector3 frontOfCar = transform.position + frontOfCarPosition;
            
            bool isAhead = position.IsFurtherInDirection(frontOfCar, transform.forward);
            return isAhead;
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
            else if (speed > DesiredSpeed)
                isAccelerating = false;
            else if (DesiredSpeed == 0)
                isAccelerating = false;
            else if (isPushingAnotherRacer)
                isAccelerating = false;

            //check to accelerate
            else
            {
                isAccelerating = autoDrive ||
                                 (isPlayerDrivingEnabled && (InputManager.Instance.CarInput.Accelerate.IsPressed || isReversing));
                
                //if accelerating from reverse, change the gear
                if (isPlayerDrivingEnabled && InputManager.Instance.CarInput.Accelerate.IsPressed && currentGear == 0)
                    ChangeGear(1);
            }

            CarSimulation carSimulation = GetComponent<CarSimulation>();
            if (carSimulation != null && carSimulation.IsSimulating)
                isAccelerating = true;
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
            if (speed <= 0) 
                return;

            const float speedForMaxAngle = 300;
            float speedPercent = Mathf.Clamp01(speed / speedForMaxAngle);
            float maxSteerAngle = autoDrive ? autoDriveMaxSteerAngle : maxSteerAngleCurve.Evaluate(speedPercent);

            if (autoDrive)
            {
                Vector3 directionToTarget = targetPosition - transform.position;
                float angle = Mathf.Clamp(-Vector2.SignedAngle(Rigidbody.velocity.FlattenAsVector2(), directionToTarget.FlattenAsVector2()), -maxSteerAngle, maxSteerAngle);
                desiredSteerAngle = angle;
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
            
            if (inCollision && autoDrive && !isRacer && !isPlayer)
                return;
            
            if (isReversing)
                return;

            //is speeding over the desired speed? 
            const float speedingLeewayPercent = 5; //the amount the player can speed past the desired speed before needing to brake
            float speedingLeeway = speedingLeewayPercent * DesiredSpeed;
            if (autoDrive && speed > DesiredSpeed + speedingLeeway)
            {
                speedToBrakeTo = DesiredSpeed;
                isBraking = true;
            }

            //check if needs to be stationary
            if (DesiredSpeed <= 0)
            {
                speedToBrakeTo = 0;
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
            
            if (autoDrive && isPushingAnotherRacer)
            {
                isBraking = true;
                speedToBrakeTo = 0;
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
            if (IsStationary)
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
            {
                //for now, autodrive has no reverse, so make sure it is not in reverse
                //might want to handle cases if car is completely blocked ahead and reverse out
                if (isReversing)
                    OnStopReversing();
                return;
            }
            
            if (isReversing && !InputManager.Instance.CarInput.Brake.IsPressed)
                OnStopReversing();
            if (!isReversing && (IsStationary || speed < 1 || currentGear == 0) && InputManager.Instance.CarInput.Brake.IsPressed)
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

        private void OnStartBraking()
        {
            Rigidbody.drag = dragWhenBraking;
            
            foreach (WheelCollider wheelCollider in AllWheelColliders)
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
            
            foreach (WheelCollider wheelCollider in AllWheelColliders)
            {
                wheelCollider.brakeTorque = 0;
            }
        }
        
        private void CheckForCorner()
        {
            const float min = 2;
            float metresPerSecond = Mathf.Max(min, SpeedUtils.FromKmh(speed));
            float visionDistance = metresPerSecond * brakingReactionTime;
            
            var targetPos = GetPositionAhead(visionDistance);
            if (targetPos == null)
                return;
            
            var (chunk, targetPosition, targetRotation, splineSample) = targetPos.Value;
            Vector3 directionToTarget = targetPosition - transform.position;

            float angleAhead = Vector2.Angle(transform.forward.FlattenAsVector2(), directionToTarget.FlattenAsVector2());
            float angleAheadToBrakeTo = Mathf.Max(Mathf.Abs(desiredSteerAngle), angleAhead);
            
            corneringSpeed = cornerBrakingCurve.Evaluate(angleAheadToBrakeTo);
        }

        private void TryCreateMovementPathCollider()
        {
            if (movementPathCollider != null)
                return;
            
            movementPathCollider = new GameObject("MovementPath").AddComponent<BoxCollider>();
            movementPathCollider.transform.SetParent(transform);
            movementPathCollider.gameObject.layer = (int)LayersAndTags.Layer.MovementPath;
            movementPathCollider.isTrigger = true;
        }
        
        private void UpdateMovementPathCollider()
        {
            movementPathCollider.transform.localPosition = frontOfCarPosition;

            float distanceToPredictedPosition = Rigidbody.velocity.magnitude * predictedPositionReactionTime;
            movementPathCollider.size = new Vector3(carWidth, carWidth, distanceToPredictedPosition);
            
            //center is half the size so it points outwards
            movementPathCollider.center = new Vector3(0, 0, movementPathCollider.size.z / 2f);
            
            //rotate towards target position
            Vector3 finalTargetPosition = targetPosition.OffsetY(frontOfCarPosition.y);
            movementPathCollider.transform.LookAt(finalTargetPosition);
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
        public void UpdateWheelMeshes()
        {
            //do rear wheels first as the front wheels require their rotation
            for (int count = 0; count < rearWheelMeshes.Length; count++)
            {
                WheelMesh rearWheelMesh = rearWheelMeshes[count];
                WheelCollider rearWheelCollider = rearWheelColliders[count];
                
                rearWheelCollider.GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);
                rearWheelMesh.transform.position = wheelPosition;
                rearWheelMesh.transform.rotation = wheelRotation;
            }

            for (int count = 0; count < frontWheelMeshes.Length; count++)
            {
                WheelMesh frontWheelMesh = frontWheelMeshes[count];
                WheelCollider frontWheelCollider = frontWheelColliders[count];
                
                frontWheelCollider.GetWorldPose(out Vector3 wheelPosition, out _);
                frontWheelMesh.transform.position = wheelPosition;

                //rotation is the same as the rear wheel, but with interpolated steer speed
                WheelMesh rearWheelRotation = rearWheelMeshes[count];
                frontWheelMesh.transform.rotation = rearWheelRotation.transform.rotation;
                
                //set the steer amount
                Transform steerPivot = frontWheelMesh.transform.parent;
                steerPivot.Rotate(Vector3.up, visualSteerAngle);
            }

            //add camber
            foreach (WheelCollider wheelCollider in AllWheelColliders)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                if (stanceModification != null)
                    stanceModification.AddCamberRotation();
            }
        }

        private void TryAvoidObstacles()
        {
            if (!autoDrive)
                return;
            
            allDirectionsAreBlocked = false;

            Vector3 futurePosition = targetPosition.OffsetY(frontOfCarPosition.y);
            if (TryBoxcast(futurePosition, 0))
            {
                SetTargetPositionOffset(0);
                return;
            }

            float? bestLeftOffset = TryBoxcastsInDirection(false);
            float? bestRightOffset = TryBoxcastsInDirection(true);

            bool bothFree = bestLeftOffset != null && bestRightOffset != null;
            if (bothFree)
            {
                //get the direction with least angle
                Vector3 potentialTargetPositionLeft = targetPosition + (transform.right * bestLeftOffset.Value);
                Vector3 potentialTargetPositionRight = targetPosition + (transform.right * bestRightOffset.Value);

                Vector3 directionLeft = Vector3.Normalize(potentialTargetPositionLeft - transform.position);
                Vector3 directionRight = Vector3.Normalize(potentialTargetPositionRight - transform.position);

                float angleLeft = Vector3.Angle(transform.forward, directionLeft);
                float angleRight = Vector3.Angle(transform.forward, directionRight);
                
                SetTargetPositionOffset(angleLeft < angleRight ? bestLeftOffset.Value : bestRightOffset.Value);
                return;
            }

            bool onlyLeftFree = bestLeftOffset != null;
            if (onlyLeftFree)
            {
                SetTargetPositionOffset(bestLeftOffset.Value);
                return;
            }

            bool onlyRightFree = bestRightOffset != null;
            if (onlyRightFree)
            {
                SetTargetPositionOffset(bestRightOffset.Value);
                return;
            }
            
            allDirectionsAreBlocked = true;
        }
        
        /// <returns>True if the boxcast wasn't blocked, or false if it was blocked.</returns>
        private bool TryBoxcast(Vector3 targetPosition, float offset)
        {
            Vector3 startPosition = transform.TransformPoint(frontOfCarPosition);
            Vector3 detectorSize = new Vector3(carWidth / 2f, carWidth / 2f, 0);

            Vector3 finalTargetPosition = targetPosition + (transform.right * offset);
            Vector3 directionToFuturePosition = Vector3.Normalize(finalTargetPosition - startPosition);
            Quaternion rotation = Quaternion.LookRotation(directionToFuturePosition);

            const float sizeModifierByOffset = 0.1f; //the larger the offset, the smaller the distance is
            float distanceToFuturePosition = GetMovementTargetDistance() / (Mathf.Abs((offset) * sizeModifierByOffset) + 1f); //make the length shorter as the offset gets wider
            
            int hits = Physics.BoxCastNonAlloc(startPosition, detectorSize, directionToFuturePosition, blockagesTemp, rotation, distanceToFuturePosition, obstacleLayers);

            RaycastHit? actualHit = null;
            for (int index = 0; index < hits; index++)
            {
                RaycastHit hit = blockagesTemp[index];

                if (hit.rigidbody != null)
                {
                    bool hitSelf = ReferenceEquals(hit.rigidbody.gameObject, gameObject);
                    if (hitSelf)
                        continue;
                    
                    AICar hitCar = hit.rigidbody.gameObject.GetComponent<AICar>();
                    if (hitCar != null)
                    {
                        if (isPlayer && hitCar.isRacer)
                            continue; //when player is autodriving, don't try avoid racers
                        
                        bool otherCarIsAhead = IsPositionAhead(hit.transform.position + hitCar.frontOfCarPosition);
                        if (!otherCarIsAhead)
                            continue; //don't include if this car is ahead of the other

                        const float percentAllowed = 0.1f;
                        float percentOfSpeed = hitCar.speed * percentAllowed;
                        bool isGoingSlower = speed < hitCar.speed - percentOfSpeed;
                        if (isGoingSlower)
                            continue;
                    }
                }

                //good hit
                actualHit = hit;
                break;
            }
            
#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(startPosition, detectorSize, rotation, directionToFuturePosition, distanceToFuturePosition, actualHit != null ? Color.magenta : Color.gray);
#endif

            return actualHit == null;
        }

        /// <returns>The offset to the left that is unblocked, or -1 if none unblocked.</returns>
        private float? TryBoxcastsInDirection(bool isRight)
        {
            const float distance = 2;
            const float distanceModifier = 0.7f; //as the angle gets larger, the distance offset gets smaller
            const float maxAngle = 35;
            
            float offset = isRight ? distance : -distance;
            float angle = 0;
            int count = 0;
            
            while (angle < maxAngle)
            {
                Vector3 futurePosition = targetPosition.OffsetY(frontOfCarPosition.y);

                bool successful = TryBoxcast(futurePosition, offset);
                if (successful)
                    return offset;
                
                Vector3 potentialTargetPosition = targetPosition + (transform.right * offset);
                Vector3 direction = Vector3.Normalize(potentialTargetPosition - transform.position);
                angle = Vector3.Angle(transform.forward, direction);

                float distanceWithModifier = distance + (count * distanceModifier);
                offset += isRight ? distanceWithModifier : -distanceWithModifier;

                count++;
            }
            
            return null;
        }

        private void SetTargetPositionOffset(float offset)
        {
            targetPositionOffset = offset;
            Vector3 offsetVector = transform.right * offset;
            targetPosition += offsetVector;
        }
        
        private float GetMovementTargetDistance()
        {
            const float min = 2;
            float metresPerSecond = Mathf.Max(min, SpeedUtils.FromKmh(speed));
            return metresPerSecond * predictedPositionReactionTime;
        }
        
        private void Despawn()
        {
            gameObject.Pool();
            GlobalLoggers.AICarLogger.Log($"Despawned at {transform.position}");
        }

        private void CacheAllWheelMeshes()
        {
            int indexCount = 0;
            allWheelMeshesCached = new WheelMesh[frontWheelMeshes.Length + rearWheelMeshes.Length];
            foreach (WheelMesh wheelMesh in frontWheelMeshes)
            {
                allWheelMeshesCached[indexCount] = wheelMesh;
                indexCount++;
            }
            foreach (WheelMesh wheelMesh in rearWheelMeshes)
            {
                allWheelMeshesCached[indexCount] = wheelMesh;
                indexCount++;
            }
        }

        private void CacheAllWheelColliders()
        {
            int indexCount = 0;
            allWheelCollidersCached = new WheelCollider[frontWheelColliders.Length + rearWheelColliders.Length];
            foreach (WheelCollider wheelCollider in frontWheelColliders)
            {
                wheelCollider.gameObject.GetComponent<WheelColliderData>(true);
                
                allWheelCollidersCached[indexCount] = wheelCollider;
                indexCount++;
            }
            foreach (WheelCollider wheelCollider in rearWheelColliders)
            {
                if (defaultRearWheelStiffness < 0)
                    defaultRearWheelStiffness = wheelCollider.sidewaysFriction.stiffness;
                        
                wheelCollider.gameObject.GetComponent<WheelColliderData>(true);

                allWheelCollidersCached[indexCount] = wheelCollider;
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
        private (Chunk, Vector3, Quaternion, SplineSample)? GetPositionAhead(float distance)
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
                return (splineSampleAhead.Value.Item2, splineSampleAhead.Value.Item1.position, splineSampleAhead.Value.Item1.rotation, splineSampleAhead.Value.Item1);
            }
            else
            {
                var (position, rotation) = CurrentChunk.TrafficManager.GetLanePosition(splineSampleAhead.Value.Item1, currentLaneDistance, currentLaneDirection);
                return (splineSampleAhead.Value.Item2, position, rotation, splineSampleAhead.Value.Item1);
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
        
        private void CheckIfCollidedWithRacer(AICar car, bool adding)
        {
            if (!isRacer && !isPlayer)
                return;
            
            if (!car.isRacer && !car.isPlayer)
                return;

            if (car == this)
                return;

            if (adding)
                racersCollidingWith.Add(car);
            else racersCollidingWith.Remove(car);
        }

        private void InitialiseWheelStance()
        {
            //check if the wheels have stance options and initialise
            foreach (WheelCollider wheelCollider in AllWheelColliders)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                if (stanceModification != null)
                    stanceModification.Initialise(this);
            }
        }
        
        private void CachePeakTorque()
        {
            HashSet<int> keys = new();
            
            //set the default peak torque
            float highestTorque = 0;
            foreach (Keyframe key in torqueCurve.keys)
            {
                float torque = key.value;
                if (torque > highestTorque)
                    highestTorque = torque;
            }
            defaultPeakTorque = highestTorque;

            //cache the peak torque keys
            for (int index = 0; index < torqueCurve.keys.Length; index++)
            {
                Keyframe key = torqueCurve.keys[index];
                float torque = key.value;
                if (torque.Approximately(highestTorque, 1))
                    keys.Add(index);
            }

            peakTorqueKeys = keys.ToArray();
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
