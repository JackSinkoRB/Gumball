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

        public const string TerrainTag = "Terrain";
        
        /// <summary>
        /// Connects the chunks with NEW blend data.
        /// Puts chunk2 at the end of chunk1, and aligns the splines.
        /// <remarks>Should only be used at edit-time. Use pre-generated blend data at runtime.</remarks>
        /// </summary>
        public static ChunkBlendData CreateBlendData(Chunk chunk1, Chunk chunk2, bool canUndo = false)
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

            RotateChunkToAlign(chunk2, chunk1);

            //update immediately
            UpdateSplineImmediately(chunk1);
            UpdateSplineImmediately(chunk2);

            //set the position of chunk 2 to the end of chunk 1
            Vector3 differenceFromChunkCenter = chunk2.FirstSample.position - chunk2.transform.position;
            chunk2.transform.position = chunk1.LastSample.position - differenceFromChunkCenter;
            
            //update immediately as the position has changed
            UpdateSplineImmediately(chunk2);

            ChunkBlendData blendData = new ChunkBlendData(chunk1, chunk2);

            //update immediately as the position has changed
            UpdateSplineImmediately(chunk1);
            UpdateSplineImmediately(chunk2);
            
            chunk1.OnConnectChunkAfter(chunk2);
            chunk2.OnConnectChunkBefore(chunk1);

            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);

            return blendData;
        }

        /// <summary>
        /// Connects the chunks using EXISTING blend data.
        /// Puts chunk2 at the end of chunk1, and aligns the splines.
        /// </summary>
        public static void ConnectChunks(Chunk chunk1, Chunk chunk2, ChunkBlendData blendData, bool canUndo = false)
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
            
            RotateChunkToAlign(chunk2, chunk1);

            chunk1.UpdateSplineSampleData();
            chunk2.UpdateSplineSampleData();

            //set the position of chunk 2 to the end of chunk 1
            Vector3 differenceFromChunkCenter = chunk2.FirstSample.position - chunk2.transform.position;
            chunk2.transform.position = chunk1.LastSample.position - differenceFromChunkCenter;
            
            blendData.ApplyToChunks(chunk1, chunk2);
            
            chunk1.OnConnectChunkAfter(chunk2);
            chunk2.OnConnectChunkBefore(chunk1);
            
            //update immediately as the position has changed
            UpdateSplineImmediately(chunk2);
            
            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);
        }
        
        public static Vector3 GetTangentVectorFromPoint(SplinePoint point)
        {
            //vector = tangent 2 - tangent 1 
            return point.tangent2 - point.tangent;
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

        private static void RotateChunkToAlign(Chunk chunkToAlign, Chunk chunkToAlignWith)
        {
            SplinePoint chunkToAlignPoint = chunkToAlign.SplineComputer.GetPoint(0);
            SplinePoint chunkToAlignWithPoint = chunkToAlignWith.SplineComputer.GetPoint(chunkToAlignWith.LastPointIndex);
            
            //get the tangent vector for each point
            Vector3 chunkToAlignTangentVector = GetTangentVectorFromPoint(chunkToAlignPoint);
            Vector3 chunkToAlignWithTangentVector = GetTangentVectorFromPoint(chunkToAlignWithPoint);

            //calculate the relative rotation between the initial and target tangents
            Quaternion rotationToAlign = Quaternion.FromToRotation(chunkToAlignTangentVector, chunkToAlignWithTangentVector);

            //apply the relative rotation while preserving the existing rotation
            chunkToAlign.transform.rotation = rotationToAlign * chunkToAlign.transform.rotation;
        }
        
        public static void UpdateSplineImmediately(Chunk chunk)
        {
            chunk.SplineComputer.RebuildImmediate();
            chunk.UpdateSplineSampleData();
        }

    }
}
