using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AYellowpaper.SerializedCollections;
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
        [ReadOnly, SerializeField] private MinMaxInt loadedChunksIndices;
        [ReadOnly, SerializeField] private List<LoadedChunkData> currentCustomLoadedChunks = new();

        /// <summary>
        /// int = the map index
        /// </summary>
        [SerializedDictionary("Map Index", "Data")]
        public SerializedDictionary<int, LoadedChunkData> CurrentChunks = new();

        [Obsolete("To be removed - for testing only")]
        public MapData TestingMap => testingMap;
        public MapData CurrentMap => currentMap;
        
        private bool isLoading;
        private readonly TrackedCoroutine distanceLoadingCoroutine = new();
        private float timeSinceLastLoadCheck;

        /// <summary>
        /// Returns whether the chunk is within the load distance radius. Can be false if the chunk is custom loaded.
        /// </summary>
        public bool IsChunkWithinLoadRadius(int chunkMapIndex)
        {
            return chunkMapIndex >= loadedChunksIndices.Min && chunkMapIndex <= loadedChunksIndices.Max;
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
                
                if (GetLoadedChunkByMapIndex(chunkAheadIndex) == null)
                    break;

                chunkAheadIndex++;
            }
            
            int chunkBehindIndex = loadedChunksIndices.Min - 1;
            while (chunkBehindIndex >= currentChunkMapIndex)
            {
                if (chunkBehindIndex == currentChunkMapIndex)
                    return true;
                
                if (GetLoadedChunkByMapIndex(chunkBehindIndex) == null)
                    break;

                chunkBehindIndex++;
            }

            return false;
        }
        
        public Chunk GetLoadedChunkByMapIndex(int chunkMapIndex)
        {
            if (CurrentChunks.ContainsKey(chunkMapIndex))
                return CurrentChunks[chunkMapIndex].Chunk;
            
            foreach (LoadedChunkData data in currentCustomLoadedChunks)
            {
                if (data.MapIndex.Equals(chunkMapIndex))
                    return data.Chunk;
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
            foreach (LoadedChunkData data in CurrentChunks.Values)
            {
                if (data.Chunk.Equals(chunk))
                    return data.MapIndex;
            }

            foreach (LoadedChunkData data in currentCustomLoadedChunks)
            {
                if (data.Chunk.Equals(chunk))
                    return data.MapIndex;
            }

            throw new ArgumentException($"The chunk {chunk.name} is not currently loaded.");
        }
        
        public IEnumerator LoadMap(MapData map)
        {
            GlobalLoggers.LoadingLogger.Log($"Loading map '{map.name}'");
            isLoading = true;
            currentMap = map;
            CurrentChunks.Clear();

            map.OnMapLoad();

            //load the chunks in range
            distanceLoadingCoroutine.SetCoroutine(LoadChunksAroundPosition(map.VehicleStartingPosition));
            yield return distanceLoadingCoroutine.Coroutine;
            
            isLoading = false;
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

            if (distanceLoadingCoroutine.IsPlaying)
                return;

            timeSinceLastLoadCheck += Time.deltaTime;
            if (timeSinceLastLoadCheck < timeBetweenLoadingChecks)
                return;

            //can perform loading check
            timeSinceLastLoadCheck = 0;
            distanceLoadingCoroutine.SetCoroutine(LoadChunksAroundPosition(PlayerCarManager.Instance.CurrentCar.transform.position));
        }

        private IEnumerator LoadChunksAroundPosition(Vector3 position)
        {
            TrackedCoroutine firstChunk = null;
            bool firstChunkNeedsLoading = loadedChunksIndices.Min == 0 && loadedChunksIndices.Max == 0;
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
            loadedChunksIndices = new MinMaxInt(currentMap.StartingChunkIndex, currentMap.StartingChunkIndex);
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
            for (int chunkAheadIndex = loadedChunksIndices.Max; chunkAheadIndex >= loadedChunksIndices.Min; chunkAheadIndex--)
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

                loadedChunksIndices.Max--;

                //check if next chunk is custom loaded, and ignore it
                while (GetCustomLoadedChunkData(loadedChunksIndices.Max) != null)
                {
                    loadedChunksIndices.Max--;
                }
            }
            
            //check to unload chunks behind
            for (int chunkBehindIndex = loadedChunksIndices.Min; chunkBehindIndex < loadedChunksIndices.Max; chunkBehindIndex++)
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
                
                loadedChunksIndices.Min++;
                
                //check if next chunk is custom loaded, and ignore it
                while (GetCustomLoadedChunkData(loadedChunksIndices.Min) != null)
                {
                    loadedChunksIndices.Min++;
                }
            }

            foreach (int indexToRemove in chunksToUnload)
            {
                if (CurrentChunks.Count == 1)
                    break; //keep at least 1 chunk
                
                LoadedChunkData chunkData = CurrentChunks[indexToRemove];
                UnloadChunk(chunkData);
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
                    ? loadedChunksIndices.Max + 1
                    : loadedChunksIndices.Min - 1;
                
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
                    continue;
                }

                trackedCoroutines.Add(new TrackedCoroutine(LoadChunkAsync(indexToLoad, direction)));
                RegisterLoadedChunkIndex(indexToLoad); //only register once the chunk has been created

                //update the distance
                ChunkMapData chunkData = currentMap.GetChunkData(indexToLoad);
                Vector3 furthestPointOnChunk = direction == ChunkUtils.LoadDirection.AFTER ? chunkData.SplineEndPosition : chunkData.SplineStartPosition;
                distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnChunk);
            }

            return trackedCoroutines;
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
            
            LoadedChunkData loadedChunkData = new LoadedChunkData(chunk, chunkAddressableKey, mapIndex);

            if (loadDirection == ChunkUtils.LoadDirection.CUSTOM)
            {
                currentCustomLoadedChunks.Add(loadedChunkData);
            } else
            {
                CurrentChunks[mapIndex] = loadedChunkData;
            }

            ChunkMapData chunkMapData = currentMap.GetChunkData(mapIndex);
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to get chunk data.");

            if (!isLoading)
                yield return new WaitForEndOfFrame();
            stopwatch.Restart();

            chunkMapData.ApplyToChunk(chunk);
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to apply chunk data.");
            
            if (!isLoading)
                yield return new WaitForEndOfFrame();
            stopwatch.Restart();

            //TODO: can this just be unity_editor?
            //should create a copy of the mesh so it doesn't directly edit the saved mesh in runtime
            MeshFilter meshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
            Mesh meshCopy = Instantiate(meshFilter.sharedMesh);
            chunk.CurrentTerrain.GetComponent<MeshFilter>().sharedMesh = meshCopy;
            chunk.CurrentTerrain.GetComponent<MeshCollider>().sharedMesh = meshCopy;
            
            GlobalLoggers.LoadingLogger.Log($"Took '{stopwatch.ElapsedMilliseconds}ms' to update components.");
            stopwatch.Restart();

            if (!isLoading)
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
            const float maxTimeAllowedPerFrameMs = 6;
            
            //instantiate all the instances across multiple frames
            foreach (string assetKey in chunk.ChunkObjectData.Keys)
            {
                AsyncOperationHandle<GameObject> handle = handlesLookup[assetKey];
                foreach (ChunkObjectData chunkObjectData in chunk.ChunkObjectData[assetKey])
                {
                    GameObject chunkObject = chunkObjectData.LoadIntoChunk(handle, chunk);
                    GlobalLoggers.LoadingLogger.Log($"Loaded {chunkObject.name} at {stopwatch.ElapsedMilliseconds}ms.");
                    
                    if (!isLoading && stopwatch.ElapsedMilliseconds > maxTimeAllowedPerFrameMs)
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
            Destroy(chunkData.Chunk.gameObject);
            currentCustomLoadedChunks.Remove(chunkData);
        }
        
    }
}
