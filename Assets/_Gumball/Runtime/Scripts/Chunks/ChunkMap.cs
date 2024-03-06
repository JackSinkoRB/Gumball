using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBox;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.ResourceManagement.ResourceLocations;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Chunk Map")]
    public class ChunkMap : ScriptableObject
    {
        //ChunkMap
        [SerializeField] private int startingChunkIndex;
        [SerializeField] private Vector3 vehicleStartingPosition;
        [SerializeField] private Vector3 vehicleStartingRotation;

        [Space(5)]
        [SerializeField] private Material skybox;
        [SerializeField] private float chunkLoadDistance = 700;
#if UNITY_EDITOR
        [SerializeField] private AssetReferenceGameObject[] chunkReferences;
#endif

        [Header("Debugging")]
        [SerializeField, ReadOnly] private string[] runtimeChunkAssetKeys;
        [SerializeField, ReadOnly] private List<int> chunksWithCustomLoadDistance = new();
        [SerializeField, ReadOnly] private ChunkMapData[] chunkData;

#if UNITY_EDITOR
        public AssetReferenceGameObject[] ChunkReferences => chunkReferences;
#endif

        public int StartingChunkIndex => startingChunkIndex;
        public Vector3 VehicleStartingPosition => vehicleStartingPosition;
        public Vector3 VehicleStartingRotation => vehicleStartingRotation;
        public string[] RuntimeChunkAssetKeys => runtimeChunkAssetKeys;
        
        public List<int> ChunksWithCustomLoadDistance => chunksWithCustomLoadDistance;
        public float ChunkLoadDistance => chunkLoadDistance;

        public void OnMapLoad()
        {
            UpdateSkybox();
        }

        private void UpdateSkybox()
        {
            if (skybox == null)
            {
                Debug.LogWarning($"{name} is missing a skybox reference.");
                return;
            }

            RenderSettings.skybox = skybox;
        }
        
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
            bool failed = false;
            Chunk[] runtimeChunks = new Chunk[chunkReferences.Length];
            try
            {
                chunksWithCustomLoadDistance.Clear();
                chunkData = new ChunkMapData[chunkReferences.Length];

                AsyncOperationHandle[] handles = new AsyncOperationHandle[chunkReferences.Length];
                runtimeChunkAssetKeys = new string[chunkReferences.Length];

                HashSet<string> runtimeChunksCreated = new HashSet<string>();

                for (int index = 0; index < chunkReferences.Length; index++)
                {
                    AssetReferenceGameObject chunkReference = chunkReferences[index];

                    //only create the runtime chunk once
                    if (!runtimeChunksCreated.Contains(chunkReference.editorAsset.name))
                    {
                        GlobalLoggers.ChunkLogger.Log($"Updating runtime reference for {chunkReference.editorAsset.name}");
                        runtimeChunkAssetKeys[index] = ChunkUtils.CreateRuntimeChunk(chunkReference.editorAsset.gameObject, false);
                        runtimeChunksCreated.Add(chunkReference.editorAsset.name);
                    }
                    else
                    {
                        //already exists
                        runtimeChunkAssetKeys[index] = $"{chunkReference.editorAsset.gameObject.name}{ChunkUtils.RuntimeChunkSuffix}";
                    }

                    GlobalLoggers.ChunkLogger.Log($"Loading {runtimeChunkAssetKeys[index]}");
                    AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(runtimeChunkAssetKeys[index]);

                    handles[index] = handle;

                    int finalIndex = index;
                    handle.Completed += x =>
                    {
                        if (failed)
                            return;
                        
                        GlobalLoggers.ChunkLogger.Log($"Instantiating {runtimeChunkAssetKeys[finalIndex]}");
                        GameObject instantiatedChunk = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero));
                        instantiatedChunk.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                        Chunk chunk = instantiatedChunk.GetComponent<Chunk>();
                        runtimeChunks[finalIndex] = chunk;

                        if (chunk.HasCustomLoadDistance)
                            chunksWithCustomLoadDistance.Add(finalIndex);
                    };
                }

                await runtimeChunks.WaitForNoNulls(15);

                //connect the chunks
                for (int index = 1; index < chunkReferences.Length; index++)
                {
                    Chunk chunk = runtimeChunks[index];
                    Chunk previousChunk = runtimeChunks[index - 1];

                    GlobalLoggers.ChunkLogger.Log($"Connecting {chunk.name} and {previousChunk.name}");

                    //create the chunk data
                    ChunkBlendData newBlendData = ChunkUtils.ConnectChunksWithNewBlendData(previousChunk, chunk, ChunkUtils.LoadDirection.AFTER);
                    chunkData[index - 1] = new ChunkMapData(previousChunk, newBlendData.BlendedFirstChunkMeshData);
                    chunkData[index] = new ChunkMapData(chunk, newBlendData.BlendedLastChunkMeshData);
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets(); //saves this map data and also saves the runtime chunks
            }
            catch (Exception e)
            {
                //button method won't log the error if it's in a separate thread - so make sure it is logged
                Debug.LogException(e);
                failed = true;
            }
            finally
            {
                foreach (Chunk chunk in runtimeChunks)
                {
                    GlobalLoggers.ChunkLogger.Log($"Destroying {chunk.name}");
                    DestroyImmediate(chunk.gameObject);
                }
            }
        }
#endif
    }
}
