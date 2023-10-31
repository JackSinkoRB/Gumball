using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<int> chunksWithCustomLoadDistance = new();
        [SerializeField, ReadOnly] private ChunkMapData[] chunkData;

        public int StartingChunkIndex => startingChunkIndex;
        public Vector3 VehicleStartingPosition => vehicleStartingPosition;
        public Vector3 VehicleStartingRotation => vehicleStartingRotation;
        public AssetReferenceGameObject[] ChunkReferences => chunkReferences;
        public List<int> ChunksWithCustomLoadDistance => chunksWithCustomLoadDistance;

        public ChunkMapData GetChunkData(int index)
        {
            if (index >= chunkData.Length || index < 0)
                throw new IndexOutOfRangeException($"No chunk data for index {index}");

            return chunkData[index];
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public async Task RebuildData()
        {
            chunksWithCustomLoadDistance.Clear();
            chunkData = new ChunkMapData[chunkReferences.Length];

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

                    if (chunk.HasCustomLoadDistance)
                        chunksWithCustomLoadDistance.Add(finalIndex);
                };
            }
            
            await Task.WhenAll(handles.Select(handle => handle.Task));
            
            //connect the chunks
            for (int index = 1; index < chunkReferences.Length; index++)
            {
                Chunk chunk = chunks[index];
                Chunk previousChunk = chunks[index - 1];
                
                GlobalLoggers.ChunkLogger.Log($"Connecting {chunk.name} and {previousChunk.name}");
                    
                //create the chunk data
                ChunkBlendData newBlendData = ChunkUtils.ConnectChunksWithNewBlendData(previousChunk, chunk, ChunkUtils.LoadDirection.AFTER);
                chunkData[index - 1] = new ChunkMapData(previousChunk, newBlendData.BlendedFirstChunkMeshData);
                chunkData[index] = new ChunkMapData(chunk, newBlendData.BlendedLastChunkMeshData);

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
