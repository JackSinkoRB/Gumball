using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            [SerializeField] private float racingLineOffset;

            public AssetReferenceGameObject AssetReference => assetReference;
            public PositionAndRotation StartingPosition => startingPosition;
            public float RacingLineOffset => racingLineOffset;
        }
        
        [SerializeField] private AssetReferenceT<ChunkMap> chunkMapAssetReference;
        [SerializeField] private RacerSessionData[] racerData;
        [Tooltip("Optional: set a race distance. At the end of the distance is the finish line.")]
        [SerializeField] protected float raceDistanceMetres;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool inProgress;
        [Tooltip("The distance traveled along the spline (only tracked if there's a finish line).")]
        [SerializeField, ReadOnly, ConditionalField(nameof(raceDistanceMetres))] protected float splineDistanceTraveled;
        
        /// <summary>
        /// The distance along the spline from the player's starting position to the start of the map. 
        /// </summary>
        private float initialSplineDistance;
        
        private AsyncOperationHandle<ChunkMap> chunkMapHandle;
        private ChunkMap currentChunkMapCached;

        public AssetReferenceT<ChunkMap> ChunkMapAssetReference => chunkMapAssetReference;
        public bool InProgress => inProgress;
        public float SplineDistanceTraveled => splineDistanceTraveled;
        public float RaceDistanceMetres => raceDistanceMetres;
        
        public abstract string GetName();

        public void StartSession()
        {
            GameSessionManager.Instance.SetCurrentSession(this);
            CoroutineHelper.Instance.StartCoroutine(StartSessionIE());
        }

        public IEnumerator LoadChunkMap()
        {
            Debug.LogWarning("Loading chunk map");
            //load the map:
            chunkMapHandle = Addressables.LoadAssetAsync<ChunkMap>(chunkMapAssetReference);
            yield return chunkMapHandle;
            
            currentChunkMapCached = Instantiate(chunkMapHandle.Result);
        }
        
        public IEnumerator SetupSession()
        {
            WarehouseManager.Instance.CurrentCar.gameObject.SetActive(true);

            AvatarManager.Instance.HideAvatars(true);

            SetupPlayerCar(currentChunkMapCached);

            //load the map chunks
            Stopwatch chunkLoadingStopwatch = Stopwatch.StartNew();
            yield return ChunkManager.Instance.LoadMap(currentChunkMapCached);
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

            OnSessionEnd();
            
            GameSessionManager.Instance.SetCurrentSession(null);

            if (chunkMapHandle.IsValid())
                Addressables.Release(chunkMapHandle);
        }

        protected virtual void OnSessionEnd()
        {
            splineDistanceTraveled = 0; //reset
            InputManager.Instance.CarInput.Disable();
        }
        
        public virtual void UpdateWhenCurrent()
        {
            UpdateDistanceTraveled();
        }

        protected virtual IEnumerator LoadSession()
        {
            //setup racers
            if (racerData != null && racerData.Length > 0)
                yield return InitialiseRacers();
            
            //setup finish line
            if (raceDistanceMetres > 0)
            {
                initialSplineDistance = GetSplineDistanceTraveled();
                SetupFinishLine();
            }
            
            GlobalLoggers.LoadingLogger.Log("Loaded session");

            inProgress = true;
            InputManager.Instance.CarInput.Enable();
        }

        private IEnumerator StartSessionIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            yield return LoadChunkMap();
            yield return MapDrivingSceneManager.LoadMapDrivingSceneIE();
            yield return SetupSession();
            
            GlobalLoggers.LoadingLogger.Log("Loading session...");
            yield return LoadSession();

            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

        private void SetupPlayerCar(ChunkMap chunkMap)
        {
            //freeze the car
            Rigidbody currentCarRigidbody = WarehouseManager.Instance.CurrentCar.Rigidbody;
            currentCarRigidbody.velocity = Vector3.zero;
            currentCarRigidbody.angularVelocity = Vector3.zero;
            currentCarRigidbody.isKinematic = true;
            
            //move the car to the right position
            Vector3 startingPosition = chunkMap.VehicleStartingPosition;
            Vector3 startingRotation = chunkMap.VehicleStartingRotation;
            currentCarRigidbody.Move(startingPosition, Quaternion.Euler(startingRotation));
            GlobalLoggers.LoadingLogger.Log($"Moved vehicle to map's starting position: {startingPosition}");
            
            WarehouseManager.Instance.CurrentCar.SetAutoDrive(false);
        }

        private IEnumerator InitialiseRacers()
        {
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
            
            foreach (RacerSessionData data in racerData)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(data.AssetReference);
                handle.Completed += h =>
                {
                    AICar racer = Instantiate(h.Result, data.StartingPosition.Position, data.StartingPosition.Rotation).GetComponent<AICar>();
                    racer.GetComponent<AddressableReleaseOnDestroy>(true).Init(h);
                    
                    racer.InitialiseAsRacer();
                    
                    racer.SetRacingLineOffset(data.RacingLineOffset);
                };
                handles.Add(handle);
            }
            
            yield return new WaitUntil(() => handles.AreAllComplete());
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
        
        private void UpdateDistanceTraveled()
        {
            splineDistanceTraveled = GetSplineDistanceTraveled() - initialSplineDistance;

            if (splineDistanceTraveled >= raceDistanceMetres)
            {
                OnCrossFinishLine();
            }
        }
        
        private void OnCrossFinishLine()
        {
            EndSession();
        }
        
        /// <summary>
        /// Gets the distance along the spline from the start of the map to the closest spline sample to the player.
        /// </summary>
        private float GetSplineDistanceTraveled()
        {
            Chunk currentChunk = ChunkManager.Instance.GetChunkPlayerIsOn();
            if (currentChunk == null || !ChunkManager.Instance.HasLoaded)
                return 0;
            
            //get the distance in the current chunk
            int currentChunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(currentChunk);
            Vector3 playerPosition = WarehouseManager.Instance.CurrentCar.transform.position;
            float distanceInCurrentChunk = currentChunk.GetDistanceTravelledAlongSpline(playerPosition);
            
            //get the distance in previous chunks
            float distanceInPreviousChunks = 0;
            for (int index = 0; index < currentChunkIndex; index++)
            {
                ChunkMapData chunkData = ChunkManager.Instance.CurrentChunkMap.GetChunkData(index);
                distanceInPreviousChunks += chunkData.SplineLength;
            }

            return distanceInCurrentChunk + distanceInPreviousChunks;
        }

    }
}
