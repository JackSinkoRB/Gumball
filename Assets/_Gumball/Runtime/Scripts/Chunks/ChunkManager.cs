using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        [SerializeField] private float chunkLoadDistance = 50;

        [Header("Debugging")]
        [ReadOnly, SerializeField] private MapData currentMap;
        [ReadOnly, SerializeField] private MinMaxInt loadedChunksIndices;
        [ReadOnly, SerializeField] private List<LoadedChunkData> currentCustomLoadedChunks = new();

        private readonly Dictionary<int, LoadedChunkData> currentChunks = new();

        [Obsolete("To be removed - for testing only")]
        public MapData TestingMap => testingMap;
        public MapData CurrentMap => currentMap;
        
        private bool isLoading;
        private readonly TrackedCoroutine distanceLoadingCoroutine = new();
        private float timeSinceLastLoadCheck;

        public IEnumerator LoadMap(MapData map)
        {
            GlobalLoggers.LoadingLogger.Log($"Loading map '{map.name}'");
            isLoading = true;
            currentMap = map;
            currentChunks.Clear();

            //load the first chunk since none are loaded
            loadedChunksIndices = new MinMaxInt(map.StartingChunkIndex, map.StartingChunkIndex);
            yield return LoadChunkAsync(map.StartingChunkIndex, 
                map.GetChunkData(map.StartingChunkIndex).HasCustomLoadDistance
                    ? ChunkUtils.LoadDirection.CUSTOM : ChunkUtils.LoadDirection.AFTER);
            
            //load the rest of the chunks in range
            yield return LoadChunksAroundPosition(map.VehicleStartingPosition);
            
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
            yield return UpdateCustomLoadDistanceChunks(position);
            
            UnloadChunksAroundPosition(position);
            
            yield return LoadChunksInDirection(position, ChunkUtils.LoadDirection.BEFORE);
            yield return LoadChunksInDirection(position, ChunkUtils.LoadDirection.AFTER);
        }

        private IEnumerator UpdateCustomLoadDistanceChunks(Vector3 position)
        {
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
                    yield return LoadChunkAsync(chunkIndexWithCustomLoadDistance, ChunkUtils.LoadDirection.CUSTOM);
                }
                if (!isWithinLoadDistance && isChunkCustomLoaded)
                {
                    //should not be loaded - unload if so
                    UnloadChunk(customLoadedData.Value);
                }
            }
        }

        private void UnloadChunksAroundPosition(Vector3 position)
        {
            List<int> chunksToUnload = new List<int>();
            float chunkLoadDistanceSqr = chunkLoadDistance * chunkLoadDistance;

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
                if (currentChunks.Count == 1)
                    break; //keep at least 1 chunk
                
                LoadedChunkData chunkData = currentChunks[indexToRemove];
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

        private IEnumerator LoadChunksInDirection(Vector3 startingPosition, ChunkUtils.LoadDirection direction)
        {
            float chunkLoadDistanceSqr = chunkLoadDistance * chunkLoadDistance;
            
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
                
                if (indexToLoad < 0 || indexToLoad >= currentMap.ChunkReferences.Length)
                {
                    //end of map - no more chunks to load
                    yield break;
                }
                
                if (indexToLoad > loadedChunksIndices.Max)
                    loadedChunksIndices.Max = indexToLoad;
                if (indexToLoad < loadedChunksIndices.Min)
                    loadedChunksIndices.Min = indexToLoad;

                LoadedChunkData? customLoadedChunk = GetCustomLoadedChunkData(indexToLoad);
                bool chunkIsCustomLoaded = customLoadedChunk != null;
                if (chunkIsCustomLoaded)
                {
                    //update the distance
                    ChunkMapData customLoadedChunkData = currentMap.GetChunkData(indexToLoad);
                    Vector3 furthestPointOnCustomLoadedChunk = direction == ChunkUtils.LoadDirection.AFTER ? customLoadedChunkData.SplineEndPosition : customLoadedChunkData.SplineStartPosition;
                    distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnCustomLoadedChunk);
                    continue;
                }

                yield return LoadChunkAsync(indexToLoad, direction);
                
                //update the distance
                ChunkMapData chunkData = currentMap.GetChunkData(indexToLoad);
                Vector3 furthestPointOnChunk = direction == ChunkUtils.LoadDirection.AFTER ? chunkData.SplineEndPosition : chunkData.SplineStartPosition;
                distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnChunk);
            }
        }

        private IEnumerator LoadChunkAsync(int mapIndex, ChunkUtils.LoadDirection loadDirection)
        {
            AssetReferenceGameObject chunkAssetReference = currentMap.ChunkReferences[mapIndex];
            
#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Loading chunk '{chunkAssetReference.editorAsset.name}'...");
#endif
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkAssetReference);
            yield return handle;
            
            GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), transform);
            instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            Chunk chunk = instantiatedChunk.GetComponent<Chunk>();

            LoadedChunkData loadedChunkData = new LoadedChunkData(chunk, chunkAssetReference, mapIndex);

            if (loadDirection == ChunkUtils.LoadDirection.CUSTOM)
            {
                currentCustomLoadedChunks.Add(loadedChunkData);
            } else
            {
                currentChunks[mapIndex] = loadedChunkData;
            }

            ChunkMapData chunkMapData = currentMap.GetChunkData(mapIndex);
            chunkMapData.ApplyToChunk(chunk);

            //TODO: can this just be unity_editor?
            //should create a copy of the mesh so it doesn't directly edit the saved mesh in runtime
            MeshFilter meshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
            Mesh meshCopy = Instantiate(meshFilter.sharedMesh);
            chunk.CurrentTerrain.GetComponent<MeshFilter>().sharedMesh = meshCopy;

            onChunkLoad?.Invoke(chunk);
            
#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Chunk loading '{chunkAssetReference.editorAsset.name}' complete.");
#endif
        }
        
        private void UnloadChunk(LoadedChunkData chunkData)
        {
            onChunkUnload?.Invoke(chunkData.Chunk);
            Destroy(chunkData.Chunk.gameObject);
            currentCustomLoadedChunks.Remove(chunkData);
        }
        
    }
}
