using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    public class ChunkTrafficManager : MonoBehaviour
    {
        
        /// <summary>
        /// The direction of travel, where forward is moving into chunks with higher index, and backward is moving into chunks with lower index.
        /// </summary>
        public enum LaneDirection
        {
            NONE,
            FORWARD,
            BACKWARD
        }
        
        /// <summary>
        /// The minimum distance around a position required to not have an obstacle in order to spawn a car.
        /// </summary>
        private const float minDistanceRequiredToSpawn = 10f;
        private const float randomLaneOffset = 1f;
        
        [Tooltip("If true, the cars will drive on the left hand side (like Australia). If false, they will drive on the right hand side (like the US).")]
        [SerializeField] private bool driveOnLeft = true;
        [Tooltip("The chunks racing lines. Can have none, one or multiple. If multiple, the first lines will take priority.")]
        [SerializeField] private CustomDrivingPath[] racingLines;
        [SerializeField] private float speedLimitKmh = 40;
        [Tooltip("This value represents the number of metres for each car. Eg. A value of 10 means 1 car every 10 metres.")]
        [SerializeField] private int density = 100;
        
        [Header("Traffic lanes")]
        [SerializeField] private TrafficLane[] lanesForward;
        [SerializeField] private TrafficLane[] lanesBackward;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunk;
        
        private static readonly LayerMask spawnPositionBlockageLayers = 1 << (int)LayersAndTags.Layer.TrafficCar
                                                                        | 1 << (int)LayersAndTags.Layer.PlayerCar
                                                                        | 1 << (int)LayersAndTags.Layer.RacerCar
                                                                        | 1 << (int)LayersAndTags.Layer.Barrier;
        
        private static readonly LayerMask spawnPositionBlockageLayersExcludingPlayer = 1 << (int)LayersAndTags.Layer.TrafficCar
                                                                                       | 1 << (int)LayersAndTags.Layer.RacerCar
                                                                                       | 1 << (int)LayersAndTags.Layer.Barrier;
        
        private readonly Collider[] tempSpawnCheckHolder = new Collider[1];
        
        public TrafficLane[] LanesForward => lanesForward;
        public TrafficLane[] LanesBackward => lanesBackward;
        public Chunk Chunk => chunk;
        public float SpeedLimitKmh => speedLimitKmh;
        public int NumberOfCarsToSpawn => Mathf.RoundToInt(chunk.SplineLengthCached / density);
        public CustomDrivingPath[] RacingLines => racingLines;
        public bool HasBackwardLanes => lanesBackward != null && lanesBackward.Length > 0;
        public bool HasForwardLanes => lanesForward != null && lanesForward.Length > 0;
        
        private void OnValidate()
        {
            if (chunk == null)
                chunk = transform.GetComponentInAllParents<Chunk>();
        }
        
        private void OnEnable()
        {
            chunk.onFullyLoaded += OnChunkLoadedAndReady;

            InitialiseLanes();
        }
        
        private void OnDisable()
        {
            chunk.onFullyLoaded -= OnChunkLoadedAndReady;
        }
        
        private void OnChunkLoadedAndReady()
        {
            InitialiseCars();
        }
        
        /// <summary>
        /// Spawns the initial cars in a random order on the chunk.
        /// </summary>
        private void InitialiseCars()
        {
            if (GameSessionManager.Instance.CurrentSession.TrafficIsProcedural)
            {
                for (int count = 0; count < NumberOfCarsToSpawn; count++)
                    SpawnCarInRandomPosition();
            }
            else
            {
                SpawnNonProceduralCars();
            }
        }

        public void SpawnCarInRandomPosition(LaneDirection direction = LaneDirection.NONE)
        {
            const int maxAttempts = 5;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                //make sure there is a direction
                if (direction == LaneDirection.NONE)
                {
                    LaneDirection? randomDirection = ChooseRandomLaneDirection();
                    if (randomDirection == null)
                    {
                        Debug.LogError("Could not spawn car as there are no lanes.");
                        return;
                    }
                    
                    direction = randomDirection.Value;
                }
                
                TrafficLane randomLane = direction == LaneDirection.FORWARD ? lanesForward.GetRandom() : lanesBackward.GetRandom();
                if (TrySpawnCarInLane(randomLane, direction))
                    return;
                
#if UNITY_EDITOR
                bool isLastAttempt = attempt == maxAttempts - 1;
                if (isLastAttempt)
                    Debug.LogWarning($"Could not find free position for traffic car spawn after {maxAttempts} attempts.");
#endif
            }
        }
        
        /// <summary>
        /// Gets the position of a lane from a certain spline sample.
        /// </summary>
        public PositionAndRotation GetLanePosition(SplineSample splineSample, float laneDistance, LaneDirection direction)
        {
            Vector3 laneOffset = splineSample.right * laneDistance;
            Vector3 finalPos = splineSample.position + laneOffset;
            Quaternion rotation = Quaternion.LookRotation(driveOnLeft && direction == LaneDirection.FORWARD ? splineSample.forward : -splineSample.forward);
            
            return new PositionAndRotation(finalPos, rotation);
        }

        public bool CanSpawnCarAtPosition(AICar carDetails, Vector3 position, Quaternion orientation, bool ignorePlayer = false)
        {
            //box cast around the car with car width and front of car + minDistance in front of the car
            //include racers, traffic cars, and barriers , but exclude player if ignored
            const float height = 5;
            var halfExtents = new Vector3(carDetails.CarWidth / 2f, height, carDetails.FrontOfCarPosition.z + minDistanceRequiredToSpawn);
            int hits = Physics.OverlapBoxNonAlloc(position, halfExtents, tempSpawnCheckHolder, orientation, ignorePlayer ? spawnPositionBlockageLayersExcludingPlayer : spawnPositionBlockageLayers);
            
#if UNITY_EDITOR
            BoxCastUtils.DrawBox(position, halfExtents, orientation, hits > 0 ? Color.red : Color.black);
#endif
            
            return hits == 0;
        }
        
        private void InitialiseLanes()
        {
            //if no lanes, just create one in the centre
            if (!HasForwardLanes && !HasBackwardLanes)
                lanesForward = new[] { new TrafficLane(0) };
        }
        
        private LaneDirection? ChooseRandomLaneDirection()
        {
            if (HasBackwardLanes && HasForwardLanes)
                return Random.Range(0, 2) == 0 ? LaneDirection.FORWARD : LaneDirection.BACKWARD;
            if (HasBackwardLanes)
                return LaneDirection.BACKWARD;
            if (HasForwardLanes)
                return LaneDirection.FORWARD;
            return null;
        }

        private bool TrySpawnCarInLane(TrafficLane lane, LaneDirection direction)
        {
            //get a random lane
            float additionalOffset = Random.Range(-randomLaneOffset, randomLaneOffset);
            PositionAndRotation lanePosition = lane.Type == TrafficLane.LaneType.CUSTOM_SPLINE
                ? GetLanePosition(lane.Path.SplineSamples.GetRandom(), additionalOffset, direction)
                : GetLanePosition(chunk.SplineSamples.GetRandom(), lane.DistanceFromCenter + additionalOffset, direction);

            AICar randomVariant = TrafficCarSpawner.Instance.GetRandomCarPrefab();
            if (!CanSpawnCarAtPosition(randomVariant, lanePosition.Position, lanePosition.Rotation))
                return false;

            //spawn an instance of the car and intialise
            AICar car = TrafficCarSpawner.Instance.SpawnCar(lanePosition.Position, lanePosition.Rotation, randomVariant);
            
            car.SetCurrentLane(lane, direction, additionalOffset);
            car.SetSpeed(car.DesiredSpeed);
            return true;
        }
        
        private void SpawnNonProceduralCars()
        {
            //get manual spawn data
            List<TrafficSpawnPosition> spawnPositionsInChunk = GetManualSpawnPositionsInChunk();

            //spawn the cars
            for (int index = 0; index < spawnPositionsInChunk.Count; index++)
            {
                TrafficSpawnPosition spawnPosition = spawnPositionsInChunk[index];

                if (spawnPosition.LaneDirection == LaneDirection.NONE)
                {
                    Debug.LogError($"The traffic spawn position in {chunk.name} at index {index} is invalid. The lane direction must not be 'none'.");
                    continue;
                }

                if ((spawnPosition.LaneDirection == LaneDirection.FORWARD && (lanesForward == null || lanesForward.Length == 0))
                    || (spawnPosition.LaneDirection == LaneDirection.BACKWARD && (lanesBackward == null || lanesBackward.Length == 0)))
                {
                    Debug.LogError($"The traffic spawn position in {chunk.name} at index {index} is invalid. There is no lanes in direction {spawnPosition.LaneDirection.ToString()}.");
                    continue;
                }
                
                TrafficLane lane = spawnPosition.LaneDirection == LaneDirection.FORWARD ? lanesForward[spawnPosition.LaneIndex] : lanesBackward[spawnPosition.LaneIndex];
                
                if (!TrySpawnCarInLane(lane, spawnPosition.LaneDirection))
                    GlobalLoggers.AICarLogger.Log($"Could not spawn traffic car at index {index} because there was no room.");
            }
        }

        private List<TrafficSpawnPosition> GetManualSpawnPositionsInChunk()
        {
            //TODO: if this is called more than once, cache it and only calculate once
            List<TrafficSpawnPosition> spawnPositionsInChunk = new List<TrafficSpawnPosition>();
            
            float[] chunkLengths = GameSessionManager.Instance.CurrentSession.CurrentChunkMap.ChunkLengthsCalculated;
            if (chunkLengths == null || chunkLengths.Length == 0)
            {
                Debug.LogError($"The chunk map {GameSessionManager.Instance.CurrentSession.CurrentChunkMap.name.Replace("(Clone)", "")} hasn't calculate the chunk lengths. You need to rebuild the map data for manual spawn positions to work.");
                return spawnPositionsInChunk;
            }
            
            int currentChunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunk);
            float chunkStartDistance = currentChunkIndex == 0 ? 0 : chunkLengths[currentChunkIndex - 1];
            float chunkEndDistance = chunkLengths[currentChunkIndex];

            foreach (TrafficSpawnPosition spawnPosition in GameSessionManager.Instance.CurrentSession.TrafficSpawnPositions)
            {
                if (spawnPosition.DistanceFromMapStart >= chunkStartDistance
                    && spawnPosition.DistanceFromMapStart < chunkEndDistance)
                {
                    spawnPositionsInChunk.Add(spawnPosition);
                }
            }

            return spawnPositionsInChunk;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            SplineSample firstSample = chunk.SplineSamples[0];
            SplineSample middleSample = chunk.SplineSamples[Mathf.FloorToInt(chunk.SplineSamples.Length / 2f)];
            SplineSample lastSample = chunk.SplineSamples[^1];
            
            if (lanesForward != null)
            {
                Gizmos.color = Color.yellow;
                
                foreach (TrafficLane lane in lanesForward)
                {
                    if (lane.Type == TrafficLane.LaneType.DISTANCE_FROM_CENTER)
                    {
                        Vector3 firstSamplePos = firstSample.position + lane.DistanceFromCenter * firstSample.right;
                        Gizmos.DrawSphere(firstSamplePos, 1f);

                        Vector3 middleSamplePos = middleSample.position + lane.DistanceFromCenter * middleSample.right;
                        Gizmos.DrawSphere(middleSamplePos, 1f);

                        Vector3 lastSamplePos = lastSample.position + lane.DistanceFromCenter * lastSample.right;
                        Gizmos.DrawSphere(lastSamplePos, 1f);
                    }
                }
            }
            
            if (lanesBackward != null)
            {
                Gizmos.color = Color.blue;
                
                foreach (TrafficLane lane in lanesBackward)
                {
                    if (lane.Type == TrafficLane.LaneType.DISTANCE_FROM_CENTER)
                    {
                        Vector3 firstSamplePos = firstSample.position + lane.DistanceFromCenter * firstSample.right;
                        Gizmos.DrawSphere(firstSamplePos, 1f);

                        Vector3 middleSamplePos = middleSample.position + lane.DistanceFromCenter * middleSample.right;
                        Gizmos.DrawSphere(middleSamplePos, 1f);

                        Vector3 lastSamplePos = lastSample.position + lane.DistanceFromCenter * lastSample.right;
                        Gizmos.DrawSphere(lastSamplePos, 1f);
                    }
                }
            }
        }
#endif
        
    }
}