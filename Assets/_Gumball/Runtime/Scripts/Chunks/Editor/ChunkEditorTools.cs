using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    public class ChunkEditorTools : MonoBehaviour
    {

        private Chunk chunk => GetComponent<Chunk>();

        #region Connect to a chunk
        [SerializeField] private Chunk chunkToConnectWith;
        
        /// <summary>
        /// Connect the chunk with the specified chunk.
        /// </summary>
        [ButtonMethod]
        public void Connect()
        {
            if (chunkToConnectWith == null)
                throw new NullReferenceException($"There is no '{nameof(chunkToConnectWith)}' value set in the inspector.");
            
            ChunkUtils.ConnectChunks(chunkToConnectWith, chunk, true);
        }
        #endregion

        #region Generate terrain
        private const float debugLineDuration = 5; 
        [SerializeField] private float terrainWidth = 100;
        [SerializeField] private float terrainResolution = 100;

        [ButtonMethod]
        public void GenerateTerrain()
        {
            CreateGrid();
        }

        private void CreateGrid()
        {
            Vector3 splineCenter = chunk.GetMiddleOfSpline();
            float gridSize = chunk.SplineComputer.CalculateLength();
            float cellSize = gridSize / terrainResolution;
            
            SampleCollection sampleCollection = new SampleCollection();
            chunk.SplineComputer.GetSamples(sampleCollection);
            
            SplineSample firstPoint = sampleCollection.samples[0];
            Vector3 firstTangent = firstPoint.right.Flatten();
            //debugging:
            Debug.DrawLine(firstPoint.position.Flatten(), firstPoint.position.Flatten() + firstTangent * gridSize, Color.magenta, debugLineDuration);
            Debug.DrawLine(firstPoint.position.Flatten(), firstPoint.position.Flatten() + -firstTangent * gridSize, Color.magenta, debugLineDuration);

            SplineSample lastPoint = sampleCollection.samples[sampleCollection.length-1];
            Vector3 lastTangent = lastPoint.right.Flatten();
            //debugging:
            Debug.DrawLine(lastPoint.position.Flatten(), lastPoint.position.Flatten() + lastTangent * gridSize, Color.magenta, debugLineDuration);
            Debug.DrawLine(lastPoint.position.Flatten(), lastPoint.position.Flatten() + -lastTangent * gridSize, Color.magenta, debugLineDuration);
            
            for (int x = 0; x <= terrainResolution; x++)
            {
                for (int z = 0; z <= terrainResolution; z++)
                {
                    //start at top left (center of spline - gridSize)
                    Vector3 topLeft = splineCenter.Flatten() - new Vector3(gridSize/2f, 0, gridSize/2f); //TODO: determine lowest point instead of 0
                    Vector3 position = topLeft + new Vector3(x * cellSize, 0, z * cellSize);

                    if (IsBelowTangent(firstPoint.position.Flatten() - firstTangent, firstPoint.position.Flatten() + firstTangent, position))
                    {
                        Debug.DrawLine(position, position + Vector3.up * 10, Color.red, debugLineDuration);
                        continue;
                    }
                    
                    if (IsAboveTangent(lastPoint.position.Flatten() - lastTangent, lastPoint.position.Flatten() + lastTangent, position))
                    {
                        Debug.DrawLine(position, position + Vector3.up * 10, Color.red, debugLineDuration);
                        continue;
                    }
                    
                    if (Vector3.Distance(chunk.GetClosestPointOnSpline(position).position, position) > terrainWidth)
                    {
                        Debug.DrawLine(position, position + Vector3.up * 10, Color.cyan, debugLineDuration);
                        continue;
                    }
                    
                    Debug.DrawLine(position, position + Vector3.up * 20, Color.blue, debugLineDuration);
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
        
        #endregion
        
    }
}
