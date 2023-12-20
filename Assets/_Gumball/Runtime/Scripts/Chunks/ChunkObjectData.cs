using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MyBox;
using Object = UnityEngine.Object;

namespace Gumball
{
    [Serializable]
    public struct ChunkObjectData
    {
        [SerializeField] private string assetKey;
        [SerializeField] private Vector3 localPosition;
        [SerializeField] private Quaternion localRotation;
        [SerializeField] private Vector3 localScale;

        public string AssetKey => assetKey;
        public Vector3 LocalPosition => localPosition;
        public Quaternion LocalRotation => localRotation;
        public Vector3 LocalScale => localScale;

        public ChunkObjectData(string assetKey, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            this.assetKey = assetKey;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }

        public IEnumerator LoadIntoChunk(Chunk chunk)
        {
            if (assetKey.IsNullOrEmpty())
            {
                Debug.LogError($"Could not load as the asset key is missing.");
                yield break;
            }

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetKey);
            yield return handle;
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"There was an error loading chunk object with key {assetKey}.");
                yield break;
            }
                
            GameObject chunkObject = Object.Instantiate(handle.Result, chunk.transform.TransformPoint(localPosition), localRotation, chunk.transform);
            chunkObject.transform.localScale = localScale;
            chunkObject.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
        }
    }
}
