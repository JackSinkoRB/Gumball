using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class ChunkBlendData
    {
        
        private readonly Chunk firstChunk;
        private readonly Chunk lastChunk;

        [SerializeField, ReadOnly] private string firstChunkID;
        [SerializeField, ReadOnly] private ChunkMeshData blendedFirstChunkMeshData;
        [SerializeField, ReadOnly] private string lastChunkID;
        [SerializeField, ReadOnly] private ChunkMeshData blendedLastChunkMeshData;

        public ChunkMeshData BlendedFirstChunkMeshData => blendedFirstChunkMeshData;
        public ChunkMeshData BlendedLastChunkMeshData => blendedLastChunkMeshData;

#if UNITY_EDITOR
        public ChunkBlendData(Chunk firstChunk, Chunk lastChunk)
        {
            try
            {
                this.firstChunk = firstChunk;
                this.lastChunk = lastChunk;

                firstChunkID = firstChunk.UniqueID;
                lastChunkID = lastChunk.UniqueID;

                //align
                // - we move the chunk with more end vertices to the chunk with less end vertices
                // - then, we do the opposite, in case there's any vertices in the other chunk that didn't have a connection
                ChunkMeshData chunkWithMoreEndVertices = firstChunk.ChunkMeshData.LastEndVertices.Count > lastChunk.ChunkMeshData.FirstEndVertices.Count ? firstChunk.ChunkMeshData : lastChunk.ChunkMeshData;
                ChunkMeshData chunkWithLessEndVertices = chunkWithMoreEndVertices == firstChunk.ChunkMeshData ? lastChunk.ChunkMeshData : firstChunk.ChunkMeshData;
                AlignEdges(chunkWithMoreEndVertices, chunkWithLessEndVertices);
                AlignEdges(chunkWithLessEndVertices, chunkWithMoreEndVertices);

                //blend
                BlendWithEdges(firstChunk);
                BlendWithEdges(lastChunk);

                FixOverlappingVertices(firstChunk);
                FixOverlappingVertices(lastChunk);

                firstChunk.ChunkMeshData.ApplyChanges();
                lastChunk.ChunkMeshData.ApplyChanges();

                //need to update colours once all the vertices and normals have been set
                Color[] firstChunkColors = firstChunk.ChunkMeshData.CalculateVertexColors();
                Color[] lastChunkColors = lastChunk.ChunkMeshData.CalculateVertexColors();
                firstChunk.ChunkMeshData.UpdateVertexColors(firstChunkColors.ToSerializableColors());
                lastChunk.ChunkMeshData.UpdateVertexColors(lastChunkColors.ToSerializableColors());
                
                //save the ChunkMeshData's, so they can be applied at a later time
                blendedFirstChunkMeshData = firstChunk.ChunkMeshData;
                blendedLastChunkMeshData = lastChunk.ChunkMeshData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating blend data between {firstChunk.gameObject.name} and {lastChunk.gameObject.name}\n{e.StackTrace}");
            }
        }

        private void AlignEdges(ChunkMeshData chunkToMove, ChunkMeshData chunkToMatch)
        {
            GlobalLoggers.ChunkLogger.Log($"Moving {chunkToMove.Chunk.gameObject.name} vertices to {chunkToMatch.Chunk.gameObject.name}");
            var chunkToMoveEndVertices = chunkToMove == firstChunk.ChunkMeshData ? chunkToMove.LastEndVertices : chunkToMove.FirstEndVertices;
            foreach (ChunkMeshData.Vertex chunkToMoveVertex in chunkToMoveEndVertices)
            {
                var chunkToMatchEndVertices = chunkToMatch == firstChunk.ChunkMeshData ? chunkToMatch.LastEndVertices : chunkToMatch.FirstEndVertices;
                var (chunkToMatchVertex, distanceBetweenVertices) = GetClosestVertex(chunkToMoveVertex.WorldPosition, chunkToMatchEndVertices);
                MoveVertexToMatchAnother(chunkToMoveVertex, chunkToMatchVertex);
            }
        }
        
        private void MoveVertexToMatchAnother(ChunkMeshData.Vertex chunkMeshVertexToMove, ChunkMeshData.Vertex chunkMeshVertexToMatch)
        {
            //set the same world Y position - the average of both
            float desiredY = (chunkMeshVertexToMove.WorldPosition.y + chunkMeshVertexToMatch.WorldPosition.y) / 2;
            Vector3 desiredPosition = chunkMeshVertexToMatch.WorldPosition.SetY(desiredY);

            //if a vertex is already matched with an opposite vertex, use the other vertices Y
            if (VertexHasConnection(chunkMeshVertexToMatch))
            {
                Vector3 previousPosition = chunkMeshVertexToMatch.GetCurrentWorldPosition();
                desiredPosition = previousPosition;
            }
            
            connections.Add(new VertexConnection(chunkMeshVertexToMove, chunkMeshVertexToMatch));
            
            Debug.DrawLine(chunkMeshVertexToMove.WorldPosition, desiredPosition, Color.white, 15);
            chunkMeshVertexToMove.MeshBelongsTo.SetVertexWorldPosition(chunkMeshVertexToMove.Index, desiredPosition);
            chunkMeshVertexToMatch.MeshBelongsTo.SetVertexWorldPosition(chunkMeshVertexToMatch.Index, desiredPosition);
        }

        private void BlendWithEdges(Chunk chunk)
        {
            float terrainBlendDistance = chunk.GetComponent<ChunkEditorTools>().TerrainData.ChunkBlendDistance;
            if (terrainBlendDistance.Approximately(0))
                return; //not blending

            bool isFirstChunk = chunk == firstChunk;
            ChunkMeshData meshData = chunk.ChunkMeshData;
            ReadOnlyCollection<ChunkMeshData.Vertex> otherChunksEndVertices = isFirstChunk ? lastChunk.ChunkMeshData.FirstEndVertices : firstChunk.ChunkMeshData.LastEndVertices;

            //check all the vertices if they're within distance to blend with the new middle heights
            for (int vertexIndex = 0; vertexIndex < meshData.Vertices.Length; vertexIndex++)
            {
                Vector3 vertexPositionWorld = meshData.GetCurrentVertexWorldPosition(vertexIndex);

                var (closestVertex, distanceToClosestVertex) = GetClosestVertex(vertexPositionWorld, otherChunksEndVertices);

                if (distanceToClosestVertex <= terrainBlendDistance)
                {
                    //don't blend if should be under the should
                    if (IsPositionUnderRoad(vertexPositionWorld, chunk))
                        continue;

                    float differencePercent = 1 - Mathf.Clamp01(distanceToClosestVertex / terrainBlendDistance);

                    float currentHeight = vertexPositionWorld.y;
                    
                    Vector3 closestVertexPositionWorld = closestVertex.GetCurrentWorldPosition();
                    float closestVertexHeight = closestVertexPositionWorld.y;

                    float heightDifference = closestVertexHeight - currentHeight;
                    float heightBlended = currentHeight + (heightDifference * differencePercent);
                    Vector3 newDesiredPosition = vertexPositionWorld.SetY(heightBlended);
                    
                    meshData.SetVertexWorldPosition(vertexIndex, newDesiredPosition);
                }
            }
        }
        
        private bool IsPositionUnderRoad(Vector3 position, Chunk chunk)
        {
            ChunkEditorTools chunkEditorTools = chunk.GetComponent<ChunkEditorTools>();
            var (closestSample, distanceToSplineSqr) = chunk.GetClosestSampleOnSpline(position, true);

            //check to flatten under road
            float roadFlattenDistanceSqr = chunkEditorTools.TerrainData.RoadFlattenDistance * chunkEditorTools.TerrainData.RoadFlattenDistance;
            bool isUnderRoad = distanceToSplineSqr < roadFlattenDistanceSqr;
            return isUnderRoad;
        }

        private (ChunkMeshData.Vertex, float) GetClosestVertex(Vector3 worldPosition, ReadOnlyCollection<ChunkMeshData.Vertex> verticesToCheck)
        {
            Vector2 worldPositionFlattened = worldPosition.FlattenAsVector2();

            float minDistanceSquared = Mathf.Infinity;
            ChunkMeshData.Vertex closestChunkMeshVertex = default;
            foreach (ChunkMeshData.Vertex vertex in verticesToCheck)
            {
                float distanceSquared = Vector2.SqrMagnitude(worldPositionFlattened - vertex.WorldPosition.FlattenAsVector2());
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestChunkMeshVertex = vertex;
                }
            }

            return (closestChunkMeshVertex, Mathf.Sqrt(minDistanceSquared));
        }

        private void FixOverlappingVertices(Chunk chunk)
        {
            bool isFirstChunk = chunk == firstChunk;
            Chunk otherChunk = isFirstChunk ? lastChunk : firstChunk;

            for (var vertexIndex = 0; vertexIndex < chunk.ChunkMeshData.Mesh.vertices.Length; vertexIndex++)
            {
                ChunkMeshData.Vertex chunkMeshVertex = new ChunkMeshData.Vertex(vertexIndex, chunk.ChunkMeshData.Mesh.vertices[vertexIndex], chunk);
                
                if (VertexHasConnection(chunkMeshVertex))
                    continue; //don't do for end vertices
                
                //get the end point tangent direction
                SplineSample endPoint = isFirstChunk ? chunk.LastSample : chunk.FirstSample;
                Vector3 tangentDirection = isFirstChunk ? -endPoint.forward : endPoint.forward;
                Debug.DrawLine(endPoint.position, endPoint.position + tangentDirection * 100, isFirstChunk ? Color.red : Color.green, 15);
                
                //get the closest end vertex on the other chunk
                var (closestEndVertex, closestEndVertexDistance) = GetClosestVertex(chunkMeshVertex.WorldPosition, otherChunk.ChunkMeshData.FirstEndVertices);

                //check which point is further in the tangent direction
                bool isOverlapping = IsPointFurtherInDirection(chunkMeshVertex.WorldPosition, closestEndVertex.WorldPosition, tangentDirection);
                if (isOverlapping)
                {
                    //move the vertex to the closest end vertex to stop overlapping
                    Vector3 closestEndVertexPositionWorld = closestEndVertex.GetCurrentWorldPosition();
                    
                    Debug.DrawLine(closestEndVertexPositionWorld, chunkMeshVertex.WorldPosition, Color.black, 15);

                    chunk.ChunkMeshData.SetVertexWorldPosition(vertexIndex, closestEndVertexPositionWorld);
                }
            }
        }

        private bool IsPointFurtherInDirection(Vector3 point1, Vector3 point2, Vector3 direction)
        {
            float distanceToA = Vector3.Dot(point1, direction.normalized);
            float distanceToB = Vector3.Dot(point2, direction.normalized);

            return distanceToA < distanceToB;
        }
        
        #region Vertex Connections
        private struct VertexConnection
        {
            internal ChunkMeshData.Vertex vertex1;
            internal ChunkMeshData.Vertex vertex2;

            internal VertexConnection(ChunkMeshData.Vertex vertex1, ChunkMeshData.Vertex vertex2)
            {
                this.vertex1 = vertex1;
                this.vertex2 = vertex2;
            }
        }

        private readonly List<VertexConnection> connections = new();
        
        private bool VertexHasConnection(ChunkMeshData.Vertex chunkMeshVertex)
        {
            foreach (VertexConnection connection in connections)
            {
                if (connection.vertex1.Equals(chunkMeshVertex))
                    return true;
                if (connection.vertex2.Equals(chunkMeshVertex))
                    return true;
            }

            return false;
        }
        #endregion
#endif
    }
}
