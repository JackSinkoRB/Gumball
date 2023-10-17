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
        
        private const float timeBetweenLoadingChecks = 0.5f;

        [Header("Settings")]
        [Obsolete("To be removed - for testing only")]
        [SerializeField] private MapData testingMap;
        [SerializeField] private float chunkLoadDistance = 50;

        [Header("Debugging")]
        [ReadOnly, SerializeField] private MapData currentMap;
        [ReadOnly, SerializeField] private List<LoadedChunkData> currentChunks = new();

        [Obsolete("To be removed - for testing only")]
        public MapData TestingMap => testingMap;
        public MapData CurrentMap => currentMap;
        
        private bool isLoading;
        private MinMaxInt loadedChunksIndices;
        private readonly TrackedCoroutine distanceLoadingCoroutine = new();
        private float timeSinceLastLoadCheck;

        [Serializable]
        public struct LoadedChunkData
        {
            private readonly Chunk chunk;
            private readonly AssetReferenceGameObject assetReference;

            public Chunk Chunk => chunk;
            public AssetReferenceGameObject AssetReference => assetReference;

            public LoadedChunkData(Chunk chunk, AssetReferenceGameObject assetReference)
            {
                this.chunk = chunk;
                this.assetReference = assetReference;
            }
        }
        
        public IEnumerator LoadMap(MapData map)
        {
            GlobalLoggers.LoadingLogger.Log($"Loading map '{map.name}'");
            isLoading = true;
            currentMap = map;
            currentChunks.Clear();

            //load the first chunk since none are loaded
            loadedChunksIndices = new MinMaxInt(map.StartingChunkIndex, map.StartingChunkIndex);
            yield return LoadChunkAsync(map.ChunkReferences[map.StartingChunkIndex]);
            
            //load the rest of the chunks in range
            yield return LoadChunksAroundPosition(map.VehicleStartingPosition);
            
            isLoading = false;
        }

        private void LateUpdate()
        {
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
            UnloadChunksAroundPosition(position);
            
            yield return LoadChunksInDirection(position, ChunkUtils.LoadDirection.BEFORE);
            yield return LoadChunksInDirection(position, ChunkUtils.LoadDirection.AFTER);
        }

        private void UnloadChunksAroundPosition(Vector3 position)
        {
            List<int> chunksToUnload = new List<int>();
            float chunkLoadDistanceSqr = chunkLoadDistance * chunkLoadDistance;

            //check to unload chunks ahead
            for (int chunkAheadIndex = currentChunks.Count - 1; chunkAheadIndex >= 0; chunkAheadIndex--)
            {
                LoadedChunkData chunkData = currentChunks[chunkAheadIndex];
                float distanceToChunk = Vector3.SqrMagnitude(position - chunkData.Chunk.FirstSample.position);
                if (distanceToChunk <= chunkLoadDistanceSqr)
                {
                    //is good!
                    break;
                }
                
                //can unload
                chunksToUnload.Add(chunkAheadIndex);
                loadedChunksIndices.Max--;
            }
            
            //check to unload chunks behind
            for (int chunkBehindIndex = 0; chunkBehindIndex < currentChunks.Count; chunkBehindIndex++)
            {
                LoadedChunkData chunkData = currentChunks[chunkBehindIndex];
                float distanceToChunk = Vector3.SqrMagnitude(position - chunkData.Chunk.LastSample.position);
                if (distanceToChunk <= chunkLoadDistanceSqr)
                {
                    //is good!
                    break;
                }
                
                //can unload
                chunksToUnload.Add(chunkBehindIndex);
                loadedChunksIndices.Min++;
            }

            foreach (int indexToRemove in chunksToUnload)
            {
                if (currentChunks.Count == 1)
                    break; //keep at least 1 chunk
                
                LoadedChunkData chunkData = currentChunks[indexToRemove];
                currentChunks.RemoveAt(indexToRemove);
                Destroy(chunkData.Chunk.gameObject);
            }
        }
        
        private IEnumerator LoadChunksInDirection(Vector3 startingPosition, ChunkUtils.LoadDirection direction)
        {
            float chunkLoadDistanceSqr = chunkLoadDistance * chunkLoadDistance;
            
            Chunk startingChunk = direction == ChunkUtils.LoadDirection.AFTER ? currentChunks[^1].Chunk : currentChunks[0].Chunk;
            SplinePoint endPoint = startingChunk.SplineComputer.GetPoint(direction == ChunkUtils.LoadDirection.AFTER ? startingChunk.LastPointIndex : 0);
            
            float distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - endPoint.position);
            while (distanceToEndOfChunk < chunkLoadDistanceSqr)
            {
                //load next chunk
                int indexToLoad = direction == ChunkUtils.LoadDirection.AFTER ? loadedChunksIndices.Max + 1 : loadedChunksIndices.Min - 1;
                if (indexToLoad < 0 || indexToLoad >= currentMap.ChunkReferences.Length)
                {
                    //end of map - no more chunks to load
                    break;
                }

                if (indexToLoad > loadedChunksIndices.Max)
                    loadedChunksIndices.Max = indexToLoad;
                if (indexToLoad < loadedChunksIndices.Min)
                    loadedChunksIndices.Min = indexToLoad;

                AssetReferenceGameObject nextChunk = currentMap.ChunkReferences[indexToLoad];
                
                yield return LoadChunkAsync(nextChunk, direction);
                
                //update the distance
                Chunk newestChunk = direction == ChunkUtils.LoadDirection.AFTER ? currentChunks[^1].Chunk : currentChunks[0].Chunk;
                SplinePoint furthestPointOnNewestChunk = newestChunk.SplineComputer.GetPoint(direction == ChunkUtils.LoadDirection.AFTER ? newestChunk.LastPointIndex : 0);
                distanceToEndOfChunk = Vector3.SqrMagnitude(startingPosition - furthestPointOnNewestChunk.position);
            }
        }

        private IEnumerator LoadChunkAsync(AssetReferenceGameObject chunkAssetReference, ChunkUtils.LoadDirection loadDirection = ChunkUtils.LoadDirection.AFTER)
        {
#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Loading chunk '{chunkAssetReference.editorAsset.name}'...");
#endif
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkAssetReference);
            yield return handle;
            
            GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), transform);
            instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            Chunk chunk = instantiatedChunk.GetComponent<Chunk>();

            LoadedChunkData loadedChunkData = new LoadedChunkData(chunk, chunkAssetReference);
            if (loadDirection == ChunkUtils.LoadDirection.AFTER)
                currentChunks.Add(loadedChunkData);
            else currentChunks.Insert(0, loadedChunkData);

            //connect to the previous chunk (if there is one)
            if (currentChunks.Count > 1)
            {
                if (loadDirection == ChunkUtils.LoadDirection.AFTER)
                {
                    int connectionIndex = loadedChunksIndices.Max - 1;
                    LoadedChunkData previousChunkData = currentChunks[^2];
                    ChunkUtils.ConnectChunks(previousChunkData.Chunk, chunk, ChunkUtils.LoadDirection.AFTER, currentMap.GetBlendData(connectionIndex));
                }
                if (loadDirection == ChunkUtils.LoadDirection.BEFORE)
                {
                    int connectionIndex = loadedChunksIndices.Min;
                    LoadedChunkData previousChunkData = currentChunks[1];
                    ChunkUtils.ConnectChunks(previousChunkData.Chunk, chunk, ChunkUtils.LoadDirection.BEFORE, currentMap.GetBlendData(connectionIndex));
                }
            }
            
            //should create a copy of the mesh so it doesn't directly edit the saved mesh in runtime
            MeshFilter meshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
            Mesh meshCopy = Instantiate(meshFilter.sharedMesh);
            chunk.CurrentTerrain.GetComponent<MeshFilter>().sharedMesh = meshCopy;

#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Chunk loading '{chunkAssetReference.editorAsset.name}' complete.");
#endif
        }
        
    }
}
