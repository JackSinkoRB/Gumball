using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public enum ChunkLOD
        {
            LOW,
            HIGH
        }

        public event Action onFullyLoaded;
        public event Action onBecomeAccessible;
        public event Action onBecomeInaccessible;
        public event Action onTerrainChanged;
        public event Action onChunkUnload;
        
        private const float childMeshRendererDistance = 800;

        [SerializeField] private ChunkTrafficManager trafficManager;
        [SerializeField] private ChunkPowerpoleManager powerpoleManager;
        [SerializeField] private Collider[] barriers;
        
        [Header("Modify")]
        [HelpBox("For this value to take effect, you must rebuild the map data (for any maps that are using this chunk).", MessageType.Warning, onlyShowWhenDefaultValue: true, inverse: true)]
        [SerializeField] private bool hasCustomLoadDistance;
        [Tooltip("The distance that the player must be within for the chunk to be loaded.")]
        [ConditionalField(nameof(hasCustomLoadDistance)), SerializeField] private float customLoadDistance = 3000;
        [Tooltip("If the players distance from the road is equal or greater to this distance, the player is reset.")]
        [SerializeField] private float distanceFromRoadSplineToResetPlayer = 50;
        [Tooltip("The distance racers look ahead for the next racing line to start interpolating to its position.")]
        [SerializeField] private float nextRacingLineInterpolateDistance = 65;
        
        [Header("Terrains")]
        [Tooltip("The distance for the player to be within to use the high LOD.")]
        [SerializeField] private float terrainHighLODDistance = 500;
        [SerializeField, ReadOnly] private ChunkLOD currentTerrainLOD;
        [SerializeField, ReadOnly] private ChunkLOD currentChildMeshLOD;
        [SerializeField, ReadOnly] private GameObject terrainHighLOD;
        [SerializeField, ReadOnly] private GameObject terrainLowLOD;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isFullyLoaded;
        [SerializeField, ReadOnly] private bool isAccessible;
        [SerializeField, ReadOnly] private ChunkMeshData chunkMeshData;
        [SerializeField, ReadOnly] private GameObject chunkDetector;
        [SerializeField, ReadOnly] private float splineLengthCached = -1;

        [SerializeField, HideInInspector] private MeshRenderer[] childMeshRenderers;
        private bool hasInitialisedLODs;
        
        private SplineComputer splineComputer => GetComponent<SplineComputer>();
        public string UniqueID => GetComponent<UniqueIDAssigner>().UniqueID;

        public bool IsFullyLoaded => isFullyLoaded;
        public bool IsAccessible => isAccessible;
        public GameObject ChunkDetector => chunkDetector;
        public ChunkMeshData ChunkMeshData => chunkMeshData;
        public int LastPointIndex => splineComputer.pointCount - 1;
        public SplineComputer SplineComputer => splineComputer;
        public SplineSample[] SplineSamples => splineSampleCollection.samples;
        
        public bool IsAutomaticTerrainRecreationDisabled { get; private set; }
        public SplineSample FirstSample { get; private set; }
        public SplineSample LastSample { get; private set; }
        public Vector3 FirstTangent { get; private set; }
        public Vector3 LastTangent { get; private set; }

        public bool HasCustomLoadDistance => hasCustomLoadDistance;
        public float CustomLoadDistance => customLoadDistance;
        public float DistanceFromRoadSplineToResetPlayer => distanceFromRoadSplineToResetPlayer;
        public float NextRacingLineInterpolateDistance => nextRacingLineInterpolateDistance;
        
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
                TryFindExistingTerrain(ChunkLOD.LOW);
                return terrainLowLOD;
            }
        }
        
        public GameObject TerrainHighLOD
        {
            get
            {
                if (terrainHighLOD != null)
                    return terrainHighLOD;
                TryFindExistingTerrain(ChunkLOD.HIGH);
                return terrainHighLOD;
            }
        }
        
        [SerializeField, HideInInspector] private SampleCollection splineSampleCollection = new();
        public SampleCollection SplineSampleCollection => splineSampleCollection;

        private const float secondsBetweenLODChecks = 1;
        private float timeOfLastLODCheck = -secondsBetweenLODChecks;
        private float timeSinceLODCheck => Time.realtimeSinceStartup - timeOfLastLODCheck;
        
        private void OnEnable()
        {
            splineComputer.updateMode = SplineComputer.UpdateMode.None; //make sure the spline computer doesn't update automatically at runtime
            splineComputer.sampleMode = SplineComputer.SampleMode.Uniform;
            
            InitialiseBarriers();
        }

        private void LateUpdate()
        {
            DoLODCheck();
        }

        /// <summary>
        /// Called once the chunk has completely loaded (ie. terrain has been applied, chunk objects have spawned etc.).
        /// </summary>
        public void OnFullyLoaded()
        {
            CacheChildMeshRenderers();
            
            isFullyLoaded = true;
            onFullyLoaded?.Invoke();
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

        public void SetMeshData(ChunkMeshData data)
        {
            chunkMeshData = data;
            data.SetChunk(this);
        }
        
        public void SetTerrain(ChunkLOD lod, GameObject terrain)
        {
            if (lod == ChunkLOD.HIGH)
            {
                terrainHighLOD = terrain;
                UpdateChunkMeshData();
                onTerrainChanged?.Invoke();
            }

            if (lod == ChunkLOD.LOW)
            {
                terrainLowLOD = terrain;
            }
        }

        public void SwitchTerrainLOD(ChunkLOD lod)
        {
            currentTerrainLOD = lod;
            terrainHighLOD.GetComponent<MeshRenderer>().enabled = lod == ChunkLOD.HIGH;
            terrainLowLOD.GetComponent<MeshRenderer>().enabled = lod == ChunkLOD.LOW;
            
            //ensure they're enabled
            if (!terrainHighLOD.activeSelf)
                terrainHighLOD.SetActive(true);
            if (!terrainLowLOD.activeSelf)
                terrainLowLOD.SetActive(true);
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
            
            var (closestSampleIndex, closestSampleDistance) = GetClosestSampleIndexOnSpline(fromPoint);

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
            {
                //destroy old one
                DestroyImmediate(chunkDetector);
            }

            if (terrainLowLOD == null || terrainHighLOD == null)
            {
                Debug.LogWarning($"Could not create chunk detector for {gameObject.name} because it is missing a terrain.");
                return;
            }
            
            chunkDetector = new GameObject("ChunkDetector");
            chunkDetector.gameObject.layer = (int) LayersAndTags.Layer.ChunkDetector;
            chunkDetector.transform.SetParent(transform);
            
            chunkDetector.transform.position = terrainLowLOD.transform.position;

            MeshCollider meshCollider = chunkDetector.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = terrainLowLOD.GetComponent<MeshFilter>().sharedMesh;
        }
        
        private void CacheChildMeshRenderers()
        {
            HashSet<MeshRenderer> hashSet = new();
            foreach (MeshRenderer childMeshRenderer in transform.GetComponentsInAllChildren<MeshRenderer>())
            {
                if (childMeshRenderer.gameObject == terrainHighLOD || childMeshRenderer.gameObject == terrainLowLOD)
                    continue;
                
                if (childMeshRenderer.gameObject.tag.Equals(LayersAndTags.Tag.DontHideMeshWhenFarAway.ToString()))
                    continue;
                
                hashSet.Add(childMeshRenderer);
            }
            childMeshRenderers = hashSet.ToArray();
        }
        
        private void TryFindExistingTerrain(ChunkLOD lod)
        {
            if ((lod == ChunkLOD.HIGH && terrainHighLOD != null)
                || (lod == ChunkLOD.LOW && terrainLowLOD != null))
                return; //already exists

            foreach (Transform child in transform)
            {
                if (child.tag.Equals(ChunkUtils.TerrainTag))
                {
                    if (lod == ChunkLOD.LOW)
                        terrainLowLOD = child.gameObject;
                    if (lod == ChunkLOD.HIGH)
                        terrainHighLOD = child.gameObject;
                    return;
                }
            }
        }

        private void DoLODCheck()
        {
            if (!ChunkManager.ExistsRuntime || WarehouseManager.Instance.CurrentCar == null)
                return;

            if (!isFullyLoaded)
                return;
            
            if (timeSinceLODCheck < secondsBetweenLODChecks)
                return;

            timeOfLastLODCheck = Time.realtimeSinceStartup;

            float shortestDistanceSqr = GetShortestDistanceToChunk();
            
            //terrain LOD:
            float highLODDistanceSqr = terrainHighLODDistance * terrainHighLODDistance;
            ChunkLOD desiredTerrainLOD = shortestDistanceSqr <= highLODDistanceSqr ? ChunkLOD.HIGH : ChunkLOD.LOW;
            if (currentTerrainLOD != desiredTerrainLOD || !hasInitialisedLODs)
                SwitchTerrainLOD(desiredTerrainLOD);

            //child mesh LOD:
            const float childMeshRendererDistanceSqr = childMeshRendererDistance * childMeshRendererDistance;
            ChunkLOD desiredChildMeshLOD = shortestDistanceSqr <= childMeshRendererDistanceSqr ? ChunkLOD.HIGH : ChunkLOD.LOW;
            if (currentChildMeshLOD != desiredChildMeshLOD || !hasInitialisedLODs)
            {
                currentChildMeshLOD = desiredTerrainLOD;
                foreach (MeshRenderer meshRenderer in childMeshRenderers)
                    meshRenderer.enabled = desiredChildMeshLOD == ChunkLOD.HIGH;
            }

            hasInitialisedLODs = true;
        }

        private float GetShortestDistanceToChunk()
        {
            Vector3 carPosition = WarehouseManager.Instance.CurrentCar.transform.position;
            Chunk chunkPlayerIsOn = WarehouseManager.Instance.CurrentCar.CurrentChunk;
            
            if (chunkPlayerIsOn == null)
                return Vector3.SqrMagnitude(carPosition - GetCenterOfSpline());
                
            if (chunkPlayerIsOn == this)
                return 0;
            
            float distanceToStartSqr = Vector3.SqrMagnitude(carPosition - FirstSample.position);
            float distanceToEndSqr = Vector3.SqrMagnitude(carPosition - LastSample.position);
            return distanceToStartSqr < distanceToEndSqr ? distanceToStartSqr : distanceToEndSqr;
        }
        
        private void InitialiseBarriers()
        {
            foreach (Collider barrier in barriers)
            {
                if (barrier == null)
                {
                    Debug.LogWarning($"Chunk {name} has a barrier that is null.");
                    continue;
                }
                
                barrier.gameObject.layer = (int) LayersAndTags.Layer.Barrier;
                barrier.sharedMaterial = ChunkManager.Instance.SlipperyPhysicsMaterial;
            }
        }
        
        public void CalculateSplineLength()
        {
            splineLengthCached = splineComputer.CalculateLength();
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //draw the interpolation distance
            if (nextRacingLineInterpolateDistance > 0)
            {
                if (SplineSamples == null || SplineSamples.Length == 0)
                    UpdateSplineSampleData();

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(LastSample.position - LastSample.forward * nextRacingLineInterpolateDistance, 1f);
            }
        }
#endif
        
    }
}
