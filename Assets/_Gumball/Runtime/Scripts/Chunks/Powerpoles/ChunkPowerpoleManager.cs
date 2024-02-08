using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AYellowpaper.SerializedCollections;
using MyBox;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    public class ChunkPowerpoleManager : MonoBehaviour
    {
        
        public static void OnLoadChunkObject(Chunk chunk, GameObject chunkObject)
        {
            if (chunk.PowerpoleManager != null)
            {
                Powerpole powerpole = chunkObject.GetComponent<Powerpole>();
                if (powerpole != null)
                    chunk.PowerpoleManager.TrackPoleInChunk(powerpole);
            }
        }

        [SerializeField, ReadOnly] private Chunk chunk;
        [SerializeField] public SerializedDictionary<Powerpole.PowerpolePosition, List<Powerpole>> poles = new();

        private void OnEnable()
        {
            if (transform.parent == null || transform.parent.GetComponent<Chunk>() == null)
            {
                enabled = false; //disable the script
                throw new NullReferenceException($"{gameObject.name} ({nameof(ChunkPowerpoleManager)}) must be a child of a chunk.");
            }

            chunk = transform.parent.GetComponent<Chunk>();
            
            ChunkManager.Instance.onChunkBecomeAccessibleAndLoaded += OnChunkBecomeAccessibleAndLoaded;
        }

        private void OnDisable()
        {
            ChunkManager.Instance.onChunkBecomeAccessibleAndLoaded -= OnChunkBecomeAccessibleAndLoaded;
        }

        public void TrackPoleInChunk(Powerpole pole)
        {
            pole.CalculatePosition(chunk);

            if (!poles.ContainsKey(pole.Position))
                poles[pole.Position] = new List<Powerpole>();

            List<Powerpole> polesInGroup = poles[pole.Position];
            
            if (polesInGroup.Contains(pole))
                return; //already tracked

            polesInGroup.Add(pole);
        }
        
        private void OnChunkBecomeAccessibleAndLoaded(LoadedChunkData loadedChunkData)
        {
            if (chunk != loadedChunkData.Chunk)
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();
            SortPolesByDistance();
            ConnectLinesInChunk();
            
            int previousChunkIndex = loadedChunkData.MapIndex - 1;
            LoadedChunkData? previousChunk = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(previousChunkIndex);
            if (previousChunk != null)
                ConnectToAnotherChunk(previousChunk.Value.Chunk);
            
            GlobalLoggers.LoadingLogger.Log($"Took: {stopwatch.ElapsedMilliseconds}ms to set up powerlines for {loadedChunkData.Chunk.gameObject.name}");
        }

        /// <summary>
        /// Sorts the pole groups, so that the closest poles to the spline start are first in the list, and the furthest poles from the spline start are last in the list.
        /// </summary>
        private void SortPolesByDistance()
        {
            foreach (Powerpole.PowerpolePosition position in poles.Keys)
            {
                List<Powerpole> values = poles[position];
                values.Sort((powerpole1, powerpole2) => powerpole1.ClosestSplineIndex.CompareTo(powerpole2.ClosestSplineIndex));
            }
        }

        private void ConnectLinesInChunk()
        {
            foreach (Powerpole.PowerpolePosition position in poles.Keys)
            {
                List<Powerpole> polesAtPosition = poles[position];
                
                //connect from this pole to previous pole - not including first pole
                for (int index = 1; index < polesAtPosition.Count; index++)
                {
                    Powerpole pole = polesAtPosition[index];
                    Powerpole previousPole = polesAtPosition[index - 1];

                    pole.ConnectLines(previousPole);
                }
            }
        }

        private void ConnectToAnotherChunk(Chunk otherChunk)
        {
            if (otherChunk.PowerpoleManager == null)
                return;

            foreach (Powerpole.PowerpolePosition position in poles.Keys)
            {
                if (!otherChunk.PowerpoleManager.poles.ContainsKey(position))
                    continue; //nothing to connect to
                
                List<Powerpole> polesAtPosition = poles[position];
                List<Powerpole> otherPolesAtPosition = otherChunk.PowerpoleManager.poles[position];

                //connect first pole to last pole on previous chunk
                Powerpole pole = polesAtPosition[0];
                Powerpole previousPole = otherPolesAtPosition[^1];

                pole.ConnectLines(previousPole);
            }
        }

    }
}
