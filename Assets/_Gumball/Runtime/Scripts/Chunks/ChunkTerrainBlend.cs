using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class ChunkTerrainBlend
    {

        private struct Vertex
        {
            public readonly int Index;
            public readonly Vector3 WorldPosition;

            public Vertex(int index, Vector3 worldPosition)
            {
                Index = index;
                WorldPosition = worldPosition;
            }
        }
        
        private class ChunkMeshData
        {
            private readonly Mesh mesh;
            
            public Vector3[] Vertices { get; }
            public MeshFilter MeshFilter { get; }

            public ChunkMeshData(Chunk chunk)
            {
                MeshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
                mesh = MeshFilter.sharedMesh;
                Vertices = mesh.vertices;
            }

            public void SetVertexWorldPosition(int index, Vector3 worldPosition)
            {
                Vertices[index] = MeshFilter.transform.InverseTransformPoint(worldPosition);
            }

            public void ApplyChanges()
            {
                Mesh meshToUse = mesh;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    meshToUse = Object.Instantiate(mesh); //use a mesh copy so that the MeshFilter can be undone
#endif
                
                meshToUse.vertices = Vertices;
                meshToUse.RecalculateBounds();
                meshToUse.RecalculateNormals();
                
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    MeshFilter.sharedMesh = meshToUse; //set the mesh copy so that the MeshFilter can be undone       
#endif
            }
        }

        private readonly Chunk chunk1;
        private readonly Chunk chunk2;
        
        private readonly ChunkMeshData chunk1MeshData;
        private readonly ChunkMeshData chunk2MeshData;
        private readonly HashSet<Vertex> closestVerticesUsed = new();

        public ChunkTerrainBlend(Chunk chunk1, Chunk chunk2)
        {
            this.chunk1 = chunk1;
            this.chunk2 = chunk2;
            
            chunk1.UpdateSplineSampleData();
            chunk2.UpdateSplineSampleData();
            
            chunk1MeshData = new ChunkMeshData(chunk1);
            chunk2MeshData = new ChunkMeshData(chunk2);
        }
        
        /// <summary>
        /// Blends the end of chunk 1 with the start of chunk 2.
        /// </summary>
        public void TryBlendTerrains()
        {
            if (chunk1.CurrentTerrain == null || chunk2.CurrentTerrain == null)
                return; //cannot blend if terrain doesn't exist

            //get the vertices on each chunks tangent
            SplinePoint endPoint = chunk1.SplineComputer.GetPoint(chunk1.LastPointIndex);
            List<Vertex> chunk1EndVertices = GetVerticesOnTangent(chunk1, endPoint.position - chunk1.LastTangent * 1000, endPoint.position + chunk1.LastTangent * 1000);
            SplinePoint oppositeEndPoint = chunk2.SplineComputer.GetPoint(0);
            List<Vertex> chunk2EndVertices = GetVerticesOnTangent(chunk2, oppositeEndPoint.position - chunk2.FirstTangent * 1000, oppositeEndPoint.position + chunk2.FirstTangent * 1000);

            GlobalLoggers.TerrainLogger.Log($"Found {chunk1EndVertices.Count} vertices at the end of chunk1 ({chunk1.name}).");
            GlobalLoggers.TerrainLogger.Log($"Found {chunk2EndVertices.Count} vertices at the end of chunk2 ({chunk2.name}).");

            foreach (Vertex chunk1Vertex in chunk1EndVertices)
            {
                Vertex chunk2Vertex = GetClosestVertex(chunk1Vertex.WorldPosition, chunk2EndVertices);

                //whichever chunk has more vertices on the end (ie chunk 1 (20) vs chunk 2 (13)), move the vertices to the same position
                if (chunk1EndVertices.Count > chunk2EndVertices.Count)
                {
                    //move chunk1 vertices to chunk 2 vertices
                    MoveVertexToMatchAnother(chunk1Vertex, chunk2Vertex);
                }
                else
                {
                    //move chunk2 vertices to chunk 1 vertices
                    MoveVertexToMatchAnother(chunk2Vertex, chunk1Vertex);
                }
            }
            chunk1MeshData.ApplyChanges();
            chunk2MeshData.ApplyChanges();
        }

        private void MoveVertexToMatchAnother(Vertex vertexToMove, Vertex vertexToMatch)
        {
            //set the same world Y position - the average of both
            float desiredY = (vertexToMove.WorldPosition.y + vertexToMatch.WorldPosition.y) / 2;
            Vector3 desiredPosition = vertexToMatch.WorldPosition.SetY(desiredY);
                    
            //if a vertex is already matched with an opposite vertex, use the other vertices Y
            bool alreadyMatched = closestVerticesUsed.Contains(vertexToMatch);
            closestVerticesUsed.Add(vertexToMatch);
            if (alreadyMatched)
            {
                Vector3 previousPositionLocal = chunk2MeshData.Vertices[vertexToMatch.Index];
                Vector3 previousPositionWorld = chunk2MeshData.MeshFilter.transform.TransformPoint(previousPositionLocal);
                desiredPosition = previousPositionWorld;
            }

            chunk1MeshData.SetVertexWorldPosition(vertexToMove.Index, desiredPosition);
            chunk2MeshData.SetVertexWorldPosition(vertexToMatch.Index, desiredPosition);
        }

        private Vertex GetClosestVertex(Vector3 worldPosition, List<Vertex> verticesToCheck)
        {
            Vector2 worldPositionFlattened = worldPosition.FlattenAsVector2();

            float minDistanceSquared = Mathf.Infinity;
            Vertex closestVertex = default;
            foreach (Vertex vertex in verticesToCheck)
            {
                float distanceSquared = Vector2.SqrMagnitude(worldPositionFlattened - vertex.WorldPosition.FlattenAsVector2());
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestVertex = vertex;
                }
            }

            return closestVertex;
        }
        
        private List<Vertex> GetVerticesOnTangent(Chunk chunk, Vector3 tangentStart, Vector3 tangentEnd)
        {
            List<Vertex> verticesOnTangent = new();
            MeshFilter meshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;

            for (var vertexIndex = 0; vertexIndex < mesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = mesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = meshFilter.transform.TransformPoint(vertexPosition);

                if (IsPointOnTangent(vertexPositionWorld, tangentStart, tangentEnd))
                {
                    verticesOnTangent.Add(new Vertex(vertexIndex, vertexPositionWorld));
                    Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 10, Color.magenta, 15);
                }
            }

            return verticesOnTangent;
        }

        private bool IsPointOnTangent(Vector3 point, Vector3 tangentStart, Vector3 tangentEnd)
        {
            Debug.DrawLine(tangentStart, tangentEnd, Color.yellow, 15);

            const float tolerance = 0.1f;
            
            Vector2 tangentDirection = (tangentEnd.FlattenAsVector2() - tangentStart.FlattenAsVector2()).normalized;
            Vector2 perpendicularDirection = new Vector2(-tangentDirection.y, tangentDirection.x);
            Vector2 vectorToTarget = point.FlattenAsVector2() - tangentStart.FlattenAsVector2();

            float dotProduct = Vector2.Dot(vectorToTarget, perpendicularDirection);

            return Mathf.Abs(dotProduct) < tolerance;
        }
        
    }
}
