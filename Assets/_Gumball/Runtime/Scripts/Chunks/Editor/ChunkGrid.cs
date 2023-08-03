using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dreamteck.Splines;
using UnityEngine;

namespace Gumball
{
    public class ChunkGrid
    {
        
        private const float debugLineDuration = 6;

        private readonly Chunk chunk;
        private readonly float resolution;
        private readonly float widthAroundRoad;
        private readonly List<List<int>> verticesAsGrid = new();
        private readonly List<Vector3> vertices = new();

        public ReadOnlyCollection<Vector3> Vertices => vertices.AsReadOnly();
        public Vector3 GridCenter { get; private set; }
        /// <summary>
        /// The width/height of the grid.
        /// </summary>
        public float GridLength { get; private set; }
            
        public ChunkGrid(Chunk chunk, float resolution, float widthAroundRoad, bool showDebugLines = false)
        {
            this.chunk = chunk;
            this.resolution = resolution;
            this.widthAroundRoad = widthAroundRoad;
            
            CreateGrid(showDebugLines);
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

        private void CreateGrid(bool showDebugLines = false)
        {
            //TODO: while generating the grid, the spline should be flattened
            GridCenter = chunk.GetCenterOfSpline();
            GridLength = chunk.SplineComputer.CalculateLength() + widthAroundRoad;
            float distanceBetweenVertices = GridLength / resolution;
            
            SampleCollection sampleCollection = new SampleCollection();
            chunk.SplineComputer.GetSamples(sampleCollection);
            
            SplineSample firstSample = sampleCollection.samples[0];
            Vector3 firstTangent = firstSample.right.Flatten();
            if (showDebugLines)
            {
                Debug.DrawLine(firstSample.position.Flatten(), firstSample.position.Flatten() + firstTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(firstSample.position.Flatten(), firstSample.position.Flatten() + -firstTangent * GridLength, Color.magenta, debugLineDuration);
            }

            SplineSample lastSample = sampleCollection.samples[sampleCollection.length-1];
            Vector3 lastTangent = lastSample.right.Flatten();
            if (showDebugLines)
            {
                Debug.DrawLine(lastSample.position.Flatten(), lastSample.position.Flatten() + lastTangent * GridLength, Color.magenta, debugLineDuration);
                Debug.DrawLine(lastSample.position.Flatten(), lastSample.position.Flatten() + -lastTangent * GridLength, Color.magenta, debugLineDuration);
            }

            int vertexCount = 0;
            vertices.Clear();
            verticesAsGrid.Clear();
            for (int column = 0; column <= resolution; column++)
            {
                verticesAsGrid.Add(new List<int>());
                for (int row = 0; row <= resolution; row++)
                {
                    //add empty to [column] as [row]
                    verticesAsGrid[column].Add(-1);
                    
                    //start at top left (center of spline - gridSize)
                    Vector3 startVertexPosition = GridCenter.Flatten() - new Vector3(GridLength/2f, 0, GridLength/2f); //TODO: determine lowest point instead of 0
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
                    
                    vertices.Add(vertexPosition);
                    //add to [column] as [row]
                    verticesAsGrid[column][row] = vertexCount;
                    
                    vertexCount++;

                    if (showDebugLines)
                        Debug.DrawLine(vertexPosition, vertexPosition + Vector3.up * 20, Color.blue, debugLineDuration);
                }
            }
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

    }
}
