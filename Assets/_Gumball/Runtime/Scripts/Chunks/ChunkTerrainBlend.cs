using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gumball
{
    public class ChunkTerrainBlend
    {

        private struct Vertex
        {
            public readonly int Index;
            public readonly Vector3 WorldPosition;
            public readonly ChunkMeshData MeshBelongsTo;

            public Vertex(int index, Vector3 worldPosition, ChunkMeshData meshBelongsTo)
            {
                Index = index;
                WorldPosition = worldPosition;
                MeshBelongsTo = meshBelongsTo;
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
        
        private ChunkMeshData chunk1MeshData;
        private ChunkMeshData chunk2MeshData;
        private List<Vertex> chunk1EndVertices;
        private List<Vertex> chunk2EndVertices;
        
        private readonly HashSet<Vertex> closestVerticesUsed = new();

        public ChunkTerrainBlend(Chunk chunk1, Chunk chunk2)
        {
            this.chunk1 = chunk1;
            this.chunk2 = chunk2;
        }

        private void InitialiseBlendData()
        {
            chunk1.UpdateSplineSampleData();
            chunk2.UpdateSplineSampleData();
            
            chunk1MeshData = new ChunkMeshData(chunk1);
            chunk2MeshData = new ChunkMeshData(chunk2);

            FindVerticesOnTangents();
        }

        /// <summary>
        /// Blends the end of chunk 1 with the start of chunk 2.
        /// </summary>
        public void TryBlendTerrains()
        {
            if (chunk1.CurrentTerrain == null || chunk2.CurrentTerrain == null)
                return; //cannot blend if terrain doesn't exist

            InitialiseBlendData();
            AlignEdges();
            BlendWithEdges(true);
            BlendWithEdges(false);
            
            chunk1MeshData.ApplyChanges();
            chunk2MeshData.ApplyChanges();
        }

        private void AlignEdges()
        {
            foreach (Vertex chunk1Vertex in chunk1EndVertices)
            {
                var (chunk2Vertex, distanceBetweenVertices) = GetClosestVertex(chunk1Vertex.WorldPosition, chunk2EndVertices);

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
        }

        private void BlendWithEdges(bool useFirstChunk)
        {
            Chunk chunkToUse = useFirstChunk ? chunk1 : chunk2;
            ChunkMeshData meshData = useFirstChunk ? chunk1MeshData : chunk2MeshData;
            ChunkMeshData otherChunkMeshData = useFirstChunk ? chunk2MeshData : chunk1MeshData;
            List<Vertex> otherChunksEndVertices = useFirstChunk ? chunk2EndVertices : chunk1EndVertices;
            
            if (chunkToUse.TerrainBlendDistance.Approximately(0))
                return; //not blending
            
            //check all the vertices if they're within distance to blend with the new middle heights
            for (int vertexIndex = 0; vertexIndex < meshData.Vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = meshData.Vertices[vertexIndex];
                Vector3 vertexPositionWorld = meshData.MeshFilter.transform.TransformPoint(vertexPosition);
                
                var (closestVertex, distanceToClosestVertex) = GetClosestVertex(vertexPositionWorld, otherChunksEndVertices);

                if (distanceToClosestVertex <= chunkToUse.TerrainBlendDistance)
                {
                    float differencePercent = 1 - Mathf.Clamp01(distanceToClosestVertex / chunkToUse.TerrainBlendDistance);

                    float currentHeight = vertexPositionWorld.y;

                    Vector3 closestVertexPosition = otherChunkMeshData.Vertices[closestVertex.Index];
                    Vector3 closestVertexPositionLocal = otherChunkMeshData.MeshFilter.transform.TransformPoint(closestVertexPosition);
                    float closestVertexHeight = closestVertexPositionLocal.y;

                    float heightDifference = closestVertexHeight - currentHeight;
                    float desiredHeight = currentHeight + (heightDifference * differencePercent);

                    Vector3 desiredPosition = vertexPositionWorld.SetY(desiredHeight);
                    meshData.SetVertexWorldPosition(vertexIndex, desiredPosition);
                }
            }
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
                Vector3 previousPositionLocal = vertexToMatch.MeshBelongsTo.Vertices[vertexToMatch.Index];
                Vector3 previousPositionWorld = vertexToMatch.MeshBelongsTo.MeshFilter.transform.TransformPoint(previousPositionLocal);
                desiredPosition = previousPositionWorld;
            }
            
            vertexToMove.MeshBelongsTo.SetVertexWorldPosition(vertexToMove.Index, desiredPosition);
            vertexToMatch.MeshBelongsTo.SetVertexWorldPosition(vertexToMatch.Index, desiredPosition);
        }

        private (Vertex, float) GetClosestVertex(Vector3 worldPosition, List<Vertex> verticesToCheck)
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

            return (closestVertex, Mathf.Sqrt(minDistanceSquared));
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
                    verticesOnTangent.Add(new Vertex(vertexIndex, vertexPositionWorld, chunk == chunk1 ? chunk1MeshData : chunk2MeshData));
                    Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 10, chunk == chunk1 ? Color.magenta : Color.yellow, 15);
                }
            }

            return verticesOnTangent;
        }

        private bool IsPointOnTangent(Vector3 point, Vector3 tangentStart, Vector3 tangentEnd)
        {
            const float tolerance = 0.1f;
            
            Vector2 tangentDirection = (tangentEnd.FlattenAsVector2() - tangentStart.FlattenAsVector2()).normalized;
            Vector2 perpendicularDirection = new Vector2(-tangentDirection.y, tangentDirection.x);
            Vector2 vectorToTarget = point.FlattenAsVector2() - tangentStart.FlattenAsVector2();

            float dotProduct = Vector2.Dot(vectorToTarget, perpendicularDirection);

            return Mathf.Abs(dotProduct) < tolerance;
        }
        
        private void FindVerticesOnTangents()
        {
            Quaternion previousRotation = chunk2.transform.rotation;
            chunk2.transform.rotation = Quaternion.Euler(new Vector3(0, chunk2.transform.rotation.eulerAngles.y, 0));
            
            //get the vertices on each chunks tangent
            Vector3 endPoint = chunk1.LastSample.position;
            chunk1EndVertices = GetVerticesOnTangent(chunk1, endPoint - chunk1.LastTangent, endPoint + chunk1.LastTangent);
            Vector3 oppositeEndPoint = chunk2.FirstSample.position;
            chunk2EndVertices = GetVerticesOnTangent(chunk2, oppositeEndPoint - chunk2.FirstTangent, oppositeEndPoint + chunk2.FirstTangent);

            chunk2.transform.rotation = previousRotation;

            //draw the tangents
            Debug.DrawLine(endPoint - chunk1.LastTangent * 250, endPoint + chunk1.LastTangent * 250, Color.magenta, 15);
            Debug.DrawLine(oppositeEndPoint - chunk2.FirstTangent * 200, oppositeEndPoint + chunk2.FirstTangent * 200, Color.yellow, 15);

            GlobalLoggers.TerrainLogger.Log($"Found {chunk1EndVertices.Count} vertices at the end of chunk1 ({chunk1.name}) - position = {endPoint}.");
            GlobalLoggers.TerrainLogger.Log($"Found {chunk2EndVertices.Count} vertices at the end of chunk2 ({chunk2.name}) - position = {oppositeEndPoint}.");
        }
        
    }
}
