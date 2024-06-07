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
        
        [Header("Forward traffic lanes")]
        [SerializeField] private float[] laneDistancesForward;
        [Tooltip("Assign custom traffic lanes that use splines rather than offset from the center of the chunk.")]
        [SerializeField] private CustomDrivingPath[] customTrafficLanesForward;
        
        [Header("Backward traffic lanes")]
        [SerializeField] private float[] laneDistancesBackward;
        [Tooltip("Assign custom traffic lanes that use splines rather than offset from the center of the chunk.")]
        [SerializeField] private CustomDrivingPath[] customTrafficLanesBackward;
        
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
        
        public float[] LaneDistancesForward => laneDistancesForward;
        public float[] LaneDistancesBackward => laneDistancesBackward;
        public Chunk Chunk => chunk;
        public float SpeedLimitKmh => speedLimitKmh;
        public int NumberOfCarsToSpawn => Mathf.RoundToInt(chunk.SplineLengthCached / density);
        public CustomDrivingPath[] RacingLines => racingLines;
        public bool HasBackwardLanes => (customTrafficLanesBackward != null && customTrafficLanesBackward.Length > 0) || (laneDistancesBackward != null && laneDistancesBackward.Length > 0);
        public bool HasForwardLanes => (customTrafficLanesForward != null && customTrafficLanesForward.Length > 0) || (laneDistancesForward != null && laneDistancesForward.Length > 0);
        
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
                    LaneDirection? randomDirection = ChooseRandomLaneDirection();
                    if (randomDirection == null)
                    {
                        Debug.LogError("Could not spawn car as there are no lanes.");
                        return;
                    }
                    
                    direction = randomDirection.Value;
                }
                
                if (TrySpawnCarInDirection(direction))
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
            if (laneDistancesForward.Length == 0 && laneDistancesBackward.Length == 0)
                laneDistancesForward = new[] { 0f };
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
        
        private bool TrySpawnCarInDirection(LaneDirection direction)
        {
            int totalOptions = direction == LaneDirection.FORWARD
                ? laneDistancesForward.Length + customTrafficLanesForward.Length
                : laneDistancesBackward.Length + customTrafficLanesBackward.Length;
            
            //decide whether it will be a custom lane or distance based
            float laneOffset = Random.Range(-randomLaneOffset, randomLaneOffset);
            CustomDrivingPath customLane = null;
            float randomLaneDistance = -1;
            (Vector3, Quaternion) lanePosition;
            
            int random = Random.Range(0, totalOptions);
            int amountOfNormalLanes = direction == LaneDirection.FORWARD ? laneDistancesForward.Length : laneDistancesBackward.Length;
            bool isCustomLane = random >= amountOfNormalLanes;
            if (isCustomLane)
            {
                int customlaneIndex = random - amountOfNormalLanes - 1;
                CustomDrivingPath[] lanesInDirection = direction == LaneDirection.FORWARD ? customTrafficLanesForward : customTrafficLanesBackward;
                customLane = lanesInDirection[customlaneIndex];
                
                lanePosition = GetLanePosition(customLane.SplineSamples.GetRandom(), laneOffset, direction);
            }
            else
            {
                randomLaneDistance = GetRandomLaneDistance(direction);
                
                lanePosition = GetLanePosition(chunk.SplineSamples.GetRandom(), randomLaneDistance, direction);
            }
            
            AICar randomVariant = TrafficCarSpawner.Instance.GetRandomCarPrefab();
            if (!CanSpawnCarAtPosition(randomVariant, lanePosition.Item1, lanePosition.Item2))
                return false;

            //spawn an instance of the car and intialise
            AICar car = TrafficCarSpawner.Instance.SpawnCar(lanePosition.Item1, lanePosition.Item2, randomVariant);
            
            if (isCustomLane)
                car.SetLaneDistance(customLane, laneOffset, direction);
            else
                car.SetLaneDistance(randomLaneDistance + laneOffset, direction);
            
            car.SetSpeed(car.DesiredSpeed);
            return true;
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