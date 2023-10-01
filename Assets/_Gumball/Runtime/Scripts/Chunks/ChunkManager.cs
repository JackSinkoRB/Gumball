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
        [ReadOnly, SerializeField] private List<Chunk> currentChunks = new();

        [Obsolete("To be removed - for testing only")]
        public MapData TestingMap => testingMap;
        public MapData CurrentMap => currentMap;
        
        private bool isLoading;
        private MinMaxInt loadedChunksIndices;
        private readonly TrackedCoroutine distanceLoadingCoroutine = new();
        private float timeSinceLastLoadCheck;
        
        public IEnumerator LoadMap(MapData map)
        {
            GlobalLoggers.LoadingLogger.Log($"Loading map '{map.name}'");
            isLoading = true;
            currentMap = map;

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
            distanceLoadingCoroutine.Set(LoadChunksAroundPosition(PlayerCarManager.Instance.CurrentCar.transform.position));
        }

        private IEnumerator LoadChunksAroundPosition(Vector3 position)
        {
            float chunkLoadDistanceSqr = chunkLoadDistance * chunkLoadDistance;
            
            //TODO: check to unload chunks
            
            //check to load chunks ahead
            Chunk lastChunk = currentChunks[^1];
            SplinePoint lastPoint = lastChunk.SplineComputer.GetPoint(lastChunk.LastPointIndex);
            float distanceToEndOfChunk = Vector3.SqrMagnitude(position - lastPoint.position);
            while (distanceToEndOfChunk < chunkLoadDistanceSqr)
            {
                //load next chunk
                int indexToLoad = loadedChunksIndices.Max + 1;
                if (indexToLoad >= currentMap.ChunkReferences.Length)
                {
                    //end of map - no more chunks to load
                    break;
                }

                loadedChunksIndices.Max = indexToLoad;
                AssetReferenceGameObject nextChunk = currentMap.ChunkReferences[indexToLoad];
                
                yield return LoadChunkAsync(nextChunk);
                
                //update the distance
                SplinePoint nextChunkLastPoint = currentChunks[^1].SplineComputer.GetPoint(lastChunk.LastPointIndex);
                distanceToEndOfChunk = Vector3.SqrMagnitude(position - nextChunkLastPoint.position);
            }
            
            //check to load chunks behind
            Chunk firstChunk = currentChunks[0];
            SplinePoint firstPoint = firstChunk.SplineComputer.GetPoint(0);
            float distanceToStartOfChunk = Vector3.SqrMagnitude(position - firstPoint.position);
            while (distanceToStartOfChunk < chunkLoadDistanceSqr)
            {
                //load next chunk
                int indexToLoad = loadedChunksIndices.Min - 1;
                if (indexToLoad < 0)
                {
                    //start of map - no more chunks to load
                    break;
                }
            
                loadedChunksIndices.Min = indexToLoad;
                AssetReferenceGameObject nextChunk = currentMap.ChunkReferences[indexToLoad];
            
                yield return LoadChunkAsync(nextChunk);
                
                //update the distance
                SplinePoint nextChunkFirstPoint = currentChunks[0].SplineComputer.GetPoint(0);
                distanceToStartOfChunk = Vector3.SqrMagnitude(position - nextChunkFirstPoint.position);
            }
        }

        private IEnumerator LoadChunkAsync(AssetReferenceGameObject chunkAssetReference)
        {
#if UNITY_EDITOR
            GlobalLoggers.LoadingLogger.Log($"Loading chunk '{chunkAssetReference.editorAsset.name}'...");
#endif
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkAssetReference);
            yield return handle;
            
            GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), transform);
            instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            Chunk chunk = instantiatedChunk.GetComponent<Chunk>();

            currentChunks.Add(chunk);

            //connect to the previous chunk (if there is one)
            if (currentChunks.Count > 1)
            {
                Chunk previousChunk = currentChunks[^2];
                ChunkUtils.ConnectChunks(previousChunk, chunk, currentMap.BlendData);
            }
            
#if UNITY_EDITOR
            //should create a copy of the mesh so it doesn't directly edit the saved mesh in runtime
            MeshFilter meshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
            Mesh meshCopy = Instantiate(meshFilter.sharedMesh);
            chunk.CurrentTerrain.GetComponent<MeshFilter>().sharedMesh = meshCopy;

            GlobalLoggers.LoadingLogger.Log($"Chunk loading '{chunkAssetReference.editorAsset.name}' complete.");
#endif
        }
        
    }
}
