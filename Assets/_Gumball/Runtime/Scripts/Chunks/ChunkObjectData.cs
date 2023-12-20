using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Gumball
{
    [Serializable]
    public struct ChunkObjectData
    {
        [SerializeField] private Vector3 localPosition;
        [SerializeField] private Quaternion localRotation;
        [SerializeField] private Vector3 localScale;

        public ChunkObjectData(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }

        public GameObject LoadIntoChunk(AsyncOperationHandle<GameObject> handle, Chunk chunk)
        {
            GameObject chunkObject = Object.Instantiate(handle.Result, chunk.transform);
            chunkObject.transform.localPosition = localPosition;
            chunkObject.transform.localRotation = localRotation;
            chunkObject.transform.localScale = localScale;
            chunkObject.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            return chunkObject;
        }
    }
}
