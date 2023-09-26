using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class ChunkManager : Singleton<ChunkManager>
    {
        
        private const float chunkLoadDistance = 100;

        [Obsolete("To be removed - for testing only")]
        [SerializeField] private MapData testingMap;
        [ReadOnly, SerializeField] private MapData currentMap;
        [ReadOnly, SerializeField] private MapData currentMapLoading;
        [ReadOnly, SerializeField] private List<Chunk> currentChunks = new();
        
        private readonly List<TrackedCoroutine> chunkLoadingTasks = new();

        [Obsolete("To be removed - for testing only")]
        public MapData TestingMap => testingMap;
        public MapData CurrentMap => currentMap;
        private bool isLoading => currentMapLoading != null;

        public IEnumerator LoadMap(MapData map)
        {
            GlobalLoggers.LoadingLogger.Log($"Loading map '{map.name}'");
            currentMapLoading = map;
            LoadChunksAroundPosition(Vector3.zero);

            yield return new WaitUntil(IsChunkLoadingComplete);

            currentMap = map;
            currentMapLoading = null;
        }

        private bool IsChunkLoadingComplete()
        {
            foreach (TrackedCoroutine task in chunkLoadingTasks)
            {
                if (task.IsPlaying)
                    return false;
            }

            return true;
        }

        private void LoadChunksAroundPosition(Vector3 position)
        {
            chunkLoadingTasks.Clear();

            List<AssetReferenceGameObject> chunksAroundPosition = currentMapLoading.GetChunksAroundPosition(position, chunkLoadDistance);
            foreach (AssetReferenceGameObject chunk in chunksAroundPosition) //TODO: get all the chunks within distance from position to position + chunkLoadDistance radius
            {
#if UNITY_EDITOR
                GlobalLoggers.LoadingLogger.Log($"Loading chunk '{chunk.editorAsset.name}'");
#endif
                TrackedCoroutine loadingTask = new TrackedCoroutine(LoadChunkAsync(chunk));
                chunkLoadingTasks.Add(loadingTask);
            }
        }

        private IEnumerator LoadChunkAsync(AssetReferenceGameObject chunkAssetReference)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkAssetReference);
            yield return handle;
            
            GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), transform);
            Chunk chunk = instantiatedChunk.GetComponent<Chunk>();
            
            //should create a copy of the mesh so it doesn't directly edit the saved mesh in runtime
            chunk.CurrentTerrain.GetComponent<MeshFilter>().sharedMesh = Instantiate(chunk.CurrentTerrain.GetComponent<MeshFilter>().sharedMesh);
            
            currentChunks.Add(chunk);

            if (currentChunks.Count > 1)
            {
                //connect the chunk
                chunk.Connect(currentChunks[^2]);
            }
        }

    }
}
