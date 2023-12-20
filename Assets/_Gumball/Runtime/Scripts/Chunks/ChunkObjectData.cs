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
        [SerializeField] private bool alwaysGrounded;

        public ChunkObjectData(ChunkObject chunkObject)
        {
            localPosition = chunkObject.transform.localPosition;
            localRotation = chunkObject.transform.localRotation;
            localScale = chunkObject.transform.localScale;
            alwaysGrounded = chunkObject.AlwaysGrounded;
        }

        public GameObject LoadIntoChunk(AsyncOperationHandle<GameObject> handle, Chunk chunk)
        {
            GameObject chunkObject = Object.Instantiate(handle.Result, chunk.transform);
            chunkObject.transform.localPosition = localPosition;
            chunkObject.transform.localRotation = localRotation;
            chunkObject.transform.localScale = localScale;
            if (alwaysGrounded)
                ChunkUtils.GroundObject(chunkObject.transform);

            chunkObject.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            return chunkObject;
        }
    }
}
