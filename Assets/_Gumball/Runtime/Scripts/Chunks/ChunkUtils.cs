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
            CUSTOM,
            BEFORE,
            AFTER
        }

        public const string TerrainTag = "Terrain";
        public const string TerrainMeshAssetFolderPath = "Assets/_Gumball/Runtime/Meshes/Terrains/";
        public const string TerrainMeshPrefix = "ProceduralTerrain_";
        public const string RoadMeshAssetFolderPath = "Assets/_Gumball/Runtime/Meshes/Roads/";
        public const string RoadMeshPrefix = "RoadMesh_";
        public const string ChunkFolderPath = "Assets/_Gumball/Runtime/Prefabs/Chunks";
        
#if UNITY_EDITOR
        /// <summary>
        /// Connects the chunks with NEW blend data.
        /// Puts chunk2 at the start or end of chunk1 (depending on direction), and aligns the splines.
        /// <remarks>Should only be used at edit-time. Use pre-generated blend data at runtime.</remarks>
        /// </summary>
        public static ChunkBlendData ConnectChunksWithNewBlendData(Chunk chunk1, Chunk chunk2, LoadDirection direction, bool canUndo = false)
        {
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
            
            GlobalLoggers.ChunkLogger.Log($"Appending {chunk2.name} to the end of {chunk1.name}");

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
            
            chunk1.GetComponent<ChunkEditorTools>().OnConnectChunkAfter(chunk2);
            chunk2.GetComponent<ChunkEditorTools>().OnConnectChunkBefore(chunk1);
            
            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);

            return blendData;
        }
#endif

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
            
            GlobalLoggers.ChunkLogger.Log($"Appending {chunk2.name} to the end of {chunk1.name}");
            
            chunk1.DisableAutomaticTerrainRecreation(true);
            chunk2.DisableAutomaticTerrainRecreation(true);

            chunk1.UpdateSplineSampleData();
            chunk2.UpdateSplineSampleData();

            MoveChunkToOther(chunk1, chunk2, direction);

            Chunk firstChunk = chunk1.UniqueID.Equals(blendData.FirstChunkID) ? chunk1 : chunk2;
            Chunk lastChunk = chunk2.UniqueID.Equals(blendData.LastChunkID) ? chunk2 : chunk1;
            blendData.ApplyToChunks(firstChunk, lastChunk);

#if UNITY_EDITOR
            chunk1.GetComponent<ChunkEditorTools>().OnConnectChunkAfter(chunk2);
            chunk2.GetComponent<ChunkEditorTools>().OnConnectChunkBefore(chunk1);
#endif
            
            //update immediately as the position has changed
            chunk2.UpdateSplineImmediately();
            
            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);
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

#if UNITY_EDITOR
        public static void BakeRoadMesh(Chunk chunk, bool replace = true)
        {
            bool alreadyBaked = chunk.RoadMesh.baked;
            if (alreadyBaked && !replace)
                return;

            chunk.RoadMesh.Unbake();
            chunk.RoadMesh.Bake(true, true);

            MeshFilter meshFilter = chunk.RoadMesh.GetComponent<MeshFilter>();
            
            string path = $"{RoadMeshAssetFolderPath}/{RoadMeshPrefix}{chunk.UniqueID}.asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);

            MeshCollider meshCollider = chunk.RoadMesh.GetComponent<MeshCollider>();
            Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            meshFilter.sharedMesh = savedMesh;
            meshCollider.sharedMesh = savedMesh;
            
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshCollider);
            PrefabUtility.RecordPrefabInstancePropertyModifications(chunk.RoadMesh);
            EditorUtility.SetDirty(chunk.gameObject);
            
            AssetDatabase.SaveAssets();
            
            GlobalLoggers.ChunkLogger.Log("Baked " + path);
        }
#endif
        
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
    }
}
