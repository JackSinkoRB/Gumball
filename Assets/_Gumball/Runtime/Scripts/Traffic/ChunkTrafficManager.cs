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
        
        //when map driving scene loads, load all the traffic cars (eg. a traffic manager that holds reference to all traffic cars)

        [SerializeField] private int numberOfCars = 5;
        [SerializeField] private LaneData[] laneData;

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
            
            if (laneData.Length == 0)
                laneData = new[] { new LaneData() };
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
            LaneData randomLane = laneData.GetRandom();
            Vector3 randomPosition = GetRandomPosition(randomLane);
            TrafficCarSpawner.Instance.SpawnCar(randomPosition);
        }

        /// <summary>
        /// Gets a random position in a lane.
        /// </summary>
        private Vector3 GetRandomPosition(LaneData lane)
        {
            //get a random sample on the spline, then get the distance depending on the lane
            SplineSample randomSample = chunk.GetRandomSplineSample();
            Vector3 laneOffset = randomSample.right * lane.Distance;
            
            return randomSample.position + laneOffset;
        }

    }
}
