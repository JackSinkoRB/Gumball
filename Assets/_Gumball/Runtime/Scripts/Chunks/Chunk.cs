using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gumball
{
#if UNITY_EDITOR
    [RequireComponent(typeof(ChunkEditorTools))]
    [RequireComponent(typeof(UniqueIDAssigner))]
    [RequireComponent(typeof(SplineComputer))]
#endif
    public class Chunk : MonoBehaviour
    {

        public enum TerrainLOD
        {
            LOW,
            HIGH
        }

        public event Action onFullyLoaded;
        public event Action onBecomeAccessible;
        public event Action onBecomeInaccessible;
        public event Action onTerrainChanged;
        public event Action onChunkUnload;
        
        [SerializeField] private ChunkTrafficManager trafficManager;
        [SerializeField] private ChunkPowerpoleManager powerpoleManager;
        [SerializeField] private Collider[] barriers;

        [Header("Modify")]
        [HelpBox("For this value to take effect, you must rebuild the map data (for any maps that are using this chunk).", MessageType.Warning, true, true)]
        [SerializeField] private bool hasCustomLoadDistance;
        [Tooltip("The distance that the player must be within for the chunk to be loaded.")]
        [ConditionalField(nameof(hasCustomLoadDistance)), SerializeField] private float customLoadDistance = 3000;

        [Header("Terrains")]
        [Tooltip("The distance for the player to be within to use the high LOD.")]
        [SerializeField] private float terrainHighLODDistance = 150;
        [SerializeField, ReadOnly] private TerrainLOD currentLOD;
        [SerializeField, ReadOnly] private GameObject terrainHighLOD;
        [SerializeField, ReadOnly] private GameObject terrainLowLOD;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isFullyLoaded;
        [SerializeField, ReadOnly] private bool isAccessible;
        [Tooltip("A list of child spline meshes. These are automatically assigned when the chunk asset is saved.")]
        [SerializeField, ReadOnly] private SplineMesh[] splineMeshes;
        [SerializeField, ReadOnly] private ChunkMeshData chunkMeshData;
        [SerializeField, ReadOnly] private GameObject chunkDetector;
        [SerializeField, ReadOnly] private float splineLengthCached = -1;

        private SplineComputer splineComputer => GetComponent<SplineComputer>();
        public string UniqueID => GetComponent<UniqueIDAssigner>().UniqueID;

        public bool IsFullyLoaded => isFullyLoaded;
        public bool IsAccessible => isAccessible;
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
        public ChunkPowerpoleManager PowerpoleManager => powerpoleManager;
        
        public float SplineLengthCached
        {
            get
            {
                if (splineLengthCached < 0)
                    CalculateSplineLength();
                return splineLengthCached;
            }
        }

        public GameObject TerrainLowLOD
        {
            get
            {
                if (terrainLowLOD != null)
                    return terrainLowLOD;
                TryFindExistingTerrain(TerrainLOD.LOW);
                return terrainLowLOD;
            }
        }
        
        public GameObject TerrainHighLOD
        {
            get
            {
                if (terrainHighLOD != null)
                    return terrainHighLOD;
                TryFindExistingTerrain(TerrainLOD.HIGH);
                return terrainHighLOD;
            }
        }
        
        [SerializeField, HideInInspector] private SampleCollection splineSampleCollection = new();
        public SampleCollection SplineSampleCollection => splineSampleCollection;

        [SerializedDictionary("AssetKey", "Data")]
        public SerializedDictionary<string, List<ChunkObjectData>> ChunkObjectData = new();

        private const float secondsBetweenTerrainLODChecks = 1;
        private float timeOfLastLODCheck = -secondsBetweenTerrainLODChecks;
        private float timeSinceTerrainLODCheck => Time.realtimeSinceStartup - timeOfLastLODCheck;
        
        private void OnEnable()
        {
            splineComputer.updateMode = SplineComputer.UpdateMode.None; //make sure the spline computer doesn't update automatically at runtime
            splineComputer.sampleMode = SplineComputer.SampleMode.Uniform;
            
            InitialiseBarriers();
        }

        private void LateUpdate()
        {
            DoTerrainLODCheck();
        }

        /// <summary>
        /// Called once the chunk has completely loaded (ie. terrain has been applied, chunk objects have spawned etc.).
        /// </summary>
        public void OnFullyLoaded()
        {
            isFullyLoaded = true;
            onFullyLoaded?.Invoke();
            
            //move the chunk detector relative to the chunk (as it may have rotated)
            chunkDetector.transform.position = terrainHighLOD.transform.position.OffsetY(-500);
        }

        public void OnBecomeAccessible()
        {
            isAccessible = true;
            onBecomeAccessible?.Invoke();
        }

        public void OnBecomeInaccessible()
        {
            isAccessible = false;
            onBecomeInaccessible?.Invoke();
        }
        
        public void OnChunkUnload()
        {
            onChunkUnload?.Invoke();
        }
        
        public void SetChunkObjectData(Dictionary<string, List<ChunkObjectData>> chunkObjectData)
        {
            ChunkObjectData = new SerializedDictionary<string, List<ChunkObjectData>>(chunkObjectData);
            Debug.Log($"Setting {chunkObjectData.Keys.Count} chunk object data for {gameObject.name}");
        }

        public void SetMeshData(ChunkMeshData data)
        {
            chunkMeshData = data;
            data.SetChunk(this);
        }
        
        public void SetTerrain(TerrainLOD lod, GameObject terrain)
        {
            if (lod == TerrainLOD.HIGH)
            {
                terrainHighLOD = terrain;
                UpdateChunkMeshData();
                onTerrainChanged?.Invoke();
            }

            if (lod == TerrainLOD.LOW)
            {
                terrainLowLOD = terrain;
            }
        }

        public void SwitchTerrainLOD(TerrainLOD lod)
        {
            currentLOD = lod;
            terrainHighLOD.SetActive(lod == TerrainLOD.HIGH);
            terrainLowLOD.SetActive(lod == TerrainLOD.LOW);
        }
        
        public void UpdateChunkMeshData()
        {
            DisableAutomaticTerrainRecreation(true);
            if (TerrainHighLOD == null)
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
            double travel = splineComputer.Travel(0.0, SplineLengthCached / 2f, Spline.Direction.Forward);
            Vector3 middle = splineComputer.EvaluatePosition(travel);
            return middle;
        }
        
        /// <summary>
        /// Gets the distance from the start of the chunk to the closest spline sample to the specified point.
        /// <remarks>Must loop through all the spline samples in the chunk to get the index of the closest spline sample.</remarks>
        /// </summary>
        public float GetDistanceTravelledAlongSpline(Vector3 fromPoint)
        {
            if (splineComputer.sampleMode != SplineComputer.SampleMode.Uniform)
            {
                Debug.LogError("Could not get distance travelled along spline because the samples are not in uniform.");
                return -1;
            }

            float distanceBetweenSamples = SplineLengthCached / SplineSamples.Length; //assuming the spline sample distance is uniform
            
            var (closestSampleIndex, closestSampleDistance)  = GetClosestSampleIndexOnSpline(fromPoint);

            float totalDistance = distanceBetweenSamples * closestSampleIndex;
            return totalDistance;
        }
        
        public (SplineSample, float) GetClosestSampleOnSpline(Vector3 fromPoint)
        {
            UpdateSplineSampleData();
            return splineSampleCollection.GetClosestSampleOnSpline(fromPoint);
        }
        
        public (int, float) GetClosestSampleIndexOnSpline(Vector3 fromPoint)
        {
            UpdateSplineSampleData();
            return splineSampleCollection.GetClosestSampleIndexOnSpline(fromPoint);
        }
        
        public void TryCreateChunkDetector()
        {
            if (chunkDetector != null)
                return; //already exists

            if (terrainLowLOD == null || terrainHighLOD == null)
            {
                Debug.LogWarning($"Could not create chunk detector for {gameObject.name} because it is missing a terrain.");
                return;
            }
            
            chunkDetector = new GameObject("ChunkDetector");
            chunkDetector.gameObject.layer = (int) LayersAndTags.Layer.ChunkDetector;
            chunkDetector.transform.SetParent(transform);
            
            const float yOffset = -500;
            chunkDetector.transform.position = terrainLowLOD.transform.position.OffsetY(yOffset);

            MeshCollider meshCollider = chunkDetector.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = terrainLowLOD.GetComponent<MeshFilter>().sharedMesh;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Iterates through children to find SplineMeshes, and caches the references.
        /// </summary>
        public void FindSplineMeshes()
        {
            splineMeshes = transform.GetComponentsInAllChildren<SplineMesh>().ToArray();
            GlobalLoggers.ChunkLogger.Log($"Found {splineMeshes.Length} spline meshes under {gameObject.name}.");
        }
#endif

        private void TryFindExistingTerrain(TerrainLOD lod)
        {
            if ((lod == TerrainLOD.HIGH && terrainHighLOD != null)
                || (lod == TerrainLOD.LOW && terrainLowLOD != null))
                return; //already exists

            foreach (Transform child in transform)
            {
                if (child.tag.Equals(ChunkUtils.TerrainTag))
                {
                    if (lod == TerrainLOD.LOW)
                        terrainLowLOD = child.gameObject;
                    if (lod == TerrainLOD.HIGH)
                        terrainHighLOD = child.gameObject;
                    return;
                }
            }
        }

        private void DoTerrainLODCheck()
        {
            if (!ChunkManager.ExistsRuntime || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            if (timeSinceTerrainLODCheck < secondsBetweenTerrainLODChecks)
                return;

            timeOfLastLODCheck = Time.realtimeSinceStartup;

            TerrainLOD desiredLOD = GetDesiredLOD();
            
            if (currentLOD != desiredLOD)
                SwitchTerrainLOD(desiredLOD);
        }

        private TerrainLOD GetDesiredLOD()
        {
            //if player is on the chunk, return high
            //else, check if forward or backward is closer, then get the distance to whicher is closer

            Vector3 carPosition = WarehouseManager.Instance.CurrentCar.transform.position;
            Chunk chunkPlayerIsOn = WarehouseManager.Instance.CurrentCar.CurrentChunk;

            float shortestDistanceSqr;
            if (chunkPlayerIsOn == null)
                shortestDistanceSqr = Vector3.SqrMagnitude(carPosition - GetCenterOfSpline());
            else if (chunkPlayerIsOn == this)
                return TerrainLOD.HIGH;
            else
            {
                float distanceToStartSqr = Vector3.SqrMagnitude(carPosition - FirstSample.position);
                float distanceToEndSqr = Vector3.SqrMagnitude(carPosition - LastSample.position);

                if (distanceToStartSqr < distanceToEndSqr)
                    shortestDistanceSqr = distanceToStartSqr;
                else shortestDistanceSqr = distanceToEndSqr;
            }

            float highLODDistanceSqr = terrainHighLODDistance * terrainHighLODDistance;

            return shortestDistanceSqr <= highLODDistanceSqr ? TerrainLOD.HIGH : TerrainLOD.LOW;
        }
        
        private void InitialiseBarriers()
        {
            foreach (Collider barrier in barriers)
            {
                barrier.gameObject.layer = (int) LayersAndTags.Layer.Barrier;
                barrier.sharedMaterial = ChunkManager.Instance.SlipperyPhysicsMaterial;
            }
        }
        
        public void CalculateSplineLength()
        {
            splineLengthCached = splineComputer.CalculateLength();
        }
        
    }
}
