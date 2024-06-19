using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Dreamteck.Splines;
using JBooth.VertexPainterPro;
using MyBox;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

                if (firstChunk == null || firstChunk.ChunkMeshData == null || firstChunk.ChunkMeshData.Mesh == null)
                {
                    Debug.LogError($"There's an issue with the chunk '{firstChunk.gameObject.name}'. Does the terrain need to be rebaked?");
                    return;
                }
                
                if (lastChunk == null || lastChunk.ChunkMeshData == null || lastChunk.ChunkMeshData.Mesh == null)
                {
                    Debug.LogError($"There's an issue with the chunk '{lastChunk.gameObject.name}'. Does the terrain need to be rebaked?");
                    return;
                }
                
                //create unique mesh data as we are editing it
                blendedFirstChunkMeshData = firstChunk.ChunkMeshData.Clone();
                blendedLastChunkMeshData = lastChunk.ChunkMeshData.Clone();
                
                firstChunkID = firstChunk.UniqueID;
                lastChunkID = lastChunk.UniqueID;

                Stopwatch stopwatch = Stopwatch.StartNew();
                //align
                // - we move the chunk with more end vertices to the chunk with less end vertices
                // - then, we do the opposite, in case there's any vertices in the other chunk that didn't have a connection
                ChunkMeshData chunkWithMoreEndVertices = blendedFirstChunkMeshData.LastEndVertices.Count > blendedLastChunkMeshData.FirstEndVertices.Count ? blendedFirstChunkMeshData : blendedLastChunkMeshData;
                ChunkMeshData chunkWithLessEndVertices = chunkWithMoreEndVertices == blendedFirstChunkMeshData ? blendedLastChunkMeshData : blendedFirstChunkMeshData;
                AlignEdges(chunkWithMoreEndVertices, chunkWithLessEndVertices);
                AlignEdges(chunkWithLessEndVertices, chunkWithMoreEndVertices);
                GlobalLoggers.LoadingLogger.Log($"AlignEdges = {stopwatch.ElapsedMilliseconds}ms");
                
                //blend
                stopwatch.Restart();
                BlendWithEdges(firstChunk);
                BlendWithEdges(lastChunk);
                GlobalLoggers.LoadingLogger.Log($"BlendWithEdges = {stopwatch.ElapsedMilliseconds}ms");

                stopwatch.Restart();
                FixOverlappingVertices(firstChunk);
                FixOverlappingVertices(lastChunk);
                GlobalLoggers.LoadingLogger.Log($"FixOverlappingVertices = {stopwatch.ElapsedMilliseconds}ms");

                stopwatch.Restart();
                blendedFirstChunkMeshData.ApplyChanges();
                blendedLastChunkMeshData.ApplyChanges();
                GlobalLoggers.LoadingLogger.Log($"ApplyChanges = {stopwatch.ElapsedMilliseconds}ms");

                //calculate normals
                stopwatch.Restart();
                CalculateNormals();
                GlobalLoggers.LoadingLogger.Log($"CalculateNormals = {stopwatch.ElapsedMilliseconds}ms");

                //need to update colours once all the vertices and normals have been set
                stopwatch.Restart();
                Color[] firstChunkColors = blendedFirstChunkMeshData.CalculateVertexColors();
                Color[] lastChunkColors = blendedLastChunkMeshData.CalculateVertexColors();
                blendedFirstChunkMeshData.UpdateVertexColors(firstChunkColors.ToSerializableColors());
                blendedLastChunkMeshData.UpdateVertexColors(lastChunkColors.ToSerializableColors());
                GlobalLoggers.LoadingLogger.Log($"UpdateVertexColors = {stopwatch.ElapsedMilliseconds}ms");
                
                stopwatch.Restart();
                BlendPaintedVertexColours(firstChunk);
                BlendPaintedVertexColours(lastChunk);
                GlobalLoggers.LoadingLogger.Log($"BlendPaintedVertexColours = {stopwatch.ElapsedMilliseconds}ms");
                
            } catch (Exception e)
            {
                Debug.LogError($"Error creating blend data between {firstChunk.gameObject.name} and {lastChunk.gameObject.name}\n{e.Message}\n{e.StackTrace}");
            }
        }
        
        private void CalculateNormals()
        {
            //recalculate and apply the normals to the mesh
            blendedFirstChunkMeshData.Mesh.RecalculateNormals();
            blendedLastChunkMeshData.Mesh.RecalculateNormals();
            blendedFirstChunkMeshData.SetNormals(blendedFirstChunkMeshData.Mesh.normals);
            blendedLastChunkMeshData.SetNormals(blendedLastChunkMeshData.Mesh.normals);
            
            foreach (VertexConnection connection in connections)
            {
                var (closestEndVertexIndex2, distanceToClosestEndVertexSqr1) = GetClosestVertexIndex(connection.vertexIndexChunk1.GetCurrentVertexWorldPosition(connection.vertexIndex1), connection.vertexIndexChunk2.VerticesExcludingEnds, connection.vertexIndexChunk2);
                Vector3 normal = connection.vertexIndexChunk2.Normals[closestEndVertexIndex2];
            
                var (closestEndVertexIndex1, distanceToClosestEndVertexSqr2) = GetClosestVertexIndex(connection.vertexIndexChunk2.GetCurrentVertexWorldPosition(connection.vertexIndex2), connection.vertexIndexChunk1.VerticesExcludingEnds, connection.vertexIndexChunk1);
                Vector3 otherNormal = connection.vertexIndexChunk1.Normals[closestEndVertexIndex1];
                
                Vector3 average = (normal + otherNormal) / 2f;
                
                connection.vertexIndexChunk1.ModifyNormal(connection.vertexIndex1, average);
                connection.vertexIndexChunk2.ModifyNormal(connection.vertexIndex2, average);
            
                Debug.DrawLine(connection.vertexIndexChunk1.GetCurrentVertexWorldPosition(closestEndVertexIndex1), connection.vertexIndexChunk2.GetCurrentVertexWorldPosition(closestEndVertexIndex2), Color.green, 60);
            }
            
            //refresh the mesh
            blendedFirstChunkMeshData.UpdateNormals();
            blendedLastChunkMeshData.UpdateNormals();
            blendedFirstChunkMeshData.Mesh.RecalculateTangents();
            blendedLastChunkMeshData.Mesh.RecalculateTangents();
        }
        
        private void BlendPaintedVertexColours(Chunk chunk)
        {
            //loop all the end vertices
            // check if it has a painted vertex on the other ends vertices within 0.1 radius 
            // apply the same colours of hte painted vertex to this vertex
            
            bool isFirstChunk = chunk == firstChunk;
            ChunkMeshData meshData = isFirstChunk ? blendedFirstChunkMeshData : blendedLastChunkMeshData;
            Chunk otherChunk = isFirstChunk ? lastChunk : firstChunk;
            ChunkMeshData otherMeshData = isFirstChunk ? blendedLastChunkMeshData : blendedFirstChunkMeshData;

            VertexInstanceStream vertexInstanceStream = otherChunk.TerrainHighLOD.GetComponent<VertexInstanceStream>();
            if (vertexInstanceStream == null)
                return; //no painted vertices to blend

            var endVertices = isFirstChunk ? blendedFirstChunkMeshData.LastEndVertices : blendedLastChunkMeshData.FirstEndVertices;
            var otherEndVertices = isFirstChunk ? blendedLastChunkMeshData.FirstEndVertices : blendedFirstChunkMeshData.LastEndVertices;

            Color[] colors = meshData.VertexColors.ToColors();
            foreach (int endVertexIndex in endVertices)
            {
                var (closestEndVertexIndex, distanceToClosestEndVertexSqr) = GetClosestVertexIndex(meshData.GetCurrentVertexWorldPosition(endVertexIndex), otherEndVertices, otherMeshData);

                bool isPaintedVertex = vertexInstanceStream.paintedVertices.ContainsKey(closestEndVertexIndex);
                if (!isPaintedVertex)
                    continue;

                List<VertexInstanceStream.PaintData> closestPaintedVertex = vertexInstanceStream.paintedVertices[closestEndVertexIndex];
                
                foreach (VertexInstanceStream.PaintData data in closestPaintedVertex)
                {
                    colors[endVertexIndex] = Color.Lerp(colors[endVertexIndex], data.color, data.strength);
                    meshData.TrackPaintData(endVertexIndex, data);
                }
            }

            meshData.UpdateVertexColors(colors.ToSerializableColors());
        }

        private void AlignEdges(ChunkMeshData chunkToMove, ChunkMeshData chunkToMatch)
        {
            GlobalLoggers.ChunkLogger.Log($"Moving {chunkToMove.Chunk.gameObject.name} vertices to {chunkToMatch.Chunk.gameObject.name}");
            var chunkToMoveEndVertices = chunkToMove == blendedFirstChunkMeshData ? chunkToMove.LastEndVertices : chunkToMove.FirstEndVertices;
            foreach (int chunkToMoveVertexIndex in chunkToMoveEndVertices)
            {
                ReadOnlyCollection<int> chunkToMatchEndVertices = chunkToMatch == blendedFirstChunkMeshData ? chunkToMatch.LastEndVertices : chunkToMatch.FirstEndVertices;
                var (chunkToMatchVertexIndex, distanceBetweenVerticesSqr) = GetClosestVertexIndex(chunkToMove.GetCurrentVertexWorldPosition(chunkToMoveVertexIndex), chunkToMatchEndVertices, chunkToMatch);
                MoveVertexToMatchAnother(chunkToMove, chunkToMoveVertexIndex, chunkToMatch, chunkToMatchVertexIndex);
            }
        }

        private void MoveVertexToMatchAnother(ChunkMeshData meshDataMovingFrom, int chunkMeshVertexIndexToMove, ChunkMeshData meshDataMovingTo, int chunkMeshVertexIndexToMatch)
        {
            //set the same world Y position - the average of both
            var fromWorldPosition = meshDataMovingFrom.GetCurrentVertexWorldPosition(chunkMeshVertexIndexToMove);
            var toWorldPosition = meshDataMovingTo.GetCurrentVertexWorldPosition(chunkMeshVertexIndexToMatch);
            
            float desiredY = (fromWorldPosition.y + toWorldPosition.y) / 2;
            Vector3 desiredPosition = toWorldPosition.SetY(desiredY);

            //if the opposite vertex already has a connection (previous vertex), set this desired position to the same position.
            if (VertexHasConnection(chunkMeshVertexIndexToMatch, meshDataMovingTo))
            {
                desiredPosition = toWorldPosition;
            }
            
            connections.Add(new VertexConnection(chunkMeshVertexIndexToMove, meshDataMovingFrom, chunkMeshVertexIndexToMatch, meshDataMovingTo));

#if UNITY_EDITOR
            if (meshDataMovingFrom.Chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                Debug.DrawLine(fromWorldPosition, desiredPosition, Color.white, 15);
#endif
            
            Debug.DrawLine(meshDataMovingFrom.GetCurrentVertexWorldPosition(chunkMeshVertexIndexToMove), desiredPosition, Color.cyan, 60);
            Debug.DrawLine(meshDataMovingTo.GetCurrentVertexWorldPosition(chunkMeshVertexIndexToMatch), desiredPosition, Color.magenta, 60);
            
            meshDataMovingFrom.SetVertexWorldPosition(chunkMeshVertexIndexToMove, desiredPosition);
            meshDataMovingTo.SetVertexWorldPosition(chunkMeshVertexIndexToMatch, desiredPosition);
        }

        private void BlendWithEdges(Chunk chunk)
        {
            float terrainBlendDistance = chunk.GetComponent<ChunkEditorTools>().TerrainData.ChunkBlendDistance;
            if (terrainBlendDistance.Approximately(0))
                return; //not blending

            bool isFirstChunk = chunk == firstChunk;
            ChunkMeshData meshData = isFirstChunk ? blendedFirstChunkMeshData : blendedLastChunkMeshData;
            ReadOnlyCollection<int> endVertices = isFirstChunk ? meshData.LastEndVertices : meshData.FirstEndVertices;

            float maxDistance = terrainBlendDistance + terrainBlendDistance;
            SplineSample endOfSplineSample = isFirstChunk ? chunk.LastSample : chunk.FirstSample;
            Debug.DrawLine(endOfSplineSample.position, endOfSplineSample.position + endOfSplineSample.right * maxDistance, Color.red, 60);

            Vector3 chunkForwardDirection = isFirstChunk ? endOfSplineSample.forward : -endOfSplineSample.forward;
            
            //check all the vertices if they're within distance to blend with the new middle heights
            foreach (int vertexIndex in meshData.VerticesExcludingEnds)
            {
                Vector3 vertexPositionWorld = meshData.GetCurrentVertexWorldPosition(vertexIndex);
                
                //get the distance to the tangent
                Vector2 intersectionPoint = VectorUtils.FindIntersection(vertexPositionWorld.FlattenAsVector2(), 
                    (vertexPositionWorld + chunkForwardDirection * maxDistance).FlattenAsVector2(), 
                    endOfSplineSample.position.FlattenAsVector2(), 
                    (endOfSplineSample.position + endOfSplineSample.right * maxDistance).FlattenAsVector2());
                if (intersectionPoint == Vector2.zero || float.IsNaN(intersectionPoint.x) || float.IsNaN(intersectionPoint.y))
                    continue; //is paralell, no intersection

                float distanceToTangentSqr = Vector2.SqrMagnitude(intersectionPoint - vertexPositionWorld.FlattenAsVector2());

                float terrainBlendDistanceSqr = terrainBlendDistance * terrainBlendDistance;
                if (distanceToTangentSqr > terrainBlendDistanceSqr)
                    continue;
                
                //don't blend if should be under the road
                if (IsPositionUnderRoad(vertexPositionWorld, chunk))
                    continue;

                float differencePercent = 1 - Mathf.Clamp01(distanceToTangentSqr / terrainBlendDistanceSqr);
                
                //get the closest end vertex
                var (closestVertexIndex, distanceToClosestVertexSqr) = GetClosestVertexIndex(vertexPositionWorld, endVertices, meshData);
                Vector3 closestVertexPositionWorld = meshData.GetCurrentVertexWorldPosition(closestVertexIndex);
                float closestVertexHeight = closestVertexPositionWorld.y;
                
                //get the closest vertex at the end of the blend
                Vector3 startPosition = vertexPositionWorld + chunkForwardDirection * Mathf.Sqrt(distanceToTangentSqr);
                Vector3 endOfBlendPositionWorld = startPosition - chunkForwardDirection * terrainBlendDistance;

                var (furthestVertexIndex, distanceToFurthestVertexSqr) = GetClosestVertexIndex(endOfBlendPositionWorld, meshData.VerticesExcludingEnds, meshData);

                Vector3 furthestVertexPositionWorld = meshData.GetCurrentVertexWorldPosition(furthestVertexIndex);
                Debug.DrawLine(vertexPositionWorld, furthestVertexPositionWorld, Color.magenta, 60);
                Debug.DrawLine(vertexPositionWorld, endOfBlendPositionWorld, Color.magenta, 60);

                float desiredHeight = furthestVertexPositionWorld.y;
                float heightDifference = closestVertexHeight - desiredHeight;
                float heightBlended = desiredHeight + (heightDifference * differencePercent);
                Vector3 newDesiredPosition = vertexPositionWorld.SetY(heightBlended);
                
                Debug.DrawLine(meshData.GetCurrentVertexWorldPosition(vertexIndex), newDesiredPosition, Color.blue, 60);
                
                meshData.SetVertexWorldPosition(vertexIndex, newDesiredPosition);
            }
        }
        
        private bool IsPositionUnderRoad(Vector3 position, Chunk chunk)
        {
            ChunkEditorTools chunkEditorTools = chunk.GetComponent<ChunkEditorTools>();
            var (closestSample, distanceToSplineSqr) = chunk.GetClosestSampleOnSpline(position);

            //check to flatten under road
            float roadFlattenDistanceSqr = chunkEditorTools.TerrainData.RoadFlattenDistance * chunkEditorTools.TerrainData.RoadFlattenDistance;
            bool isUnderRoad = distanceToSplineSqr < roadFlattenDistanceSqr;
            return isUnderRoad;
        }

        private (int, float) GetClosestVertexIndex(Vector3 worldPosition, ReadOnlyCollection<int> verticesToCheck, ChunkMeshData meshData)
        {
            Vector2 worldPositionFlattened = worldPosition.FlattenAsVector2();

            float minDistanceSquared = Mathf.Infinity;
            int closestChunkMeshVertex = default;
            foreach (int vertexIndex in verticesToCheck)
            {
                float distanceSquared = Vector2.SqrMagnitude(worldPositionFlattened - meshData.GetCurrentVertexWorldPosition(vertexIndex).FlattenAsVector2());
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestChunkMeshVertex = vertexIndex;
                }
            }

            return (closestChunkMeshVertex, minDistanceSquared);
        }

        private void FixOverlappingVertices(Chunk chunk)
        {
            bool isFirstChunk = chunk == firstChunk;
            Chunk otherChunk = isFirstChunk ? lastChunk : firstChunk;
            ChunkMeshData meshData = isFirstChunk ? blendedFirstChunkMeshData : blendedLastChunkMeshData;
            ChunkMeshData otherChunkMeshData = isFirstChunk ? blendedLastChunkMeshData : blendedFirstChunkMeshData;

            for (var vertexIndex = 0; vertexIndex < meshData.Mesh.vertices.Length; vertexIndex++)
            {
                if (VertexHasConnection(vertexIndex, meshData))
                    continue; //don't do for end vertices that have connected
                
                //get the end point tangent direction
                SplineSample endPoint = isFirstChunk ? chunk.LastSample : chunk.FirstSample;
                Vector3 tangentDirection = isFirstChunk ? -endPoint.forward : endPoint.forward;
                
#if UNITY_EDITOR
                if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                    Debug.DrawLine(endPoint.position, endPoint.position + tangentDirection * 100, isFirstChunk ? Color.red : Color.green, 15);
#endif
                
                //get the closest end vertex on the other chunk
                var (closestEndVertexIndex, closestEndVertexDistanceSqr) = GetClosestVertexIndex(meshData.GetCurrentVertexWorldPosition(vertexIndex), otherChunkMeshData.FirstEndVertices, otherChunkMeshData);
                
                //check which point is further in the tangent direction
                bool isOverlapping = IsPointFurtherInDirection(meshData.GetCurrentVertexWorldPosition(vertexIndex), otherChunkMeshData.GetCurrentVertexWorldPosition(closestEndVertexIndex), tangentDirection);
                if (isOverlapping)
                {
                    //move the vertex to the closest end vertex to stop overlapping
                    Vector3 closestEndVertexPositionWorld = otherChunkMeshData.GetCurrentVertexWorldPosition(closestEndVertexIndex);
                    
#if UNITY_EDITOR
                    if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                        Debug.DrawLine(closestEndVertexPositionWorld, meshData.GetCurrentVertexWorldPosition(vertexIndex), Color.black, 15);
#endif
                    
                    meshData.SetVertexWorldPosition(vertexIndex, closestEndVertexPositionWorld);
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
            internal int vertexIndex1;
            internal ChunkMeshData vertexIndexChunk1;

            internal int vertexIndex2;
            internal ChunkMeshData vertexIndexChunk2;

            internal VertexConnection(int vertexIndex1, ChunkMeshData vertexIndexChunk1, int vertexIndex2, ChunkMeshData vertexIndexChunk2)
            {
                this.vertexIndex1 = vertexIndex1;
                this.vertexIndexChunk1 = vertexIndexChunk1;
                
                this.vertexIndex2 = vertexIndex2;
                this.vertexIndexChunk2 = vertexIndexChunk2;
            }
        }

        private readonly List<VertexConnection> connections = new();
        
        private bool VertexHasConnection(int vertexIndex, ChunkMeshData chunkMeshData)
        {
            foreach (VertexConnection connection in connections)
            {
                if (vertexIndex == connection.vertexIndex1 && chunkMeshData.Equals(connection.vertexIndexChunk1))
                    return true;
                if (vertexIndex == connection.vertexIndex2 && chunkMeshData.Equals(connection.vertexIndexChunk2))
                    return true;
            }

            return false;
        }
        #endregion
#endif
    }
}
