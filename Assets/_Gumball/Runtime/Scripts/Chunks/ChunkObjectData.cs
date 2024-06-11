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
        
        [SerializeField] private Vector3 positionRelativeToChunk;
        [SerializeField] private Quaternion rotation;
        [SerializeField] private Vector3 scaleRelativeToChunk;
        [SerializeField] private bool alwaysGrounded;

        public ChunkObjectData(Chunk chunkReference, ChunkObject chunkObject)
        {
            positionRelativeToChunk = chunkReference.transform.InverseTransformPoint(chunkObject.transform.position);
            rotation = chunkObject.transform.rotation;
            scaleRelativeToChunk = GetScaleRelativeToChunk(chunkReference, chunkObject);
            alwaysGrounded = chunkObject.AlwaysGrounded;
        }

        public GameObject LoadIntoChunk(AsyncOperationHandle<GameObject> handle, Chunk chunk)
        {
            GameObject chunkObject = Object.Instantiate(handle.Result, chunk.transform);
            chunkObject.transform.localPosition = positionRelativeToChunk;
            chunkObject.transform.localRotation = rotation;
            chunkObject.transform.localScale = scaleRelativeToChunk;
            
            if (alwaysGrounded)
            {
                bool groundedSuccessfully = ChunkUtils.GroundObject(chunkObject.transform);
                if (!groundedSuccessfully)
                    chunkObject.SetActive(false); //don't allow floating object
            }

            chunkObject.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            return chunkObject;
        }

        private static Vector3 GetScaleRelativeToChunk(Chunk chunkReference, ChunkObject chunkObject)
        {
            Vector3 totalScale = Vector3.one;
            Transform parent = chunkObject.transform;
            while (parent != null && parent != chunkReference.transform)
            {
                totalScale = totalScale.Multiply(parent.localScale);
                parent = parent.parent;
            }

            return totalScale;
        }
        
    }
}
