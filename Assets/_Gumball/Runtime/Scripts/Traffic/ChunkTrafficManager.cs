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
        
        [Tooltip("If true, the cars will drive on the left hand side (like Australia). If false, they will drive on the right hand side (like the US).")]
        [SerializeField] private bool driveOnLeft = true;
        
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
        public int NumberOfCarsToSpawn => Mathf.RoundToInt(chunk.SplineLength / density);

        //have a distance for spawning cars (based on chunk distance, not car distance)
        //have an activation distance - if not activated, rigidbody is kinematic and collider is disabled
        
        
        //all cars spawn initially based on settings - all move at relative speed to the chunk speed limit - with acceleration/deceleration (eg. 60kms)
        
        //number of desired cars is combination of all the currentChunks desired cars
        
        //detect when car no longer has a chunk under it - and despawn it
        
        private void OnValidate()
        {
            if (chunk == null)
                chunk = transform.FindComponentInParents<Chunk>();
        }

        private void OnEnable()
        {
            ChunkManager.Instance.onChunkLoad += OnChunkLoad;

            InitialiseLanes();
        }

        private void OnDisable()
        {
            ChunkManager.Instance.onChunkLoad -= OnChunkLoad;
        }

        private void OnChunkLoad(Chunk loadedChunk)
        {
            if (loadedChunk == chunk)
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

        public LaneDirection GetLaneDirection(float laneDistance)
        {
            if (driveOnLeft)
                return laneDistance < 0 ? LaneDirection.FORWARD : LaneDirection.BACKWARD;
            return laneDistance < 0 ? LaneDirection.BACKWARD : LaneDirection.FORWARD;
        }

        public void SpawnCarInRandomPosition(LaneDirection direction = LaneDirection.NONE)
        {
            MinMaxFloat randomLaneOffset = new(-0.5f, 0.5f);

            float randomLaneDistance = GetRandomLaneDistance(direction);
            
            var (position, rotation) = GetLanePosition(chunk.SplineSamples.GetRandom(), randomLaneDistance);
            TrafficCar car = TrafficCarSpawner.Instance.SpawnCar(chunk, position, rotation);
            car.SetLaneDistance(randomLaneDistance + randomLaneOffset.RandomInRange());
        }
        
        public (Vector3, Quaternion) GetLanePosition(SplineSample splineSample, float laneDistance)
        {
            //get a random sample on the spline, then get the distance depending on the lane
            Vector3 laneOffset = splineSample.right * laneDistance;
            Vector3 finalPos = splineSample.position + laneOffset;
            Quaternion rotation = Quaternion.LookRotation(driveOnLeft && laneDistance < 0 ? splineSample.forward : -splineSample.forward);
            
            return (finalPos, rotation);
        }
        
        private float GetRandomLaneDistance(LaneDirection direction)
        {
            if (laneDistancesBackwardCached.Length == 0 || laneDistancesForwardCached.Length == 0)
                Debug.LogError("Here: " + chunk.gameObject.name);
            
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
