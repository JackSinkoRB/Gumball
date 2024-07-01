using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Gumball
{
    [Serializable]
    public abstract class GameSession : ScriptableObject, ISerializationCallbackReceiver
    {

        private static readonly int LightStrShaderID = Shader.PropertyToID("_Light_Str");

        [Header("Info")]
        [SerializeField] private string description = "Description of session";
        
        [Header("Map setup")]
        [SerializeField] private AddressableSceneReference scene;
        [SerializeField] private AssetReferenceT<ChunkMap> chunkMapAssetReference;
        [SerializeField] private Vector3 vehicleStartingPosition;
        [SerializeField] private Vector3 vehicleStartingRotation;
        
        [Header("Lighting")]
        [Tooltip("This is directional light intensity value.")]
        [SerializeField] private float globalLightIntensity = 1;
        [Tooltip("This is environment reflections intensity multiplier value that is passed to the environment rendering settings.")]
        [Range(0, 1), SerializeField] private float reflectionIntensity = 1;
        [Tooltip("This is the value that is passed to the shader for fake lighting using the alpha channel.")]
        [Range(0, 30), SerializeField] private float fakeLightingIntensity;

        [Header("Session setup")]
        [SerializeField] private float introTime = 3;
        [SerializeField] protected float raceDistanceMetres;

        [Header("Racers")]
        [SerializeField] private RacerSessionData[] racerData;
        [Tooltip("Optional: set a race distance. At the end of the distance is the finish line.")]
        [SerializeField] private float racersStartingSpeed = 70;

        [Header("Traffic")]
        [HelpBox("Use the button at the bottom of this component to randomise the traffic, or directly modify in the list below.", MessageType.Info, true)]
        [Tooltip("If enabled, each frame it will check to spawn traffic to keep the desired traffic density designated in the chunks.")]
        [SerializeField] private bool trafficIsProcedural = true;
        [Tooltip("This value represents the number of metres for each car. Eg. A value of 10 means 1 car every 10 metres.")]
        [SerializeField] private int trafficDensity = 100;
        [SerializeField] private AICar[] trafficBikes;
        [SerializeField] private AICar[] trafficCars;
        [SerializeField] private AICar[] trafficTrucks;
        [SerializeField, ConditionalField(nameof(trafficIsProcedural), true)] private CollectionWrapperTrafficSpawnPosition trafficSpawnPositions;

        [Header("Rewards")]
        [SerializeField, DisplayInspector] private CorePart[] corePartRewards = Array.Empty<CorePart>();
        [SerializeField, DisplayInspector] private SubPart[] subPartRewards = Array.Empty<SubPart>();

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool inProgress;
        [SerializeField, ReadOnly] private GenericDictionary<AICar, RacerSessionData> currentRacers = new();

        private AsyncOperationHandle<ChunkMap> chunkMapHandle;
        private ChunkMap currentChunkMapCached;
        private Coroutine sessionCoroutine;
        
        private DrivingCameraController drivingCameraController => ChunkMapSceneManager.Instance.DrivingCameraController;

        public string Description => description;
        public AssetReferenceT<ChunkMap> ChunkMapAssetReference => chunkMapAssetReference;
        public Vector3 VehicleStartingPosition => vehicleStartingPosition;
        public bool InProgress => inProgress;
        public float RaceDistanceMetres => raceDistanceMetres;
        public GenericDictionary<AICar, RacerSessionData> CurrentRacers => currentRacers;
        public CorePart[] CorePartRewards => corePartRewards;
        public SubPart[] SubPartRewards => subPartRewards;
        public bool HasStarted { get; private set; }
        public bool TrafficIsProcedural => trafficIsProcedural;
        public int TrafficDensity => trafficDensity;
        public AICar[] TrafficBikes => trafficBikes;
        public AICar[] TrafficCars => trafficCars;
        public AICar[] TrafficTrucks => trafficTrucks;
        public TrafficSpawnPosition[] TrafficSpawnPositions => trafficSpawnPositions.Value;
        public ChunkMap CurrentChunkMap => currentChunkMapCached;
        
        public abstract string GetName();

        public void StartSession()
        {
            GameSessionManager.Instance.SetCurrentSession(this);
            sessionCoroutine = CoroutineHelper.Instance.StartCoroutine(StartSessionIE());
        }
        
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (scene != null && scene.IsDirty)
            {
                EditorUtility.SetDirty(this);
                scene.SetDirty(false);
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            
        }
        
#if UNITY_EDITOR
        [SerializeField, HideInInspector] private CorePart[] previousCorePartRewards = Array.Empty<CorePart>();
        [SerializeField, HideInInspector] private SubPart[] previousSubPartRewards = Array.Empty<SubPart>();

        private void OnValidate()
        {
            TrackCorePartRewards();
            TrackSubPartRewards();
        }

#if UNITY_EDITOR
        [ButtonMethod(ButtonMethodDrawOrder.AfterInspector, nameof(trafficIsProcedural), true)]
        public void RandomiseTraffic()
        {
            List<TrafficSpawnPosition> spawnPositions = new();

            ChunkMap chunkMap = chunkMapAssetReference.editorAsset;
            
            float chunkStartDistance = 0;
            for (int chunkIndex = 0; chunkIndex < chunkMap.ChunkReferences.Length; chunkIndex++)
            {
                AssetReferenceGameObject chunkReference = chunkMap.ChunkReferences[chunkIndex];
                Chunk chunk = chunkReference.editorAsset.gameObject.GetComponent<Chunk>();
                
                float chunkEndDistance = chunkStartDistance + chunk.SplineLengthCached;
                
                int desiredCars = chunk.TrafficManager.NumberOfCarsToSpawn;

                for (int count = 0; count < desiredCars; count++)
                {
                    float randomDistance = Random.Range(chunkStartDistance, chunkEndDistance);
                    
                    ChunkTrafficManager.LaneDirection? randomDirection = chunk.TrafficManager.ChooseRandomLaneDirection();
                    if (randomDirection == null)
                    {
                        Debug.LogError($"Could not spawn traffic car at index {count} because there are no lanes in {chunk.name}.");
                        continue;
                    }
                    
                    TrafficLane[] lanes = randomDirection == ChunkTrafficManager.LaneDirection.FORWARD ? chunk.TrafficManager.LanesForward : chunk.TrafficManager.LanesBackward;
                    int randomLaneIndex = Random.Range(0, lanes.Length);
                    
                    spawnPositions.Add(new TrafficSpawnPosition(randomDistance, randomDirection.Value, randomLaneIndex));
                }

                chunkStartDistance += chunk.SplineLengthCached;
            }

            trafficSpawnPositions = new CollectionWrapperTrafficSpawnPosition();
            trafficSpawnPositions.Value = spawnPositions.ToArray();
        }
#endif
        
        private void TrackCorePartRewards()
        {
            foreach (CorePart corePart in corePartRewards)
            {
                if (corePart == null)
                    continue;
                
                corePart.TrackAsReward(this);
            }
            
            foreach (CorePart corePart in previousCorePartRewards)
            {
                if (corePart == null)
                    continue;
                
                if (!corePartRewards.Contains(corePart))
                    corePart.UntrackAsReward(this);
            }
            
            previousCorePartRewards = (CorePart[])corePartRewards.Clone();
        }
        
        private void TrackSubPartRewards()
        {
            foreach (SubPart subPart in subPartRewards)
            {
                if (subPart == null)
                    continue;
                
                subPart.TrackAsReward(this);
            }
            
            foreach (SubPart subPart in previousSubPartRewards)
            {
                if (subPart == null)
                    continue;
                
                if (!subPartRewards.Contains(subPart))
                    subPart.UntrackAsReward(this);
            }
            
            previousSubPartRewards = (SubPart[])subPartRewards.Clone();
        }
#endif

        public IEnumerator LoadChunkMap()
        {
            //load the map:
            chunkMapHandle = Addressables.LoadAssetAsync<ChunkMap>(chunkMapAssetReference);
            yield return chunkMapHandle;
            
            currentChunkMapCached = Instantiate(chunkMapHandle.Result);
        }
        
        public IEnumerator SetupSession()
        {
            PanelManager.GetPanel<DrivingControlsPanel>().Show();
            
            //setup car:
            WarehouseManager.Instance.CurrentCar.ResetState();
            WarehouseManager.Instance.CurrentCar.gameObject.SetActive(true);
            //start with max NOS
            WarehouseManager.Instance.CurrentCar.NosManager.SetNos(1);
            
            AvatarManager.Instance.HideAvatars(true);

            SetupPlayerCar();

            //load the map chunks
            Stopwatch chunkLoadingStopwatch = Stopwatch.StartNew();
            yield return ChunkManager.Instance.LoadMap(currentChunkMapCached, vehicleStartingPosition);
            chunkLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"Loaded chunks for map in {chunkLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            //set car rigidbody as dynamic
            Rigidbody currentCarRigidbody = WarehouseManager.Instance.CurrentCar.Rigidbody;
            currentCarRigidbody.isKinematic = false;

            //set driving states:
            if (AvatarManager.Instance.DriverAvatar != null && AvatarManager.Instance.CoDriverAvatar != null)
            {
                AvatarManager.Instance.HideAvatars(false);
                AvatarManager.Instance.DriverAvatar.StateManager.SetState<AvatarDrivingState>();
                AvatarManager.Instance.CoDriverAvatar.StateManager.SetState<AvatarDrivingState>();
            }
        }

        public void EndSession()
        {
            inProgress = false;

            if (sessionCoroutine != null)
                CoroutineHelper.Instance.StopCoroutine(sessionCoroutine);
            
            OnSessionEnd();
            
            GameSessionManager.Instance.SetCurrentSession(null);

            if (chunkMapHandle.IsValid())
                Addressables.Release(chunkMapHandle);
        }

        protected virtual void OnSessionEnd()
        {
            HasStarted = false;
            
            PanelManager.GetPanel<DrivingControlsPanel>().Hide();
            
            drivingCameraController.SetState(drivingCameraController.OutroState);
            
            //disable NOS
            WarehouseManager.Instance.CurrentCar.NosManager.Deactivate();
            
            //come to a stop
            WarehouseManager.Instance.CurrentCar.SetTemporarySpeedLimit(0);
            
            InputManager.Instance.CarInput.Disable();

            RemoveDistanceCalculators();
            
            GiveRewards();
        }
        
        public virtual void UpdateWhenCurrent()
        {
            SplineTravelDistanceCalculator playerDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (raceDistanceMetres > 0 && playerDistanceCalculator != null && playerDistanceCalculator.DistanceTraveled >= raceDistanceMetres)
                OnCrossFinishLine();
        }

        protected virtual IEnumerator LoadSession()
        {
            //setup racers
            yield return InitialiseRacers();
            
            //setup finish line
            if (raceDistanceMetres > 0)
            {
                InitialiseRaceMode();
            }
            
            GlobalLoggers.LoadingLogger.Log("Loaded session");

            inProgress = true;
        }

        private IEnumerator StartSessionIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            yield return LoadChunkMap();
            yield return LoadScene();
            yield return SetupSession();
            
            GlobalLoggers.LoadingLogger.Log("Loading session...");
            yield return LoadSession();

            PanelManager.GetPanel<LoadingPanel>().Hide();
            InputManager.Instance.CarInput.Enable();
            
            yield return IntroCinematicIE();

            OnSessionStart();
            
            drivingCameraController.SetState(drivingCameraController.CurrentDrivingState);
            
            WarehouseManager.Instance.CurrentCar.SetAutoDrive(false);
            InputManager.Instance.CarInput.Accelerate.SetPressedOverride(true); //auto accelerate
        }

        protected virtual void OnSessionStart()
        {
            HasStarted = true;
        }

        private IEnumerator LoadScene()
        {
            GlobalLoggers.LoadingLogger.Log("Scene loading started...");
            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            
            yield return Addressables.LoadSceneAsync(scene.Address);
            
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{scene.SceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            SetupLighting();
        }

        private void SetupLighting()
        {
            GlobalLoggers.LoadingLogger.Log("Setting up lighting...");
            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            
            //set the global light
            GameObject directionalLightGameObject = GameObject.Find("Directional Light");
            if (directionalLightGameObject != null)
                directionalLightGameObject.GetComponent<Light>().intensity = globalLightIntensity;
            else 
                Debug.LogError("Could not find directional light in scene. Does it have the name 'Directional Light'?");
            
            //set reflection intensity
            RenderSettings.reflectionIntensity = reflectionIntensity;
            
            //set the fake lighting
            ChunkManager.Instance.TerrainMaterial.SetFloat(LightStrShaderID, fakeLightingIntensity);
            
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{scene.SceneName} lighting setup complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
        }
        
        private IEnumerator IntroCinematicIE()
        {
            if (introTime <= 0)
                yield break;
            
            drivingCameraController.SetState(drivingCameraController.IntroState);
            drivingCameraController.SkipTransition();
                
            //start the transition to driving start
            drivingCameraController.SetState(drivingCameraController.CurrentDrivingState);
                
            WarehouseManager.Instance.CurrentCar.SetAutoDrive(true);

            yield return IntroCountdownIE();
        }
        
        private IEnumerator IntroCountdownIE()
        {
            PanelManager.GetPanel<SessionIntroPanel>().Show();
            
            int remainingIntroTime = Mathf.CeilToInt(introTime);
            while (remainingIntroTime > 0)
            {
                PanelManager.GetPanel<SessionIntroPanel>().UpdateCountdownLabel($"{remainingIntroTime}");
                const int timeBetweenCountdownUpdates = 1;
                yield return new WaitForSeconds(timeBetweenCountdownUpdates);
                    
                remainingIntroTime -= timeBetweenCountdownUpdates;
            }
            
            PanelManager.GetPanel<SessionIntroPanel>().Hide();
        }

        private void SetupPlayerCar()
        {
            //freeze the car
            Rigidbody currentCarRigidbody = WarehouseManager.Instance.CurrentCar.Rigidbody;
            currentCarRigidbody.velocity = Vector3.zero;
            currentCarRigidbody.angularVelocity = Vector3.zero;
            currentCarRigidbody.isKinematic = true;
            
            //remove constraints
            WarehouseManager.Instance.CurrentCar.Rigidbody.constraints = RigidbodyConstraints.None;
            
            //move the car to the right position
            currentCarRigidbody.Move(vehicleStartingPosition, Quaternion.Euler(vehicleStartingRotation));
            GlobalLoggers.LoadingLogger.Log($"Moved vehicle to map's starting position: {vehicleStartingPosition}");
        }
        
        private IEnumerator InitialiseRacers()
        {
            racerData ??= Array.Empty<RacerSessionData>();
                
            currentRacers.Clear();
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

            for (int index = 0; index < racerData.Length; index++)
            {
                RacerSessionData data = racerData[index];

                if (data.AssetReference == null)
                {
                    Debug.LogError($"There is a null racer at index {index} in {name}. Skipping it.");
                    continue;
                }
                
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(data.AssetReference);

                handle.Completed += h =>
                {
                    if (handle.Result == null)
                    {
                        Debug.LogError($"There is a null racer at index {index} in {name}. Skipping it.");
                        return;
                    }
                    
                    AICar racer = Instantiate(h.Result, data.StartingPosition.Position, data.StartingPosition.Rotation).GetComponent<AICar>();
                    racer.GetComponent<AddressableReleaseOnDestroy>(true).Init(h);

                    racer.InitialiseAsRacer();

                    currentRacers[racer] = data;
                };
                handles.Add(handle);
            }

            //add the player's car as a racer
            currentRacers[WarehouseManager.Instance.CurrentCar] = new RacerSessionData();

            yield return new WaitUntil(() => handles.AreAllComplete());
            
            //set initial speeds
            foreach (AICar racer in currentRacers.Keys)
            {
                racer.SetSpeed(racersStartingSpeed);
            }
        }

        private void InitialiseRaceMode()
        {
            //add distance calculators to racers
            foreach (AICar racer in currentRacers.Keys)
            {
                racer.gameObject.AddComponent<SplineTravelDistanceCalculator>();
            }

            SetupFinishLine();
        }
        
        private void RemoveDistanceCalculators()
        {
            foreach (AICar racer in currentRacers.Keys)
            {
                Destroy(racer.gameObject.GetComponent<SplineTravelDistanceCalculator>());
            }
        }
        
        private void SetupFinishLine()
        {
            float mapLength = ChunkManager.Instance.CurrentChunkMap.TotalLengthMetres;
            if (mapLength < raceDistanceMetres)
            {
                Debug.LogError($"Could not create finish line as the race distance {raceDistanceMetres} is bigger than the map length {mapLength}.");
                return;
            }
            
            //TODO:
        }
        
        private void OnCrossFinishLine()
        {
            EndSession();
        }

        private void GiveRewards()
        {
            if (corePartRewards != null)
            {
                foreach (CorePart corePartReward in corePartRewards)
                {
                    if (!corePartReward.IsUnlocked)
                        RewardManager.GiveReward(corePartReward);
                }
            }

            if (subPartRewards != null)
            {
                foreach (SubPart subPartReward in subPartRewards)
                {
                    if (!subPartReward.IsUnlocked)
                        RewardManager.GiveReward(subPartReward);
                }
            }
        }
        
    }
}
