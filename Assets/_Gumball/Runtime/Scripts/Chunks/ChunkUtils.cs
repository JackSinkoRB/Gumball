using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    public static class ChunkUtils
    {
        
        public enum LoadDirection
        {
            BEFORE,
            AFTER
        }

        public const string TerrainTag = "Terrain";
        public const string MeshAssetFolderPath = "Assets/_Gumball/Runtime/Meshes/Terrains/";
        private const string chunkFolderPath = "Assets/_Gumball/Runtime/Prefabs/Chunks";

        /// <summary>
        /// Connects the chunks with NEW blend data.
        /// Puts chunk2 at the start or end of chunk1 (depending on direction), and aligns the splines.
        /// <remarks>Should only be used at edit-time. Use pre-generated blend data at runtime.</remarks>
        /// </summary>
        public static ChunkBlendData ConnectChunksWithNewBlendData(Chunk chunk1, Chunk chunk2, LoadDirection direction, bool canUndo = false)
        {
#if UNITY_EDITOR
            if (canUndo)
            {
                Undo.RecordObjects(new Object[]
                {
                    chunk1, chunk2,
                    chunk2.transform,
                    chunk1.CurrentTerrain.GetComponent<MeshFilter>(),
                    chunk2.CurrentTerrain.GetComponent<MeshFilter>()
                }, "Connect Chunk");
            }
#endif
            
            GlobalLoggers.TerrainLogger.Log($"Appending {chunk2.name} to the end of {chunk1.name}");

            chunk1.DisableAutomaticTerrainRecreation(true);
            chunk2.DisableAutomaticTerrainRecreation(true);

            //update immediately
            chunk1.UpdateSplineImmediately();
            chunk2.UpdateSplineImmediately();

            MoveChunkToOther(chunk1, chunk2, direction);
            
            //update immediately as the position has changed
            chunk2.UpdateSplineImmediately();

            ChunkBlendData blendData = new ChunkBlendData(chunk1, chunk2);

            //update immediately as the position has changed
            chunk1.UpdateSplineImmediately();
            chunk2.UpdateSplineImmediately();
            
            chunk1.OnConnectChunkAfter(chunk2);
            chunk2.OnConnectChunkBefore(chunk1);

            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);

            return blendData;
        }

        /// <summary>
        /// Connects the chunks using EXISTING blend data.
        /// Puts chunk2 at the start or end of chunk1 (depending on direction), and aligns the splines.
        /// </summary>
        public static void ConnectChunks(Chunk chunk1, Chunk chunk2, LoadDirection direction, ChunkBlendData blendData, bool canUndo = false)
        {
#if UNITY_EDITOR
            if (canUndo)
            {
                Undo.RecordObjects(new Object[]
                {
                    chunk1, chunk2,
                    chunk2.transform,
                    chunk1.CurrentTerrain.GetComponent<MeshFilter>(),
                    chunk2.CurrentTerrain.GetComponent<MeshFilter>()
                }, "Connect Chunk");
            }
#endif
            
            GlobalLoggers.TerrainLogger.Log($"Appending {chunk2.name} to the end of {chunk1.name}");
            
            chunk1.DisableAutomaticTerrainRecreation(true);
            chunk2.DisableAutomaticTerrainRecreation(true);

            chunk1.UpdateSplineSampleData();
            chunk2.UpdateSplineSampleData();

            MoveChunkToOther(chunk1, chunk2, direction);

            Chunk firstChunk = chunk1.UniqueID.Equals(blendData.FirstChunkID) ? chunk1 : chunk2;
            Chunk lastChunk = chunk2.UniqueID.Equals(blendData.LastChunkID) ? chunk2 : chunk1;
            blendData.ApplyToChunks(firstChunk, lastChunk);
            
            chunk1.OnConnectChunkAfter(chunk2);
            chunk2.OnConnectChunkBefore(chunk1);
            
            //update immediately as the position has changed
            chunk2.UpdateSplineImmediately();
            
            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);
        }
        
        private static void MoveChunkToOther(Chunk chunk1, Chunk chunk2, LoadDirection direction)
        {
            //align the rotation of chunk2 to match chunk1
            Quaternion firstSplineEndRotation = direction == LoadDirection.AFTER ? chunk1.LastSample.rotation : chunk1.FirstSample.rotation;
            Quaternion secondSplineStartRotation = direction == LoadDirection.AFTER ? chunk2.FirstSample.rotation : chunk2.LastSample.rotation;
            Quaternion rotationDifference = firstSplineEndRotation * Quaternion.Inverse(secondSplineStartRotation);
            chunk2.transform.rotation *= rotationDifference;
            
            //update immediately
            chunk2.UpdateSplineImmediately();

            //set the position of chunk 2 to the end of chunk 1
            SplineSample chunk2ConnectionPoint = direction == LoadDirection.AFTER ? chunk2.FirstSample : chunk2.LastSample;
            SplineSample chunk1ConnectionPoint = direction == LoadDirection.AFTER ? chunk1.LastSample : chunk1.FirstSample;
            chunk2.transform.position += chunk1ConnectionPoint.position - chunk2ConnectionPoint.position;
        }
        
        /// <summary>
        /// Gets the UV coordinates from the vertex positions in world space (triplanar mapping).
        /// </summary>
        public static Vector2[] GetTriplanarUVs(List<Vector3> vertexPositions, Transform terrain)
        {
            return GetTriplanarUVs(vertexPositions.ToArray(), terrain);
        }
        
        /// <summary>
        /// Gets the UV coordinates from the vertex positions in world space (triplanar mapping).
        /// </summary>
        public static Vector2[] GetTriplanarUVs(Vector3[] vertexPositions, Transform terrain)
        {
            Vector2[] uvs = new Vector2[vertexPositions.Length];

            for (int vertexIndex = 0; vertexIndex < vertexPositions.Length; vertexIndex++)
            {
                Vector3 vertexPosition = vertexPositions[vertexIndex];
                Vector3 vertexPositionWorld = terrain.TransformPoint(vertexPosition);

                Vector2 uvX = new Vector2(vertexPositionWorld.z, vertexPositionWorld.y);
                Vector2 uvY = new Vector2(vertexPositionWorld.x, vertexPositionWorld.z);
                Vector2 uvZ = new Vector2(vertexPositionWorld.x, vertexPositionWorld.y);

                Vector2 finalUV = uvX + uvY + uvZ;
                uvs[vertexIndex] = finalUV;
            }

            return uvs;
        }
        
        public static void CleanupUnusedMeshes(Chunk ignoreChunk = null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            //find all the used meshes
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { chunkFolderPath });
            HashSet<string> whitelistedIds = new HashSet<string>();
            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                Chunk chunkPrefab = AssetDatabase.LoadAssetAtPath<Chunk>(prefabPath);
                if (chunkPrefab == null)
                    continue; 
                if (chunkPrefab.CurrentTerrain == null)
                    continue;

                whitelistedIds.Add(chunkPrefab.UniqueID);
            }

            //find all the terrain meshes
            string[] meshGuids = AssetDatabase.FindAssets("t:Mesh", new[] { MeshAssetFolderPath });
            foreach (string meshGuid in meshGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(meshGuid);
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                bool isWhitelisted = false;
                foreach (string whitelistedId in whitelistedIds)
                {
                    if (assetPath.Contains(whitelistedId))
                    {
                        isWhitelisted = true;
                        break;
                    }
                }

                if (ignoreChunk != null && assetPath.Contains(ignoreChunk.UniqueID))
                    isWhitelisted = true;
                
                //only delete scene instance if scene is loaded
                bool isApartOfScene = assetPath.Replace("ProceduralTerrain_", "").Contains("_") && !assetPath.Contains("Prefab");
                bool isApartOfCurrentScene = assetPath.Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                if (isApartOfScene)
                {
                    if (isApartOfCurrentScene)
                    {
                        //search all gameobjects for chunk components
                        List<Chunk> chunksInScene = SceneUtils.GetAllComponentsInActiveScene<Chunk>();
                        foreach (Chunk chunkInScene in chunksInScene)
                        {
                            if (assetPath.Contains(chunkInScene.UniqueID))
                            {
                                isWhitelisted = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isWhitelisted = true;
                    }
                }

                if (!isWhitelisted)
                {
                    //remove it
                    string path = AssetDatabase.GetAssetPath(mesh);
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log($"Removed unused terrain mesh asset: {path}");
                }
            }
            AssetDatabase.SaveAssets();
#endif
        }

    }
}
