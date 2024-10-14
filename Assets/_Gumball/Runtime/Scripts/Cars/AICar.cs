using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
#if UNITY_EDITOR
using Gumball.Editor;
using UnityEditor.SceneManagement;
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

        public delegate void TeleportDelegate(Vector3 previousPosition, Vector3 newPosition); 
        public static event TeleportDelegate onPlayerTeleport;

        public static event Action<AICar> onPerformanceProfileUpdated;

        private enum WheelConfiguration
        {
            REAR_WHEEL_DRIVE,
            FRONT_WHEEL_DRIVE,
            ALL_WHEEL_DRIVE
        }

        private const float dumbDistance = 50;
        private const float timeBetweenCornerChecksWhenDumb = 1;
        
        [Header("Details")]
        [SerializeField] private string displayName;
        [SerializeField] private string makeDisplayName = "Toyota";
        
        public string DisplayName => displayName.IsNullOrEmpty() ? name.Replace("(Clone)", "").Replace("_", " ") : displayName;
        public string MakeDisplayName => makeDisplayName;
        
        [Header("Player car")]
        [SerializeField] private bool canBeDrivenByPlayer;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField] private CarIKManager avatarIKManager;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField] private SteeringWheel steeringWheel;
        
        [Space(5)]
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField] private Transform cockpitCameraTarget;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField] private Transform rearViewCameraTarget;
        
        [Space(5)]
        [SerializeField, ReadOnly] private bool isPlayerDrivingEnabled;
        [ConditionalField(nameof(canBeDrivenByPlayer)), SerializeField, ReadOnly] private int carIndex;

        public CarIKManager AvatarIKManager => avatarIKManager;
        public SteeringWheel SteeringWheel => steeringWheel;
        public int CarIndex => carIndex;
        public string SaveKey => GetSaveKeyFromIndex(carIndex);

        public bool CanBeDrivenByPlayer => canBeDrivenByPlayer;
        public Transform CockpitCameraTarget => cockpitCameraTarget;
        public Transform RearViewCameraTarget => rearViewCameraTarget;

        [Header("Lighting")]
        [SerializeField] private Light[] headlights;
        [SerializeField] private BrakeLights brakelights;
        
        private Tween brakeLightIntensityTween;
        private float desiredBrakeLightIntensity = -1;
        private float currentBrakeLightIntensity;
        
        [Header("Performance settings")]
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private CorePart defaultEngine;
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private CorePart defaultWheels;
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private CorePart defaultDrivetrain;
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private CarPerformanceSettings performanceSettings;
        [Space(5)]
        [SerializeField, ReadOnly(nameof(canBeDrivenByPlayer))] private AnimationCurve torqueCurve;
        [SerializeField, ReadOnly, ConditionalField(nameof(canBeDrivenByPlayer))] private CarPerformanceProfile performanceProfile;
        [Tooltip("This is calculated at runtime only, using the upgrade data.")]
        [SerializeField, ReadOnly, ConditionalField(nameof(canBeDrivenByPlayer))] private PerformanceRatingCalculator currentPerformanceRating;
#if UNITY_EDITOR
        [Space(5)]
        [SerializeField, ReadOnly, ConditionalField(nameof(canBeDrivenByPlayer))] private PerformanceRatingCalculator performanceRatingWithMinProfile;
        [SerializeField, ReadOnly, ConditionalField(nameof(canBeDrivenByPlayer))] private PerformanceRatingCalculator performanceRatingWithMaxProfile;
