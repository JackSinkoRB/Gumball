using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    public class ChunkManager : Singleton<ChunkManager>
    {
        
        public event Action<Chunk> onChunkLoad;
        public event Action<Chunk> onChunkUnload;
        
        private const float timeBetweenLoadingChecks = 0.5f;

        [Header("Settings")]
        [Obsolete("To be removed - for testing only")]
        [SerializeField] private MapData testingMap;

        [Header("Debugging")]
        [ReadOnly, SerializeField] private MapData currentMap;
        [Tooltip("The range of chunk indexes (in terms of the map data) that are currently loaded OR in the loading process.")]
        [ReadOnly, SerializeField] private MinMaxInt loadingOrLoadedChunksIndices;
        [Tooltip("The range of chunk indexes (in terms of the map data) that are currently loaded.")]
        [ReadOnly, SerializeField] private MinMaxInt loadedChunksIndices;
        [ReadOnly, SerializeField] private List<LoadedChunkData> currentCustomLoadedChunks = new();
        [Tooltip("A list of the current loaded chunks, in order of map index.\nDoes NOT include custom loaded chunks.")]
        [SerializeField] private List<LoadedChunkData> currentChunks = new();
        
        [Obsolete("To be removed - for testing only")]
        public MapData TestingMap => testingMap;
        public MapData CurrentMap => currentMap;
        /// <summary>
        /// A list of the current loaded chunks, in order of map index.
        /// <remarks>Does NOT include custom loaded chunks.</remarks>
        /// </summary>
        public ReadOnlyCollection<LoadedChunkData> CurrentChunks => currentChunks.AsReadOnly();
        public ReadOnlyCollection<LoadedChunkData> CurrentCustomLoadedChunks => currentCustomLoadedChunks.AsReadOnly();
        
        private readonly TrackedCoroutine distanceLoadingCoroutine = new();
        private float timeSinceLastLoadCheck;
        private Chunk chunkPlayerIsOnCached;
        private int lastFramePlayerChunkWasCached = -1;
        
        public bool HasLoaded { get; private set; }
        public MinMaxInt LoadedChunksIndices => loadedChunksIndices;
        public TrackedCoroutine DistanceLoadingCoroutine => distanceLoadingCoroutine;
        
        /// <returns>The chunk the player is on, else null if it can't be found.</returns>
        public Chunk GetChunkPlayerIsOn()
        {
            if (lastFramePlayerChunkWasCached != Time.frameCount)
            {
                lastFramePlayerChunkWasCached = Time.frameCount;
                
                if (!PlayerCarManager.ExistsRuntime || PlayerCarManager.Instance.CurrentCar == null)
                {
                    Debug.LogWarning("Can't get the chunk the player is in because the current car doesn't exist.");
                    chunkPlayerIsOnCached = null;
                }

                //raycast down to terrain
                if (Physics.Raycast(PlayerCarManager.Instance.CurrentCar.transform.position, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.ChunkDetector)))
                    chunkPlayerIsOnCached = hitDown.transform.parent.GetComponent<Chunk>();
                else chunkPlayerIsOnCached = null;
            }
            
            return chunkPlayerIsOnCached;
        }
        
        /// <summary>
        /// Returns whether the chunk is within the load distance radius. Can be false if the chunk is custom loaded.
        /// </summary>
        public bool IsChunkWithinLoadRadius(int chunkMapIndex)
        {
            return chunkMapIndex >= loadingOrLoadedChunksIndices.Min && chunkMapIndex <= loadingOrLoadedChunksIndices.Max;
        }
        
        /// <summary>
        /// Returns whether the chunk is within the load distance radius. Can be false if the chunk is custom loaded.
        /// </summary>
        public bool IsChunkWithinLoadRadius(Chunk chunk)
        {
            int chunkMapIndex = GetMapIndexOfLoadedChunk(chunk);
            return IsChunkWithinLoadRadius(chunkMapIndex);
        }

        /// <summary>
        /// Returns whether the chunk is accessible to the player (ie. is connected with the chunks that the player is near).
        /// </summary>
        public bool CanPlayerAccessChunk(Chunk chunk)
        {
            int currentChunkMapIndex = GetMapIndexOfLoadedChunk(chunk);

            if (IsChunkWithinLoadRadius(chunk))
                return true;

            int chunkAheadIndex = loadedChunksIndices.Max + 1;
            while (chunkAheadIndex <= currentChunkMapIndex)
            {
                if (chunkAheadIndex == currentChunkMapIndex)
                    return true;
                
                if (GetLoadedChunkDataByMapIndex(chunkAheadIndex) == null)
                    break;

                chunkAheadIndex++;
            }
            
            int chunkBehindIndex = loadedChunksIndices.Min - 1;
            while (chunkBehindIndex >= currentChunkMapIndex)
            {
                if (chunkBehindIndex == currentChunkMapIndex)
                    return true;
                
                if (GetLoadedChunkDataByMapIndex(chunkBehindIndex) == null)
                    break;

                chunkBehindIndex++;
            }

            return false;
        }

        public LoadedChunkData? GetLoadedChunkDataByMapIndex(int chunkMapIndex)
        {
            foreach (LoadedChunkData data in currentChunks)
            {
                if (data.MapIndex.Equals(chunkMapIndex))
                    return data;
            }
            
            foreach (LoadedChunkData data in currentCustomLoadedChunks)
            {
                if (data.MapIndex.Equals(chunkMapIndex))
                    return data;
            }

            return null;
        } 

        /// <summary>
        /// Get the map index of the supplied chunk.
        /// <remarks>The chunk must be loaded.</remarks>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the chunk is not currently loaded.</exception>
        public int GetMapIndexOfLoadedChunk(Chunk chunk)
        {
            foreach (LoadedChunkData data in currentChunks)
            {
                if (data.Chunk.Equals(chunk))
                    return data.MapIndex;
            }

            foreach (LoadedChunkData data in currentCustomLoadedChunks)
            {
                if (data.Chunk.Equals(chunk))
                    return data.MapIndex;
            }

            Debug.LogWarning($"The chunk {chunk.name} is not currently loaded.");
            return -1;
        }
        
        public IEnumerator LoadMap(MapData map)
        {
            GlobalLoggers.LoadingLogger.Log($"Loading map '{map.name}'");
            HasLoaded = false;
            currentMap = map;
            currentChunks.Clear();

            map.OnMapLoad();

            //load the chunks in range
            distanceLoadingCoroutine.SetCoroutine(LoadChunksAroundPosition(map.VehicleStartingPosition));
            yield return distanceLoadingCoroutine.Coroutine;
            
            HasLoaded = true;
        }

        private void LateUpdate()
        {
            if (currentMap != null)
                DoLoadingCheck();
        }

        public void DoLoadingCheck(bool force = false)
        {
            if (!PlayerCarManager.ExistsRuntime || PlayerCarManager.Instance.CurrentCar == null)
                return;

            if (distanceLoadingCoroutine.IsPlaying)
                return;

            if (!force)
            {
                timeSinceLastLoadCheck += Time.deltaTime;
                if (timeSinceLastLoadCheck < timeBetweenLoadingChecks)
                    return;
            }

            //can perform loading check
            timeSinceLastLoadCheck = 0;
            distanceLoadingCoroutine.SetCoroutine(LoadChunksAroundPosition(PlayerCarManager.Instance.CurrentCar.transform.position));
        }

        private IEnumerator LoadChunksAroundPosition(Vector3 position)
        {
            TrackedCoroutine firstChunk = null;
            bool firstChunkNeedsLoading = loadingOrLoadedChunksIndices.Min == 0 && loadingOrLoadedChunksIndices.Max == 0;
            if (firstChunkNeedsLoading)
            {
                //load the first chunk since none are loaded
                firstChunk = new TrackedCoroutine(LoadFirstChunk());
            }
            
            List<TrackedCoroutine> customLoadChunks = UpdateCustomLoadDistanceChunks(position);
            List<TrackedCoroutine> chunksBefore = LoadChunksInDirection(position, ChunkUtils.LoadDirection.BEFORE);
            List<TrackedCoroutine> chunksAfter = LoadChunksInDirection(position, ChunkUtils.LoadDirection.AFTER);
            
            yield return new WaitUntil(() => (firstChunk == null || !firstChunk.IsPlaying)
                                             && customLoadChunks.AreAllComplete()
                                             && chunksBefore.AreAllComplete()
                                             && chunksAfter.AreAllComplete());
            
            UnloadChunksAroundPosition(position);
        }

        private IEnumerator LoadFirstChunk()
        {
            loadingOrLoadedChunksIndices = new MinMaxInt(currentMap.StartingChunkIndex, currentMap.StartingChunkIndex);
            yield return LoadChunkAsync(currentMap.StartingChunkIndex, 
                currentMap.GetChunkData(currentMap.StartingChunkIndex).HasCustomLoadDistance
                    ? ChunkUtils.LoadDirection.CUSTOM : ChunkUtils.LoadDirection.AFTER);
        }

        private List<TrackedCoroutine> UpdateCustomLoadDistanceChunks(Vector3 position)
        {
            List<TrackedCoroutine> trackedCoroutines = new List<TrackedCoroutine>();
            
            foreach (int chunkIndexWithCustomLoadDistance in currentMap.ChunksWithCustomLoadDistance)
            {
                ChunkMapData chunkMapData = currentMap.GetChunkData(chunkIndexWithCustomLoadDistance);
                float customLoadDistanceSqr = chunkMapData.CustomLoadDistance * chunkMapData.CustomLoadDistance;
                float distanceToStartSqr = (chunkMapData.SplineStartPosition - position).sqrMagnitude;
                float distanceToEndSqr = (chunkMapData.SplineEndPosition - position).sqrMagnitude;

                LoadedChunkData? customLoadedData = GetCustomLoadedChunkData(chunkIndexWithCustomLoadDistance);
                bool isChunkCustomLoaded = customLoadedData != null;

                bool isWithinLoadDistance = distanceToStartSqr < customLoadDistanceSqr
                                            || distanceToEndSqr < customLoadDistanceSqr;
                
                if (isWithinLoadDistance && !isChunkCustomLoaded)
                {
                    //load
                    trackedCoroutines.Add(new TrackedCoroutine(LoadChunkAsync(chunkIndexWithCustomLoadDistance, ChunkUtils.LoadDirection.CUSTOM)));
                }
                if (!isWithinLoadDistance && isChunkCustomLoaded)
                {
                    //should not be loaded - unload if so
                    UnloadChunk(customLoadedData.Value);
                }
            }

            return trackedCoroutines;
        }

        private void UnloadChunksAroundPosition(Vector3 position)
        {
            List<int> chunksToUnload = new List<int>();
            float chunkLoadDistanceSqr = currentMap.ChunkLoadDistance * currentMap.ChunkLoadDistance;

            //check to unload chunks ahead
            for (int chunkAheadIndex = loadingOrLoadedChunksIndices.Max; chunkAheadIndex >= loadingOrLoadedChunksIndices.Min; chunkAheadIndex--)
            {
                ChunkMapData chunkData = currentMap.GetChunkData(chunkAheadIndex);
                Vector3 chunkPosition = chunkData.SplineStartPosition;

                float distanceToChunk = Vector3.SqrMagnitude(position - chunkPosition);
                if (distanceToChunk <= chunkLoadDistanceSqr)
                {
                    //is good!
                    break;
                }
                
                if (!chunkData.HasCustomLoadDistance)
                    chunksToUnload.Add(chunkAheadIndex);

                loadingOrLoadedChunksIndices.Max--;
                if (loadedChunksIndices.Max > loadingOrLoadedChunksIndices.Max)
                    loadedChunksIndices.Max = loadingOrLoadedChunksIndices.Max;

                //check if next chunk is custom loaded, and ignore it
                while (GetCustomLoadedChunkData(loadingOrLoadedChunksIndices.Max) != null)
                {
                    loadingOrLoadedChunksIndices.Max--;
                    if (loadedChunksIndices.Max > loadingOrLoadedChunksIndices.Max)
                        loadedChunksIndices.Max = loadingOrLoadedChunksIndices.Max;
                }
            }
            
            //check to unload chunks behind
            for (int chunkBehindIndex = loadingOrLoadedChunksIndices.Min; chunkBehindIndex < loadingOrLoadedChunksIndices.Max; chunkBehindIndex++)
            {
                ChunkMapData chunkData = currentMap.GetChunkData(chunkBehindIndex);
                Vector3 chunkPosition = chunkData.SplineEndPosition;
                
                float distanceToChunk = Vector3.SqrMagnitude(position - chunkPosition);
                if (distanceToChunk <= chunkLoadDistanceSqr)
                {
                    //is good!
                    break;
                }
                
                if (!chunkData.HasCustomLoadDistance)
                    chunksToUnload.Add(chunkBehindIndex);
                
                loadingOrLoadedChunksIndices.Min++;
                if (loadedChunksIndices.Min < loadingOrLoadedChunksIndices.Min)
                    loadedChunksIndices.Min = loadingOrLoadedChunksIndices.Min;
                
                //check if next chunk is custom loaded, and ignore it
                while (GetCustomLoadedChunkData(loadingOrLoadedChunksIndices.Min) != null)
                {
                    loadingOrLoadedChunksIndices.Min++;
                    if (loadedChunksIndices.Min < loadingOrLoadedChunksIndices.Min)
                        loadedChunksIndices.Min = loadingOrLoadedChunksIndices.Min;
                }
            }

            foreach (int indexToRemove in chunksToUnload)
            {
                if (currentChunks.Count == 1)
                    break; //keep at least 1 chunk
                
                LoadedChunkData? chunkData = GetLoadedChunkDataByMapIndex(indexToRemove);
                if (chunkData == null)
                {
                    Debug.LogError($"Trying to unload chunk at index {indexToRemove}, although it isn't loaded.");
                    continue;
                }
                UnloadChunk(chunkData.Value);
            }
        }

        private LoadedChunkData? GetCustomLoadedChunkData(int mapIndex)
        {
            foreach (LoadedChunkData loadData in currentCustomLoadedChunks)
            {
                if (loadData.MapIndex == mapIndex)
                    return loadData;
            }

            return null;
        }
        
        private List<TrackedCoroutine> LoadChunksInDirection(Vector3 startingPosition, ChunkUtils.LoadDirection direction)
        {
            List<TrackedCoroutine> trackedCoroutines = new List<TrackedCoroutine>();
            
            float chunkLoadDistanceSqr = currentMap.ChunkLoadDistance * currentMap.ChunkLoadDistance;
            
            Vector3 endOfChunk = direction == ChunkUtils.LoadDirection.AFTER
                ? currentMap.GetChunkData(loadedChunksIndices.Max).SplineEndPosition
                : currentMap.GetChunkData(loadedChunksIndices.Min).SplineStartPosition;
            
            float distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - endOfChunk);
            while (distanceToEndOfChunk < chunkLoadDistanceSqr)
            {
                //load next chunk
                int indexToLoad = direction == ChunkUtils.LoadDirection.AFTER
                    ? loadingOrLoadedChunksIndices.Max + 1
                    : loadingOrLoadedChunksIndices.Min - 1;
                
                if (indexToLoad < 0 || indexToLoad >= currentMap.RuntimeChunkAssetKeys.Length)
                {
                    //end of map - no more chunks to load
                    return trackedCoroutines;
                }

                LoadedChunkData? customLoadedChunk = GetCustomLoadedChunkData(indexToLoad);
                bool chunkIsCustomLoaded = customLoadedChunk != null;
                if (chunkIsCustomLoaded)
                {
                    //update the distance
                    ChunkMapData customLoadedChunkData = currentMap.GetChunkData(indexToLoad);
                    Vector3 furthestPointOnCustomLoadedChunk = direction == ChunkUtils.LoadDirection.AFTER ? customLoadedChunkData.SplineEndPosition : customLoadedChunkData.SplineStartPosition;
                    distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnCustomLoadedChunk);

                    RegisterLoadedChunkIndex(indexToLoad);
                    RegisterLoadingOrLoadedChunkIndex(indexToLoad);
                    continue;
                }

                trackedCoroutines.Add(new TrackedCoroutine(LoadChunkAsync(indexToLoad, direction)));
                RegisterLoadingOrLoadedChunkIndex(indexToLoad);

                //update the distance
                ChunkMapData chunkData = currentMap.GetChunkData(indexToLoad);
                Vector3 furthestPointOnChunk = direction == ChunkUtils.LoadDirection.AFTER ? chunkData.SplineEndPosition : chunkData.SplineStartPosition;
                distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnChunk);
            }

            return trackedCoroutines;
        }

        private void RegisterLoadingOrLoadedChunkIndex(int index)
        {
            if (index > loadingOrLoadedChunksIndices.Max)
                loadingOrLoadedChunksIndices.Max = index;
            if (index < loadingOrLoadedChunksIndices.Min)
                loadingOrLoadedChunksIndices.Min = index;
        }

        private void RegisterLoadedChunkIndex(int index)
        {
            if (index > loadedChunksIndices.Max)
                loadedChunksIndices.Max = index;
            if (index < loadedChunksIndices.Min)
                loadedChunksIndices.Min = index;
        }
        
        private IEnumerator LoadChunkAsync(int mapIndex, ChunkUtils.LoadDirection loadDirection)
        {
            string chunkAddressableKey = currentMap.RuntimeChunkAssetKeys[mapIndex];
            
#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Loading chunk '{chunkAddressableKey}'...");
#endif
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkAddressableKey);
            yield return handle;
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), transform);
            instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            Chunk chunk = instantiatedChunk.GetComponent<Chunk>();
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to instantiate.");
            stopwatch.Restart();
            
            if (mapIndex == loadedChunksIndices.Max + 1 || mapIndex == loadedChunksIndices.Min - 1)
                RegisterLoadedChunkIndex(mapIndex);
            
            LoadedChunkData loadedChunkData = new LoadedChunkData(chunk, chunkAddressableKey, mapIndex);

            if (loadDirection == ChunkUtils.LoadDirection.CUSTOM)
            {
                currentCustomLoadedChunks.Add(loadedChunkData);
            } else
            {
                currentChunks.Add(loadedChunkData);
            }

            ChunkMapData chunkMapData = currentMap.GetChunkData(mapIndex);
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to get chunk data.");

            if (HasLoaded)
                yield return new WaitForEndOfFrame();
            stopwatch.Restart();

            chunkMapData.ApplyToChunk(chunk);
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to apply chunk data.");
            
            if (HasLoaded)
                yield return new WaitForEndOfFrame();
            stopwatch.Restart();

            //TODO: can this just be unity_editor?
            //should create a copy of the mesh so it doesn't directly edit the saved mesh in runtime
            MeshFilter meshFilter = chunk.TerrainHighLOD.GetComponent<MeshFilter>();
            Mesh meshCopy = Instantiate(meshFilter.sharedMesh);
            chunk.TerrainHighLOD.GetComponent<MeshFilter>().sharedMesh = meshCopy;
            chunk.TerrainHighLOD.GetComponent<MeshCollider>().sharedMesh = meshCopy;
            
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to update components.");
            stopwatch.Restart();

            if (HasLoaded)
                yield return new WaitForEndOfFrame();
            yield return LoadChunkObjects(chunk);
            
            stopwatch.Restart();
            onChunkLoad?.Invoke(chunk);
            
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to invoke events.");

