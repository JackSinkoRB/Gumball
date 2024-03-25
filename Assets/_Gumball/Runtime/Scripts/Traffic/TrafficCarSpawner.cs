using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    public class TrafficCarSpawner : Singleton<TrafficCarSpawner>
    {
        
        #region STATIC
        private static readonly HashSet<AICar> currentCars = new();
        
        public static IReadOnlyCollection<AICar> CurrentCars => currentCars;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            currentCars.Clear();
        }
        
        public static void TrackCar(AICar trafficCar)
        {
            currentCars.Add(trafficCar);
        }

        public static void UntrackCar(AICar trafficCar)
        {
            currentCars.Remove(trafficCar);
        }
        #endregion

#if UNITY_EDITOR
        private int trafficCarID; //a unique identifier for debugging
#endif
        
        private const float timeBetweenSpawnChecks = 1;
        
        [SerializeField] private AICar[] trafficCarPrefabs;

        private float lastSpawnCheckTime;
        
        private float timeSinceLastSpawnCheck => Time.realtimeSinceStartup - lastSpawnCheckTime;
        
        public AICar SpawnCar(Vector3 position, Quaternion rotation)
        {
            AICar randomCarVariant = trafficCarPrefabs.GetRandom().gameObject.GetSpareOrCreate<AICar>(transform, position, rotation);
            
            randomCarVariant.SetAutoDrive(true);
            TrackCar(randomCarVariant);
            randomCarVariant.onDisable += () => UntrackCar(randomCarVariant);

#if UNITY_EDITOR
            randomCarVariant.name = $"TrafficCar-{randomCarVariant.gameObject.name}-{trafficCarID}";
            trafficCarID++;
#endif
            
            return randomCarVariant;
        }

        private void LateUpdate()
        {
            if (timeSinceLastSpawnCheck >= timeBetweenSpawnChecks)
                CheckToSpawnCars();
        }

        private void CheckToSpawnCars()
        {
            if (!ChunkManager.Instance.HasLoaded)
                return;
            
            lastSpawnCheckTime = Time.realtimeSinceStartup;
            
            int numberOfCars = currentCars.Count;
            int desiredCars = GetTotalDesiredCars();

            int carsWaitingToSpawn = Mathf.Max(desiredCars - numberOfCars, 0);

            for (int count = 0; count < carsWaitingToSpawn; count++)
            {
                int random = Random.Range(0, 2);
                bool useFirstChunk = random == 0;
                
                Chunk firstChunk = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(ChunkManager.Instance.AccessibleChunksIndices.Min).Value.Chunk;
                Chunk lastChunk = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(ChunkManager.Instance.AccessibleChunksIndices.Max).Value.Chunk;
                Chunk chunkToUse = useFirstChunk ? firstChunk : lastChunk;
                
                chunkToUse.TrafficManager.SpawnCarInRandomPosition(useFirstChunk ? ChunkTrafficManager.LaneDirection.FORWARD : ChunkTrafficManager.LaneDirection.BACKWARD);
            }
        }

        private int GetTotalDesiredCars()
        {
            int total = 0;
            foreach (LoadedChunkData loadedChunkData in ChunkManager.Instance.CurrentChunks)
                total += loadedChunkData.Chunk.TrafficManager.NumberOfCarsToSpawn;
            
            return total;
        }
        
    }
}
