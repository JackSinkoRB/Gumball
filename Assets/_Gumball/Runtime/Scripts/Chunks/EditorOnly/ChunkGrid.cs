#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dreamteck.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    [ExecuteAlways]
    public class ChunkGrid
    {
        
        private const float debugLineDuration = 6;

        private readonly Chunk chunk;
        private readonly int resolution;
        private readonly float widthAroundRoad;
        private readonly List<List<int>> verticesAsGrid = new();
        private readonly List<Vector3> vertices = new();
        private readonly float timeToStopShowingDebug;
        private readonly bool showDebugLines;
        
        private int vertexCount;
        private float distanceBetweenVertices;
        private Vector3 startVertexPosition;

        public ReadOnlyCollection<Vector3> Vertices => vertices.AsReadOnly();
        public Vector3 GridCenter { get; private set; }
        /// <summary>
        /// The width/height of the grid.
        /// </summary>
        public float GridLength { get; private set; }
        public readonly HashSet<int> VerticesAlongFirstTangent = new();
        public readonly HashSet<int> VerticesAlongLastTangent = new();
        
        public ChunkGrid(Chunk chunk, int resolution, float widthAroundRoad, bool showDebugLines = false)
        {
            this.chunk = chunk;
            this.resolution = resolution;
            this.widthAroundRoad = widthAroundRoad;
            this.showDebugLines = showDebugLines;

            timeToStopShowingDebug = Time.realtimeSinceStartup + debugLineDuration;

            UpdateGridData();
            CreateGrid();
        }
        
        public void OnDrawGizmos()
        {
            ShowDebugLabels();
        }

        public int GetVertexIndexAt(int column, int row)
        {
            return verticesAsGrid[column][row];
        }
        
        public int GetNumberOfColumns()
        {
            return resolution + 1;
        }
        
        public int GetNumberOfRows()
        {
            return resolution + 1;
        }
        
        public int GetVertexAbove(int column, int row)
        {
            if (row + 1 >= GetNumberOfRows())
                return -1;
            if (column < 0 || column >= GetNumberOfColumns())
                return -1;
            return verticesAsGrid[column][row + 1];
        }
        
        public int GetVertexBelow(int column, int row)
        {
            if (row <= 0)
                return -1;
            if (column < 0 || column >= GetNumberOfColumns())
                return -1;
            return verticesAsGrid[column][row - 1];
        }
                
        public int GetVertexOnRight(int column, int row)
        {
            if (column + 1 >= GetNumberOfColumns())
                return -1;
            if (row < 0 || row >= GetNumberOfRows())
                return -1;
            return verticesAsGrid[column + 1][row];
        }
        
        public int GetVertexOnLeft(int column, int row)
        {
            if (column <= 0)
                return -1;
            if (row < 0 || row >= GetNumberOfRows())
                return -1;
            return verticesAsGrid[column - 1][row];
        }

        private void UpdateGridData()
        {
            GridCenter = chunk.GetCenterOfSpline();
            GridLength = chunk.SplineComputer.CalculateLength() + (widthAroundRoad);
            distanceBetweenVertices = GridLength / resolution;
            startVertexPosition = GridCenter.Flatten() - new Vector3(GridLength/2f, 0, GridLength/2f);
        }

        private void CreateGrid()
        {
            chunk.UpdateSplineSampleData();
            
            if (showDebugLines)
            {
                Debug.DrawLine(chunk.FirstSample.position.Flatten(), chunk.FirstSample.position.Flatten() + chunk.FirstTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(chunk.FirstSample.position.Flatten(), chunk.FirstSample.position.Flatten() - chunk.FirstTangent * GridLength, Color.magenta, debugLineDuration);
                
                Debug.DrawLine(chunk.LastSample.position.Flatten(), chunk.LastSample.position.Flatten() + chunk.LastTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(chunk.LastSample.position.Flatten(), chunk.LastSample.position.Flatten() - chunk.LastTangent * GridLength, Color.magenta, debugLineDuration);
            }
            
            vertexCount = 0;
            vertices.Clear();
            verticesAsGrid.Clear();
            for (int column = 0; column <= resolution; column++)
            {
                verticesAsGrid.Add(new List<int>()); //add each column, even if no vertex is found

                for (int row = 0; row <= resolution; row++)
                {
                    verticesAsGrid[column].Add(-1); //add each row, even if no vertex is found
                    
                    //start at top left (center of spline - gridSize)
                    Vector3 vertexPosition = startVertexPosition + new Vector3(column * distanceBetweenVertices, 0, row * distanceBetweenVertices);

                    if (IsBelowTangent(chunk.FirstSample.position.Flatten() - chunk.FirstTangent, chunk.FirstSample.position.Flatten() + chunk.FirstTangent, vertexPosition))
                    {
                        if (showDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.red, debugLineDuration);
                        continue;
                    }

                    if (IsAboveTangent(chunk.LastSample.position.Flatten() - chunk.LastTangent, chunk.LastSample.position.Flatten() + chunk.LastTangent, vertexPosition))
                    {
                        if (showDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.red, debugLineDuration);
                        continue;
                    }

                    float widthAroundRoadSqr = widthAroundRoad * widthAroundRoad;
                    var (closestSample, distanceToSplineSqr) = chunk.GetClosestSampleOnSpline(vertexPosition);
                    if (distanceToSplineSqr > widthAroundRoadSqr)
                    {
                        if (showDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.cyan, debugLineDuration);
                        continue;
                    }

                    AddVertex(vertexPosition, column, row);
                }
            }

            //move the last vertices to the tangents to bring the terrain all the way to the spline edge
            AlignEdges();
        }
        
        
        private void AddVertex(Vector3 vertexPosition, int column, int row)
        {
            bool containsIndex = column < verticesAsGrid.Count;
            if (!containsIndex)
                verticesAsGrid[column] = new List<int>();
            
            vertices.Add(vertexPosition);

            if (row < verticesAsGrid[column].Count)
                //replace
                verticesAsGrid[column][row] = vertexCount;
            
            //add to [column] as [row]]
            else verticesAsGrid[column].Insert(row, vertexCount);

            vertexCount++;
        }

        private void AlignEdges()
        {
            AlignEdges(true); //align first
            AlignEdges(false); //align last
        }

        /// <summary>
        /// Moves the edges to the first or last tangent.
        /// </summary>
        private void AlignEdges(bool useFirstTangent)
        {
            SplineSample sample = useFirstTangent ? chunk.FirstSample : chunk.LastSample;
            Vector3 tangent = useFirstTangent ? chunk.FirstTangent : chunk.LastTangent;

            Vector3 tangentStart = sample.position.Flatten() - tangent;
            Vector3 tangentEnd = sample.position.Flatten() + tangent;

            //angle between -180 and 180
            float tangentAngle = Vector2.SignedAngle(tangent.FlattenAsVector2(), Vector2.right);

            //angles when using the last tangent - first tangent should be opposite
            bool movingRight = tangentAngle >= 45 && tangentAngle <= 135;
            bool movingLeft = tangentAngle <= -45 && tangentAngle >= -135;
            bool movingUp = tangentAngle > -45 && tangentAngle < 45;
            bool movingDown = tangentAngle > 135 || tangentAngle < -135;

            if (useFirstTangent)
            {
                //flip
                movingRight = !movingRight;
                movingLeft = !movingLeft;
                movingUp = !movingUp;
                movingDown = !movingDown;
            }
            
            if (showDebugLines)
            {
                Debug.DrawLine(sample.position.Flatten(), sample.position.Flatten() + tangent * GridLength, Color.yellow, debugLineDuration);
                Debug.DrawLine(sample.position.Flatten(), sample.position.Flatten() + Vector3.right * GridLength, Color.yellow, debugLineDuration);
                GlobalLoggers.ChunkLogger.Log(Vector2.SignedAngle(tangent.FlattenAsVector2(), Vector2.right) + " - " + movingRight + ", " + movingLeft + ", " + movingUp + ", " + movingDown);
            }
            
            if (movingRight || movingLeft)
            {
                //using rows
                for (int row = 0; row < GetNumberOfRows(); row++)
                {
                    var (firstValidVertexInRow, lastValidVertexInRow) = GetValidVertexBoundsInRow(row);
                 
                    //moving right? move the end rows to the tangent
                    //moving left? move the first rows to the tangent
                    int vertexToUse = movingLeft ? firstValidVertexInRow : lastValidVertexInRow;
                    if (vertexToUse != -1)
                    {
                        float z = startVertexPosition.z + row * distanceBetweenVertices;
                        Vector3 pointOnLastTangent = GetPointOnTangentUsingZ(z, tangentStart, tangentEnd);
                        float distance = Vector2.Distance(pointOnLastTangent.FlattenAsVector2(), vertices[vertexToUse].FlattenAsVector2());
                        if (distance < distanceBetweenVertices)
                        {
                            vertices[vertexToUse] = pointOnLastTangent;
                            if (useFirstTangent)
                                VerticesAlongFirstTangent.Add(vertexToUse);
                            else VerticesAlongLastTangent.Add(vertexToUse);
                        }
                    }
                }
            }
            
            if (movingUp || movingDown)
            {
                //using columns
                for (int column = 0; column < GetNumberOfColumns(); column++)
                {
                    var (firstValidVertexInColumn, lastValidVertexInColumn) = GetValidVertexBoundsInColumn(column);

                    //moving up? move the end columns to the tangent
                    //moving down? move the first columns to the tangent
                    int vertexToUse = movingUp ? lastValidVertexInColumn : firstValidVertexInColumn;
                    if (vertexToUse != -1)
                    {
                        float x = startVertexPosition.x + column * distanceBetweenVertices;
                        Vector3 pointOnLastTangent = GetPointOnTangentUsingX(x, tangentStart, tangentEnd);
                        float distance = Vector2.Distance(pointOnLastTangent.FlattenAsVector2(), vertices[vertexToUse].FlattenAsVector2());
                        if (distance < distanceBetweenVertices)
                        {
                            vertices[vertexToUse] = pointOnLastTangent;
                            if (useFirstTangent)
                                VerticesAlongFirstTangent.Add(vertexToUse);
                            else VerticesAlongLastTangent.Add(vertexToUse);
                        }
                    }
                }
            }
        }
        
        /// <returns>
        /// Item 1: the index of the first valid vertex in the column<br></br>
        /// Item 2: the index of the last valid vertex in the column
        /// </returns>
        private (int, int) GetValidVertexBoundsInColumn(int column)
        {
            int firstValidVertexInColumn = -1;
            int lastValidVertexInColumn = -1;

            for (int row = 0; row < GetNumberOfRows(); row++)
            {
                int vertexIndex = GetVertexIndexAt(column, row);
                if (vertexIndex == -1)
                    continue;
                
                if (firstValidVertexInColumn == -1)
                    firstValidVertexInColumn = vertexIndex;
                lastValidVertexInColumn = vertexIndex;
            }
                
            return (firstValidVertexInColumn, lastValidVertexInColumn);
        }
        
        /// <returns>
        /// Item 1: the index of the first valid vertex in the column<br></br>
        /// Item 2: the index of the last valid vertex in the column
        /// </returns>
        private (int, int) GetValidVertexBoundsInRow(int row)
        {
            int firstValidVertexInRow = -1;
            int lastValidVertexInRow = -1;

            for (int column = 0; column < GetNumberOfColumns(); column++)
            {
                int vertexIndex = GetVertexIndexAt(column, row);
                if (vertexIndex == -1)
                    continue;
                
                if (firstValidVertexInRow == -1)
                    firstValidVertexInRow = vertexIndex;
                lastValidVertexInRow = vertexIndex;
            }
                
            return (firstValidVertexInRow, lastValidVertexInRow);
        }

        private Vector3 GetPointOnTangentUsingX(float xPos, Vector3 tangentStartPoint, Vector3 tangentEndPoint)
        {
            Vector3 direction = (tangentEndPoint - tangentStartPoint).normalized;
            float distance = xPos - tangentStartPoint.x;
            float z = tangentStartPoint.z + distance * direction.z / direction.x;
            return new Vector3(xPos, 0, z);
        }
        
        private Vector3 GetPointOnTangentUsingZ(float zPos, Vector3 tangentStartPoint, Vector3 tangentEndPoint)
        {
            Vector3 direction = (tangentEndPoint - tangentStartPoint).normalized;
            float distance = (zPos - tangentStartPoint.z) * (direction.x / direction.z);
            float x = tangentStartPoint.x + distance;
            return new Vector3(x, 0, zPos);
        }

        private bool IsBelowTangent(Vector3 tangentStartPoint, Vector3 tangentEndPoint, Vector3 point)
        {
            float crossProduct = GetCrossProductOfTangentAndPoint(tangentStartPoint, tangentEndPoint, point);
            return crossProduct > 0;
        }

        private bool IsAboveTangent(Vector3 tangentStartPoint, Vector3 tangentEndPoint, Vector3 point)
        {
            float crossProduct = GetCrossProductOfTangentAndPoint(tangentStartPoint, tangentEndPoint, point);
            return crossProduct < 0;
        }

        private float GetCrossProductOfTangentAndPoint(Vector3 tangentStartPoint, Vector3 tangentEndPoint, Vector3 point)
        {
            //convert to vector2 since we are dealing with a grid
            Vector2 dirVector1 = point.FlattenAsVector2() - tangentStartPoint.FlattenAsVector2();
            Vector2 dirVector2 = tangentEndPoint.FlattenAsVector2() - tangentStartPoint.FlattenAsVector2();
            
            return Vector3.Cross(dirVector1, dirVector2).z;
        }

        private void ShowDebugLabels()
        {
            if (!showDebugLines)
                return;
            
            float timeLeft = timeToStopShowingDebug - Time.realtimeSinceStartup;
            if (timeLeft < 0)
                return;
            
            for (int vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++)
            {
                Vector3 vertexPosition = Vertices[vertexIndex];
                
                Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 20, Color.blue);
                Handles.Label(vertexPosition, vertexIndex.ToString());
            }
        }
        
    }
}
#endif