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
        
        public static GameObject GenerateTerrainMeshFromGrid(ChunkGrid grid)
        {
            GameObject terrain = new GameObject("Terrain");

            terrain.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

            mesh.SetVertices(grid.Vertices);
            mesh.SetTriangles(GetTrianglesForGrid(grid), 0);
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.sharedMesh = mesh;
            return terrain;
        }

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

        private static List<int> GetTrianglesForGrid(ChunkGrid grid)
        {
            List<int> triangleIndexes = new List<int>();
            
            //iterate over all the columns
            int vertexIndex = 0;
            for (int column = 0; column < grid.GetNumberOfColumns(); column++)
            {
                for (int row = 0; row < grid.GetNumberOfRowsInColumn(column); row++)
                {
                    if (grid.GetVertexIndexAt(column, row) == -1)
                        continue;
                    
                    int vertexIndexAbove = grid.GetVertexAbove(column, row);
                    int vertexIndexBelow = grid.GetVertexBelow(column, row);
                    int vertexIndexOnRight = grid.GetVertexOnRight(column, row);
                    int vertexIndexOnLeft = grid.GetVertexOnLeft(column, row);

                    bool hasVertexAbove = vertexIndexAbove != -1;
                    bool hasVertexBelow = vertexIndexBelow != -1;
                    bool hasVertexOnRight = vertexIndexOnRight != -1;
                    bool hasVertexOnLeft = vertexIndexOnLeft != -1;

                    if (hasVertexAbove && hasVertexOnRight)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexAbove);
                        triangleIndexes.Add(vertexIndexOnRight);
                    }
                    
                    if (hasVertexAbove && hasVertexOnLeft)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexOnLeft);
                        triangleIndexes.Add(vertexIndexAbove);
                    }
                    
                    if (hasVertexBelow && hasVertexOnRight)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexOnRight);
                        triangleIndexes.Add(vertexIndexBelow);
                    }
                    
                    if (hasVertexBelow && hasVertexOnLeft)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexBelow);
                        triangleIndexes.Add(vertexIndexOnLeft);
                    }

                    vertexIndex++;
                }
            }

            return triangleIndexes;
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
