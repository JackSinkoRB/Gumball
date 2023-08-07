using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;
using Random = System.Random;

namespace Gumball
{
    [Serializable]
    public class ChunkTerrainData
    {

        [PositiveValueOnly, SerializeField] private float widthAroundRoad = 100;
        [PositiveValueOnly, SerializeField] private float resolution = 100;
        [PositiveValueOnly, SerializeField] private float roadFlattenDistance = 15;
        [PositiveValueOnly, SerializeField] private float roadBlendDistance = 5;
        
        [SerializeField] private TerrainHeightData heightData;

        public float WidthAroundRoad => widthAroundRoad;
        public float Resolution => resolution;
        
        private Chunk chunk;
        private ChunkGrid grid;

        private float maxPerlinHeight;
        private float minPerlinHeight;
        
        public GameObject Create(Chunk chunkToUse, Material[] materialsToUse = null)
        {
            chunk = chunkToUse;
            UpdateGrid();
            return GenerateTerrainMeshFromGrid(materialsToUse);
        }

        private void UpdateGrid()
        {
            grid = new ChunkGrid(chunk, resolution, widthAroundRoad);
        }
        
        private GameObject GenerateTerrainMeshFromGrid(Material[] materialsToAssign = null)
        {
            //create the gameobject
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.SetParent(chunk.transform);
            terrain.transform.position = grid.GridCenter;
            terrain.tag = ChunkUtils.TerrainTag;
            
            //apply materials
            MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materialsToAssign.HasValidMaterials()
                ? materialsToAssign
                : new[] { new Material(Shader.Find("Diffuse")) }; //set a material with the default shader if none

            //apply mesh
            MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

            //apply height data
            List<Vector3> verticesWithHeightData = ApplyHeightDataToVertices();

            //offset the vertices so the origin is (0,0,0)
            for (int i = 0; i < verticesWithHeightData.Count; i++)
                verticesWithHeightData[i] -= grid.GridCenter;

            //setup the mesh
            mesh.SetVertices(verticesWithHeightData);
            mesh.SetTriangles(CreateTrianglesFromGrid(), 0);
            mesh.SetUVs(0, GetUVs());
            
            //apply the changes to the mesh
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;

            return terrain;
        }

        private Vector2[] GetUVs()
        {
            Vector2[] uvs = new Vector2[grid.Vertices.Count];

            int vertexIndex = 0;
            for (int column = 0; column < grid.GetNumberOfColumns(); column++)
            {
                for (int row = 0; row < grid.GetNumberOfRowsInColumn(column); row++)
                {
                    if (grid.GetVertexIndexAt(column, row) == -1)
                        continue;
                    
                    //use the grid position
                    float u = row;
                    float v = column;
                    uvs[vertexIndex] = new Vector2(u, v);

                    vertexIndex++;
                }
            }

            return uvs;
        }
        
        private List<int> CreateTrianglesFromGrid()
        {
            List<int> triangleIndexes = new List<int>();
            
            //iterate over all the columns
            int vertexIndex = 0;
            for (int column = 0; column < grid.GetNumberOfColumns(); column++)
            {
                for (int row = 0; row < grid.GetNumberOfRowsInColumn(column); row++)
                {
                    if (grid.GetVertexIndexAt(column, row) == -1)
                        continue;
                    
                    int vertexIndexAbove = grid.GetVertexAbove(column, row);
                    int vertexIndexBelow = grid.GetVertexBelow(column, row);
                    int vertexIndexOnRight = grid.GetVertexOnRight(column, row);
                    int vertexIndexOnLeft = grid.GetVertexOnLeft(column, row);

                    bool hasVertexAbove = vertexIndexAbove != -1;
                    bool hasVertexBelow = vertexIndexBelow != -1;
                    bool hasVertexOnRight = vertexIndexOnRight != -1;
                    bool hasVertexOnLeft = vertexIndexOnLeft != -1;

                    if (hasVertexAbove && hasVertexOnRight)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexAbove);
                        triangleIndexes.Add(vertexIndexOnRight);
                    }
                    
                    if (hasVertexAbove && hasVertexOnLeft)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexOnLeft);
                        triangleIndexes.Add(vertexIndexAbove);
                    }
                    
