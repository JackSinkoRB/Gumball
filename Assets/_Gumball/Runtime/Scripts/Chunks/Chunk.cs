using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Gumball.Editor;
using UnityEditor;
#endif

namespace Gumball
{
#if UNITY_EDITOR
    [RequireComponent(typeof(ChunkEditorTools))]
#endif
    public class Chunk : MonoBehaviour
    {

        public event Action onTerrainChanged;
        
        [Header("Required")]
        [SerializeField] private SplineComputer splineComputer;
        [SerializeField] private ChunkTrafficManager trafficManager;

        [Header("Optional")]
        [SerializeField] private SplineMesh[] splineMeshes;

        [Header("Modify")]
        [HelpBox("For this value to take effect, you must rebuild the map data (for any maps that are using this chunk).", MessageType.Warning, true, true)]
        [SerializeField] private bool hasCustomLoadDistance;
        [Tooltip("The distance that the player must be within for the chunk to be loaded.")]
        [ConditionalField(nameof(hasCustomLoadDistance)), SerializeField] private float customLoadDistance = 3000;
        
        [Header("Debugging")]
        [ReadOnly, SerializeField] private GameObject currentTerrain;
        [ReadOnly, SerializeField] private ChunkMeshData chunkMeshData;

        public string UniqueID => GetComponent<UniqueIDAssigner>().UniqueID;

        public ChunkMeshData ChunkMeshData => chunkMeshData;
        public int LastPointIndex => splineComputer.pointCount - 1;
        public SplineComputer SplineComputer => splineComputer;
        public SplineMesh[] SplinesMeshes => splineMeshes;
        public SplineSample[] SplineSamples => splineSampleCollection.samples;

        public bool IsAutomaticTerrainRecreationDisabled { get; private set; }
        public SplineSample FirstSample { get; private set; }
        public SplineSample LastSample { get; private set; }
        public Vector3 FirstTangent { get; private set; }
        public Vector3 LastTangent { get; private set; }

        public bool HasCustomLoadDistance => hasCustomLoadDistance;
        public float CustomLoadDistance => customLoadDistance;

        public ChunkTrafficManager TrafficManager => trafficManager;
        
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
        
        [SerializeField, HideInInspector] private SampleCollection splineSampleCollection = new();

        [SerializeField] private ChunkObjectData[] chunkObjectData;

        public ChunkObjectData[] ChunkObjectData => chunkObjectData;
        
        public void SetChunkObjectData(ChunkObjectData[] chunkObjectData)
        {
            this.chunkObjectData = chunkObjectData;
            Debug.Log($"Setting {chunkObjectData.Length} chunk object data for {gameObject.name}");
        }

        public void SetTerrain(GameObject terrain)
        {
            currentTerrain = terrain;
            UpdateChunkMeshData();
            onTerrainChanged?.Invoke();
        }
        
        public void UpdateChunkMeshData()
        {
            DisableAutomaticTerrainRecreation(true);
            if (CurrentTerrain == null)
            {
                chunkMeshData = null;
                return;
            }

            chunkMeshData = new ChunkMeshData(this);
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

        public Vector3 GetCenterOfSpline()
        {
            float splineLength = splineComputer.CalculateLength();
            double travel = splineComputer.Travel(0.0, splineLength / 2f, Spline.Direction.Forward);
            Vector3 middle = splineComputer.EvaluatePosition(travel);
            return middle;
        }
        
        public (SplineSample, float) GetClosestSampleOnSpline(Vector3 fromPoint, bool flattenTheSpline = false)
        {
            UpdateSplineSampleData();
            float closestDistanceSqr = Mathf.Infinity;
            SplineSample closestSample = default;
            foreach (SplineSample sample in splineSampleCollection.samples)
            {
                float distance = flattenTheSpline
                        ? Vector2.SqrMagnitude(fromPoint.FlattenAsVector2() - sample.position.FlattenAsVector2())
                        : Vector3.SqrMagnitude(fromPoint - sample.position);
                if (distance < closestDistanceSqr)
                {
                    closestDistanceSqr = distance;
                    closestSample = sample;
                }
            }
            return (closestSample, closestDistanceSqr);
        }
        
        public (int, float) GetClosestSampleIndexOnSpline(Vector3 fromPoint, bool flattenTheSpline = false)
        {
            UpdateSplineSampleData();
            float closestDistanceSqr = Mathf.Infinity;
            int closestSampleIndex = -1;
            for (int index = 0; index < splineSampleCollection.samples.Length; index++)
            {
                SplineSample sample = splineSampleCollection.samples[index];
                
                float distance = flattenTheSpline
                    ? Vector2.SqrMagnitude(fromPoint.FlattenAsVector2() - sample.position.FlattenAsVector2())
                    : Vector3.SqrMagnitude(fromPoint - sample.position);
                if (distance < closestDistanceSqr)
                {
                    closestDistanceSqr = distance;
                    closestSampleIndex = index;
                }
            }

            return (closestSampleIndex, closestDistanceSqr);
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
