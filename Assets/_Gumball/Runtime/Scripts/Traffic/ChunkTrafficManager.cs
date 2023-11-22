using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class ChunkTrafficManager : MonoBehaviour
    {

        [Tooltip("If true, the cars will drive on the left hand side (like Australia). If false, they will drive on the right hand side (like the US).")]
        [SerializeField] private bool driveOnLeft = true;
        
        //when map driving scene loads, load all the traffic cars (eg. a traffic manager that holds reference to all traffic cars)

        [Tooltip("This value represents the number of metres for each car. Eg. A value of 10 means 1 car every 10 metres.")]
        [SerializeField] private int density = 100;
        [SerializeField] private float[] laneDistances;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunk;

        public Chunk Chunk => chunk;
        public bool DriveOnLeft => driveOnLeft;
        
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
            
            //if no lanes, just create one in the centre
            if (laneDistances.Length == 0)
                laneDistances = new[] { 0f };
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
            float splineLength = chunk.SplineComputer.CalculateLength();
            int numberOfCars = Mathf.RoundToInt(splineLength / density);
            for (int count = 0; count < numberOfCars; count++)
            {
                SpawnCarInRandomPosition();
            }
        }

        private void SpawnCarInRandomPosition()
        {
            MinMaxFloat randomLaneOffset = new(-0.5f, 0.5f);

            float randomLaneDistance = laneDistances.GetRandom();
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
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
