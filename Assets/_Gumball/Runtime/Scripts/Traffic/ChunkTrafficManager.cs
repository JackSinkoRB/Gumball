using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

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
        [SerializeField, InitializationField] private float[] laneDistances;
        [SerializeField, ReadOnly] private float[] laneDistancesForwardCached;
        [SerializeField, ReadOnly] private float[] laneDistancesBackwardCached;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunk;
        
        public Chunk Chunk => chunk;
        public float SpeedLimitKmh => speedLimitKmh;
        public int NumberOfCarsToSpawn => Mathf.RoundToInt(chunk.SplineLengthCached / density);
        public RacingLine RacingLine => racingLine;
        
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

        public LaneDirection GetLaneDirection(float laneDistance)
        {
            if (driveOnLeft)
                return laneDistance < 0 ? LaneDirection.FORWARD : LaneDirection.BACKWARD;
            return laneDistance < 0 ? LaneDirection.BACKWARD : LaneDirection.FORWARD;
        }

        public void SpawnCarInRandomPosition(LaneDirection direction = LaneDirection.NONE)
        {
            const int maxAttempts = 5;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float randomLaneDistance = GetRandomLaneDistance(direction);

                var (position, rotation) = GetLanePosition(chunk.SplineSamples.GetRandom(), randomLaneDistance);
                
                if (!CanSpawnCarAtPosition(position, randomLaneDistance))
                {
                    bool isLastAttempt = attempt == maxAttempts - 1;
                    if (isLastAttempt)
                        Debug.LogWarning($"Could not find free position for traffic car spawn after {maxAttempts} attempts.");
                    continue;
                }

                AICar car = TrafficCarSpawner.Instance.SpawnCar(position, rotation);
                car.SetLaneDistance(randomLaneDistance + randomLaneOffset.RandomInRange());
                car.SetSpeed(car.DesiredSpeed);
                break;
            }
        }
        
        public (Vector3, Quaternion) GetLanePosition(SplineSample splineSample, float laneDistance)
        {
            //get a random sample on the spline, then get the distance depending on the lane
            Vector3 laneOffset = splineSample.right * laneDistance;
            Vector3 finalPos = splineSample.position + laneOffset;
            Quaternion rotation = Quaternion.LookRotation(driveOnLeft && laneDistance < 0 ? splineSample.forward : -splineSample.forward);
            
            return (finalPos, rotation);
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
        
        private bool CanSpawnCarAtPosition(Vector3 position, float laneDistance)
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

            //get distance to the player car
            float distanceToPlayerSqr = Vector3.SqrMagnitude(position - WarehouseManager.Instance.CurrentCar.transform.position);
            if (distanceToPlayerSqr <= minDistanceSqr)
                return false;
            
            return true;
        }
        
        private float GetRandomLaneDistance(LaneDirection direction)
        {
            return direction switch
            {
                LaneDirection.NONE => laneDistances.GetRandom(),
                LaneDirection.FORWARD => laneDistancesForwardCached.GetRandom(),
                LaneDirection.BACKWARD => laneDistancesBackwardCached.GetRandom(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private void InitialiseLanes()
        {
            //if no lanes, just create one in the centre
            if (laneDistances.Length == 0)
                laneDistances = new[] { 0f };
            
            CacheLaneDistances();
        }
        
        /// <summary>
        /// Splits the lane distances into forward and backward arrays.
        /// </summary>
        private void CacheLaneDistances()
        {
            HashSet<float> laneDistancesForward = new();
            HashSet<float> laneDistancesBackward = new();

            foreach (float lane in laneDistances)
            {
                if (GetLaneDirection(lane) == LaneDirection.FORWARD)
                    laneDistancesForward.Add(lane);
                if (GetLaneDirection(lane) == LaneDirection.BACKWARD)
                    laneDistancesBackward.Add(lane);
            }

            //copy to the cached array
            laneDistancesForwardCached = new float[laneDistancesForward.Count];
            laneDistancesForward.CopyTo(laneDistancesForwardCached);

            laneDistancesBackwardCached = new float[laneDistancesBackward.Count];
            laneDistancesBackward.CopyTo(laneDistancesBackwardCached);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (laneDistances == null)
                return;
            
            SplineSample firstSample = chunk.SplineSamples[0];
            SplineSample lastSample = chunk.SplineSamples[^1];
            
            Gizmos.color = Color.yellow;

            for (int i = 0; i < laneDistances.Length; i++)
            {
                Vector3 firstSamplePos = firstSample.position + laneDistances[i] * firstSample.right;
                Gizmos.DrawSphere(firstSamplePos, 1f);

                Vector3 lastSamplePos = lastSample.position + laneDistances[i] * lastSample.right;
                Gizmos.DrawSphere(lastSamplePos, 1f);
            }
        }
#endif
        
    }
}
