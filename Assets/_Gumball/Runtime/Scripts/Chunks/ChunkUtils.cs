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
            
            //get (or create) the connection
            chunk1.Connector.ClearConnections();
            //set the connection position to the end of chunk 1
            chunk1.Connector.transform.position = chunk1.SplineComputer.GetPoint(chunk1.LastPointIndex).position;

            //set the position of chunk 2 to the end of chunk 1
            chunk2.transform.position = chunk1.SplineComputer.GetPoint(chunk1.LastPointIndex).position;

            RotateChunkToAlign(chunk2, chunk1);

            //add the connections
            chunk1.Connector.AddConnection(chunk1.SplineComputer, chunk1.LastPointIndex);
            chunk1.Connector.AddConnection(chunk2.SplineComputer, 0);
            
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
            SplinePoint chunkToAlignWithPoint = chunkToAlignWith.SplineComputer.GetPoint(chunkToAlign.LastPointIndex);
            
            //get the tangent vector for each point
            Vector3 chunkToAlignTangentVector = GetTangentVectorFromPoint(chunkToAlignPoint);
            Vector3 chunkToAlignWithTangentVector = GetTangentVectorFromPoint(chunkToAlignWithPoint);

            //normalise the vectors
            Vector3 chunkToAlignTangentVectorNormalised = Vector3.Normalize(chunkToAlignTangentVector);
            Vector3 chunkToAlignWithTangentVectorNormalised = Vector3.Normalize(chunkToAlignWithTangentVector);
            
            //get the axis to rotate around (cross product)
            Vector3 axisToRotateAround = Vector3.Cross(chunkToAlignTangentVectorNormalised, chunkToAlignWithTangentVectorNormalised);

            //get the angle at which to rotate
            float dotProduct = Vector3.Dot(chunkToAlignTangentVectorNormalised, chunkToAlignWithTangentVectorNormalised);
            float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            Vector3 pointToRotateAround = chunkToAlignPoint.position;
            
            chunkToAlign.transform.RotateAround(pointToRotateAround, axisToRotateAround, angle);
        }

        private static Vector3 GetTangentVectorFromPoint(SplinePoint point)
        {
            //vector = tangent 2 - tangent 1 
            return point.tangent2 - point.tangent;
        }
        
    }
}
