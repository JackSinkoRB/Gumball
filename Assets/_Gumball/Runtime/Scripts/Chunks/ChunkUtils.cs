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
        /// Puts chunk2 at the end of chunk1, and aligns the splines.
        /// </summary>
        public static void ConnectChunks(Chunk chunk1, Chunk chunk2, bool canUndo = false)
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

            chunk1.SetConnecting(true);
            chunk2.SetConnecting(true);

            RotateChunkToAlign(chunk2, chunk1);

            //set the position of chunk 2 to the end of chunk 1
            Vector3 differenceFromChunkCenter = chunk2.FirstSample.position - chunk2.transform.position;
            chunk2.transform.position = chunk1.LastSample.position - differenceFromChunkCenter;
            
            //update immediately
            chunk2.SplineComputer.RebuildImmediate();
            chunk2.UpdateSplineSampleData();
            
            chunk1.OnConnectChunkAfter(chunk2);
            chunk2.OnConnectChunkBefore(chunk1);

            ChunkTerrainBlend terrainBlend = new ChunkTerrainBlend(chunk1, chunk2);
            terrainBlend.TryBlendTerrains();
            
            chunk1.SetConnecting(false);
            chunk2.SetConnecting(false);
        }
        
        public static Vector3 GetTangentVectorFromPoint(SplinePoint point)
        {
            //vector = tangent 2 - tangent 1 
            return point.tangent2 - point.tangent;
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
            
            //update immediately
            chunkToAlign.SplineComputer.RebuildImmediate();
            chunkToAlign.UpdateSplineSampleData();
        }

    }
}
