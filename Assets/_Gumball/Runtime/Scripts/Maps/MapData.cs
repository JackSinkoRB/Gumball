using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Map Data")]
    public class MapData : ScriptableObject
    {

        [SerializeField] private int startingChunkIndex;
        [SerializeField] private Vector3 vehicleStartingPosition;
        [SerializeField] private Vector3 vehicleStartingRotation;
        
        [Space(5)]
        [SerializeField] private AssetReferenceGameObject[] chunkReferences;

        public int StartingChunkIndex => startingChunkIndex;
        public Vector3 VehicleStartingPosition => vehicleStartingPosition;
        public Vector3 VehicleStartingRotation => vehicleStartingRotation;
        public AssetReferenceGameObject[] ChunkReferences => chunkReferences;

        [SerializeField, ReadOnly] private ChunkBlendData blendData;

        public ChunkBlendData BlendData => blendData;
        
#if UNITY_EDITOR
        [ButtonMethod]
        public void RebuildBlendData()
        {
            Chunk previousChunk = null;
            
            //you need to spawn every single chunk one after the other 
            foreach (AssetReferenceGameObject chunkReference in chunkReferences)
            {
                GlobalLoggers.TerrainLogger.Log($"Loading {chunkReference.editorAsset.name}");
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkReference);
                handle.WaitForCompletion();
            
                GlobalLoggers.TerrainLogger.Log($"Instantiating {chunkReference.editorAsset.name}");
                GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), ChunkManager.Instance.transform);
                instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                Chunk chunk = instantiatedChunk.GetComponent<Chunk>();

                //connect to the previous chunk (if there is one)
                if (previousChunk != null)
                {
                    GlobalLoggers.TerrainLogger.Log($"Connecting {chunk.gameObject.name} and {previousChunk.gameObject.name}");
                    
                    //create the blend data
                    blendData = ChunkUtils.CreateBlendData(previousChunk, chunk);
                    
                    GlobalLoggers.TerrainLogger.Log($"Destroying {previousChunk.gameObject.name}");
                    DestroyImmediate(previousChunk.gameObject);
                }

                previousChunk = chunk;
            }

            if (previousChunk != null)
            {
                GlobalLoggers.TerrainLogger.Log($"Destroying {previousChunk.gameObject.name}");
                DestroyImmediate(previousChunk.gameObject);
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
