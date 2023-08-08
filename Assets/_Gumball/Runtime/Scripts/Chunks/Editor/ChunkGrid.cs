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
        private SplineSample firstSample;
        private Vector3 firstTangent;
        private SplineSample lastSample;
        private Vector3 lastTangent;

        public ReadOnlyCollection<Vector3> Vertices => vertices.AsReadOnly();
        public Vector3 GridCenter { get; private set; }
        /// <summary>
        /// The width/height of the grid.
        /// </summary>
        public float GridLength { get; private set; }

        public ChunkGrid(Chunk chunk, int resolution, float widthAroundRoad, bool showDebugLines = false)
        {
            this.chunk = chunk;
            this.resolution = resolution;
            this.widthAroundRoad = widthAroundRoad;
            this.showDebugLines = showDebugLines;

            timeToStopShowingDebug = Time.realtimeSinceStartup + debugLineDuration;

            UpdateSplineSampleData();
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
            return verticesAsGrid.Count;
        }
        
        public int GetNumberOfRowsInColumn(int column)
        {
            return verticesAsGrid[column].Count;
        }
        
        public int GetVertexAbove(int column, int row)
        {
            if (row + 1 >= GetNumberOfRowsInColumn(column))
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
            if (row < 0 || row >= GetNumberOfRowsInColumn(column+1))
                return -1;
            return verticesAsGrid[column + 1][row];
        }
        
        public int GetVertexOnLeft(int column, int row)
        {
            if (column <= 0)
                return -1;
            if (row < 0 || row >= GetNumberOfRowsInColumn(column-1))
                return -1;
            return verticesAsGrid[column - 1][row];
        }

        private void UpdateSplineSampleData()
        {
            SampleCollection sampleCollection = new SampleCollection();
            chunk.SplineComputer.GetSamples(sampleCollection);
            
            firstSample = sampleCollection.samples[0];
            firstTangent = firstSample.right.Flatten();
            
            lastSample = sampleCollection.samples[sampleCollection.length-1];
            lastTangent = lastSample.right.Flatten();
        }

        private void CreateGrid()
        {
            GridCenter = chunk.GetCenterOfSpline();
            GridLength = chunk.SplineComputer.CalculateLength() + (widthAroundRoad);
            float distanceBetweenVertices = GridLength / resolution;
            Vector3 startVertexPosition = GridCenter.Flatten() - new Vector3(GridLength/2f, 0, GridLength/2f);
            
            Vector3 firstTangentStart = firstSample.position.Flatten() - firstTangent;
            Vector3 firstTangentEnd = firstSample.position.Flatten() + firstTangent;
            Vector3 lastTangentStart = lastSample.position.Flatten() - lastTangent;
            Vector3 lastTangentEnd = lastSample.position.Flatten() + lastTangent;
            
            if (showDebugLines)
            {
                Debug.DrawLine(firstSample.position.Flatten(), firstSample.position.Flatten() + firstTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(firstSample.position.Flatten(), firstSample.position.Flatten() + -firstTangent * GridLength, Color.magenta, debugLineDuration);
            }
            
            if (showDebugLines)
            {
                Debug.DrawLine(lastSample.position.Flatten(), lastSample.position.Flatten() + lastTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(lastSample.position.Flatten(), lastSample.position.Flatten() + -lastTangent * GridLength, Color.magenta, debugLineDuration);
            }

            vertexCount = 0;
            vertices.Clear();
            verticesAsGrid.Clear();
            for (int column = 0; column <= resolution; column++)
            {
                verticesAsGrid.Add(new List<int>()); //add each column, even if no vertex is found

                int firstValidVertexColumn = -1;
                int lastValidVertexInColumn = -1;

                for (int row = 0; row <= resolution; row++)
                {
                    verticesAsGrid[column].Add(-1); //add each row, even if no vertex is found
                    
                    //start at top left (center of spline - gridSize)
                    Vector3 vertexPosition = startVertexPosition + new Vector3(column * distanceBetweenVertices, 0, row * distanceBetweenVertices);

                    if (IsBelowTangent(firstSample.position.Flatten() - firstTangent, firstSample.position.Flatten() + firstTangent, vertexPosition))
                    {
                        if (showDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.red, debugLineDuration);
                        continue;
                    }

                    if (IsAboveTangent(lastSample.position.Flatten() - lastTangent, lastSample.position.Flatten() + lastTangent, vertexPosition))
                    {
                        if (showDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.red, debugLineDuration);
                        continue;
                    }
                    
                    if (Vector3.Distance(chunk.GetClosestPointOnSpline(vertexPosition).position, vertexPosition) > widthAroundRoad)
                    {
                        if (showDebugLines)
                            Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 10, Color.cyan, debugLineDuration);
                        continue;
                    }

                    AddVertex(vertexPosition, column, row);

                    if (firstValidVertexColumn == -1)
                        firstValidVertexColumn = vertexCount - 1;
                    lastValidVertexInColumn = vertexCount - 1;
                }
                
                //move the last vertices to the tangents to bring the terrain all the way to the spline edge:
                
                if (firstValidVertexColumn != -1)
                {
                    //check to move vertex on the first tangent
                    float firstTangentX = startVertexPosition.x + column * distanceBetweenVertices;
                    Vector3 pointOnFirstTangent = GetPointOnTangent(firstTangentX, firstTangentStart, firstTangentEnd);
                    float distance = Vector2.Distance(pointOnFirstTangent.FlattenAsVector2(), vertices[firstValidVertexColumn].FlattenAsVector2());
                    if (distance < distanceBetweenVertices)
                        vertices[firstValidVertexColumn] = pointOnFirstTangent;
                }
                
                if (lastValidVertexInColumn != -1)
                {
                    //check to move vertex on the last tangent
                    float lastTangentX = startVertexPosition.x + column * distanceBetweenVertices;
                    Vector3 pointOnLastTangent = GetPointOnTangent(lastTangentX, lastTangentStart, lastTangentEnd);
                    float distance = Vector2.Distance(pointOnLastTangent.FlattenAsVector2(), vertices[lastValidVertexInColumn].FlattenAsVector2());
                    if (distance < distanceBetweenVertices)
                        vertices[lastValidVertexInColumn] = pointOnLastTangent;
                }
            }

        }

        private void AddVertex(Vector3 vertexPosition, int column, int row)
        {
            bool containsIndex = column < verticesAsGrid.Count;
            if (!containsIndex)
                verticesAsGrid[column] = new List<int>();
            
            vertices.Add(vertexPosition);

            //add to [column] as [row]
            verticesAsGrid[column].Insert(row, vertexCount);

            vertexCount++;
            
            if (showDebugLines)
                Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 20, Color.blue, debugLineDuration);
        }
        
        private Vector3 GetPointOnTangent(float xPos, Vector3 tangentStartPoint, Vector3 tangentEndPoint)
        {
            Vector3 direction = (tangentEndPoint - tangentStartPoint).normalized;
            float distance = xPos - tangentStartPoint.x;
            float z = tangentStartPoint.z + distance * direction.z / direction.x;
            return new Vector3(xPos, 0, z);
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
                Vector3 vertex = Vertices[vertexIndex];
                Handles.Label(vertex, vertexIndex.ToString());
            }
        }
        
    }
}
