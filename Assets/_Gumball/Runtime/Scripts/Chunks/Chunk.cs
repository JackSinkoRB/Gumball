using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gumball
{
    [RequireComponent(typeof(ChunkEditorTools))]
    public class Chunk : MonoBehaviour
    {
        
        [SerializeField] private SplineComputer splineComputer;
        
        [Header("Modify")]
        [PositiveValueOnly, SerializeField] private float terrainBlendDistance = 50;

        [Header("Debugging")]
        [ReadOnly, SerializeField] private Chunk chunkBefore;
        [ReadOnly, SerializeField] private Chunk chunkAfter;
        
        public int LastPointIndex => splineComputer.pointCount - 1;
        public SplineComputer SplineComputer => splineComputer;

        public Chunk ChunkBefore => chunkBefore;
        public Chunk ChunkAfter => chunkAfter;
        public bool HasChunkConnected => chunkBefore != null || chunkAfter != null;
        
        public bool IsConnecting { get; private set; }
        public SplineSample FirstSample { get; private set; }
        public SplineSample LastSample { get; private set; }
        public Vector3 FirstTangent { get; private set; }
        public Vector3 LastTangent { get; private set; }
        public float TerrainBlendDistance => terrainBlendDistance;

        public GameObject CurrentTerrain
        {
            get
            {
                if (currentTerrain != null)
                    return currentTerrain;
                TryFindExistingTerrain();
                return currentTerrain;
            }
        }
        
        private GameObject currentTerrain;
        private readonly SampleCollection splineSampleCollection = new();

        public void SetTerrain(GameObject terrain)
        {
            currentTerrain = terrain;
        }

        public void SetConnecting(bool isConnecting)
        {
            IsConnecting = isConnecting;
        }

        public void UpdateSplineSampleData()
        {
            splineComputer.GetSamples(splineSampleCollection);
            
            FirstSample = splineSampleCollection.samples[0];
            FirstTangent = FirstSample.right.Flatten();
            
            LastSample = splineSampleCollection.samples[splineSampleCollection.length-1];
            LastTangent = LastSample.right.Flatten();
        }
        
        /// <summary>
        /// Puts the chunk at the end of an existing chunk.
        /// </summary>
        public void Connect(Chunk chunkToAppendTo)
        {
            ChunkUtils.ConnectChunks(chunkToAppendTo, this);
        }

        public void OnConnectChunkBefore(Chunk chunk)
        {
            OnConnectChunk();
            chunkBefore = chunk;
        }
        
        public void OnConnectChunkAfter(Chunk chunk)
        {
            OnConnectChunk();
            chunkAfter = chunk;
        }

        private void OnConnectChunk()
        {
            
        }

        private void OnDisconnectChunk()
        {
            if (!HasChunkConnected)
                transform.rotation = Quaternion.Euler(Vector3.zero); //reset rotation
        }

        public void DisconnectAll(bool canUndo = false)
        {
#if UNITY_EDITOR
            if (canUndo)
            {
                List<Object> objectsToRecord = new List<Object>();

                objectsToRecord.Add(transform);
                objectsToRecord.Add(currentTerrain.GetComponent<MeshFilter>());

                if (chunkAfter != null)
                {
                    objectsToRecord.Add(chunkAfter);
                    if (chunkAfter.chunkBefore != null)
                        objectsToRecord.Add(chunkAfter.chunkBefore);
                }

                if (chunkBefore != null)
                {
                    objectsToRecord.Add(chunkBefore);
                    if (chunkBefore.chunkAfter != null)
                        objectsToRecord.Add(chunkBefore.chunkAfter);
                }
                
                Undo.RecordObjects(objectsToRecord.ToArray(), "Disconnect Chunk");
            }
#endif
            
            OnDisconnectChunkAfter();
            OnDisconnectChunkBefore();
        }

        public void OnDisconnectChunkBefore()
        {
            if (chunkBefore == null)
                return;

            Chunk previousChunk = chunkBefore;
            chunkBefore = null;
            previousChunk.OnDisconnectChunkAfter();
            OnDisconnectChunk();
        }

        public void OnDisconnectChunkAfter()
        {
            if (chunkAfter == null)
                return;
            
            Chunk previousChunk = chunkAfter;
            chunkAfter = null;
            previousChunk.OnDisconnectChunkBefore();
            OnDisconnectChunk();
        }

        public Vector3 GetCenterOfSpline()
        {
            float splineLength = splineComputer.CalculateLength();
            double travel = splineComputer.Travel(0.0, splineLength / 2f, Spline.Direction.Forward);
            Vector3 middle = splineComputer.EvaluatePosition(travel);
            return middle;
        }

        public (SplineSample, float) GetClosestSampleOnSpline(Vector3 fromPoint, bool flattenTheSpline = false)
        {
            float closestDistance = Mathf.Infinity;
            SplineSample closestSample = default;
            foreach (SplineSample sample in splineSampleCollection.samples)
            {
                float distance = flattenTheSpline
                        ? Vector2.SqrMagnitude(fromPoint.FlattenAsVector2() - sample.position.FlattenAsVector2())
                        : Vector3.SqrMagnitude(fromPoint - sample.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSample = sample;
                }
            }
            return (closestSample, Mathf.Sqrt(closestDistance));
        }

        private void TryFindExistingTerrain()
        {
            if (currentTerrain != null)
                return; //already exists

            foreach (Transform child in transform)
            {
                if (child.tag.Equals(ChunkUtils.TerrainTag))
                {
                    currentTerrain = child.gameObject;
                    return;
                }
            }
        }

    }
}
