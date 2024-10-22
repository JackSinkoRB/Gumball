using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dreamteck.Splines;
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
    public abstract class GameSession : UniqueScriptableObject, ISerializationCallbackReceiver
    {

        public delegate void OnSessionEndDelegate(GameSession session, ProgressStatus progress);
        public static OnSessionEndDelegate onSessionEnd;
        
        public static Action<GameSession> onSessionStart;
        
        public enum ProgressStatus
        {
            NOT_ATTEMPTED,
            ATTEMPTED,
            COMPLETE
        }

        [Header("Info")]
        [SerializeField] private string displayName = "Level";
        [SerializeField] private string description = "Description of session";
        
        [Header("Map setup")]
        [SerializeField] private AddressableSceneReference scene;
        [SerializeField] private AssetReferenceT<ChunkMap> chunkMapAssetReference;
        [SerializeField] private Vector3 vehicleStartingPosition;
        [SerializeField] private Vector3 vehicleStartingRotation;
        
        [Header("Lighting")]
        [Tooltip("Whether or not car night lights, and night-time objects are shown.")]
        [SerializeField] private bool isNightTime;
        [Tooltip("This is directional light intensity value.")]
        [SerializeField] private float globalLightIntensity = 1;
        [Tooltip("This is environment reflections intensity multiplier value that is passed to the environment rendering settings.")]
        [Range(0, 1), SerializeField] private float reflectionIntensity = 1;

        public bool IsNightTime => isNightTime;

        [Header("Session setup")]
        [SerializeField] private float introTime = 3;
        [SerializeField] protected float raceDistanceMetres;
        [SerializeField] private CheckpointMarkers finishLineMarkers;

        [Header("Racers")]
        [SerializeField] protected RacerSessionData[] racerData;
        [Tooltip("Optional: set a race distance. At the end of the distance is the finish line.")]
        [SerializeField] private float racersStartingSpeed = 70;

        [Header("Traffic")]
        [HelpBox("Use the button at the bottom of this component to randomise the traffic, or directly modify in the 'Traffic Spawn Positions' collection below.", MessageType.Info, onlyShowWhenDefaultValue: true)]
        [Tooltip("If enabled, each frame it will check to spawn traffic to keep the desired traffic density designated in the chunks.")]
        [SerializeField] private bool trafficIsProcedural = true;
        [Tooltip("This value represents the number of metres for each car. Eg. A value of 10 means 1 car every 10 metres.")]
        [SerializeField] private int trafficDensity = 100;
        [SerializeField] private AssetReferenceGameObject[] trafficBikes;
        [SerializeField] private AssetReferenceGameObject[] trafficCars;
        [SerializeField] private AssetReferenceGameObject[] trafficTrucks;
        [SerializeField, ConditionalField(nameof(trafficIsProcedural), true)] private CollectionWrapperTrafficSpawnPosition trafficSpawnPositions;

        [Header("Rewards")]
        [SerializeField] private Rewards rewards;

        [Header("Challenges")]
        [SerializeField] private Challenge[] subObjectives;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool inProgress;
        [SerializeField, ReadOnly] private GenericDictionary<AICar, RacerSessionData> currentRacers = new();

        private AsyncOperationHandle<ChunkMap> chunkMapHandle;
        private ChunkMap currentChunkMapCached;
        private Coroutine sessionCoroutine;
        private Dictionary<AssetReferenceGameObject, AsyncOperationHandle> trafficPrefabHandles = new();
        
        private DrivingCameraController drivingCameraController => ChunkMapSceneManager.Instance.DrivingCameraController;

        public ProgressStatus LastProgress { get; private set; }
        public ProgressStatus Progress
        {
            get => DataManager.GameSessions.Get($"SessionStatus.{ID}", ProgressStatus.NOT_ATTEMPTED);
            private set => DataManager.Player.Set($"SessionStatus.{ID}", value);
        }

        public Challenge[] SubObjectives => subObjectives;

        public string DisplayName => displayName;
        public string Description => description;
        public AssetReferenceT<ChunkMap> ChunkMapAssetReference => chunkMapAssetReference;
        public Vector3 VehicleStartingPosition => vehicleStartingPosition;
        public bool InProgress => inProgress;
        public float RaceDistanceMetres => raceDistanceMetres;
        public GenericDictionary<AICar, RacerSessionData> CurrentRacers => currentRacers;
        public Rewards Rewards => rewards;
        public bool HasLoaded { get; private set; }
        public bool HasStarted { get; private set; }
        public bool TrafficIsProcedural => trafficIsProcedural;
        public int TrafficDensity => trafficDensity;
        public AssetReferenceGameObject[] TrafficBikes => trafficBikes;
        public AssetReferenceGameObject[] TrafficCars => trafficCars;
        public AssetReferenceGameObject[] TrafficTrucks => trafficTrucks;
        public TrafficSpawnPosition[] TrafficSpawnPositions => trafficSpawnPositions.Value;
        public ChunkMap CurrentChunkMap => currentChunkMapCached;

        protected abstract GameSessionPanel GetSessionPanel();
        protected abstract SessionEndPanel GetSessionEndPanel();

        public abstract string GetModeDisplayName();
        public abstract Sprite GetModeIcon();
        public abstract ObjectiveUI.FakeChallengeData GetChallengeData();
        public abstract string GetMainObjectiveGoalValue();

        public void StartSession()
        {
            HasLoaded = false;
            HasStarted = false;
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

        protected override void OnValidate()
        {
            base.OnValidate();
            
            TrackCorePartRewards();
            TrackSubPartRewards();

            foreach (RacerSessionData data in racerData)
                data.OnValidate();
        }

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
                
                int desiredCars = Mathf.RoundToInt(chunk.SplineLengthCached / trafficDensity);

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

        private void TrackCorePartRewards()
        {
            foreach (CorePart corePart in rewards.CoreParts)
            {
                if (corePart == null)
                    continue;
                
                corePart.TrackAsReward(this);
            }
            
            foreach (CorePart corePart in previousCorePartRewards)
            {
                if (corePart == null)
                    continue;
                
                if (!rewards.CoreParts.Contains(corePart))
                    corePart.UntrackAsReward(this);
            }
            
            previousCorePartRewards = (CorePart[])rewards.CoreParts.Clone();
        }
        
        private void TrackSubPartRewards()
        {
            foreach (SubPart subPart in rewards.SubParts)
            {
                if (subPart == null)
                    continue;
                
                subPart.TrackAsReward(this);
            }
            
            foreach (SubPart subPart in previousSubPartRewards)
            {
                if (subPart == null)
                    continue;
                
                if (!rewards.SubParts.Contains(subPart))
                    subPart.UntrackAsReward(this);
            }
            
            previousSubPartRewards = (SubPart[])rewards.SubParts.Clone();
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
            
            //reset skill check manager
            SkillCheckManager.Instance.ResetForSession();
        }

        public void EndSession(ProgressStatus progress)
        {
            inProgress = false;
            LastProgress = progress;
            
            if (sessionCoroutine != null)
                CoroutineHelper.Instance.StopCoroutine(sessionCoroutine);
            
            OnSessionEnd();
            
            if (Progress != ProgressStatus.COMPLETE && progress == ProgressStatus.COMPLETE)
            {
                OnCompleteSessionForFirstTime();
            }
            else
            {
                OnFailMission();
            }

            onSessionEnd?.Invoke(this, progress);
        }

        public void UnloadSession()
        {
            GameSessionManager.Instance.SetCurrentSession(null);
            
            if (chunkMapHandle.IsValid())
                Addressables.Release(chunkMapHandle);
            
            StopTrackingObjectives();
            
            Destroy(currentChunkMapCached);
            trafficPrefabHandles.Clear(); //remove the traffic car references so they can be unloaded 
        }
        
        public AsyncOperationHandle GetTrafficVehicleHandle(AssetReferenceGameObject assetReference)
        {
            return trafficPrefabHandles[assetReference];
        }

        protected virtual void OnSessionEnd()
        {
            HasStarted = false;

            if (PanelManager.ExistsRuntime && PanelManager.PanelExists<DrivingControlsPanel>())
                PanelManager.GetPanel<DrivingControlsPanel>().Hide();

            if (PanelManager.PanelExists<DrivingResetButtonPanel>() && PanelManager.GetPanel<DrivingResetButtonPanel>().IsShowing)
                PanelManager.GetPanel<DrivingResetButtonPanel>().Hide(); //hide the reset button

            if (ChunkMapSceneManager.ExistsRuntime)
                drivingCameraController.SetState(drivingCameraController.OutroState);
            
            //disable NOS
            WarehouseManager.Instance.CurrentCar.NosManager.Deactivate();

            WarehouseManager.Instance.CurrentCar.SetAutoDrive(true);

            InputManager.Instance.CarInput.Disable();

            RemoveDistanceCalculators();
            
            //convert skill points to followers
            if (SkillCheckManager.ExistsRuntime)
                FollowersManager.AddFollowers(Mathf.RoundToInt(SkillCheckManager.Instance.CurrentPoints));

            if (GetSessionPanel() != null)
                GetSessionPanel().Hide();
            CoroutineHelper.StartCoroutineOnCurrentScene(ShowSessionEndPanel());
        }

        private IEnumerator ShowSessionEndPanel()
        {
            yield return new WaitForSeconds(1f);
            
            if (PanelManager.PanelExists<VignetteBackgroundPanel>())
                PanelManager.GetPanel<VignetteBackgroundPanel>().Show();
            
            yield return new WaitForSeconds(1f);
            
            if (PanelManager.PanelExists<RetrySessionButtonPanel>())
                PanelManager.GetPanel<RetrySessionButtonPanel>().Show();
            
            if (GetSessionEndPanel() != null)
                GetSessionEndPanel().Show();
        }

        public virtual void UpdateWhenCurrent()
        {
            SplineTravelDistanceCalculator playerDistanceCalculator = WarehouseManager.Instance.CurrentCar.GetComponent<SplineTravelDistanceCalculator>();
            if (raceDistanceMetres > 0 && playerDistanceCalculator != null && playerDistanceCalculator.DistanceInMap >= raceDistanceMetres)
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
            inProgress = false;
            
            PanelManager.GetPanel<LoadingPanel>().Show();

            yield return LoadChunkMap();
            yield return LoadScene();
            yield return LoadTrafficVehicles();
            yield return SetupSession();
            
            GlobalLoggers.LoadingLogger.Log("Loading session...");
            yield return LoadSession();

            HasLoaded = true;
            PanelManager.GetPanel<LoadingPanel>().Hide();
            
            InputManager.Instance.CarInput.Enable();
            
            yield return IntroCinematicIE();

            OnSessionStart();
            
            drivingCameraController.SetState(drivingCameraController.CurrentDrivingState);
            
            WarehouseManager.Instance.CurrentCar.SetAutoDrive(false);
            
            //auto accelerate (for non-buttons layout)
            DrivingControlLayoutManager layoutManager = PanelManager.GetPanel<DrivingControlsPanel>().LayoutManager;
            InputManager.Instance.CarInput.Accelerate.SetPressedOverride(layoutManager.CurrentLayout.AutoAccelerate);
        }

        protected virtual void OnSessionStart()
        {
            StartTrackingObjectives();
            
            //only take fuel once session has properly started (in case loading failed)
            FuelManager.Instance.TakeFuel();
            
            onSessionStart?.Invoke(this);
            
            if (GetSessionPanel() != null)
                GetSessionPanel().Show();

            HasStarted = true;
        }

        public void StartTrackingObjectives()
        {
            if (subObjectives == null)
                return;
            
            foreach (Challenge subObjective in subObjectives)
            {
                subObjective.Tracker.StartListening(subObjective.ChallengeID, subObjective.Goal);
            }
        }

        private void StopTrackingObjectives()
        {
            if (subObjectives == null)
                return;
            
            foreach (Challenge subObjective in subObjectives)
            {
                subObjective.Tracker.StopListening(subObjective.ChallengeID);
            }
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
            //
            // //shader adjustments
            // foreach (GameSessionMaterialAdjustment materialAdjustment in materialAdjustments)
            //     materialAdjustment.UpdateMaterial();
            //
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
            PanelManager.GetPanel<DrivingControlsIntroPanel>().Show();
            
            int remainingIntroTime = Mathf.CeilToInt(introTime);
            while (remainingIntroTime > 0)
            {
                PanelManager.GetPanel<SessionIntroPanel>().UpdateCountdownLabel($"{remainingIntroTime}");
                const int timeBetweenCountdownUpdates = 1;
                yield return new WaitForSeconds(timeBetweenCountdownUpdates);
                    
                remainingIntroTime -= timeBetweenCountdownUpdates;
            }
            
            PanelManager.GetPanel<DrivingControlsIntroPanel>().Hide();
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
            
            WarehouseManager.Instance.CurrentCar.SetObeySpeedLimit(false);
            
            //move the car to the right position
            currentCarRigidbody.Move(vehicleStartingPosition, Quaternion.Euler(vehicleStartingRotation));
            GlobalLoggers.LoadingLogger.Log($"Moved vehicle to map's starting position: {vehicleStartingPosition}");
        }
        
        private IEnumerator InitialiseRacers()
        {
            racerData ??= Array.Empty<RacerSessionData>();
                
            currentRacers.Clear();
            AsyncOperationHandle<GameObject>[] handles = new AsyncOperationHandle<GameObject>[racerData.Length];

            for (int index = 0; index < racerData.Length; index++)
            {
                RacerSessionData data = racerData[index];

                if (data.CarAssetReference == null)
                {
                    Debug.LogError($"There is a null racer at index {index} in {name}. Skipping it.");
                    continue;
                }
                
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(data.CarAssetReference);
                handles[index] = handle;
            }

            //add the player's car as a racer
            currentRacers[WarehouseManager.Instance.CurrentCar] = new RacerSessionData();

            yield return new WaitUntil(() => handles.AreAllComplete());

            for (int index = 0; index < racerData.Length; index++)
            {
                if (handles[index].Result == null)
                {
                    Debug.LogError($"There is a null racer at index {index} in {name}. Skipping it.");
                    continue;
                }

                RacerSessionData data = racerData[index];
                AICar racer = Instantiate(handles[index].Result, data.StartingPosition.Position, data.StartingPosition.Rotation).GetComponent<AICar>();
                racer.GetComponent<AddressableReleaseOnDestroy>(true).Init(handles[index]);

                racer.InitialiseAsRacer();
                data.LoadIntoCar(racer);

                currentRacers[racer] = data;
            }

            //set initial speeds
            foreach (AICar racer in currentRacers.Keys)
            {
                racer.SetSpeed(racersStartingSpeed);
            }
        }

        private IEnumerator LoadTrafficVehicles()
        {
            trafficPrefabHandles.Clear();
            
            AssetReferenceGameObject[] allVehicles = (trafficBikes ?? Array.Empty<AssetReferenceGameObject>())
                .Concat(trafficCars ?? Array.Empty<AssetReferenceGameObject>())
                .Concat(trafficTrucks ?? Array.Empty<AssetReferenceGameObject>())
                .ToArray();
            
            foreach (AssetReferenceGameObject assetReference in allVehicles)
            {
                if (assetReference == null)
                {
                    Debug.LogError($"There is a null traffic vehicle in {name}. Skipping it.");
                    continue;
                }

                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
                trafficPrefabHandles[assetReference] = handle;
            }
            
            //wait until all cars have loaded
            yield return new WaitUntil(() => trafficPrefabHandles.Values.AreAllComplete());
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

            finishLineMarkers.Spawn(raceDistanceMetres);
        }

        private void OnCrossFinishLine()
        {
            ProgressStatus status = ProgressStatus.ATTEMPTED;
            if (AreAllSubObjectivesComplete() && IsCompleteOnCrossFinishLine())
                status = ProgressStatus.COMPLETE;
            
            EndSession(status);
        }

        protected virtual bool IsCompleteOnCrossFinishLine()
        {
            return true;
        }

        private bool AreAllSubObjectivesComplete()
        {
            if (subObjectives == null)
                return true;
            
            foreach (Challenge subObjective in subObjectives)
            {
                if (subObjective.Tracker.GetListener(subObjective.ChallengeID).Progress < 1)
                    return false;
            }

            return true;
        }

        private void OnCompleteSessionForFirstTime()
        {
            Progress = ProgressStatus.COMPLETE;
        }
        
        private void OnFailMission()
        {
            Progress = ProgressStatus.ATTEMPTED;
        }
        
    }
}
