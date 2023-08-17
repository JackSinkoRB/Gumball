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
        
        private readonly List<TrackedCoroutine> chunkLoadingTasks = new();

        [Obsolete("To be removed - for testing only")]
        public MapData TestingMap => testingMap;
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

        private IEnumerator LoadChunkAsync(AssetReferenceGameObject chunk)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunk);
            yield return handle;
            //TODO: positioning
            Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), transform);
        }

    }
}
