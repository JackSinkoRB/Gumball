using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
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
        private readonly MinMaxFloat randomLaneOffset = new(-0.5f, 0.5f);

        [Tooltip("If true, the cars will drive on the left hand side (like Australia). If false, they will drive on the right hand side (like the US).")]
        [SerializeField] private bool driveOnLeft = true;

        [SerializeField] private RacingLine racingLine;
        
        //when map driving scene loads, load all the traffic cars (eg. a traffic manager that holds reference to all traffic cars)

        [SerializeField] private float speedLimitKmh = 40;
        [Tooltip("This value represents the number of metres for each car. Eg. A value of 10 means 1 car every 10 metres.")]
        [SerializeField] private int density = 100;
        [SerializeField] private float[] laneDistancesForward;
        [SerializeField] private float[] laneDistancesBackward;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunk;

        public float[] LaneDistancesForward => laneDistancesForward;
        public float[] LaneDistancesBackward => laneDistancesBackward;
        
        public Chunk Chunk => chunk;
        public float SpeedLimitKmh => speedLimitKmh;
        public int NumberOfCarsToSpawn => Mathf.RoundToInt(chunk.SplineLengthCached / density);
        public RacingLine RacingLine => racingLine;
        public bool HasBackwardLanes => laneDistancesBackward != null && laneDistancesBackward.Length > 0;
        public bool HasForwardLanes => laneDistancesForward != null && laneDistancesForward.Length > 0;
        
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

            TryConnectRacingLines();
        }

        private void TryConnectRacingLines()
        {
            if (racingLine == null)
                return;
            
            int currentMapIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunk);
            LoadedChunkData? previousChunk = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(currentMapIndex - 1);
            if (previousChunk == null)
                return;

            ChunkTrafficManager previousChunkTrafficManager = previousChunk.Value.Chunk.TrafficManager;
            if (previousChunkTrafficManager == null)
                return;

            if (previousChunkTrafficManager.racingLine == null)
                return;

            SplineComputer currentSpline = racingLine.SplineComputer;
            SplineComputer previousSpline = previousChunkTrafficManager.racingLine.SplineComputer;
            
            //insert to end of previous chunks racing line a point with other chunks first point
            previousSpline.SetPoint(previousSpline.pointCount, currentSpline.GetPoint(0));
            
            //insert at start of chunks racing line a node with previous chunks last node
            currentSpline.InsertPoint(0, previousSpline.GetPoint(previousSpline.pointCount-2));
        }

        /// <summary>
        /// Spawns the initial cars in a random order on the chunk.
        /// </summary>
        private void InitialiseCars()
        {
            for (int count = 0; count < NumberOfCarsToSpawn; count++)
            {
                SpawnCarInRandomPosition();
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
                    if (HasBackwardLanes && HasForwardLanes)
                        direction = Random.Range(0, 2) == 0 ? LaneDirection.FORWARD : LaneDirection.BACKWARD;
                    else if (HasBackwardLanes)
                        direction = LaneDirection.BACKWARD;
                    else if (HasForwardLanes)
                        direction = LaneDirection.FORWARD;
                    else
                    {
                        Debug.LogError("Could not spawn car as there are no lanes.");
                        return;
                    }
                }

                float randomLaneDistance = GetRandomLaneDistance(direction);

                var (position, rotation) = GetLanePosition(chunk.SplineSamples.GetRandom(), randomLaneDistance, direction);
                
                if (!CanSpawnCarAtPosition(position, randomLaneDistance))
                {
                    bool isLastAttempt = attempt == maxAttempts - 1;
                    if (isLastAttempt)
                        Debug.LogWarning($"Could not find free position for traffic car spawn after {maxAttempts} attempts.");
                    continue;
                }

                AICar car = TrafficCarSpawner.Instance.SpawnCar(position, rotation);
                car.SetLaneDistance(randomLaneDistance + randomLaneOffset.RandomInRange(), direction);
                car.SetSpeed(car.DesiredSpeed);
                break;
            }
        }
        
        /// <summary>
        /// Gets the position of a lane from a certain spline sample.
        /// </summary>
        public (Vector3, Quaternion) GetLanePosition(SplineSample splineSample, float laneDistance, LaneDirection direction)
        {
            Vector3 laneOffset = splineSample.right * laneDistance;
            Vector3 finalPos = splineSample.position + laneOffset;
            Quaternion rotation = Quaternion.LookRotation(driveOnLeft && direction == LaneDirection.FORWARD ? splineSample.forward : -splineSample.forward);
            
            return (finalPos, rotation);
        }
        
        public float GetRandomLaneDistance(LaneDirection direction)
        {
            if (direction == LaneDirection.NONE)
            {
                if (HasBackwardLanes && HasForwardLanes)
                    //pick random direction
                    return Random.Range(0, 2) == 0 ? laneDistancesForward.GetRandom() : laneDistancesBackward.GetRandom();
                if (HasBackwardLanes)
                    return laneDistancesBackward.GetRandom();
                if (HasForwardLanes)
                    return laneDistancesForward.GetRandom();
                
                Debug.LogError("Could not get random lane distance as there are no lanes.");
                return 0;
            }

            if (direction == LaneDirection.BACKWARD)
            {
                if (!HasBackwardLanes)
                {
                    Debug.LogError("Could not get random lane distance as there are no backward lanes.");
                    return 0;
                }

                return laneDistancesBackward.GetRandom();
            }

            if (direction == LaneDirection.FORWARD)
            {
                if (!HasForwardLanes)
                {
                    Debug.LogError("Could not get random lane distance as there are no forward lanes.");
                    return 0;
                }

                return laneDistancesForward.GetRandom();
            }

            throw new ArgumentOutOfRangeException();
        }
        
        public float GetOffsetFromRacingLine(Vector3 fromPoint)
        {
            var (splineSample, distanceSqr) = racingLine.SampleCollection.GetClosestSampleOnSpline(fromPoint);
            float distance = Mathf.Sqrt(distanceSqr);
            
            //is the position to the left or right of the spline?
            bool isRight = fromPoint.IsFurtherInDirection(splineSample.position, splineSample.right);
            float offsetDirection = isRight ? 1 : -1;
            
            return distance * offsetDirection;
        }
        
        public bool CanSpawnCarAtPosition(Vector3 position, float laneDistance, bool ignorePlayer = false)
        {
            const float minDistanceSqr = minDistanceRequiredToSpawn * minDistanceRequiredToSpawn;

            //loop over all cars and get distance to their position
            foreach (AICar trafficCar in TrafficCarSpawner.CurrentCars)
            {
                bool isSameLane = Mathf.Abs(trafficCar.CurrentLaneDistance - laneDistance) > randomLaneOffset.Max;
                if (isSameLane)
                    continue; //not in same lane
                
                float distanceSqr = Vector3.SqrMagnitude(position - trafficCar.transform.position);
                if (distanceSqr <= minDistanceSqr)
                    return false;
            }
            
            //loop over all racers and get distance to their position
            if (GameSessionManager.Instance.CurrentSession.CurrentRacers != null)
            {
                foreach (AICar racerCar in GameSessionManager.Instance.CurrentSession.CurrentRacers)
                {
                    if (racerCar == null || racerCar.IsPlayerCar)
                        continue;
                    
                    float distanceSqr = Vector3.SqrMagnitude(position - racerCar.transform.position);
                    if (distanceSqr <= minDistanceSqr)
                        return false;
                }
            }

            if (!ignorePlayer)
            {
                //get distance to the player car
                float distanceToPlayerSqr = Vector3.SqrMagnitude(position - WarehouseManager.Instance.CurrentCar.transform.position);
                if (distanceToPlayerSqr <= minDistanceSqr)
                    return false;
            }

            return true;
        }

        private void InitialiseLanes()
        {
            //if no lanes, just create one in the centre
            if (laneDistancesForward.Length == 0 && laneDistancesBackward.Length == 0)
                laneDistancesForward = new[] { 0f };
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            SplineSample firstSample = chunk.SplineSamples[0];
            SplineSample lastSample = chunk.SplineSamples[^1];
            
            if (laneDistancesForward != null)
            {
                Gizmos.color = Color.yellow;

                foreach (float laneDistance in laneDistancesForward)
                {
                    Vector3 firstSamplePos = firstSample.position + laneDistance * firstSample.right;
                    Gizmos.DrawSphere(firstSamplePos, 1f);

                    Vector3 lastSamplePos = lastSample.position + laneDistance * lastSample.right;
                    Gizmos.DrawSphere(lastSamplePos, 1f);
                }
            }
            
            if (laneDistancesBackward != null)
            {
                Gizmos.color = Color.blue;

                foreach (float laneDistance in laneDistancesBackward)
                {
                    Vector3 firstSamplePos = firstSample.position + laneDistance * firstSample.right;
                    Gizmos.DrawSphere(firstSamplePos, 1f);

                    Vector3 lastSamplePos = lastSample.position + laneDistance * lastSample.right;
                    Gizmos.DrawSphere(lastSamplePos, 1f);
                }
            }
        }
#endif
        
    }
}
