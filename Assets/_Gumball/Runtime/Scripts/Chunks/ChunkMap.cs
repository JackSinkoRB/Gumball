using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BezierPath;
using MyBox;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Chunk Map")]
    public class ChunkMap : ScriptableObject
    {
        
        [SerializeField] private float chunkLoadDistance = 700;
        [Tooltip("The chunk index from the chunk references that gets loaded at the world origin.")]
        [SerializeField] private int startingChunkIndex;
#if UNITY_EDITOR
        [SerializeField] private AssetReferenceGameObject[] chunkReferences;
#endif

        [Header("Debugging")]
        [SerializeField, ReadOnly] private string[] runtimeChunkAssetKeys;
        [SerializeField, ReadOnly] private List<int> chunksWithCustomLoadDistance = new();
        [SerializeField, ReadOnly] private ChunkMapData[] chunkData;
        [Tooltip("The sum of all the chunk spline lengths.")]
        [SerializeField, ReadOnly] private float totalLengthMetres;

#if UNITY_EDITOR
        public AssetReferenceGameObject[] ChunkReferences => chunkReferences;
#endif
        
        public string[] RuntimeChunkAssetKeys => runtimeChunkAssetKeys;
        public int StartingChunkIndex => startingChunkIndex;
        public List<int> ChunksWithCustomLoadDistance => chunksWithCustomLoadDistance;
        public float ChunkLoadDistance => chunkLoadDistance;
        public float TotalLengthMetres => totalLengthMetres;

        public ChunkMapData GetChunkData(int index)
        {
            if (index >= chunkData.Length || index < 0)
                throw new IndexOutOfRangeException($"No chunk data for index {index}");

            return chunkData[index];
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public void RebuildData()
        {
            Chunk[] chunkInstances = new Chunk[chunkReferences.Length];
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            try
            {
                //save the current scene
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                
                totalLengthMetres = 0;
                chunksWithCustomLoadDistance.Clear();
                chunkData = new ChunkMapData[chunkReferences.Length];
                runtimeChunkAssetKeys = new string[chunkReferences.Length];
                
                GlobalLoggers.ChunkLogger.Log($"Setup = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
                
                //ensure meshes are baked (only check each chunk once)
                HashSet<string> chunksBaked = new HashSet<string>();
                foreach (AssetReferenceGameObject chunkReference in chunkReferences)
                {
                    if (chunksBaked.Contains(chunkReference.editorAsset.name))
                        continue;

                    chunksBaked.Add(chunkReference.editorAsset.name);
                    
                    Chunk chunk = chunkReference.editorAsset.GetComponent<Chunk>();
                    chunk.FindSplineMeshes();
                    ChunkUtils.BakeMeshes(chunk, false, saveAssets: false);
                }
                
                GlobalLoggers.ChunkLogger.Log($"Baking meshes = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
                
                CreateRuntimeChunks();
                
                GlobalLoggers.ChunkLogger.Log($"Create runtime chunks = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
                
                //instantiate chunks
                for (int index = 0; index < chunkReferences.Length; index++)
                {
                    GlobalLoggers.ChunkLogger.Log($"Instantiating {runtimeChunkAssetKeys[index]}");
                    AssetReferenceGameObject chunkReference = chunkReferences[index];

                    GameObject chunkInstance = Instantiate(chunkReference.editorAsset.gameObject); //instantiate but keep the prefab references
                    Chunk chunk = chunkInstance.GetComponent<Chunk>();
                    chunkInstances[index] = chunk;
                    
                    if (chunk.HasCustomLoadDistance)
                        chunksWithCustomLoadDistance.Add(index);

                    totalLengthMetres += chunk.SplineLengthCached;
                }
                
                GlobalLoggers.ChunkLogger.Log($"Instantiate chunks = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
                
                //connect the chunks
                for (int index = 1; index < chunkReferences.Length; index++)
                {
                    Chunk previousChunk = chunkInstances[index - 1];
                    Chunk chunk = chunkInstances[index];

                    GlobalLoggers.ChunkLogger.Log($"Connecting {chunk.name} and {previousChunk.name}");

                    //create the blend data
                    ChunkBlendData newBlendData = ChunkUtils.ConnectChunksWithNewBlendData(previousChunk, chunk, ChunkUtils.LoadDirection.AFTER);
                    chunkData[index - 1] = new ChunkMapData(previousChunk, newBlendData.BlendedFirstChunkMeshData);
                    chunkData[index] = new ChunkMapData(chunk, newBlendData.BlendedLastChunkMeshData);
                    
                    //update the mesh data for the chunk copies, so the next blend has that data
                    previousChunk.SetMeshData(newBlendData.BlendedFirstChunkMeshData);
                    chunk.SetMeshData(newBlendData.BlendedLastChunkMeshData);
                }
                
                GlobalLoggers.ChunkLogger.Log($"Connect chunks = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
                
                //once connected, create the chunk object data
                for (int index = 0; index < chunkInstances.Length; index++)
                {
                    AssetReferenceGameObject chunkReference = chunkReferences[index];
                    Chunk blendedChunk = chunkInstances[index];
                    
                    Dictionary<string, List<ChunkObjectData>> data = ChunkUtils.CreateChunkObjectData(chunkReference.editorAsset.gameObject, blendedChunk);
                    chunkData[index].SetChunkObjectData(data);
                }
                
                GlobalLoggers.ChunkLogger.Log($"Create chunk object data = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
                
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                
                GlobalLoggers.ChunkLogger.Log($"Asset saving = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
            }
            catch (Exception e)
            {
                //button method won't log the error if it's in a separate thread - so make sure it is logged
                Debug.LogException(e);
            }
            finally
            {
                //reopen the scene to discard changes
                string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                EditorSceneManager.OpenScene(currentScenePath);
                
                GlobalLoggers.ChunkLogger.Log($"Destroying = {stopwatch.Elapsed.ToPrettyString(true)}");
                stopwatch.Restart();
            }
        }
        
        private void CreateRuntimeChunks()
        {
            HashSet<string> runtimeChunksCreated = new HashSet<string>();

            for (int index = 0; index < chunkReferences.Length; index++)
            {
                AssetReferenceGameObject chunkReference = chunkReferences[index];

                //only create the runtime chunk once
                if (!runtimeChunksCreated.Contains(chunkReference.editorAsset.name))
                {
                    runtimeChunksCreated.Add(chunkReference.editorAsset.name);
                    
                    GlobalLoggers.ChunkLogger.Log($"Updating runtime reference for {chunkReference.editorAsset.name}");
                    runtimeChunkAssetKeys[index] = ChunkUtils.CreateRuntimeChunk(chunkReference.editorAsset.gameObject, false);
                }
                else
                {
                    //already exists
                    runtimeChunkAssetKeys[index] = $"{chunkReference.editorAsset.gameObject.name}{ChunkUtils.RuntimeChunkSuffix}";
                }
            }
        }
#endif
    }
}
