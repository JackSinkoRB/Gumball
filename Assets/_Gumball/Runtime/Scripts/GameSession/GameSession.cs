using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    [Serializable]
    public abstract class GameSession : ScriptableObject
    {
        
        [Serializable]
        public struct RacerSessionData
        {
            [SerializeField] private AssetReferenceGameObject assetReference;
            [SerializeField] private PositionAndRotation startingPosition;

            public AssetReferenceGameObject AssetReference => assetReference;
            public PositionAndRotation StartingPosition => startingPosition;
        }
        
        private static readonly int LightStrShaderID = Shader.PropertyToID("_Light_Str");

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
        [SerializeField] private RacerSessionData[] racerData;
        [Tooltip("Optional: set a race distance. At the end of the distance is the finish line.")]
        [SerializeField] protected float raceDistanceMetres;
        [SerializeField] private float racersStartingSpeed = 70;

        [Header("Rewards")]
        [SerializeField, DisplayInspector] private CorePart[] corePartRewards = Array.Empty<CorePart>();
        [SerializeField, DisplayInspector] private SubPart[] subPartRewards = Array.Empty<SubPart>();

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool inProgress;
        [SerializeField, ReadOnly] private AICar[] currentRacers;

        private AsyncOperationHandle<ChunkMap> chunkMapHandle;
        private ChunkMap currentChunkMapCached;
        private Coroutine sessionCoroutine;
        
        private DrivingCameraController drivingCameraController => ChunkMapSceneManager.Instance.DrivingCameraController;
        
        public AssetReferenceT<ChunkMap> ChunkMapAssetReference => chunkMapAssetReference;
        public Vector3 VehicleStartingPosition => vehicleStartingPosition;
        public bool InProgress => inProgress;
        public float RaceDistanceMetres => raceDistanceMetres;
        public AICar[] CurrentRacers => currentRacers;
        
        public abstract string GetName();

        public void StartSession()
        {
            GameSessionManager.Instance.SetCurrentSession(this);
            sessionCoroutine = CoroutineHelper.Instance.StartCoroutine(StartSessionIE());
        }
        
#if UNITY_EDITOR
        [SerializeField, HideInInspector] private CorePart[] previousCorePartRewards = Array.Empty<CorePart>();
        [SerializeField, HideInInspector] private SubPart[] previousSubPartRewards = Array.Empty<SubPart>();

        private void OnValidate()
        {
            TrackCorePartRewards();
            TrackSubPartRewards();
        }

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
            
            WarehouseManager.Instance.CurrentCar.gameObject.SetActive(true);
            
            AvatarManager.Instance.HideAvatars(true);

            SetupPlayerCar(currentChunkMapCached);

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
            PanelManager.GetPanel<DrivingControlsPanel>().Hide();
            
            drivingCameraController.SetState(drivingCameraController.OutroState);
            
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
            InputManager.Instance.CarInput.Enable();
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

            yield return IntroCinematicIE();

            OnSessionStart();
            
            drivingCameraController.SetTarget(WarehouseManager.Instance.CurrentCar.transform);
            drivingCameraController.SetState(drivingCameraController.DrivingState);
            
            WarehouseManager.Instance.CurrentCar.SetAutoDrive(false);

            foreach (AICar racer in currentRacers)
            {
                //tween the racing line offset to 0 for optimal driving
                racer.SetRacingLineOffset(0, 3);
            }
        }

        protected virtual void OnSessionStart()
        {
            
        }

        private IEnumerator LoadScene()
        {
            GlobalLoggers.LoadingLogger.Log("Scene loading started...");
            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            
            yield return Addressables.LoadSceneAsync(scene.SceneName);
            
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
            drivingCameraController.SetTarget(WarehouseManager.Instance.CurrentCar.transform);
            drivingCameraController.SkipTransition();
                
            //start the transition to driving start
            drivingCameraController.SetState(drivingCameraController.DrivingState);
                
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

        private void SetupPlayerCar(ChunkMap chunkMap)
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
                
            currentRacers = new AICar[racerData.Length + 1];
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

            for (int index = 0; index < racerData.Length; index++)
            {
                RacerSessionData data = racerData[index];
                
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(data.AssetReference);
                int finalIndex = index;
                handle.Completed += h =>
                {
                    AICar racer = Instantiate(h.Result, data.StartingPosition.Position, data.StartingPosition.Rotation).GetComponent<AICar>();
                    racer.GetComponent<AddressableReleaseOnDestroy>(true).Init(h);

                    racer.InitialiseAsRacer();

                    //calculate the starting distance
                    racer.PerformAfterTrue(() => racer.CurrentChunk != null, () =>
                    {
                        float distance = racer.CurrentChunk.TrafficManager.GetOffsetFromRacingLine(data.StartingPosition.Position);
                        racer.SetRacingLineOffset(distance);
                    });

                    currentRacers[finalIndex] = racer;
                };
                handles.Add(handle);
            }

            //add the player's car as a racer
            currentRacers[^1] = WarehouseManager.Instance.CurrentCar;

            yield return new WaitUntil(() => handles.AreAllComplete());
            
            //set initial speeds
            foreach (AICar racer in currentRacers)
            {
                racer.SetSpeed(racersStartingSpeed);
            }
        }

        private void InitialiseRaceMode()
        {
            //add distance calculators to racers
            foreach (AICar racer in currentRacers)
            {
                racer.gameObject.AddComponent<SplineTravelDistanceCalculator>();
            }

            SetupFinishLine();
        }
        
        private void RemoveDistanceCalculators()
        {
            foreach (AICar racer in currentRacers)
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
