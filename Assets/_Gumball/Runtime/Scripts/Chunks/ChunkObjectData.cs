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
        
        [SerializeField] private bool hideWhenFarAway;
        [SerializeField] private Vector3 positionRelativeToParent;
        [SerializeField] private Quaternion locationRotation;
        [SerializeField] private Vector3 scaleRelativeToChunk;

        public ChunkObjectData(Chunk chunkReference, ChunkObject chunkObject)
        {
            hideWhenFarAway = chunkObject.HideWhenFarAway;
            Transform desiredParent = hideWhenFarAway ? chunkReference.TerrainHighLOD.transform : chunkReference.transform;
            positionRelativeToParent = desiredParent.InverseTransformPoint(chunkObject.transform.position);
            locationRotation = chunkObject.transform.localRotation;
            scaleRelativeToChunk = GetScaleRelativeToChunk(chunkReference, chunkObject);
        }

        public GameObject LoadIntoChunk(AsyncOperationHandle<GameObject> handle, Chunk chunk)
        {
            GameObject chunkObject = Object.Instantiate(handle.Result, hideWhenFarAway ? chunk.TerrainHighLOD.transform : chunk.transform);
            chunkObject.transform.localPosition = positionRelativeToParent;
            chunkObject.transform.localRotation = locationRotation;
            chunkObject.transform.localScale = scaleRelativeToChunk;

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
