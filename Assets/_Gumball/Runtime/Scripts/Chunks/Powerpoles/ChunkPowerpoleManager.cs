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
            
            chunk.onBecomeAccessible += OnChunkBecomeAccessible;
        }

        private void OnDisable()
        {
            chunk.onBecomeAccessible -= OnChunkBecomeAccessible;
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
        
        private void OnChunkBecomeAccessible()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            SortPolesByDistance();
            ConnectLinesInChunk();

            int currentIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunk);
            int previousChunkIndex = currentIndex - 1;
            LoadedChunkData? previousChunk = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(previousChunkIndex);
            if (previousChunk != null)
                ConnectToAnotherChunk(previousChunk.Value.Chunk);
            
            GlobalLoggers.LoadingLogger.Log($"Took: {stopwatch.ElapsedMilliseconds}ms to set up powerlines for {chunk.gameObject.name}");
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
            GlobalLoggers.PowerlineLogger.Log($"Connecting {chunk.gameObject.name} ({ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunk)}) with {otherChunk.gameObject.name} ({ChunkManager.Instance.GetMapIndexOfLoadedChunk(otherChunk)})");

            if (otherChunk.PowerpoleManager == null)
            {
                GlobalLoggers.PowerlineLogger.Log("'otherChunk' is missing a PowerpoleManager.");
                return;
            }

            foreach (Powerpole.PowerpolePosition position in poles.Keys)
            {
                GlobalLoggers.PowerlineLogger.Log($" - Checking {position.ToString()}");

                if (!otherChunk.PowerpoleManager.poles.ContainsKey(position))
                {
                    GlobalLoggers.PowerlineLogger.Log("   - Nothing to connect with");
                    continue; //nothing to connect to
                }

                List<Powerpole> polesAtPosition = poles[position];
                List<Powerpole> otherPolesAtPosition = otherChunk.PowerpoleManager.poles[position];

                //connect first pole to last pole on previous chunk
                Powerpole pole = polesAtPosition[0];
                Powerpole previousPole = otherPolesAtPosition[^1];

                GlobalLoggers.PowerlineLogger.Log($" - Connecting pole at {pole.ClosestSplineIndex} with pole at {previousPole.ClosestSplineIndex}");
                
                pole.ConnectLines(previousPole);
            }
        }

    }
}
