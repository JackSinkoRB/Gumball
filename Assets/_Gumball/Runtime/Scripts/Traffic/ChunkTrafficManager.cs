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
        
        [Serializable]
        public struct LaneData
        {
            [SerializeField] private int id;
            [SerializeField] private float distance;

            public int ID => id;
            public float Distance => distance;

            public LaneData(int id, float distance)
            {
                this.id = id;
                this.distance = distance;
            }
        }

        [Tooltip("If true, the cars will drive on the left hand side (like Australia). If false, they will drive on the right hand side (like the US)")]
        [SerializeField] private bool driveOnLeft = true;
        
        //when map driving scene loads, load all the traffic cars (eg. a traffic manager that holds reference to all traffic cars)

        [SerializeField] private int numberOfCars = 5;
        [SerializeField] private float[] laneDistances;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunk;
        
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
            for (int count = 0; count < numberOfCars; count++)
            {
                SpawnCarInRandomPosition();
            }
        }

        private void SpawnCarInRandomPosition()
        {
            float randomLaneDistance = laneDistances.GetRandom();
            var (position, rotation) = GetRandomPositionOnSpline(randomLaneDistance);
            TrafficCarSpawner.Instance.SpawnCar(position, rotation);
        }
        
        private (Vector3, Quaternion) GetRandomPositionOnSpline(float laneDistance)
        {
            //get a random sample on the spline, then get the distance depending on the lane
            SplineSample randomSample = chunk.GetRandomSplineSample();
            Vector3 laneOffset = randomSample.right * laneDistance;
            Vector3 finalPos = randomSample.position + laneOffset;
            Quaternion rotation = Quaternion.LookRotation(driveOnLeft && laneDistance < 0 ? -randomSample.right : randomSample.right);
            
            return (finalPos, rotation);
        }

#if UNITY_EDITOR
        private SampleCollection splineSampleCollection;

        private void OnDrawGizmos()
        {
            try
            {
                chunk.SplineComputer.GetSamples(splineSampleCollection);
            }
            catch (NullReferenceException)
            {
                chunk.SplineComputer.RebuildImmediate();
                chunk.SplineComputer.GetSamples(splineSampleCollection);
            }

            SplineSample firstSample = splineSampleCollection.samples[0];
            SplineSample lastSample = splineSampleCollection.samples[^1];
            
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
