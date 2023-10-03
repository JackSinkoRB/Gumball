using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private SerializedDictionary<ChunkPair, ChunkBlendData> blendData = new();

        public ChunkBlendData GetBlendData(AssetReferenceGameObject firstChunk, AssetReferenceGameObject lastChunk)
        {
            ChunkPair chunkPair = new ChunkPair(firstChunk, lastChunk);
            
            if (!blendData.ContainsKey(chunkPair))
#if UNITY_EDITOR
                throw new KeyNotFoundException($"Could not find blend data for chunks {firstChunk.editorAsset.name} and {lastChunk.editorAsset.name}.");
#else
                throw new KeyNotFoundException($"Could not find blend data for chunks.");
#endif
            
            return blendData[chunkPair];
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public void RebuildBlendData()
        {
            blendData.Clear();
            Chunk previousChunk = null;
            AssetReferenceGameObject previousChunkAsset = null;
            
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
                    GlobalLoggers.TerrainLogger.Log($"Connecting {chunk.name} and {previousChunk.name}");
                    
                    //create the blend data
                    ChunkBlendData newBlendData = ChunkUtils.ConnectChunksWithNewBlendData(previousChunk, chunk, ChunkUtils.LoadDirection.AFTER);
                    blendData[new ChunkPair(previousChunkAsset, chunkReference)] = newBlendData;

                    GlobalLoggers.TerrainLogger.Log($"Destroying {previousChunk.name}");
                    DestroyImmediate(previousChunk.gameObject);
                }

                previousChunk = chunk;
                previousChunkAsset = chunkReference;
            }

            if (previousChunk != null)
            {
                GlobalLoggers.TerrainLogger.Log($"Destroying {previousChunk.name}");
                DestroyImmediate(previousChunk.gameObject);
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
