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
        
        private const float timeBetweenLoadingChecks = 0.5f;

        [Header("Settings")]
        [Obsolete("To be removed - for testing only")]
        [SerializeField] private MapData testingMap;

        [Header("Debugging")]
        [ReadOnly, SerializeField] private MapData currentMap;
        [Tooltip("The range of chunk indexes (in terms of the map data) that are currently loaded OR in the loading process.")]
        [ReadOnly, SerializeField] private MinMaxInt loadingOrLoadedChunksIndices;
        [ReadOnly, SerializeField] private MinMaxInt accessibleChunksIndices;
        [SerializeField, ReadOnly] private List<LoadedChunkData> chunksWaitingToBeAccessible = new();
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
        public ReadOnlyCollection<LoadedChunkData> ChunksWaitingToBeAccessible => chunksWaitingToBeAccessible.AsReadOnly();

        private float timeSinceLastLoadCheck;
        public readonly TrackedCoroutine distanceLoadingCoroutine = new();
        private readonly List<TrackedCoroutine> customChunkLoading = new();
        private readonly List<TrackedCoroutine> chunksBeforeLoading = new();
        private readonly List<TrackedCoroutine> chunksAfterLoading = new();
        
        private Chunk chunkPlayerIsOnCached;
        private int lastFramePlayerChunkWasCached = -1;

        public bool HasLoaded;
        public bool IsLoadingChunks { get; private set; }
        public MinMaxInt LoadingOrLoadedChunksIndices => loadingOrLoadedChunksIndices;
        public MinMaxInt AccessibleChunksIndices => accessibleChunksIndices;
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
                else
                {
                    //raycast down to terrain
                    if (PlayerCarManager.Instance.CurrentCar.gameObject.scene.GetPhysicsScene().Raycast(PlayerCarManager.Instance.CurrentCar.transform.position, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.ChunkDetector)))
                    {
                        chunkPlayerIsOnCached = hitDown.transform.parent.GetComponent<Chunk>();
                    }
                    else
                    {
                        chunkPlayerIsOnCached = null;
                    }
                }
            }
            
            return chunkPlayerIsOnCached;
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
        public int GetMapIndexOfLoadedChunk(Chunk chunk)
        {
            if (chunk == null)
                throw new NullReferenceException("Cannot get map index because the chunk is null.");
            
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
            GlobalLoggers.ChunkLogger.Log("Initial loading check!");
            distanceLoadingCoroutine.SetCoroutine(LoadChunksAroundPosition(map.VehicleStartingPosition));
            yield return distanceLoadingCoroutine.Coroutine;
            GlobalLoggers.ChunkLogger.Log("Initial loading check completed!");

            HasLoaded = true;
        }

        private void LateUpdate()
        {
            if (currentMap != null)
                DoLoadingCheck();
        }
        
        private void DoLoadingCheck()
        {
            if (!PlayerCarManager.ExistsRuntime || PlayerCarManager.Instance.CurrentCar == null)
                return;

            if (IsLoadingChunks) //ensure only 1 loading check at a time
                return;

            timeSinceLastLoadCheck += Time.deltaTime;
            if (timeSinceLastLoadCheck < timeBetweenLoadingChecks)
                return;

            GlobalLoggers.ChunkLogger.Log("Doing loading check 4");

            //can perform loading check
            timeSinceLastLoadCheck = 0;
            distanceLoadingCoroutine.SetCoroutine(LoadChunksAroundPosition(PlayerCarManager.Instance.CurrentCar.transform.position));
        }

        public IEnumerator LoadChunksAroundPosition(Vector3 position)
        {
            IsLoadingChunks = true;
            GlobalLoggers.ChunkLogger.Log($"Doing loading check 5 - {position}");
            
            TrackedCoroutine firstChunk = null;
            bool firstChunkNeedsLoading = loadingOrLoadedChunksIndices.Min == 0 && loadingOrLoadedChunksIndices.Max == 0;
            if (firstChunkNeedsLoading)
            {
                //load the first chunk since none are loaded
                firstChunk = new TrackedCoroutine(LoadFirstChunk());
            }
            
            GlobalLoggers.ChunkLogger.Log($"Doing loading check 6 - including first chunk? {firstChunkNeedsLoading}");

            UpdateCustomLoadDistanceChunks(position);
            LoadChunksInDirection(position, ChunkUtils.LoadDirection.BEFORE);
            LoadChunksInDirection(position, ChunkUtils.LoadDirection.AFTER);
            
            GlobalLoggers.ChunkLogger.Log($"Doing loading check 7");
            
            yield return new WaitUntil(() =>
            {
                if (timeSinceLastMessage > 1)
                {
                    timeOfLastMessage = Time.realtimeSinceStartup;
                    GlobalLoggers.ChunkLogger.Log($"Loading check running coroutine - {(firstChunk == null || !firstChunk.IsPlaying)} {customChunkLoading.AreAllComplete()}({customChunkLoading.Count}) {chunksBeforeLoading.AreAllComplete()}({chunksBeforeLoading.Count}) {chunksAfterLoading.AreAllComplete()}({chunksAfterLoading.Count})");
                }

                return (firstChunk == null || !firstChunk.IsPlaying)
                       && customChunkLoading.AreAllComplete()
                       && chunksBeforeLoading.AreAllComplete()
                       && chunksAfterLoading.AreAllComplete();
            });
            
            GlobalLoggers.ChunkLogger.Log($"Loading check completed!");

            UnloadChunksAroundPosition(position);

            GlobalLoggers.ChunkLogger.Log($"Unloading check completed!");

            UpdateChunksAccessibility();
            
            IsLoadingChunks = false;
        }

        private float timeOfLastMessage;
        private float timeSinceLastMessage => Time.realtimeSinceStartup - timeOfLastMessage;
        
        private IEnumerator LoadFirstChunk()
        {
            loadingOrLoadedChunksIndices = new MinMaxInt(currentMap.StartingChunkIndex, currentMap.StartingChunkIndex);
            yield return LoadChunkAsync(currentMap.StartingChunkIndex, 
                currentMap.GetChunkData(currentMap.StartingChunkIndex).HasCustomLoadDistance
                    ? ChunkUtils.LoadDirection.CUSTOM : ChunkUtils.LoadDirection.AFTER);
        }

        private void UpdateCustomLoadDistanceChunks(Vector3 position)
        {
            customChunkLoading.Clear();
            
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
                    customChunkLoading.Add(new TrackedCoroutine(LoadChunkAsync(chunkIndexWithCustomLoadDistance, ChunkUtils.LoadDirection.CUSTOM)));
                }
                if (!isWithinLoadDistance && isChunkCustomLoaded)
                {
                    //should not be loaded - unload if so
                    UnloadChunk(customLoadedData.Value);
                }
            }
        }

        private bool IsChunkWithinLoadDistance(Vector3 loadPosition, int mapIndex, ChunkUtils.LoadDirection direction)
        {
            float chunkLoadDistanceSqr = currentMap.ChunkLoadDistance * currentMap.ChunkLoadDistance;
            
            ChunkMapData chunkData = currentMap.GetChunkData(mapIndex);
            Vector3 chunkPosition = direction == ChunkUtils.LoadDirection.AFTER ? chunkData.SplineStartPosition : chunkData.SplineEndPosition;
            
            float distanceToChunk = Vector3.SqrMagnitude(loadPosition - chunkPosition);
            return distanceToChunk <= chunkLoadDistanceSqr;
        }

        private void UnloadChunksAroundPosition(Vector3 position)
        {
            //check to unload chunks ahead
            bool isCustomLoadedOnEndAhead = true;
            for (int chunkAheadIndex = loadingOrLoadedChunksIndices.Max; chunkAheadIndex >= loadingOrLoadedChunksIndices.Min; chunkAheadIndex--)
            {
                LoadedChunkData? customLoadedChunk = GetCustomLoadedChunkData(chunkAheadIndex);
                bool isCustomLoaded = customLoadedChunk != null;
                if (isCustomLoaded)
                {
                    if (isCustomLoadedOnEndAhead)
                        loadingOrLoadedChunksIndices.Max--; //don't have custom loaded chunks on the end
                    continue; //uses own unload check
                }

                isCustomLoadedOnEndAhead = false; //no longer on the end

                if (IsChunkWithinLoadDistance(position, chunkAheadIndex, ChunkUtils.LoadDirection.AFTER))
                    break; //can end here

                LoadedChunkData? loadedChunkData = GetLoadedChunkDataByMapIndex(chunkAheadIndex);
                if (loadedChunkData == null)
                {
                    Debug.LogWarning($"There's null chunk data within the loading range ({loadingOrLoadedChunksIndices.Min} to {loadingOrLoadedChunksIndices.Max}) at index {chunkAheadIndex}");
                    continue;
                }
                
                UnloadChunk(loadedChunkData.Value);
                
                loadingOrLoadedChunksIndices.Max--;
            }
            
            //check to unload chunks behind
            bool isCustomLoadedOnEndBehind = true;
            for (int chunkBehindIndex = loadingOrLoadedChunksIndices.Min; chunkBehindIndex < loadingOrLoadedChunksIndices.Max; chunkBehindIndex++)
            {
                LoadedChunkData? customLoadedChunk = GetCustomLoadedChunkData(chunkBehindIndex);
                bool isCustomLoaded = customLoadedChunk != null;
                if (isCustomLoaded)
                {
                    if (isCustomLoadedOnEndBehind)
                        loadingOrLoadedChunksIndices.Min++; //don't have custom loaded chunks on the end
                    continue; //uses own unload check
                }

                isCustomLoadedOnEndBehind = false; //no longer on the end
                
                if (IsChunkWithinLoadDistance(position, chunkBehindIndex, ChunkUtils.LoadDirection.BEFORE))
                    break; //can end here

                LoadedChunkData? loadedChunkData = GetLoadedChunkDataByMapIndex(chunkBehindIndex);
                if (loadedChunkData == null)
                {
                    Debug.LogWarning($"There's null chunk data within the loading range ({loadingOrLoadedChunksIndices.Min} to {loadingOrLoadedChunksIndices.Max}) at index {chunkBehindIndex}");
                    continue;
                }
                
                UnloadChunk(loadedChunkData.Value);

                loadingOrLoadedChunksIndices.Min++;
            }
        }

        private void UpdateChunksAccessibility()
        {
            accessibleChunksIndices.Min = int.MaxValue;
            accessibleChunksIndices.Max = int.MinValue;
            
            UpdateNormalChunksAccessibility();
            UpdateCustomChunksAccessibility();
        }
        
        private void UpdateNormalChunksAccessibility()
        {
            foreach (LoadedChunkData loadedChunkData in currentChunks)
            {
                if (!loadedChunkData.Chunk.IsFullyLoaded)
                {
                    SetChunkAccessible(loadedChunkData, false);
                    continue;
                }
                
                bool isWithinLoadRange = loadedChunkData.MapIndex <= loadingOrLoadedChunksIndices.Max && loadedChunkData.MapIndex >= loadingOrLoadedChunksIndices.Min;
                SetChunkAccessible(loadedChunkData, isWithinLoadRange);
            }
        }
        
        private void UpdateCustomChunksAccessibility()
        {
            foreach (LoadedChunkData loadedChunkData in currentCustomLoadedChunks)
            {
                if (!loadedChunkData.Chunk.IsFullyLoaded)
                {
                    SetChunkAccessible(loadedChunkData, false);
                    continue;
                }

                SetChunkAccessible(loadedChunkData, IsCustomChunkAccessible(loadedChunkData));
            }
        }

        private bool IsCustomChunkAccessible(LoadedChunkData loadedChunkData)
        {
            //check if it's connected ahead
            for (int index = loadingOrLoadedChunksIndices.Max + 1; index <= loadedChunkData.MapIndex; index++)
            {
                bool isCustomLoaded = GetCustomLoadedChunkData(index) != null;
                if (!isCustomLoaded)
                    break;

                if (index == loadedChunkData.MapIndex)
                    return true;
            }
                
            //check if it's connected behind
            for (int index = loadingOrLoadedChunksIndices.Min - 1; index >= loadedChunkData.MapIndex; index--)
            {
                bool isCustomLoaded = GetCustomLoadedChunkData(index) != null;
                if (!isCustomLoaded)
                    break;

                if (index == loadedChunkData.MapIndex)
                    return true;
            }

            return false;
        }

        private void SetChunkAccessible(LoadedChunkData loadedChunkData, bool accessible)
        {
            if (accessible)
            {
                RegisterAccessibleChunkIndex(loadedChunkData.MapIndex);
                
                if (!loadedChunkData.Chunk.IsAccessible)
                {
                    //becoming accessible
                    loadedChunkData.Chunk.OnBecomeAccessible();
                    chunksWaitingToBeAccessible.Remove(loadedChunkData);
                }
            }
            else if (loadedChunkData.Chunk.IsAccessible)
            {
                //becoming inaccessible
                loadedChunkData.Chunk.OnBecomeInaccessible();
                chunksWaitingToBeAccessible.Add(loadedChunkData);
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
        
        private void LoadChunksInDirection(Vector3 startingPosition, ChunkUtils.LoadDirection direction)
        {
            if (direction == ChunkUtils.LoadDirection.BEFORE)
                chunksBeforeLoading.Clear();
            if (direction == ChunkUtils.LoadDirection.AFTER)
                chunksAfterLoading.Clear();
            
            float chunkLoadDistanceSqr = currentMap.ChunkLoadDistance * currentMap.ChunkLoadDistance;
            
            Vector3 endOfChunk = direction == ChunkUtils.LoadDirection.AFTER
                ? currentMap.GetChunkData(loadingOrLoadedChunksIndices.Max).SplineEndPosition
                : currentMap.GetChunkData(loadingOrLoadedChunksIndices.Min).SplineStartPosition;
            
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
                    return;
                }

                LoadedChunkData? customLoadedChunk = GetCustomLoadedChunkData(indexToLoad);
                bool chunkIsCustomLoaded = customLoadedChunk != null;
                if (chunkIsCustomLoaded)
                {
                    //update the distance
                    ChunkMapData customLoadedChunkData = currentMap.GetChunkData(indexToLoad);
                    Vector3 furthestPointOnCustomLoadedChunk = direction == ChunkUtils.LoadDirection.AFTER ? customLoadedChunkData.SplineEndPosition : customLoadedChunkData.SplineStartPosition;
                    distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnCustomLoadedChunk);
                    
                    RegisterLoadingOrLoadedChunkIndex(indexToLoad);
                    continue;
                }

                if (direction == ChunkUtils.LoadDirection.BEFORE)
                    chunksBeforeLoading.Add(new TrackedCoroutine(LoadChunkAsync(indexToLoad, direction)));
                if (direction == ChunkUtils.LoadDirection.AFTER)
                    chunksAfterLoading.Add(new TrackedCoroutine(LoadChunkAsync(indexToLoad, direction)));

                RegisterLoadingOrLoadedChunkIndex(indexToLoad);

                //update the distance
                ChunkMapData chunkData = currentMap.GetChunkData(indexToLoad);
                Vector3 furthestPointOnChunk = direction == ChunkUtils.LoadDirection.AFTER ? chunkData.SplineEndPosition : chunkData.SplineStartPosition;
                distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnChunk);
            }
        }

        private void RegisterLoadingOrLoadedChunkIndex(int index)
        {
            if (index > loadingOrLoadedChunksIndices.Max)
                loadingOrLoadedChunksIndices.Max = index;
            if (index < loadingOrLoadedChunksIndices.Min)
                loadingOrLoadedChunksIndices.Min = index;
        }

        private void RegisterAccessibleChunkIndex(int index)
        {
            if (index > accessibleChunksIndices.Max)
                accessibleChunksIndices.Max = index;
            if (index < accessibleChunksIndices.Min)
                accessibleChunksIndices.Min = index;
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
            
            LoadedChunkData loadedChunkData = new LoadedChunkData(chunk, chunkAddressableKey, mapIndex);
            
            if (loadDirection == ChunkUtils.LoadDirection.CUSTOM)
            {
                currentCustomLoadedChunks.Add(loadedChunkData);
            } else
            {
                currentChunks.Add(loadedChunkData);
            }
            
            chunksWaitingToBeAccessible.Add(loadedChunkData);

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
            OnChunkLoadedAndReady(loadedChunkData);

            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to invoke events.");

#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Chunk loading '{chunkAddressableKey}' complete.");
#endif
        }

        /// <summary>
        /// Called when the chunk is completely loaded and ready (ie. terrain has been applied, chunk objects have spawned etc.).
        /// </summary>
        private void OnChunkLoadedAndReady(LoadedChunkData loadedChunkData)
        {
            //check if the chunk is now accessible, and if it has made any other chunks accessible ahead
            GlobalLoggers.ChunkLogger.Log($"Chunk {loadedChunkData.Chunk.gameObject.name} loaded at map index {loadedChunkData.MapIndex}.");

            loadedChunkData.Chunk.OnFullyLoaded();
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

                    //initialise if power pole
                    ChunkPowerpoleManager.OnLoadChunkObject(chunk, chunkObject);

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
            chunkData.Chunk.OnChunkUnload();
            currentChunks.Remove(chunkData);
            Destroy(chunkData.Chunk.gameObject);
            currentCustomLoadedChunks.Remove(chunkData);
            chunksWaitingToBeAccessible.Remove(chunkData);
        }
        
    }
}
