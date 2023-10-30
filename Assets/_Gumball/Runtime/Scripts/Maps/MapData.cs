using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
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
        
        [SerializedDictionary("ChunkPair", "ChunkBlendData")]
        [SerializeField] private ChunkBlendData[] blendData;

        public ChunkBlendData GetBlendData(int connectionIndex)
        {
            if (connectionIndex >= blendData.Length || connectionIndex < 0)
                throw new IndexOutOfRangeException($"No blend data for connection index {connectionIndex}");

            return blendData[connectionIndex];
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public async Task RebuildBlendData()
        {
            blendData = new ChunkBlendData[chunkReferences.Length-1];

            AsyncOperationHandle[] handles = new AsyncOperationHandle[chunkReferences.Length];
            Chunk[] chunks = new Chunk[chunkReferences.Length];

            for (int index = 0; index < chunkReferences.Length; index++)
            {
                AssetReferenceGameObject chunkReference = chunkReferences[index];
                GlobalLoggers.ChunkLogger.Log($"Loading {chunkReference.editorAsset.name}");
     
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkReference);
                handles[index] = handle;

                int finalIndex = index;
                handle.Completed += x =>
                {
                    GlobalLoggers.ChunkLogger.Log($"Instantiating {chunkReference.editorAsset.name}");
                    GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), ChunkManager.Instance.transform);
                    instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                    Chunk chunk = instantiatedChunk.GetComponent<Chunk>();
                    chunks[finalIndex] = chunk;
                };
            }
            
            await Task.WhenAll(handles.Select(handle => handle.Task));
            
            //connect the chunks
            for (int index = 1; index < chunkReferences.Length; index++)
            {
                int connectionIndex = index - 1;
                Chunk chunk = chunks[index];
                Chunk previousChunk = chunks[index - 1];
                
                GlobalLoggers.ChunkLogger.Log($"Connecting {chunk.name} and {previousChunk.name}");
                    
                //create the blend data
                ChunkBlendData newBlendData = ChunkUtils.ConnectChunksWithNewBlendData(previousChunk, chunk, ChunkUtils.LoadDirection.AFTER);
                blendData[connectionIndex] = newBlendData;
                
                GlobalLoggers.ChunkLogger.Log($"Destroying {previousChunk.name}");
                DestroyImmediate(previousChunk.gameObject);
            }

            Chunk lastChunk = chunks[^1];
            GlobalLoggers.ChunkLogger.Log($"Destroying {lastChunk.name}");
            DestroyImmediate(lastChunk.gameObject);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
