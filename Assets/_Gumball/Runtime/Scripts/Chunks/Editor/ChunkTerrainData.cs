using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace Gumball
{
    [Serializable]
    public class ChunkTerrainData
    {
        
        [Serializable]
        private class PerlinData
        {
            [SerializeField] public int Seed = 100;

            [Tooltip("How many layers of perlin noise is combined? This can add more detail to the terrain.")]
            [SerializeField] public int LayersOfDetail = 3;
            
            [Tooltip("Controls the increase in frequency of octaves.")]
            [SerializeField] public float MountainFrequency = 1;
            
            [Tooltip("Controls the decrease in amplitude of octaves. Higher value = bigger mountains/")]
            [SerializeField] public float ElevationAmount = 3;

            [Tooltip("How much is the terrain elevating above ground versus below ground.")]
            [Range(-1,1), SerializeField] public float ElevationPercent = 0.5f;

            [SerializeField] public float Scale = 100;

            public bool IsFlat => ElevationAmount.Approximately(0);

            public Octave[] GetOctaves()
            {
                Octave[] octaves = new Octave[LayersOfDetail];
                for (int octave = 0; octave < LayersOfDetail; octave++)
                    octaves[octave] = GetOctave(octave);
                
                return octaves;
            }

            public Octave GetOctave(int index)
            {
                return new Octave(Mathf.Pow(MountainFrequency, index), Mathf.Pow(ElevationAmount, index));
            }
            
            public Vector2 GetRandomPerlinOffset()
            {
                const int maxPerlinValue = 100000; //any values above this seems to break the perlin function
                
                Random random = new Random(Seed);
                return new Vector2(
                    random.Next(-maxPerlinValue, maxPerlinValue), 
                    random.Next(-maxPerlinValue, maxPerlinValue));
            }
            
            public struct Octave
            {
                public readonly float Frequency;
                public readonly float Amplitude;
                
                public Octave(float frequency, float amplitude)
                {
                    Frequency = frequency;
                    Amplitude = amplitude;
                }
            }
        }
        
        [SerializeField] private float widthAroundRoad = 100;
        [SerializeField] private float resolution = 100;
        [SerializeField] private float distanceToFlattenAroundSpline = 10;
        
        [SerializeField] private PerlinData heightData;

        public float WidthAroundRoad => widthAroundRoad;
        public float Resolution => resolution;
        
        private Chunk chunk;
        private ChunkGrid grid;

        public GameObject Create(Chunk chunkToUse)
        {
            chunk = chunkToUse;
            UpdateGrid();
            return GenerateTerrainMeshFromGrid();
        }

        private void UpdateGrid()
        {
            grid = new ChunkGrid(chunk, resolution, widthAroundRoad);
        }
        
        private GameObject GenerateTerrainMeshFromGrid()
        {
            //create the mesh object
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.SetParent(chunk.transform);
            terrain.transform.position = grid.GridCenter;
            terrain.tag = ChunkUtils.TerrainTag;
            MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
            if (meshRenderer.sharedMaterial == null)
                meshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse")); //set a material with the default shader
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

        private List<Vector3> ApplyHeightDataToVertices()
        {
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

            //check to blend with road:
            bool canFlatten = Vector3.Distance(closestSplineSample.position.Flatten(), vertex.Flatten()) < distanceToFlattenAroundSpline;
            if (canFlatten)
                return closestSplineSample.position.y - 0.01f; //let it sit just under the road, so it doesn't clip
            
            //check to blend with other chunks:

            if (!heightData.ElevationAmount.Approximately(0))
                //use perlin:
                desiredHeight = GetDesiredHeightForVertexUsingHeightData(vertex);

            //minus the height difference from road
            float heightDifferenceFromRoad = vertex.y - closestSplineSample.position.y;
            desiredHeight -= heightDifferenceFromRoad;

            return desiredHeight;
        }

        private float GetDesiredHeightForVertexUsingHeightData(Vector3 vertex)
        {
            float combinedOctaves = 0;
            foreach (PerlinData.Octave octave in heightData.GetOctaves())
            {
                float perlinX = vertex.x / heightData.Scale * octave.Frequency + heightData.GetRandomPerlinOffset().x;
                float perlinY = vertex.z / heightData.Scale * octave.Frequency + heightData.GetRandomPerlinOffset().y;
                
                float perlinValue = Mathf.PerlinNoise(perlinX, perlinY);
                
                //formula:
                //if ElevationPercent = 1, it should = perlinValue (between 0 and 1)
                //if ElevationPercent = -1, it should = -perlinValue (between 0 and -1)
                //if ElevationPercent = 0, it should = perlinValue * 2 - 1 (between -1 and 1)
                float elevationPerlinValue = (heightData.ElevationPercent * perlinValue) + (1 - Mathf.Abs(heightData.ElevationPercent)) * (perlinValue * 2 - 1);
                combinedOctaves += elevationPerlinValue * octave.Amplitude;
            }

            return combinedOctaves;
        }
        
    }
}
