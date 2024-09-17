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
        
        public ChunkGrid(Chunk chunk, int resolution, float widthAroundRoad)
        {
            this.chunk = chunk;
            this.resolution = resolution;
            this.widthAroundRoad = widthAroundRoad;

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
            chunk.CalculateSplineLength();
            GridCenter = chunk.GetCenterOfSpline();
            GridLength = chunk.SplineComputer.CalculateLength() + (widthAroundRoad);
            distanceBetweenVertices = GridLength / resolution;
            startVertexPosition = GridCenter.Flatten() - new Vector3(GridLength/2f, 0, GridLength/2f);
        }

        private void CreateGrid()
        {
            chunk.UpdateSplineSampleData();
            
#if UNITY_EDITOR
            if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
            {
                Debug.DrawLine(chunk.FirstSample.position.Flatten(), chunk.FirstSample.position.Flatten() + chunk.FirstTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(chunk.FirstSample.position.Flatten(), chunk.FirstSample.position.Flatten() - chunk.FirstTangent * GridLength, Color.magenta, debugLineDuration);
                
                Debug.DrawLine(chunk.LastSample.position.Flatten(), chunk.LastSample.position.Flatten() + chunk.LastTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(chunk.LastSample.position.Flatten(), chunk.LastSample.position.Flatten() - chunk.LastTangent * GridLength, Color.magenta, debugLineDuration);
            }
#endif
            
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
#if UNITY_EDITOR
                        if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.red, debugLineDuration);
#endif
                        continue;
                    }

                    if (IsAboveTangent(chunk.LastSample.position.Flatten() - chunk.LastTangent, chunk.LastSample.position.Flatten() + chunk.LastTangent, vertexPosition))
                    {
#if UNITY_EDITOR
                        if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.red, debugLineDuration);
#endif
                        continue;
                    }

                    float widthAroundRoadSqr = widthAroundRoad * widthAroundRoad;
                    var (closestSample, distanceToSplineSqr) = chunk.GetClosestSampleOnSpline(vertexPosition);
                    if (distanceToSplineSqr > widthAroundRoadSqr)
                    {
#if UNITY_EDITOR
                        if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.cyan, debugLineDuration);
#endif
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
            bool isVertical = (tangentAngle >= 45 && tangentAngle <= 135) || (tangentAngle <= -45 && tangentAngle >= -135);
            bool isHorizontal = (tangentAngle > -45 && tangentAngle < 45) || tangentAngle > 135 || tangentAngle < -135;
            
#if UNITY_EDITOR
            if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
            {
                Debug.DrawLine(sample.position.Flatten(), sample.position.Flatten() + tangent * GridLength, Color.yellow, debugLineDuration);
                Debug.DrawLine(sample.position.Flatten(), sample.position.Flatten() + Vector3.right * GridLength, Color.yellow, debugLineDuration);
                GlobalLoggers.ChunkLogger.Log($"Is first tangent? {useFirstTangent} - Angle is {tangentAngle}" +
                                              $"\nisVertical = {isVertical}" +
                                              $"\nisHorizontal = {isHorizontal}");
            }
