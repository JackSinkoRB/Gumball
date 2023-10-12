using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public void RebuildBlendData()
        {
            blendData = new ChunkBlendData[chunkReferences.Length-1];

            Chunk previousChunk = null;
            int connectionIndex = 0;

            //you need to spawn every single chunk one after the other 
            foreach (AssetReferenceGameObject chunkReference in chunkReferences)
            {
                GlobalLoggers.ChunkLogger.Log($"Loading {chunkReference.editorAsset.name}");
                
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(chunkReference);

                //TODO: can make this faster by loading and instantiating all the chunks first and waiting for them ALL to complete before connecting
                handle.WaitForCompletion();
            
                GlobalLoggers.ChunkLogger.Log($"Instantiating {chunkReference.editorAsset.name}");
                GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), ChunkManager.Instance.transform);
                instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                Chunk chunk = instantiatedChunk.GetComponent<Chunk>();

                //connect to the previous chunk (if there is one)
                if (previousChunk != null)
                {
                    GlobalLoggers.ChunkLogger.Log($"Connecting {chunk.name} and {previousChunk.name}");
                    
                    //create the blend data
                    ChunkBlendData newBlendData = ChunkUtils.ConnectChunksWithNewBlendData(previousChunk, chunk, ChunkUtils.LoadDirection.AFTER);
                    blendData[connectionIndex] = newBlendData;

                    GlobalLoggers.ChunkLogger.Log($"Destroying {previousChunk.name}");
                    DestroyImmediate(previousChunk.gameObject);

                    connectionIndex++;
                }

                previousChunk = chunk;
            }

            if (previousChunk != null)
            {
                GlobalLoggers.ChunkLogger.Log($"Destroying {previousChunk.name}");
                DestroyImmediate(previousChunk.gameObject);
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