#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Chunk loading '{chunkAddressableKey}' complete.");
#endif
        }

        private IEnumerator LoadChunkObjects(Chunk chunk)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            //load all the chunk object assets
            Dictionary<string, AsyncOperationHandle<GameObject>> handlesLookup = new();
            
            foreach (string assetKey in chunk.ChunkObjectData.Keys)
            {
                if (assetKey.IsNullOrEmpty())
                {
                    Debug.LogError($"Could not load as the asset key is missing.");
                    continue;
                }

                GlobalLoggers.LoadingLogger.Log($"Loading handle for {assetKey} at {stopwatch.ElapsedMilliseconds}ms.");
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetKey);
                handlesLookup[assetKey] = handle;
            }

            List<AsyncOperationHandle<GameObject>> allHandles = handlesLookup.Values.ToList();
            yield return new WaitUntil(() => allHandles.TrueForAll(h => h.Status == AsyncOperationStatus.Succeeded));

            stopwatch.Restart();
            const float maxTimeAllowedPerFrameMs = 4;
            
            //instantiate all the instances across multiple frames
            foreach (string assetKey in chunk.ChunkObjectData.Keys)
            {
                AsyncOperationHandle<GameObject> handle = handlesLookup[assetKey];
                foreach (ChunkObjectData chunkObjectData in chunk.ChunkObjectData[assetKey])
                {
                    GameObject chunkObject = chunkObjectData.LoadIntoChunk(handle, chunk);
                    GlobalLoggers.LoadingLogger.Log($"Loaded {chunkObject.name} at {stopwatch.ElapsedMilliseconds}ms.");
                    
                    if (HasLoaded && stopwatch.ElapsedMilliseconds > maxTimeAllowedPerFrameMs)
                    {
                        GlobalLoggers.LoadingLogger.Log($"Reached max for this frame, waiting until next frame.");
                        yield return new WaitForEndOfFrame();
                        stopwatch.Restart();
                    }
                }
            }
        }
        
        private void UnloadChunk(LoadedChunkData chunkData)
        {
            onChunkUnload?.Invoke(chunkData.Chunk);
            currentChunks.Remove(chunkData);
            Destroy(chunkData.Chunk.gameObject);
            currentCustomLoadedChunks.Remove(chunkData);
        }
        
    }
}