#endif
            
            if (isVertical)
            {
                //using rows
                for (int row = 0; row < GetNumberOfRows(); row++)
                {
                    var (firstValidVertexInRow, lastValidVertexInRow) = GetValidVertexBoundsInRow(row);
                    
                    //if using last tangent, if facing right (last sample is more in the Vector3.right direction than the first sample), use the last valid vertex, else the first
                    //if using first tangent, if facing right (first sample is more in the Vector3.right direction than the last sample), use the last valid vertex, else the first
                    bool isTangentToTheRightOfOther = useFirstTangent ? chunk.FirstSample.position.x > chunk.LastSample.position.x : chunk.LastSample.position.x > chunk.FirstSample.position.x;

                    VertexWithColumnAndRow? vertexToUse = isTangentToTheRightOfOther ? lastValidVertexInRow : firstValidVertexInRow;
                    if (vertexToUse != null)
                    {
                        float z = startVertexPosition.z + row * distanceBetweenVertices;
                        Vector3 pointOnTangent = GetPointOnTangentUsingZ(z, tangentStart, tangentEnd);
                        
                        //TODO: use squared magnitude
                        float distance = Vector2.Distance(pointOnTangent.FlattenAsVector2(), vertices[vertexToUse.Value.VertexIndex].FlattenAsVector2());
                        if (distance < distanceBetweenVertices)
                        {
                            //GOAL: add all the second to last vertices in the rows to the VerticesAlongXTangent and move the vertices to the tangent
                            //here: get the vertex in the previous column
                            //can be either LEFT or RIGHT - try left first
                            int vertexInPreviousRow = GetVertexOnLeft(vertexToUse.Value.Column, row);
                            if (vertexInPreviousRow == -1)
                                vertexInPreviousRow = GetVertexOnRight(vertexToUse.Value.Column, row);
                            
                            if (vertexInPreviousRow != -1)
                                vertices[vertexInPreviousRow] = pointOnTangent;
                            
                            vertices[vertexToUse.Value.VertexIndex] = pointOnTangent;
                            
                            if (useFirstTangent)
                            {
                                VerticesAlongFirstTangent.Add(vertexToUse.Value.VertexIndex);
                                
                                if (vertexInPreviousRow != -1)
                                    VerticesAlongFirstTangent.Add(vertexInPreviousRow);
                            }
                            else
                            {
                                VerticesAlongLastTangent.Add(vertexToUse.Value.VertexIndex);
                                
                                if (vertexInPreviousRow != -1)
                                    VerticesAlongLastTangent.Add(vertexInPreviousRow);
                            }
                        }
                    }
                }
            }
            
            if (isHorizontal)
            {
                //using columns
                for (int column = 0; column < GetNumberOfColumns(); column++)
                {
                    var (firstValidVertexInColumn, lastValidVertexInColumn) = GetValidVertexBoundsInColumn(column);
                    
                    int vertexToUse = useFirstTangent ? firstValidVertexInColumn : lastValidVertexInColumn;
                    if (vertexToUse != -1)
                    {
                        float x = startVertexPosition.x + column * distanceBetweenVertices;
                        Vector3 pointOnTangent = GetPointOnTangentUsingX(x, tangentStart, tangentEnd);
                        
                        //TODO: use squared magnitude
                        float distance = Vector2.Distance(pointOnTangent.FlattenAsVector2(), vertices[vertexToUse].FlattenAsVector2());
                        if (distance < distanceBetweenVertices)
                        {
                            vertices[vertexToUse] = pointOnTangent;
                            if (useFirstTangent)
                            {
                                VerticesAlongFirstTangent.Add(vertexToUse);
                                
                                //also add the one above
                                if (vertexToUse < vertices.Count - 1)
                                {
                                    vertices[vertexToUse + 1] = pointOnTangent;
                                    VerticesAlongFirstTangent.Add(vertexToUse + 1);
                                }
                            }
                            else
                            {
                                VerticesAlongLastTangent.Add(vertexToUse);

                                //also add the one below
                                if (vertexToUse > 0)
                                {
                                    vertices[vertexToUse - 1] = pointOnTangent;
                                    VerticesAlongFirstTangent.Add(vertexToUse - 1);
                                }
                            }
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

        private struct VertexWithColumnAndRow
        {
            public int VertexIndex;
            public int Column;
            public int Row;
        
            public VertexWithColumnAndRow(int vertexIndex, int column, int row)
            {
                VertexIndex = vertexIndex;
                Column = column;
                Row = row;
            }
        }
                
        /// <returns>
        /// Item 1: the index of the first valid vertex in the column<br></br>
        /// Item 2: the index of the last valid vertex in the column
        /// </returns>
        private (VertexWithColumnAndRow?, VertexWithColumnAndRow?) GetValidVertexBoundsInRow(int row)
        {
            VertexWithColumnAndRow? firstValidVertexInRow = null;
            VertexWithColumnAndRow? lastValidVertexInRow = null;

            for (int column = 0; column < GetNumberOfColumns(); column++)
            {
                int vertexIndex = GetVertexIndexAt(column, row);
                if (vertexIndex == -1)
                    continue;
                
                if (firstValidVertexInRow == null)
                    firstValidVertexInRow = new VertexWithColumnAndRow(vertexIndex, column, row);
                lastValidVertexInRow = new VertexWithColumnAndRow(vertexIndex, column, row);
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
            float distance = zPos - tangentStartPoint.z;
            float x = tangentStartPoint.x + distance * direction.x / direction.z;
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
#if UNITY_EDITOR
            if (!chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
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
#endif
        }
        
    }
}
#endif