#endif
        
        public CarPerformanceSettings PerformanceSettings => performanceSettings; 
        public PerformanceRatingCalculator CurrentPerformanceRating => currentPerformanceRating;
        public MinMaxFloat EngineRpmRange => new(performanceSettings.EngineRpmRangeMin.GetValue(performanceProfile), performanceSettings.EngineRpmRangeMax.GetValue(performanceProfile));
        public MinMaxFloat IdealRPMRangeForGearChanges => new(idealRPMPercentForGearChanges.Min * EngineRpmRange.Max, idealRPMPercentForGearChanges.Max * EngineRpmRange.Max);
        public float RigidbodyMass => performanceSettings.RigidbodyMass.GetValue(performanceProfile);
        public float BrakeTorque => performanceSettings.BrakeTorque.GetValue(performanceProfile);
        public float HandbrakeTorque => performanceSettings.HandbrakeTorque.GetValue(performanceProfile);
        public float HandbrakeEaseOffDuration => performanceSettings.HandbrakeEaseOffDuration.GetValue(performanceProfile);
        public float SteerSpeed => performanceSettings.SteerSpeed.GetValue(performanceProfile);
        public float SteerReleaseSpeed => performanceSettings.SteerReleaseSpeed.GetValue(performanceProfile);
        public AnimationCurve MaxSteerAngleCurve => performanceSettings.MaxSteerAngle.GetValue(performanceProfile);
        public float NosDepletionRate => performanceSettings.NosDepletionRate.GetValue(performanceProfile);
        public float NosFillRate => performanceSettings.NosFillRate.GetValue(performanceProfile);
        public float NosTorqueAddition => performanceSettings.NosTorqueAddition.GetValue(performanceProfile);
        
        [Header("Customisation")]
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private CarType carType;
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private CarPartManager carPartManager;
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private BodyPaintModification bodyPaintModification;
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private CarAudioManager carAudioManager;
        [SerializeField, ConditionalField(nameof(canBeDrivenByPlayer))] private RacerIcon racerIcon;
        [Space(5)]
        [Tooltip("This gets added on initialise for every player car.")]
        [SerializeField, ReadOnly, ConditionalField(nameof(canBeDrivenByPlayer))] private NosManager nosManager;
        
        public CarType CarType => carType;
        public CarPartManager CarPartManager => carPartManager;
        public BodyPaintModification BodyPaintModification => bodyPaintModification;
        public CarAudioManager CarAudioManager => carAudioManager;
        public RacerIcon RacerIcon => racerIcon;
        public NosManager NosManager => nosManager;
        
        [Header("Sizing")]
        [SerializeField, ReadOnly] private Vector3 frontOfCarPosition;
        [SerializeField, ReadOnly] private float carWidth;

        public float CarWidth => carWidth;
        public Vector3 FrontOfCarPosition => frontOfCarPosition;
        
        [Header("Wheels")]
        [SerializeField, ReadOnly] private bool isStuck;
        [SerializeField, InitializationField] private WheelConfiguration wheelConfiguration;
        [SerializeField, InitializationField] private WheelMesh[] frontWheelMeshes;
        [SerializeField, InitializationField] private Transform[] frontWheelBrakes;
        [SerializeField, InitializationField] private WheelMesh[] rearWheelMeshes;
        [SerializeField, InitializationField] private Transform[] rearWheelBrakes;
        [SerializeField, InitializationField] private WheelCollider[] frontWheelColliders;
        [SerializeField, InitializationField] private WheelCollider[] rearWheelColliders;
        
        private float timeAcceleratingSinceMovingSlowly;

        private WheelCollider[] poweredWheels;
        private WheelMesh[] allWheelMeshesCached;
        private WheelCollider[] allWheelCollidersCached;
        
        public bool IsStuck => isStuck;
        public WheelMesh[] FrontWheelMeshes => frontWheelMeshes;
        public WheelMesh[] RearWheelMeshes => rearWheelMeshes;
        public WheelCollider[] FrontWheelColliders => frontWheelColliders;
        public WheelCollider[] RearWheelColliders => rearWheelColliders;
        public Transform[] FrontWheelBrakes => frontWheelBrakes;
        public Transform[] RearWheelBrakes => rearWheelBrakes;
        
        public WheelMesh[] AllWheelMeshes
        {
            get
            {
                if (allWheelMeshesCached == null || allWheelMeshesCached.Length == 0 || allWheelMeshesCached[0] == null || allWheelMeshesCached.Length != frontWheelMeshes.Length + rearWheelMeshes.Length)
                    CacheAllWheelMeshes();
                return allWheelMeshesCached; 
            }
        }
        
        public WheelCollider[] AllWheelColliders
        {
            get
            {
                if (allWheelCollidersCached == null || allWheelCollidersCached.Length == 0 || allWheelCollidersCached[0] == null || allWheelCollidersCached.Length != frontWheelColliders.Length + rearWheelColliders.Length)
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
        [ConditionalField(new[]{ nameof(useRacingLine), nameof(autoDrive) }, new[]{ true, false }), SerializeField, ReadOnly] private TrafficLane currentLane;
        [ConditionalField(new[]{ nameof(useRacingLine), nameof(autoDrive) }, new[]{ true, false }), SerializeField, ReadOnly] private float additionalLaneOffset;
        [ConditionalField(new[]{ nameof(useRacingLine), nameof(autoDrive) }, new[]{ true, false }), SerializeField, ReadOnly] private ChunkTrafficManager.LaneDirection currentLaneDirection;
        [Space(5)]
        [Tooltip("An offset may be temporarily applied to the racing line, for example at the start of races.")]
        [ConditionalField(nameof(autoDrive), nameof(useRacingLine)), SerializeField, ReadOnly] private float racingLineOffset;
        
        private float racingLineStartOffset;
        private float racingLineDesiredOffset;
        
        [Header("Drag")]
        [SerializeField] private float dragWhenAccelerating;
        [SerializeField] private float dragWhenIdle = 0.15f;
        [SerializeField] private float dragWhenBraking = 1;
        [SerializeField] private float dragWhenHandbraking = 0.5f;
        [SerializeField] private float angularDragWhenHandbraking = 6;
        
        [Header("Movement")]
        [SerializeField, ReadOnly] private float speedKmh;
        [SerializeField, ReadOnly] private float accelerationKmh;
        private float speedLastFrameKmh;
        private bool wasMovingLastFrame;
        private const float stationarySpeed = 2;
        
        public bool IsStationary => speedKmh < stationarySpeed && !isAccelerating;
        public float SpeedKmh => speedKmh;
        public float AccelerationKmh => accelerationKmh;

        [Header("Engine & Drivetrain")]
        [SerializeField] private float[] gearRatios = { -1.5f, 2.66f, 1.78f, 1.3f, 1, 0.7f, 0.5f };
        [SerializeField] private float finalGearRatio = 3.42f;
        [Tooltip("If RPM goes outside this range, it will try upshift/downshift to the desired RPM.")]
        [SerializeField] private MinMaxFloat idealRPMPercentForGearChanges = new(0.5f, 0.9f);
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
        public bool IsAccelerating => isAccelerating;
        
        [Header("Reversing")]
        [SerializeField] private float maxReverseSpeed = 25;
        [SerializeField, ReadOnly] private bool isReversing;
        
        public bool IsReversing => isReversing;
        
        [Header("Steering")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private float autoDriveMaxSteerAngle = 65;
        [Space(5)]
        [SerializeField, ReadOnly] private float desiredSteerAngle;
        [SerializeField, ReadOnly] private float visualSteerAngle;
        
        /// <summary>
        /// The speed that the wheel mesh is interpolated to the desired steer angle. This is different to the steer speed of the wheel collider.
        /// </summary>
        private const float visualSteerSpeed = 5;

        [Header("Braking")]
        [Tooltip("The minimum distance to check for a corner (if travelling slow).")]
        [SerializeField] private float minCornerReactionDistance = 2;
        [Tooltip("The time that the autodriving car looks ahead for curves to brake. Lowering the time can make it more aggressive around corners, while increasing can make them safer.")]
        [SerializeField] private float cornerReactionTime = 1.1f;
        [Tooltip("When the angle is supplied (x axis), the y axis represents the desired speed.")]
        [ConditionalField(nameof(autoDrive)), SerializeField] private AnimationCurve cornerBrakingCurve;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isBraking;
        [ConditionalField(nameof(autoDrive)), SerializeField, ReadOnly] private float corneringSpeed = Mathf.Infinity;
        [SerializeField, ReadOnly] private float speedToBrakeTo;
        [SerializeField, ReadOnly] private float cornerAngleAhead;
        [SerializeField, ReadOnly] private bool isBrakingForCorner;

        private bool wasBrakingLastFrame;

        public bool IsBraking => isBraking;
        
        [Header("Handbrake")]
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
        [SerializeField, ReadOnly] private List<AICar> collisions = new();
        [SerializeField, ReadOnly] private bool isPushingAnotherRacer;
        [SerializeField, ReadOnly] private List<AICar> racersCollidingWith = new();
        
        public event Action<Collision> onCollisionEnter;
        
        private float timeOfLastCollision = -Mathf.Infinity;
        private BoxCollider movementPathCollider;
        
        public GameObject Colliders => colliders;
        public bool InCollision => collisions.Count > 0;
        
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
        [Space(5)]
        [SerializeField, ReadOnly] private CustomDrivingPath currentRacingLine;
        [ConditionalField(nameof(autoDrive), nameof(useObstacleAvoidance)), SerializeField, ReadOnly] private float obstacleAvoidanceOffset; //show in inspector
        [SerializeField, ReadOnly] private bool allDirectionsAreBlocked;
        
        private static readonly LayerMask obstacleLayers = 1 << (int)LayersAndTags.Layer.TrafficCar
                                                           | 1 << (int)LayersAndTags.Layer.PlayerCar
                                                           | 1 << (int)LayersAndTags.Layer.RacerCar
                                                           | 1 << (int)LayersAndTags.Layer.Barrier
                                                           | 1 << (int)LayersAndTags.Layer.MovementPath
                                                           | 1 << (int)LayersAndTags.Layer.RacerObstacle;
        private static readonly LayerMask obstacleLayersNoCars = 1 << (int)LayersAndTags.Layer.Barrier 
                                                                 | 1 << (int)LayersAndTags.Layer.MovementPath
                                                                 | 1 << (int)LayersAndTags.Layer.RacerObstacle;
        private static readonly LayerMask racingLineObstacleLayers = 1 << (int)LayersAndTags.Layer.Barrier
                                                                     | 1 << (int)LayersAndTags.Layer.RacerObstacle;
        private static readonly LayerMask groundLayers = 1 << (int)LayersAndTags.Layer.Ground
                                                         | 1 << (int)LayersAndTags.Layer.Default
                                                         | 1 << (int)LayersAndTags.Layer.Terrain;
        /// <summary>
        /// The time that the autodriving car looks ahead for curves.
        /// </summary>
        private const float predictedPositionReactionTime = 0.65f;
        
        private Chunk lastKnownChunkForRacingLineOffset;
        private CustomDrivingPath lastKnownRacingLineForRacingLineOffset;
        private readonly RaycastHit[] blockagesTemp = new RaycastHit[10];
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isInitialised;
        [Space(5)]
        [SerializeField, ReadOnly] private Chunk currentChunkCached;
        [SerializeField, ReadOnly] private float timeWithNoChunk;
        [SerializeField, ReadOnly] private bool isFrozen;
        [SerializeField, ReadOnly] private bool isDumb;

        private Vector3 targetPosition;
        private Vector3 cornerTargetPosition;
        private int lastFrameChunkWasCached = -1;
        private (Chunk, Vector3, Quaternion, SplineSample)? targetPos;
        private readonly RaycastHit[] groundedHitsCached = new RaycastHit[10];
        private float timeSinceLastCornerCheck;
        
        private float timeSinceCollision => Time.time - timeOfLastCollision;
        private bool recoveringFromCollision => collisionRecoverDuration > 0 && (InCollision || timeSinceCollision < collisionRecoverDuration);
        private bool faceForward => useRacingLine || currentLaneDirection == ChunkTrafficManager.LaneDirection.FORWARD;
        
        public bool IsRacer => gameObject.layer == (int)LayersAndTags.Layer.RacerCar;
        public bool IsTraffic => gameObject.layer == (int)LayersAndTags.Layer.TrafficCar;
        public bool IsPlayer => gameObject.layer == (int)LayersAndTags.Layer.PlayerCar;
        
        public Rigidbody Rigidbody => GetComponent<Rigidbody>();
        public float DesiredSpeed => tempSpeedLimit >= 0 ? tempSpeedLimit : (isReversing ? maxReverseSpeed : (obeySpeedLimit && CurrentChunk != null ? CurrentChunk.TrafficManager.SpeedLimitKmh : Mathf.Infinity));
        
        public bool IsInAir
        {
            get
            {
                foreach (WheelCollider wheelCollider in WarehouseManager.Instance.CurrentCar.AllWheelColliders)
                {
                    if (wheelCollider.isGrounded)
                        return false;
                }
                return true;
            }
        }
        
        private Chunk lastKnownChunk;

        /// <summary>
        /// The chunk that the car is on or was last on.
        /// </summary>
        public Chunk LastKnownChunk
        {
            get
            {
                FindCurrentChunk();
                return lastKnownChunk;
            }
        }
        
        /// <summary>
        /// The chunk the player is on, else null if it can't be found.
        /// </summary>
        public Chunk CurrentChunk
        {
            get
            {
                FindCurrentChunk();
                return currentChunkCached;
            }
        }

        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
            
            //reset steering
            visualSteerAngle = 0;
            desiredSteerAngle = 0;
            
            InitialiseGearbox();
        }

        private void OnDisable()
        {
            onDisable?.Invoke();
            onDisable = null; //reset listener

            ResetState();
            
            onGearChanged = null; //reset listener
            
            if (InputManager.ExistsRuntime && canBeDrivenByPlayer)
            {
                InputManager.Instance.CarInput.ShiftUp.onPressed -= ShiftUp;
                InputManager.Instance.CarInput.ShiftDown.onPressed -= ShiftDown;
            }
        }

        /// <summary>
        /// Resets the car state when reusing the object.
        /// </summary>
        public void ResetState()
        {
            //reset for pooled objects:
            Unfreeze();
            isAccelerating = false;
            isBraking = false;
            isHandbrakeEngaged = false;
            wasAcceleratingLastFrame = false;
            racersCollidingWith.Clear();
            RemoveTemporarySpeedLimit();
            isStuck = false;
            timeAcceleratingSinceMovingSlowly = 0;
        }

        private void Initialise()
        {
            isInitialised = true;

            defaultAngularDrag = Rigidbody.angularDrag;
            
            OnChangeChunk(null, CurrentChunk);
            
            CachePoweredWheels();
            InitialiseSize();
            CheckToLockRigidbodyRotation();

            //set the wheel mesh positions to match the colliders
            for (int wheelIndex = 0; wheelIndex < AllWheelColliders.Length; wheelIndex++)
                AllWheelMeshes[wheelIndex].transform.position = AllWheelColliders[wheelIndex].transform.position;
        }

        /// <summary>
        /// Freeze the Y rotation if the vehicle only has 2 wheels (eg. a bike)
        /// </summary>
        private void CheckToLockRigidbodyRotation()
        {
            if (AllWheelColliders.Length > 2)
                return; //no need to lock

            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
        }
        
        public void InitialiseAsPlayer(int carIndex)
        {
            this.carIndex = carIndex;
            
            gameObject.layer = (int)LayersAndTags.Layer.PlayerCar;
            colliders.layer = (int)LayersAndTags.Layer.PlayerCar;
            
            SetAutoDrive(false);
            
            if (carPartManager != null)
                carPartManager.Initialise(this);

            if (racerIcon != null)
                racerIcon.gameObject.SetActive(false);

            //load the parts
            CorePartManager.InstallParts(carIndex);

            //construct a new performance profile
            SetPerformanceProfile(new CarPerformanceProfile(carIndex));

            nosManager = transform.GetOrAddComponent<NosManager>();
            nosManager.Initialise(this);

            InitialiseWheelStance();

            OnInitialiseTypeComplete();

            //set wheel colliders as player layer
            foreach (WheelCollider wheelCollider in AllWheelColliders)
                wheelCollider.gameObject.layer = (int)LayersAndTags.Layer.PlayerCar;
        }

        public void InitialiseAsRacer()
        {
            gameObject.layer = (int)LayersAndTags.Layer.RacerCar;
            colliders.layer = (int)LayersAndTags.Layer.RacerCar;

            SetAutoDrive(true);
            SetObeySpeedLimit(false);

            if (racerIcon != null)
                racerIcon.gameObject.SetActive(true);

            InitialiseWheelStance();
            
            OnInitialiseTypeComplete();
        }

        public void InitialiseAsTraffic()
        {
            gameObject.layer = (int)LayersAndTags.Layer.TrafficCar;
            colliders.layer = (int)LayersAndTags.Layer.TrafficCar;
            
            SetAutoDrive(true);
            
            OnInitialiseTypeComplete();
        }

        private void OnInitialiseTypeComplete()
        {
            if (bodyPaintModification != null)
                bodyPaintModification.Initialise(this);
            
            foreach (WheelMesh wheelMesh in AllWheelMeshes)
            {
                WheelPaintModification wheelPaintModification = wheelMesh.GetComponent<WheelPaintModification>();
                if (wheelPaintModification != null)
                    wheelPaintModification.Initialise(this);
            }
            
            if (carAudioManager != null)
                carAudioManager.Initialise(this);
        }
        
        public CorePart GetDefaultPart(CorePart.PartType type)
        {
            return type switch
            {
                CorePart.PartType.ENGINE => defaultEngine,
                CorePart.PartType.WHEELS => defaultWheels,
                CorePart.PartType.DRIVETRAIN => defaultDrivetrain,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public void SetPerformanceProfile(CarPerformanceProfile profile)
        {
            performanceProfile = profile;
            
            //initialise mass
            Rigidbody.mass = RigidbodyMass;
            
            //initialise torque curve
            UpdateTorqueCurve();

            currentPerformanceRating.Calculate(performanceSettings, performanceProfile);

            onPerformanceProfileUpdated?.Invoke(this);
        }

        public void UpdateTorqueCurve(float additionalTorque = 0)
        {
            torqueCurve = performanceSettings.CalculateTorqueCurve(performanceProfile, additionalTorque);
        }
        
        public void SetAutoDrive(bool autoDrive)
        {
            this.autoDrive = autoDrive;
        }

        public void SetSpeed(float speedKmh)
        {
            if (float.IsPositiveInfinity(speedKmh))
                return;
            
            Rigidbody.velocity = SpeedUtils.FromKmhToMs(speedKmh) * transform.forward;
        }
        
        public void SetTemporarySpeedLimit(float speedKmh)
        {
            tempSpeedLimit = speedKmh;
        }

        public void RemoveTemporarySpeedLimit()
        {
            tempSpeedLimit = -1;
        }

        public void SetObeySpeedLimit(bool obey)
        {
            obeySpeedLimit = obey;
        }

        private void CheckToEnableHeadlights()
        {
            bool enable = GameSessionManager.ExistsRuntime
                               && GameSessionManager.Instance.CurrentSession != null
                               && GameSessionManager.Instance.CurrentSession.IsNightTime;

            foreach (Light headlight in headlights)
            {
                if (headlight != null)
                    headlight.gameObject.SetActive(enable);
            }
        }
        
        /// <summary>
        /// Movement should only be called in FixedUpdate, but this can be called manually if simulating.
        /// </summary>
        public void SimulateMovement()
        {
            Move();
            CalculateAcceleration();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!Rigidbody.isKinematic)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }

            Vector3 previousPosition = transform.position;
            
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

            if (WarehouseManager.HasLoaded && WarehouseManager.Instance.CurrentCar == this)
                onPlayerTeleport?.Invoke(previousPosition, position);

            GlobalLoggers.AICarLogger.Log($"Teleported {gameObject.name} to {position}.");
        }

        public void SetGrounded()
        {
            int numberOfHitsDown = Physics.RaycastNonAlloc(transform.position.OffsetY(10000), Vector3.down, groundedHitsCached, Mathf.Infinity, groundLayers);

            if (numberOfHitsDown == 0)
            {
                Debug.LogWarning($"Could not ground car {gameObject.name} because there is no ground above or below.");
                return;
            }
            
            //sort so the tallest hit is first
            Array.Sort(groundedHitsCached, 0, numberOfHitsDown, Comparer<RaycastHit>.Create(
                (hit1, hit2) => hit2.point.y.CompareTo(hit1.point.y)));

            Vector3 offset = groundedHitsCached[0].point - transform.position;

            transform.position += offset;
            Rigidbody.position += offset;
            GlobalLoggers.AICarLogger.Log($"Grounded {gameObject.name} - moved {offset}");
            
            //check to apply ride height - currently only for player cars
            if (IsPlayer)
            {
                float rideHeight = DataManager.Cars.Get<float>($"{SaveKey}.RideHeight");
                transform.position = transform.position.OffsetY(rideHeight);
                Rigidbody.position = Rigidbody.position.OffsetY(rideHeight);

                GlobalLoggers.AICarLogger.Log($"Applied {rideHeight} ride height to {gameObject.name}");
            }
        }

        public void SetCurrentLane(TrafficLane lane, ChunkTrafficManager.LaneDirection direction = ChunkTrafficManager.LaneDirection.NONE, float additionalLaneOffset = float.NaN)
        {
            currentLane = lane;
            
            if (direction != ChunkTrafficManager.LaneDirection.NONE)
                currentLaneDirection = direction;
            
            if (!float.IsNaN(additionalLaneOffset))
                this.additionalLaneOffset = additionalLaneOffset;
        }

        private void Update()
        {
            if (!autoDrive)
                CheckIfStuck();

            CalculateAcceleration();

            CheckToEnableHeadlights();
            if (brakelights != null)
                brakelights.CheckToEnable(this);
        }

        private void FixedUpdate()
        {
            if (!isInitialised)
                return;

            CheckIfNoChunk();

            if (!isFrozen)
            {
                Move();
            }
        }

        private void CheckIfNoChunk()
        {
            if (CurrentChunk != null)
            {
                timeWithNoChunk = 0;
                return;
            }

            //current chunk may have despawned
            const float timeWithNoChunkToDespawn = 1;
            timeWithNoChunk += Time.deltaTime;
            if (timeWithNoChunk >= timeWithNoChunkToDespawn)
            {
                if (IsPlayer && autoDrive)
                    Freeze(); //freeze if at end of map and session has ended
                else
                    Despawn();
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            onCollisionEnter?.Invoke(collision);
            
            //limit the y velocity to prevent car getting flung into the air and keep it more solid on the ground
            const float maxVerticalVelocity = 0.1f;
            if (Rigidbody.velocity.y > maxVerticalVelocity)
                Rigidbody.velocity = Rigidbody.velocity.SetY(maxVerticalVelocity);
            
            AICar car = collision.gameObject.GetComponent<AICar>();
            if (car == null)
                return;

            CheckIfCollidedWithRacer(car, true);

            GlobalLoggers.AICarLogger.Log($"{gameObject.name} collided with {collision.gameObject.name}");

            timeOfLastCollision = Time.time; //reset collision time

            collisions.Add(car);
        }

        private void OnCollisionExit(Collision collision)
        {
            AICar car = collision.gameObject.GetComponent<AICar>();
            if (car == null)
                return;

            CheckIfCollidedWithRacer(car, false);
            
            GlobalLoggers.AICarLogger.Log($"{gameObject.name} stopped colliding with {collision.gameObject.name}");
            
            timeOfLastCollision = Time.time; //reset collision time

            collisions.Remove(car);
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

            foreach (WheelCollider wheelCollider in AllWheelColliders)
                wheelCollider.enabled = false;
        }

        private void Unfreeze()
        {
            isFrozen = false;
            
            Rigidbody.isKinematic = false;
            
            foreach (WheelCollider wheelCollider in AllWheelColliders)
                wheelCollider.enabled = true;
        }

        public void SetRacingLineOffset(float offset)
        {
            racingLineOffset = offset;
        }

        private void SetDumb(bool setEnabled)
        {
            if (setEnabled == isDumb)
                return; //is already set
            
            isDumb = setEnabled;
            GlobalLoggers.AICarLogger.Log($"Set {gameObject.name} dumb to {isDumb}.");
        }
        
        private void CheckIfDumb()
        {
            if (IsPlayer)
                return; //don't ever set player dumb
            
            if (WarehouseManager.Instance.CurrentCar == null)
            {
                SetDumb(true);
                return;
            }

            const float dumbDistanceSqr = dumbDistance * dumbDistance;
            float distanceToPlayerSqr = Vector3.SqrMagnitude(WarehouseManager.Instance.CurrentCar.transform.position - transform.position);
            bool shouldBeDumb = distanceToPlayerSqr > dumbDistanceSqr;
            SetDumb(shouldBeDumb);
        }

        private void Move()
        {
            CheckIfDumb();
            
            TryCreateMovementPathCollider();
            CheckIfPlayerDriving();

            if (autoDrive)
                CheckForCorner();

            CalculateTargetPosition();

            if (!autoDrive || !recoveringFromCollision) //don't update steering angle in collision
                CalculateSteerAngle();

            CheckIfPushingAnotherRacer();

            UpdateBrakingValues();
            DoBrakeEvents();

            CheckForHandbrake();
            CheckToReverse();

            UpdateCurrentGear();
            
            CheckToAccelerate();
            DoAccelerationEvents();
            
            ApplySteering();
            
            if (!isDumb)
                UpdateWheelMeshes();

            CalculateEngineRPM();

            UpdateDrag();

            UpdateMovementPathCollider();
            
            //debug directions:
            Debug.DrawLine(transform.TransformPoint(frontOfCarPosition), targetPosition, 
                isBraking ? Color.red : (isAccelerating ? Color.green : Color.white));
        }

        private void CalculateTargetPosition()
        {
            if (autoDrive)
            {
                if (useObstacleAvoidance)
                    UpdateTargetPositionWithAvoidance();
                else
                    UpdateAutoDriveTargetPosition();
            }
            else
            {
                targetPosition = transform.position + (transform.forward * 100);
            }
        }
        
        private void UpdateAutoDriveTargetPosition()
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
                Vector3 racingLineOffsetVector = transform.right * racingLineOffset;
                targetPosition += racingLineOffsetVector;
            }
        }

        private readonly RaycastHit[] racingLineBlockedTemp = new RaycastHit[1];
        
        private void UpdateTargetPositionWithAvoidance()
        {
            //check if there's a racing line with interpolation distance in current or next chunk
            
            if (CurrentChunk == null)
                return;

            if (GameSessionManager.Instance.CurrentSession == null || !GameSessionManager.Instance.CurrentSession.CurrentRacers.ContainsKey(this))
                return; //active/initialised racers only
            
            CustomDrivingPath nearestRacingLine = null;
            float nearestDistanceSqr = Mathf.Infinity;

            //get the racing lines in the current chunk
            int closestSampleIndexToPlayer = CurrentChunk.GetClosestSampleIndexOnSpline(transform.position).Item1;
            foreach (CustomDrivingPath racingLine in CurrentChunk.TrafficManager.RacingLines)
            {
                if (racingLine == null)
                {
                    Debug.LogError($"There's a missing/null racing line in {CurrentChunk.name}'s TrafficManager.");
                    continue;
                }
                
                if (racingLine.SplineSamples == null || racingLine.SplineSamples.Length == 0)
                    continue;
                
                Vector3 startOfRacingLine = racingLine.SplineSamples[0].position;
                
                //check if blocked
                Vector3 frontOfCar = transform.TransformPoint(frontOfCarPosition);
                Vector3 direction = Vector3.Normalize(startOfRacingLine - frontOfCar);
                int hits = Physics.RaycastNonAlloc(frontOfCar, direction, racingLineBlockedTemp, Vector3.Distance(frontOfCar, startOfRacingLine), racingLineObstacleLayers);
                Debug.DrawRay(frontOfCar, direction * Vector3.Distance(frontOfCar, startOfRacingLine), hits > 0 ? Color.red : Color.blue);
                bool isBlocked = hits > 0;
                if (isBlocked)
                    continue;
                    
                int startOfRacingLineSampleIndex = CurrentChunk.GetClosestSampleIndexOnSpline(startOfRacingLine).Item1;
                if (closestSampleIndexToPlayer >= startOfRacingLineSampleIndex)
                    continue;
                
                float distanceSqr = Vector2.SqrMagnitude(startOfRacingLine.FlattenAsVector2() - transform.position.FlattenAsVector2());
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestRacingLine = racingLine;
                }
            }

            //get the racing lines in the next chunk
            Chunk nextChunk = ChunkManager.Instance.GetNextChunk(CurrentChunk);
            if (nextChunk != null)
            {
                foreach (CustomDrivingPath racingLine in nextChunk.TrafficManager.RacingLines)
                {
                    if (racingLine.SplineSamples == null || racingLine.SplineSamples.Length == 0)
                        continue;
                    
                    Vector3 startOfRacingLine = racingLine.SplineSamples[0].position;
                    
                    //check if blocked
                    Vector3 frontOfCar = transform.TransformPoint(frontOfCarPosition);
                    Vector3 direction = Vector3.Normalize(startOfRacingLine - frontOfCar);
                    int hits = Physics.RaycastNonAlloc(frontOfCar, direction, racingLineBlockedTemp, Vector3.Distance(frontOfCar, startOfRacingLine), racingLineObstacleLayers);
                    Debug.DrawRay(frontOfCar, direction * Vector3.Distance(frontOfCar, startOfRacingLine), hits > 0 ? Color.red : Color.blue);
                    bool isBlocked = hits > 0;
                    if (isBlocked)
                        continue;
                    
                    float distanceSqr = Vector2.SqrMagnitude(startOfRacingLine.FlattenAsVector2() - transform.position.FlattenAsVector2());

                    if (distanceSqr < nearestDistanceSqr)
                    {
                        nearestDistanceSqr = distanceSqr;
                        nearestRacingLine = racingLine;
                    }
                }
            }

            if (nearestRacingLine != null)
            {
                float interpolationDistanceSqr = CurrentChunk.NextRacingLineInterpolateDistance * CurrentChunk.NextRacingLineInterpolateDistance;
                if (nearestDistanceSqr < interpolationDistanceSqr)
                {
                    //is within interpolation distance
                    
                    float percent = 1 - Mathf.Clamp01(nearestDistanceSqr / interpolationDistanceSqr);
                    if (currentRacingLine != nearestRacingLine)
                    {
                        //set start offset
                        racingLineStartOffset = CurrentChunk.SplineSampleCollection.GetOffsetFromSpline(transform.position);
                        //set desired offset
                        racingLineDesiredOffset = GameSessionManager.Instance.CurrentSession.CurrentRacers[this].GetRandomRacingLineImprecision();
                    }
                    
                    currentRacingLine = nearestRacingLine;
                    UpdateAutoDriveTargetPosition();
                    TryAvoidObstacles();
                    
                    //interpolate the racing line offset
                    float offset = Mathf.Lerp(racingLineStartOffset, racingLineDesiredOffset, percent);
                    SetRacingLineOffset(offset);
                    
                    return;
                }
            }
            
            //no racing line to interpolate with - check if it's currently on a racing line instead
            
            if (CurrentChunk.TrafficManager.RacingLines.Length == 0)
            {
                currentRacingLine = null;
                UpdateAutoDriveTargetPosition();
                TryAvoidObstacles();
                UpdateRacingLineOffsetInCurrentChunk();
                return;
            }
            
            foreach (CustomDrivingPath racingLine in CurrentChunk.TrafficManager.RacingLines)
            {
                if (racingLine == null)
                {
                    Debug.LogError($"There's a missing/null racing line in {CurrentChunk.name}'s TrafficManager.");
                    continue;
                }
                
                if (racingLine.SplineSamples.Length == 0)
                    continue;
                
                UpdateAutoDriveTargetPosition();
                
                if (targetPos == null)
                    continue;

                Vector3 endOfRacingLine = racingLine.SplineSamples[^1].position;
                int endOfRacingLineSampleIndex = CurrentChunk.GetClosestSampleIndexOnSpline(endOfRacingLine).Item1;
                int targetPositionSampleIndex = CurrentChunk.GetClosestSampleIndexOnSpline(targetPosition).Item1;
                bool hasPassedRacingLine = targetPositionSampleIndex >= endOfRacingLineSampleIndex;
                if (hasPassedRacingLine)
                    continue;
                
                currentRacingLine = racingLine;
                TryAvoidObstacles();

                if (!allDirectionsAreBlocked)
                {
                    UpdateRacingLineOffsetInCurrentChunk();
                    return;
                }
            }

            //all directions were blocked, try again but this time without cars blocking
            foreach (CustomDrivingPath racingLine in CurrentChunk.TrafficManager.RacingLines)
            {
                if (racingLine.SplineSamples == null || racingLine.SplineSamples.Length == 0)
                    continue;
                
                UpdateAutoDriveTargetPosition();
                
                if (targetPos == null)
                    continue;

                Vector3 endOfRacingLine = racingLine.SplineSamples[^1].position;
                int endOfRacingLineSampleIndex = CurrentChunk.GetClosestSampleIndexOnSpline(endOfRacingLine).Item1;
                int targetPositionSampleIndex = CurrentChunk.GetClosestSampleIndexOnSpline(targetPosition).Item1;
                bool hasPassedRacingLine = targetPositionSampleIndex >= endOfRacingLineSampleIndex;
                if (hasPassedRacingLine)
                    continue;
                
                currentRacingLine = racingLine;
                TryAvoidObstacles(false);

                if (!allDirectionsAreBlocked)
                {
                    UpdateRacingLineOffsetInCurrentChunk();
                    return;
                }
            }
            
            currentRacingLine = null;
            UpdateAutoDriveTargetPosition();
            TryAvoidObstacles();
            UpdateRacingLineOffsetInCurrentChunk();
        }

        private void UpdateRacingLineOffsetInCurrentChunk()
        {
            if (lastKnownChunkForRacingLineOffset != CurrentChunk || lastKnownRacingLineForRacingLineOffset != currentRacingLine)
            {
                if (currentRacingLine == null)
                {
                    //keep the current offset if there's no racing line
                    float distance = CurrentChunk.SplineSampleCollection.GetOffsetFromSpline(transform.position);
                    SetRacingLineOffset(distance);
                }
                else
                {
                    //if using racing line, set the imprecision range
                    float distance = GameSessionManager.Instance.CurrentSession.CurrentRacers[this].GetRandomRacingLineImprecision();
                    SetRacingLineOffset(distance);
                }

                lastKnownChunkForRacingLineOffset = CurrentChunk;
                lastKnownRacingLineForRacingLineOffset = currentRacingLine;
            }
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
            if (engineRpm < IdealRPMRangeForGearChanges.Min && currentGear > 1)
            {
                ShiftDown();
            } else if (engineRpm > IdealRPMRangeForGearChanges.Max && currentGear < NumberOfGears - 1)
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

            float engineRpmUnclamped = EngineRpmRange.Min + averagePoweredWheelRPM * gearRatios[currentGear] * finalGearRatio;
            engineRpm = EngineRpmRange.Clamp(engineRpmUnclamped);
        }

        /// <summary>
        /// Check if colliding with another racer or player and is behind it.
        /// </summary>
        private void CheckIfPushingAnotherRacer()
        {
            if (!IsPlayer && !IsRacer)
                return;
            
            //check if colliding with another racer or player
            //then check if it is behind it

            isPushingAnotherRacer = false;

            HashSet<AICar> racersAlreadyChecked = new();
            
            foreach (AICar racerCollidingWith in racersCollidingWith)
            {
                if (racerCollidingWith == null)
                    continue;
                
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
        /// Checks if the specified position is to the left or right of the player.
        /// </summary>
        public bool IsPositionOnLeft(Vector3 position)
        {
            Vector3 leftOfCar = transform.position - transform.right;
            Vector3 rightOfCar = transform.position + transform.right;

            float distanceToLeftSqr = (leftOfCar - position).sqrMagnitude;
            float distanceToRightSqr = (rightOfCar - position).sqrMagnitude;

            return distanceToLeftSqr < distanceToRightSqr;
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
            if (isBraking && speedKmh > stationarySpeed)
                isAccelerating = false;
            //don't accelerate if in collision (auto drive only)
            else if (autoDrive && recoveringFromCollision)
                isAccelerating = false;
            //can't accelerate if above desired speed
            else if (speedKmh > DesiredSpeed)
                isAccelerating = false;
            else if (DesiredSpeed == 0)
                isAccelerating = false;
            else if (isPushingAnotherRacer)
                isAccelerating = false;

            //check to accelerate
            else
            {
                isAccelerating = autoDrive ||
                                 (isPlayerDrivingEnabled && (InputManager.Instance.CarInput.Accelerate.IsPressed || isReversing || nosManager.IsActivated));
                
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
            if (speedKmh <= 0) 
                return;

            const float speedForMaxAngle = 300;
            float speedPercent = Mathf.Clamp01(speedKmh / speedForMaxAngle);
            float maxSteerAngle = autoDrive ? autoDriveMaxSteerAngle : MaxSteerAngleCurve.Evaluate(speedPercent);

            if (autoDrive)
            {
                Vector3 directionToTarget = targetPosition - transform.position;
                float angle = Mathf.Clamp(-Vector2.SignedAngle(transform.forward.FlattenAsVector2(), directionToTarget.FlattenAsVector2()), -maxSteerAngle, maxSteerAngle);
                desiredSteerAngle = angle;
            }
            else
            {
                float actualSteerSpeed = InputManager.Instance.CarInput.SteeringInput.Approximately(0) ? SteerReleaseSpeed : SteerSpeed;

                float angle = InputManager.Instance.CarInput.SteeringInput * maxSteerAngle;
                desiredSteerAngle = Mathf.LerpAngle(desiredSteerAngle, angle, actualSteerSpeed * Time.deltaTime);
            }
            
            //set the visual steer angle (same for all front wheels)
            const float minSpeedVisualSteerModifier = 20;
            float speedModifier = Mathf.Clamp01(speedKmh / minSpeedVisualSteerModifier); //adjust for low speed
            visualSteerAngle = Mathf.LerpAngle(visualSteerAngle, desiredSteerAngle, visualSteerSpeed * speedModifier * Time.deltaTime);
        }

        private void UpdateBrakingValues()
        {
            //reset for check
            isBraking = false;
            isBrakingForCorner = false;
            speedToBrakeTo = Mathf.Infinity;
            
            if (InCollision && autoDrive && !IsRacer && !IsPlayer)
                return;
            
            if (isReversing)
                return;

            //is speeding over the desired speed? 
            const float speedingLeewayPercent = 0.05f; //the amount the car can speed past the desired speed before needing to brake
            float speedingLeeway = speedingLeewayPercent * DesiredSpeed;
            if (autoDrive && speedKmh > DesiredSpeed + speedingLeeway)
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
            if (autoDrive && speedKmh > corneringSpeed && corneringSpeed < speedToBrakeTo)
            {
                speedToBrakeTo = corneringSpeed;
                isBraking = true;
                isBrakingForCorner = true;
            }

            if (autoDrive && brakeForObstacles && (!isDumb || !IsTraffic))
            {
                float speedPercent = (speedKmh - speedForBrakingRaycastLength.Min) / (speedForBrakingRaycastLength.Max - speedForBrakingRaycastLength.Min);
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
                if (speedKmh > speedToBrakeToIfBlocked && speedToBrakeToIfBlocked < speedToBrakeTo)
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
            if (!isReversing && (IsStationary || speedKmh < 1 || currentGear == 0) && InputManager.Instance.CarInput.Brake.IsPressed)
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

            if (isHandbrakeEngaged)
                OnHandbrakeUpdate();
        }

        private void OnHandbrakeUpdate()
        {
            if (nosManager != null && nosManager.IsActivated)
                nosManager.Deactivate();
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

                wheelCollider.brakeTorque = HandbrakeTorque;
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
                    .Join(DOTween.To(() => Rigidbody.angularDrag, x => Rigidbody.angularDrag = x, defaultAngularDrag, HandbrakeEaseOffDuration))
                    .Join(DOTween.To(() => sidewaysFriction.stiffness, 
                        x => sidewaysFriction.stiffness = x, defaultRearWheelStiffness, HandbrakeEaseOffDuration)
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
            
            foreach (WheelCollider wheelCollider in FrontWheelColliders)
            {
                wheelCollider.brakeTorque = BrakeTorque;
            }
        }
        
        private void OnBrake()
        {
            if (nosManager != null && nosManager.IsActivated)
                nosManager.Deactivate();
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
            //only do corner check periodically for traffic as it's fairly expensive
            if (IsTraffic && isDumb)
            {
                timeSinceLastCornerCheck += Time.deltaTime;
                if (timeSinceLastCornerCheck < timeBetweenCornerChecksWhenDumb)
                    return;
            }
            
            timeSinceLastCornerCheck = 0;
            
            float metresPerSecond = Mathf.Max(minCornerReactionDistance, SpeedUtils.FromKmhToMs(speedKmh));
            float visionDistance = metresPerSecond * cornerReactionTime;

            (Chunk, Vector3, Quaternion, SplineSample)? cornerTargetPos = GetPositionAhead(visionDistance, false);
            if (cornerTargetPos == null)
                return;
            
            var (chunk, targetPosition, targetRotation, splineSample) = cornerTargetPos.Value;
            cornerTargetPosition = targetPosition;
            
            if (useRacingLine)
            {
                //get the offset from the chunk spline
                float distance = CurrentChunk.SplineSampleCollection.GetOffsetFromSpline(transform.position);
                Vector3 racingLineOffsetVector = splineSample.right * distance;
                cornerTargetPosition += racingLineOffsetVector;
            }

            Vector3 directionToTarget = cornerTargetPosition - transform.position;

            cornerAngleAhead = Vector2.Angle(transform.forward.FlattenAsVector2(), directionToTarget.FlattenAsVector2());
            float angleAheadToBrakeTo = Mathf.Max(Mathf.Abs(desiredSteerAngle), cornerAngleAhead);
            
            corneringSpeed = cornerBrakingCurve.Evaluate(angleAheadToBrakeTo);

            Debug.DrawLine(transform.TransformPoint(frontOfCarPosition), cornerTargetPosition, Color.yellow);
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
            
            if (isDumb)
                return;
                
            Vector3 direction = Rigidbody.velocity.sqrMagnitude > 1 ? Rigidbody.velocity : transform.forward;
            float distanceToPredictedPosition = direction.magnitude * predictedPositionReactionTime;
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
            float engineRpmPercent = (engineRpm - EngineRpmRange.Min) / EngineRpmRange.Difference;
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
                StanceModification stanceModification = rearWheelCollider.GetComponent<StanceModification>();

                //apply position and rotation
                rearWheelCollider.GetWorldPose(out Vector3 wheelPosition, out Quaternion wheelRotation);
                rearWheelMesh.transform.position = wheelPosition;
                rearWheelMesh.transform.rotation = wheelRotation;
                
                //set offset
                if (stanceModification != null)
                    rearWheelMesh.transform.localPosition = rearWheelMesh.transform.localPosition.OffsetX(stanceModification.CurrentOffset);
            }

            for (int count = 0; count < frontWheelMeshes.Length; count++)
            {
                WheelMesh frontWheelMesh = frontWheelMeshes[count];
                WheelCollider frontWheelCollider = frontWheelColliders[count];
                StanceModification stanceModification = frontWheelCollider.GetComponent<StanceModification>();
                
                //apply position
                frontWheelCollider.GetWorldPose(out Vector3 wheelPosition, out _);
                frontWheelMesh.transform.position = wheelPosition;

                //rotation is the same as the rear wheel, but with interpolated steer speed
                WheelMesh rearWheelRotation = rearWheelMeshes[count % 2];
                frontWheelMesh.transform.rotation = rearWheelRotation.transform.rotation;
                
                //set the steer amount
                Transform steerPivot = frontWheelMesh.transform.parent;
                steerPivot.transform.position = wheelPosition;
                steerPivot.Rotate(Vector3.up, visualSteerAngle);
                
                //set offset
                if (stanceModification != null)
                    frontWheelMesh.transform.position = frontWheelMesh.transform.TransformPoint(new Vector3(stanceModification.CurrentOffset,0,0));
            }

            //add camber
            foreach (WheelCollider wheelCollider in AllWheelColliders)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                if (stanceModification != null)
                    stanceModification.AddCamberRotation();
            }
            
            UpdateBrakeMeshes();
        }

        private void UpdateBrakeMeshes()
        {
            for (int index = 0; index < frontWheelBrakes.Length; index++)
            {
                Transform brake = frontWheelBrakes[index];
                brake.transform.position = frontWheelMeshes[index].transform.position;
            }
            
            for (int index = 0; index < rearWheelBrakes.Length; index++)
            {
                Transform brake = rearWheelBrakes[index];
                
                brake.transform.position = rearWheelMeshes[index].transform.position;
            }
        }
        
        private void TryAvoidObstacles(bool includeCars = true)
        {
            if (!autoDrive)
                return;
            
            allDirectionsAreBlocked = false;

            Vector3 futurePosition = targetPosition.OffsetY(frontOfCarPosition.y);
            if (TryBoxcast(futurePosition, 0, includeCars))
            {
                SetObstacleAvoidanceOffset(0);
                return;
            }

            float? bestLeftOffset = TryBoxcastsInDirection(false, includeCars);
            float? bestRightOffset = TryBoxcastsInDirection(true, includeCars);

            bool bothFree = bestLeftOffset != null && bestRightOffset != null;
            if (bothFree)
            {
                //get the direction with least angle
                Vector3 potentialTargetPositionLeft = targetPosition + (transform.right * bestLeftOffset.Value);
                Vector3 potentialTargetPositionRight = targetPosition + (transform.right * bestRightOffset.Value);

                Vector3 directionLeft = Vector3.Normalize(potentialTargetPositionLeft - transform.position);
                Vector3 directionRight = Vector3.Normalize(potentialTargetPositionRight - transform.position);

                //if braking for a corner, use the direction that is aiming around the corner instead of closest to the car
                Vector3 cornerTargetDirection = cornerTargetPosition - transform.position;
                Vector3 desiredDirection = isBrakingForCorner ? cornerTargetDirection : transform.forward;
                
                float angleLeft = Vector3.Angle(desiredDirection, directionLeft);
                float angleRight = Vector3.Angle(desiredDirection, directionRight);
                
                SetObstacleAvoidanceOffset(angleLeft < angleRight ? bestLeftOffset.Value : bestRightOffset.Value);
                return;
            }

            bool onlyLeftFree = bestLeftOffset != null;
            if (onlyLeftFree)
            {
                SetObstacleAvoidanceOffset(bestLeftOffset.Value);
                return;
            }

            bool onlyRightFree = bestRightOffset != null;
            if (onlyRightFree)
            {
                SetObstacleAvoidanceOffset(bestRightOffset.Value);
                return;
            }
            
            allDirectionsAreBlocked = true;
        }
        
        /// <returns>True if the boxcast wasn't blocked, or false if it was blocked.</returns>
        private bool TryBoxcast(Vector3 targetPosition, float offset, bool includeCars = true)
        {
            Vector3 startPosition = transform.TransformPoint(frontOfCarPosition);
            Vector3 detectorSize = new Vector3(carWidth / 2f, carWidth / 2f, 0);

            Vector3 finalTargetPosition = targetPosition + (transform.right * offset);
            Vector3 directionToFuturePosition = Vector3.Normalize(finalTargetPosition - startPosition);
            Quaternion rotation = Quaternion.LookRotation(directionToFuturePosition);

            const float sizeModifierByOffset = 0.1f; //the larger the offset, the smaller the distance is
            float distanceToFuturePosition = GetMovementTargetDistance() / (Mathf.Abs((offset) * sizeModifierByOffset) + 1f); //make the length shorter as the offset gets wider
            
            int hits = Physics.BoxCastNonAlloc(startPosition, detectorSize, directionToFuturePosition, blockagesTemp, rotation, distanceToFuturePosition, includeCars ? obstacleLayers : obstacleLayersNoCars);
            
            //if it cannot cross the middle, check if crossing the middle and cancel if so
            if (GameSessionManager.Instance.CurrentSession != null
                && GameSessionManager.Instance.CurrentSession.CurrentRacers.ContainsKey(this)
                && !GameSessionManager.Instance.CurrentSession.CurrentRacers[this].CanCrossMiddle
                && autoDrive && useRacingLine)
            {
                //get cars current direction in relation to the chunk middle
                var (closestSample, closestSampleDistance) = CurrentChunk.GetClosestSampleOnSpline(transform.position);
                bool isCarCurrentlyRight = transform.position.IsFurtherInDirection(closestSample.position, closestSample.right);

                bool isNewPositionRight = finalTargetPosition.IsFurtherInDirection(closestSample.position, closestSample.right);
                
                bool crossingLeft = isCarCurrentlyRight && !isNewPositionRight;
                bool crossingRight = !isCarCurrentlyRight && isNewPositionRight;
                
                if (crossingLeft || crossingRight)
                    return false;
            }
            
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
                        if (IsPlayer && hitCar.IsRacer)
                            continue; //when player is autodriving, don't try avoid racers
                        
                        bool otherCarIsAhead = IsPositionAhead(hit.transform.position + hitCar.frontOfCarPosition);
                        if (!otherCarIsAhead)
                            continue; //don't include if this car is ahead of the other

                        const float percentAllowed = 0.1f;
                        float percentOfSpeed = hitCar.speedKmh * percentAllowed;
                        bool isGoingSlower = speedKmh < hitCar.speedKmh - percentOfSpeed;
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
        private float? TryBoxcastsInDirection(bool isRight, bool includeCars = true)
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

                bool successful = TryBoxcast(futurePosition, offset, includeCars);
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

        private void SetObstacleAvoidanceOffset(float offset)
        {
            obstacleAvoidanceOffset = offset;
            Vector3 offsetVector = transform.right * offset;
            targetPosition += offsetVector;
        }
        
        private float GetMovementTargetDistance()
        {
            const float min = 2;
            float metresPerSecond = Mathf.Max(min, SpeedUtils.FromKmhToMs(speedKmh));
            return metresPerSecond * predictedPositionReactionTime;
        }
        
        private void Despawn()
        {
            gameObject.Pool();
            if (gameObject.IsPooled())
                GlobalLoggers.AICarLogger.Log($"Despawned {gameObject.name} at {transform.position}");
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

        private int GetNumberOfPoweredWheels()
        {
            return wheelConfiguration switch
            {
                WheelConfiguration.ALL_WHEEL_DRIVE => frontWheelColliders.Length + rearWheelColliders.Length,
                WheelConfiguration.FRONT_WHEEL_DRIVE => frontWheelColliders.Length,
                WheelConfiguration.REAR_WHEEL_DRIVE => rearWheelColliders.Length,
                _ => 0
            };
        }
        
        private void CachePoweredWheels()
        {
            poweredWheels = new WheelCollider[GetNumberOfPoweredWheels()];

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
        private (Chunk, Vector3, Quaternion, SplineSample)? GetPositionAhead(float distance, bool canUseRacingLine = true)
        {
            if (CurrentChunk == null)
                return null;
            
            if (CurrentChunk.TrafficManager == null)
            {
                Debug.LogWarning($"A car is on the chunk {CurrentChunk.gameObject.name}, but it doesn't have a traffic manager.");
                return null;
            }
            
            (SplineSample, Chunk)? splineSampleAhead = GetSplineSampleAhead(distance, canUseRacingLine);
            if (splineSampleAhead == null)
                return null; //no more chunks loaded
            
            //get the lane distance
            float laneDistance = additionalLaneOffset;
            if (currentLane.Type == TrafficLane.LaneType.DISTANCE_FROM_CENTER)
                laneDistance += currentLane.DistanceFromCenter;
            
            //get the lane position
            PositionAndRotation lanePosition = CurrentChunk.TrafficManager.GetLanePosition(splineSampleAhead.Value.Item1, laneDistance, currentLaneDirection);
            return (splineSampleAhead.Value.Item2, lanePosition.Position, lanePosition.Rotation, splineSampleAhead.Value.Item1);
        }

        /// <summary>
        /// Gets the spline sample that is 'distance' metres away from the closest sample.
        /// </summary>
        public (SplineSample, Chunk)? GetSplineSampleAhead(float desiredDistance, bool canUseRacingLine = true)
        {
            if (CurrentChunk.TrafficManager == null)
                return null; //no traffic manager

            float desiredDistanceSqr = desiredDistance * desiredDistance;

            Chunk chunkToUse = CurrentChunk;
            int chunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunkToUse);
            
            bool isChunkLoaded = chunkIndex >= 0;
            if (!isChunkLoaded)
                return null; //current chunk isn't loaded

            bool isUsingRacingLine = canUseRacingLine && useRacingLine && currentRacingLine != null;
            SampleCollection sampleCollection = isUsingRacingLine ? currentRacingLine.SampleCollection : CurrentChunk.SplineSampleCollection;

            //get the closest sample, then get the next, and next, until it is X distance away from the closest
            int closestSplineIndex = sampleCollection.GetClosestSampleIndexOnSpline(transform.TransformPoint(frontOfCarPosition)).Item1;
            
            //racing line only: handle case if racing line ends before chunk ends
            if (isUsingRacingLine && closestSplineIndex == sampleCollection.length - 1)
            {
                sampleCollection = CurrentChunk.SplineSampleCollection; //already passed the last sample
                closestSplineIndex = sampleCollection.GetClosestSampleIndexOnSpline(transform.TransformPoint(frontOfCarPosition)).Item1;
            }
            
            //traffic custom path only
            if (currentLane.Type == TrafficLane.LaneType.CUSTOM_SPLINE && closestSplineIndex != sampleCollection.length - 1)
            {
                sampleCollection = currentLane.Path.SampleCollection;
                closestSplineIndex = sampleCollection.GetClosestSampleIndexOnSpline(transform.TransformPoint(frontOfCarPosition)).Item1;
            }

            SplineSample closestSampleToCar = sampleCollection.samples[closestSplineIndex];

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
                    
                    sampleCollection = chunkToUse.SplineSampleCollection;

                    //for traffic cars, update the lane with the closest one on the new chunk
                    if (!useRacingLine)
                    {
                        TrafficLane closestLane = GetClosestLaneToCurrentLane(chunkToUse);
                        if (closestLane != null) //may be null if there are no lanes in the chunk in the current direction
                        {
                            SetCurrentLane(closestLane);

                            if (closestLane.Type == TrafficLane.LaneType.CUSTOM_SPLINE)
                                sampleCollection = closestLane.Path.SampleCollection;
                        }
                    }
                    
                    //reset the values
                    previousSample = null;
                    closestSplineIndex = faceForward ? 0 : sampleCollection.samples.Length - 1;
                    offset = faceForward ? 1 : -1;
                    continue;
                }
                
                SplineSample sample = sampleCollection.samples[closestSplineIndex + offset];
                float distanceToSampleSqr = Vector3.SqrMagnitude(sample.position - closestSampleToCar.position);
                float distanceOffset = Mathf.Abs(desiredDistanceSqr - distanceToSampleSqr);
                
                bool isFurtherAway = previousSample != null && distanceOffset > previousDistanceOffset;
                if (isFurtherAway)
                    return (previousSample.Value, chunkToUse);
                
                previousDistanceOffset = distanceOffset;
                previousSample = sample;
                
                offset = faceForward ? offset + 1 : offset - 1;
            }
        }
        
        private void FindCurrentChunk()
        {
            if (lastFrameChunkWasCached == Time.frameCount)
                return; //only update once per frame
            
            lastFrameChunkWasCached = Time.frameCount;

            Chunk previousChunk = currentChunkCached;
                    
            //raycast down to terrain
            const float offset = 500;
            currentChunkCached = Physics.Raycast(transform.position.OffsetY(offset), Vector3.down, out RaycastHit hitDown, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.ChunkDetector))
                ? hitDown.transform.parent.GetComponent<Chunk>()
                : null;
                    
            if (currentChunkCached != previousChunk)
                OnChangeChunk(previousChunk, currentChunkCached);
        }
        
        private void OnChangeChunk(Chunk previous, Chunk current)
        {
            if (current != null)
                lastKnownChunk = current;
            
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

        private TrafficLane GetClosestLaneToCurrentLane(Chunk chunk)
        {
            //choose the closest lane to the current lane offset in the new chunk
            float currentLaneDistance = currentLane.GetDistanceFromCenter(this);
                    
            TrafficLane[] lanes = currentLaneDirection == ChunkTrafficManager.LaneDirection.FORWARD ? chunk.TrafficManager.LanesForward : chunk.TrafficManager.LanesBackward;
            float closestLaneDifference = Mathf.Infinity; //lane distance is the distance from the lane to the center of the chunk
            TrafficLane closestLane = null;
                    
            foreach (TrafficLane lane in lanes)
            {
                float laneDifference = Mathf.Abs(lane.GetDistanceFromCenter(this) - currentLaneDistance);

                if (laneDifference < closestLaneDifference)
                {
                    closestLane = lane;
                    closestLaneDifference = laneDifference;
                }
            }

            return closestLane;
        }
        
        private void InitialiseGearbox()
        {
            ChangeGear(1);
        }

        /// <summary>
        /// Calculates the front of the car position and the car width.
        /// </summary>
        private void InitialiseSize()
        {
            carWidth = 0;
            float furthestDistanceRight = 0;
            
            frontOfCarPosition = Vector3.zero;
            float furthestDistanceForward = 0;
            
            foreach (Collider collider in colliders.GetComponents<Collider>())
            {
                if (!collider.enabled)
                    continue;
                
                const float reallyFarAway = 1000;
                
                Vector3 positionForward = collider.ClosestPoint(transform.position + transform.forward * reallyFarAway);
                Vector3 localPositionForward = transform.InverseTransformPoint(positionForward).SetXY(0, Rigidbody.centerOfMass.y);
                float distanceForward = localPositionForward.z;
                if (distanceForward > furthestDistanceForward)
                {
                    frontOfCarPosition = localPositionForward;
                    furthestDistanceForward = distanceForward;
                }
                
                Vector3 positionRight = collider.ClosestPoint(transform.position + transform.right * reallyFarAway);
                Vector3 localPositionRight = transform.InverseTransformPoint(positionRight);
                float distanceRight = localPositionRight.x;
                if (distanceRight > furthestDistanceRight)
                {
                    furthestDistanceRight = distanceRight;
                }
            }
            
            //complete after to avoid square rooting each time
            carWidth = furthestDistanceRight * 2; //multiply by 2 for total width
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
            if (!IsRacer && !IsPlayer)
                return;
            
            if (!car.IsRacer && !car.IsPlayer)
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

        private bool ShouldBeStuck()
        {
            const float minSpeedForStuckKmh = 1f;
            if (speedKmh > minSpeedForStuckKmh)
            {
                timeAcceleratingSinceMovingSlowly = 0;
                return false;
            }

            if (IsAccelerating || IsReversing)
                timeAcceleratingSinceMovingSlowly += Time.deltaTime;
            
            const float timeAcceleratingWithNoMovementForReset = 0.5f;
            if (timeAcceleratingSinceMovingSlowly > timeAcceleratingWithNoMovementForReset)
                return true;

            //if one of the powered wheels is grounded, it is not stuck
            foreach (WheelCollider poweredWheel in poweredWheels)
            {
                if (poweredWheel.isGrounded)
                    return false;
            }
            return true;
        }
        
        private void CheckIfStuck()
        {
            bool shouldBeStuck = ShouldBeStuck();
            
            if (isStuck && !shouldBeStuck)
                isStuck = false;
            else if (!isStuck && shouldBeStuck)
                isStuck = true;
        }
        
        private void CalculateAcceleration()
        {
            speedKmh = SpeedUtils.FromMsToKmh(Rigidbody.velocity.magnitude);
            accelerationKmh = speedKmh - speedLastFrameKmh;
            
            speedLastFrameKmh = speedKmh;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 carCentreOfMassWorld = Rigidbody.transform.TransformPoint(Rigidbody.centerOfMass);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(carCentreOfMassWorld, 0.5f);
        }
#endif

        private void OnValidate()
        {
#if UNITY_EDITOR
            performanceRatingWithMinProfile.Calculate(performanceSettings, new CarPerformanceProfile(0, 0, 0, 0));
            performanceRatingWithMaxProfile.Calculate(performanceSettings, new CarPerformanceProfile(1, 1, 1, 1));
            
            bool prefabIsOpen = PrefabStageUtility.GetPrefabStage(gameObject) != null;
            if (prefabIsOpen)
                WarehouseManager.Instance.UpdateCachedData();
#endif
        }
        
    }
}
