using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gumball
{
#if UNITY_EDITOR
    [RequireComponent(typeof(ChunkEditorTools))]
#endif
    [RequireComponent(typeof(UniqueIDAssigner))]
    public class Chunk : MonoBehaviour
    {
        
        [SerializeField] private SplineComputer splineComputer;
        [SerializeField] private SplineMesh roadMesh;
        
        [Header("Modify")]
        [PositiveValueOnly, SerializeField] private float terrainBlendDistance = 50;

        [Header("Debugging")]
        [ReadOnly, SerializeField] private Chunk chunkBefore;
        [ReadOnly, SerializeField] private Chunk chunkAfter;
        [ReadOnly, SerializeField] private GameObject currentTerrain;

        public string UniqueID => GetComponent<UniqueIDAssigner>().UniqueID;

        public int LastPointIndex => splineComputer.pointCount - 1;
        public SplineComputer SplineComputer => splineComputer;
        public SplineMesh RoadMesh => roadMesh;

        public Chunk ChunkBefore => chunkBefore;
        public Chunk ChunkAfter => chunkAfter;
        public bool HasChunkConnected => chunkBefore != null || chunkAfter != null;

        public ChunkMeshData ChunkMeshData;
        public bool IsAutomaticTerrainRecreationDisabled { get; private set; }
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
        
        private readonly SampleCollection splineSampleCollection = new();

        public void SetTerrain(GameObject terrain)
        {
            currentTerrain = terrain;
            UpdateChunkMeshData();
        }
        
        public void UpdateChunkMeshData()
        {
            DisableAutomaticTerrainRecreation(true);
            if (CurrentTerrain == null)
            {
                ChunkMeshData = null;
                return;
            }

            ChunkMeshData = new ChunkMeshData(this);
            DisableAutomaticTerrainRecreation(false);
        }

        public void UpdateSplineImmediately()
        {
            splineComputer.RebuildImmediate();
            UpdateSplineSampleData();
        }

        /// <summary>
        /// If the chunk is selected and has been updated in the editor, it will recreate the terrain. Use this to disable it while values need to be updated, but the terrain not updated.
        /// </summary>
        /// <param name="isAutomaticTerrainRecreationDisabled"></param>
        public void DisableAutomaticTerrainRecreation(bool isAutomaticTerrainRecreationDisabled)
        {
            IsAutomaticTerrainRecreationDisabled = isAutomaticTerrainRecreationDisabled;
        }

        public void UpdateSplineSampleData()
        {
            splineComputer.GetSamples(splineSampleCollection);
            
            FirstSample = splineSampleCollection.samples[0];
            FirstTangent = FirstSample.right.Flatten();
            
            LastSample = splineSampleCollection.samples[splineSampleCollection.length-1];
            LastTangent = LastSample.right.Flatten();
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
                objectsToRecord.Add(CurrentTerrain.GetComponent<MeshFilter>());

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
