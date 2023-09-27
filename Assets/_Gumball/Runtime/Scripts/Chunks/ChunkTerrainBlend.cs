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

        #region Vertex
        private readonly struct Vertex : IEquatable<Vertex>
        {
            internal readonly int Index;
            internal readonly Vector3 LocalPosition;
            internal Vector3 WorldPosition => MeshBelongsTo.MeshFilter.transform.TransformPoint(LocalPosition);
            internal readonly ChunkMeshData MeshBelongsTo;

            internal Vertex(int index, Vector3 localPosition, ChunkMeshData meshBelongsTo)
            {
                Index = index;
                LocalPosition = localPosition;
                MeshBelongsTo = meshBelongsTo;
            }

            /// <summary>
            /// Because the mesh can be updated but not yet applied, use this to get the current (but not applied) value instead.
            /// </summary>
            public Vector3 GetCurrentWorldPosition()
            {
                Vector3 previousPositionLocal = MeshBelongsTo.Vertices[Index];
                Vector3 previousPositionWorld = MeshBelongsTo.MeshFilter.transform.TransformPoint(previousPositionLocal);
                return previousPositionWorld;
            }

            public bool Equals(Vertex other)
            {
                return Index == other.Index && Equals(MeshBelongsTo, other.MeshBelongsTo);
            }

            public override bool Equals(object obj)
            {
                return obj is Vertex other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Index, MeshBelongsTo);
            }
        }
        #endregion
        
        #region ChunkMeshData
        private class ChunkMeshData
        {
            private readonly bool isFirstChunk;

            internal readonly Chunk Chunk;
            internal readonly Vector3[] Vertices;
            internal readonly MeshFilter MeshFilter;
            internal readonly Mesh mesh;
            
            internal List<Vertex> EndVertices;

            internal ChunkMeshData(Chunk chunk, bool isFirstChunk)
            {
                this.Chunk = chunk;
                this.isFirstChunk = isFirstChunk;
                
                MeshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
                mesh = MeshFilter.sharedMesh;
                Vertices = mesh.vertices;

                FindVerticesOnTangent();
            }

            internal void SetVertexWorldPosition(int index, Vector3 worldPosition)
            {
                Vertices[index] = MeshFilter.transform.InverseTransformPoint(worldPosition);
            }
            
            /// <summary>
            /// Because the mesh can be updated but not yet applied, use this to get the current (but not applied) value instead.
            /// </summary>
            public Vector3 GetCurrentVertexWorldPosition(int index)
            {
                Vector3 previousPositionLocal = Vertices[index];
                Vector3 previousPositionWorld = MeshFilter.transform.TransformPoint(previousPositionLocal);
                return previousPositionWorld;
            }

            internal void ApplyChanges()
            {
                Mesh meshToUse = mesh;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    meshToUse = Object.Instantiate(mesh); //use a mesh copy so that the MeshFilter can be undone
#endif
                
                meshToUse.SetVertices(Vertices);
                
                //recalculate UVs
                meshToUse.SetUVs(0, ChunkUtils.GetTriplanarUVs(Vertices, MeshFilter.transform));
                
                meshToUse.RecalculateTangents();
                meshToUse.RecalculateNormals();
                //meshToUse.SetNormals(CalculateNormals());
                meshToUse.RecalculateBounds();

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    MeshFilter.sharedMesh = meshToUse; //set the mesh copy so that the MeshFilter can be undone       
#endif
            }

            private void FindVerticesOnTangent()
            {
                Quaternion previousRotation = Chunk.transform.rotation;
                Chunk.transform.rotation = Quaternion.Euler(new Vector3(0, Chunk.transform.rotation.eulerAngles.y, 0));
            
                //get the vertices on each chunks tangent
                Vector3 endPoint = isFirstChunk ? Chunk.LastSample.position : Chunk.FirstSample.position;
                Vector3 tangent = isFirstChunk ? Chunk.LastTangent : Chunk.FirstTangent;
                EndVertices = GetVerticesOnTangent(this, endPoint - tangent, endPoint + tangent);

                Chunk.transform.rotation = previousRotation;

                //draw the tangents
                Debug.DrawLine(endPoint - tangent * 200, endPoint + tangent * 200, Color.magenta, 15);

                GlobalLoggers.TerrainLogger.Log($"Found {EndVertices.Count} vertices at the end of ({Chunk.gameObject.name}) ({(isFirstChunk ? "is first chunk" : "is last chunk")}) - position = {endPoint}.");
            }
            
            private Vector3[] CalculateNormals()
            {
                //1. recalculate the normals like normal (up to 6:40), but if the vertex is an end vertex, get the opposite end vertex
                //2. could calculate normals based on the vertex height
                //3. allow for 1 extra row of vertices after the tangent, but don't let them be seen
                
                Vector3[] vertexNormals = new Vector3[Vertices.Length];
                return vertexNormals;
            }
        }
        #endregion

        #region Connections
        private struct Connection
        {
            internal Vertex vertex1;
            internal Vertex vertex2;

            internal Connection(Vertex vertex1, Vertex vertex2)
            {
                this.vertex1 = vertex1;
                this.vertex2 = vertex2;
            }
        }

        private List<Connection> connections = new();
        
        private bool VertexHasConnection(Vertex vertex)
        {
            foreach (Connection connection in connections)
            {
                if (connection.vertex1.Equals(vertex))
                    return true;
                if (connection.vertex2.Equals(vertex))
                    return true;
            }

            return false;
        }
        #endregion
        
        private readonly Chunk firstChunk;
        private readonly Chunk lastChunk;
        
        private ChunkMeshData firstChunkMeshData;
        private ChunkMeshData lastChunkMeshData;

        public ChunkTerrainBlend(Chunk firstChunk, Chunk lastChunk)
        {
            this.firstChunk = firstChunk;
            this.lastChunk = lastChunk;
        }

        private void InitialiseBlendData()
        {
            firstChunk.UpdateSplineSampleData();
            lastChunk.UpdateSplineSampleData();
            
            firstChunkMeshData = new ChunkMeshData(firstChunk, true);
            lastChunkMeshData = new ChunkMeshData(lastChunk, false);
        }

        /// <summary>
        /// Blends the end of chunk 1 with the start of chunk 2.
        /// </summary>
        public void TryBlendTerrains()
        {
            if (firstChunk.CurrentTerrain == null || lastChunk.CurrentTerrain == null)
                return; //cannot blend if terrain doesn't exist

            InitialiseBlendData();
            
            //align
            // - we move the chunk with more end vertices to the chunk with less end vertices
            // - then, we do the opposite, in case there's any vertices in the other chunk that didn't have a connection
            ChunkMeshData chunkWithMoreEndVertices = firstChunkMeshData.EndVertices.Count > lastChunkMeshData.EndVertices.Count ? firstChunkMeshData : lastChunkMeshData;
            ChunkMeshData chunkWithLessEndVertices = chunkWithMoreEndVertices == firstChunkMeshData ? lastChunkMeshData : firstChunkMeshData;
            AlignEdges(chunkWithMoreEndVertices, chunkWithLessEndVertices);
            AlignEdges(chunkWithLessEndVertices, chunkWithMoreEndVertices);

            //blend
            BlendWithEdges(true);
            BlendWithEdges(false);
            
            //TODO: only need to do this if chunk is rotated on X or Z
            FixOverlappingVertices(true);
            FixOverlappingVertices(false);
            
            firstChunkMeshData.ApplyChanges();
            lastChunkMeshData.ApplyChanges();
        }

        private void AlignEdges(ChunkMeshData chunkToMove, ChunkMeshData chunkToMatch)
        {
            GlobalLoggers.TerrainLogger.Log($"Moving {chunkToMove.Chunk.gameObject.name} vertices to {chunkToMatch.Chunk.gameObject.name}");
            foreach (Vertex chunkToMoveVertex in chunkToMove.EndVertices)
            {
                var (chunkToMatchVertex, distanceBetweenVertices) = GetClosestVertex(chunkToMoveVertex.WorldPosition, chunkToMatch.EndVertices);
                MoveVertexToMatchAnother(chunkToMoveVertex, chunkToMatchVertex);
            }
        }
        
        private void MoveVertexToMatchAnother(Vertex vertexToMove, Vertex vertexToMatch)
        {
            //set the same world Y position - the average of both
            float desiredY = (vertexToMove.WorldPosition.y + vertexToMatch.WorldPosition.y) / 2;
            Vector3 desiredPosition = vertexToMatch.WorldPosition.SetY(desiredY);

            //if a vertex is already matched with an opposite vertex, use the other vertices Y
            if (VertexHasConnection(vertexToMatch))
            {
                Vector3 previousPosition = vertexToMatch.GetCurrentWorldPosition();
                desiredPosition = previousPosition;
            }
            
            connections.Add(new Connection(vertexToMove, vertexToMatch));
            
            Debug.DrawLine(vertexToMove.WorldPosition, desiredPosition, Color.white, 15);
            vertexToMove.MeshBelongsTo.SetVertexWorldPosition(vertexToMove.Index, desiredPosition);
            vertexToMatch.MeshBelongsTo.SetVertexWorldPosition(vertexToMatch.Index, desiredPosition);
        }

        private void BlendWithEdges(bool useFirstChunk)
        {
            Chunk chunkToUse = useFirstChunk ? firstChunk : lastChunk;
            
            if (chunkToUse.TerrainBlendDistance.Approximately(0))
                return; //not blending
            
            ChunkMeshData meshData = useFirstChunk ? firstChunkMeshData : lastChunkMeshData;
            List<Vertex> otherChunksEndVertices = useFirstChunk ? lastChunkMeshData.EndVertices : firstChunkMeshData.EndVertices;

            //check all the vertices if they're within distance to blend with the new middle heights
            for (int vertexIndex = 0; vertexIndex < meshData.Vertices.Length; vertexIndex++)
            {
                Vector3 vertexPositionWorld = meshData.GetCurrentVertexWorldPosition(vertexIndex);

                var (closestVertex, distanceToClosestVertex) = GetClosestVertex(vertexPositionWorld, otherChunksEndVertices);

                if (distanceToClosestVertex <= chunkToUse.TerrainBlendDistance)
                {
                    float differencePercent = 1 - Mathf.Clamp01(distanceToClosestVertex / chunkToUse.TerrainBlendDistance);

                    float currentHeight = vertexPositionWorld.y;
                    
                    Vector3 closestVertexPositionWorld = closestVertex.GetCurrentWorldPosition();
                    float closestVertexHeight = closestVertexPositionWorld.y;

                    float heightDifference = closestVertexHeight - currentHeight;
                    float desiredHeight = currentHeight + (heightDifference * differencePercent);

                    Vector3 desiredPosition = vertexPositionWorld.SetY(desiredHeight);
                    meshData.SetVertexWorldPosition(vertexIndex, desiredPosition);
                }
            }
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

        private void FixOverlappingVertices(bool useFirstChunk)
        {
            ChunkMeshData chunkMeshData = useFirstChunk ? firstChunkMeshData : lastChunkMeshData;
            ChunkMeshData otherChunkMeshData = useFirstChunk ? lastChunkMeshData : firstChunkMeshData;

            for (var vertexIndex = 0; vertexIndex < chunkMeshData.mesh.vertices.Length; vertexIndex++)
            {
                Vertex vertex = new Vertex(vertexIndex, chunkMeshData.mesh.vertices[vertexIndex], chunkMeshData);
                
                if (VertexHasConnection(vertex))
                    continue; //don't do for end vertices
                
                //get the end point tangent direction
                SplineSample endPoint = useFirstChunk ? firstChunk.LastSample : lastChunk.FirstSample;
                Vector3 tangentDirection = useFirstChunk ? -endPoint.forward : endPoint.forward;
                Debug.DrawLine(endPoint.position, endPoint.position + tangentDirection * 100, useFirstChunk ? Color.red : Color.green, 15);
                
                //get the closest end vertex on the other chunk
                var (closestEndVertex, closestEndVertexDistance) = GetClosestVertex(vertex.WorldPosition, otherChunkMeshData.EndVertices);

                //check which point is further in the tangent direction
                bool isOverlapping = IsPointFurtherInDirection(vertex.WorldPosition, closestEndVertex.WorldPosition, tangentDirection);
                if (isOverlapping)
                {
                    //move the vertex to the closest end vertex to stop overlapping
                    Vector3 closestEndVertexPositionWorld = closestEndVertex.GetCurrentWorldPosition();
                    
                    Debug.DrawLine(closestEndVertexPositionWorld, vertex.WorldPosition, Color.black, 15);

                    chunkMeshData.SetVertexWorldPosition(vertexIndex, closestEndVertexPositionWorld);
                }
            }
        }

        private bool IsPointFurtherInDirection(Vector3 point1, Vector3 point2, Vector3 direction)
        {
            float distanceToA = Vector3.Dot(point1, direction.normalized);
            float distanceToB = Vector3.Dot(point2, direction.normalized);

            return distanceToA < distanceToB;
        }
        
        private static List<Vertex> GetVerticesOnTangent(ChunkMeshData chunkMeshData, Vector3 tangentStart, Vector3 tangentEnd)
        {
            List<Vertex> verticesOnTangent = new();

            for (var vertexIndex = 0; vertexIndex < chunkMeshData.mesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = chunkMeshData.mesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = chunkMeshData.MeshFilter.transform.TransformPoint(vertexPosition);

                Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 10, Color.yellow, 15);

                if (IsPointOnTangent(vertexPositionWorld, tangentStart, tangentEnd))
                {
                    verticesOnTangent.Add(new Vertex(vertexIndex, vertexPosition, chunkMeshData));
                    Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 20, Color.magenta, 15);
                }
            }

            return verticesOnTangent;
        }

        private static bool IsPointOnTangent(Vector3 point, Vector3 tangentStart, Vector3 tangentEnd)
        {
            const float tolerance = 2f;
            
            Vector2 tangentDirection = (tangentEnd.FlattenAsVector2() - tangentStart.FlattenAsVector2()).normalized;
            Vector2 perpendicularDirection = new Vector2(-tangentDirection.y, tangentDirection.x);
            Vector2 vectorToTarget = point.FlattenAsVector2() - tangentStart.FlattenAsVector2();

            float dotProduct = Vector2.Dot(vectorToTarget, perpendicularDirection);

            return Mathf.Abs(dotProduct) < tolerance;
        }

    }
}
