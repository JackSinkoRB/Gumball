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
                Undo.IncrementCurrentGroup();
                Undo.RegisterFullObjectHierarchyUndo(chunk2.gameObject, "Connect Chunk");
                int currentGroup = Undo.GetCurrentGroup();
                Undo.RegisterFullObjectHierarchyUndo(chunk1.gameObject, "Connect Chunk");
                Undo.CollapseUndoOperations(currentGroup+1);
            }
#endif

            RotateChunkToAlign(chunk2, chunk1);
            
            //set the position of chunk 2 to the end of chunk 1
            Vector3 differenceFromChunkCenter = chunk2.SplineComputer.GetPoint(0).position - chunk2.transform.position;
            chunk2.transform.position = chunk1.SplineComputer.GetPoint(chunk1.LastPointIndex).position - differenceFromChunkCenter;
            
            chunk1.OnConnectChunkAfter(chunk2);
            chunk1.OnConnectChunkBefore(chunk1);
            
#if UNITY_EDITOR
            if (canUndo)
            {
                EditorUtility.SetDirty(chunk2.gameObject);
                EditorUtility.SetDirty(chunk1.gameObject);
            }
#endif
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

        private static Vector3 GetTangentVectorFromPoint(SplinePoint point)
        {
            //vector = tangent 2 - tangent 1 
            return point.tangent2 - point.tangent;
        }
        
    }
}