                    if (hasVertexBelow && hasVertexOnRight)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexOnRight);
                        triangleIndexes.Add(vertexIndexBelow);
                    }
                    
                    if (hasVertexBelow && hasVertexOnLeft)
                    {
                        //order matters
                        triangleIndexes.Add(vertexIndex);
                        triangleIndexes.Add(vertexIndexBelow);
                        triangleIndexes.Add(vertexIndexOnLeft);
                    }

                    vertexIndex++;
                }
            }

            return triangleIndexes;
        }

        private void CalculateMinMaxPerlinHeights()
        {
            maxPerlinHeight = Mathf.NegativeInfinity;
            minPerlinHeight = Mathf.Infinity;
            
            foreach (Vector3 vertex in grid.Vertices)
            {
                float perlinHeight = GetPerlinHeightForVertex(vertex);
                
                //TODO: do we care about vertices that don't use perlin?
                
                //check if it is highest or lowest
                if (perlinHeight > maxPerlinHeight)
                    maxPerlinHeight = perlinHeight;
                if (perlinHeight < minPerlinHeight)
                    minPerlinHeight = perlinHeight;
            }
        }
        
        private List<Vector3> ApplyHeightDataToVertices()
        {
            //need to calculate the perlin values FIRST, and then get the highest and lowest
            //then apply the modifier to the desiredHeights (only the ones that USE the perlin)
            CalculateMinMaxPerlinHeights();
            
            List<Vector3> verticesWithHeightData = new List<Vector3>();

            for (int i = 0; i < grid.Vertices.Count; i++)
            {
                Vector3 vertex = grid.Vertices[i];
                float desiredHeight = GetDesiredHeightForVertex(vertex);
                
                verticesWithHeightData.Add(new Vector3(vertex.x, desiredHeight, vertex.z));
            }

            return verticesWithHeightData;
        }

        private float GetDesiredHeightForVertex(Vector3 vertex)
        {
            float desiredHeight = vertex.y;
            SplineSample closestSplineSample = chunk.GetClosestPointOnSpline(vertex);

            //check to flatten under road
            bool canFlattenUnderRoad = Vector3.Distance(closestSplineSample.position.Flatten(), vertex.Flatten()) < distanceToFlattenAroundSpline;
            if (canFlattenUnderRoad)
                return closestSplineSample.position.y - 0.01f; //let it sit just under the road, so it doesn't clip
            
            //TODO: check to blend with road
            
            //TODO: check to blend with other chunks

            if (!heightData.ElevationAmount.Approximately(0))
            {
                //use perlin:
                desiredHeight = GetPerlinHeightForVertex(vertex);
                
                //multiply by the modifier, depending on the height percent
                float difference = desiredHeight < 0 ? minPerlinHeight : maxPerlinHeight;
                float heightPercent = desiredHeight / difference;
                desiredHeight *= heightData.ElevationModifier.Evaluate(heightPercent);
            }

            //minus the height difference from road
            float heightDifferenceFromRoad = vertex.y - closestSplineSample.position.y;
            desiredHeight -= heightDifferenceFromRoad;

            return desiredHeight;
        }

        private float GetPerlinHeightForVertex(Vector3 vertex)
        {
            float noiseHeight = 0;
            foreach (TerrainHeightData.Octave octave in heightData.GetOctaves())
            {
                float perlinX = vertex.x / heightData.Scale * octave.Frequency + heightData.GetRandomPerlinOffset().x;
                float perlinY = vertex.z / heightData.Scale * octave.Frequency + heightData.GetRandomPerlinOffset().y;
                
                float perlinValue = Mathf.PerlinNoise(perlinX, perlinY);
                
                //formula:
                //if ElevationPercent = 1, it should = perlinValue (between 0 and 1)
                //if ElevationPercent = -1, it should = -perlinValue (between 0 and -1)
                //if ElevationPercent = 0, it should = perlinValue * 2 - 1 (between -1 and 1)
                float elevationPerlinValue = (heightData.ElevationPercent * perlinValue) + (1 - Mathf.Abs(heightData.ElevationPercent)) * (perlinValue * 2 - 1);
                noiseHeight += elevationPerlinValue * octave.Amplitude;
            }

            return noiseHeight;
        }
        
    }
}
