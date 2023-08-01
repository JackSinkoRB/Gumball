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
        private static List<Vector3> terrainPoints = new();
        [SerializeField] private float terrainWidth = 50;

        [ButtonMethod]
        public void GenerateTerrain()
        {
            CalculateTerrainPoints();
        }

        private void CalculateTerrainPoints()
        {
            terrainPoints.Clear();
            
            SampleCollection sampleCollection = new SampleCollection();
            chunk.SplineComputer.GetSamples(sampleCollection);
            
            Debug.Log("Samples: " + sampleCollection.length);

            CalculateTerrainPoints(sampleCollection.samples, true);
            CalculateTerrainPoints(sampleCollection.samples, false);

            //CreateMeshFromTerrainPoints();
        }

        private void CreateMeshFromTerrainPoints()
        {
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.SetParent(transform);
            MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

            //TODO HERE! Calculate triangles

            mesh.SetVertices(terrainPoints);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
        }

        private void CalculateTerrainPoints(SplineSample[] samples, bool isRightSide)
        {
            //then do the left side:
            for (var count = 0; count < samples.Length; count++)
            {
                SplineSample sample = samples[count];
                Vector3 startPoint = sample.position.Flatten();
                Vector3 direction = isRightSide ? sample.right.Flatten() : -sample.right.Flatten();
                Vector3 endPoint = startPoint + (direction * terrainWidth);

                //ignore if it is intersecting with any of the other lines
                if (count > 0 //don't ever ignore the first one
                    && count != samples.Length - 1 //don't ever ignore if the last one
                    && IsIntersectingWithAnotherSample(samples, sample, isRightSide))
                    continue;

                terrainPoints.Add(endPoint);

                //debugging:
                const float duration = 10;
                Debug.DrawLine(startPoint, endPoint, Color.blue, duration);
            }
        }
        
        private bool IsIntersectingWithAnotherSample(SplineSample[] otherSamples, SplineSample sample, bool isRightSide)
        {
            Vector3 startPoint = sample.position.Flatten();
            Vector3 direction = isRightSide ? sample.right.Flatten() : -sample.right.Flatten();
            Vector3 endPoint = startPoint + (direction * terrainWidth);
            
            foreach (SplineSample otherSample in otherSamples)
            {
                Vector3 otherStartPoint = otherSample.position.Flatten();
                Vector3 otherDirection = isRightSide ? otherSample.right.Flatten() : -otherSample.right.Flatten();
                Vector3 otherEndPoint = otherStartPoint + (otherDirection * terrainWidth);
             
                if (startPoint.Approximately(otherStartPoint))
                    continue;
                
                //skip if the line is intersecting with any other lines
                if (VectorUtils.AreLinesIntersecting(startPoint, endPoint,
                        otherStartPoint, otherEndPoint))
                {
                    //debugging:
                    const float duration = 10;
                    Debug.DrawLine(startPoint, endPoint, Color.red, duration);
                    return true;
                }
            }

            return false;
        }

        #endregion
        
    }
}
