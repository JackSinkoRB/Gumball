using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class ChunkTerrainData
    {
        
        [Serializable]
        private class PerlinData
        {
            [SerializeField] public Vector2 MountainWidth = new(100,100);
            [SerializeField] public float MountainHeight = 20;
            [SerializeField] public Vector2 Seed = new(100,100);
        }
        
        [SerializeField] private float widthAroundRoad = 100;
        [SerializeField] private float resolution = 100;
        
        [SerializeField] private PerlinData heightData;

        public float WidthAroundRoad => widthAroundRoad;
        public float Resolution => resolution;
        
        private Chunk chunk;
        private ChunkGrid grid;

        public void Create(Chunk chunkToUse)
        {
            chunk = chunkToUse;
            UpdateGrid();
            GenerateTerrainMeshFromGrid();
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
            MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Diffuse")); //set a material with the default shader
            MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

            //apply height data
            List<Vector3> verticesWithHeightData = ApplyHeightDataToVertices();

            //setup the mesh
            mesh.SetVertices(verticesWithHeightData);
            mesh.SetTriangles(CreateTrianglesFromGrid(), 0);
            
            //apply the changes to the mesh
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;

            //editor things
            Undo.RegisterCreatedObjectUndo(terrain, "Create Terrain");
            Selection.SetActiveObjectWithContext(terrain, chunk);
            
            return terrain;
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
            if (heightData.MountainHeight.Approximately(0))
                return new List<Vector3>(grid.Vertices); //is flat, so don't apply height data

            List<Vector3> verticesWithHeightData = new List<Vector3>();

            foreach (Vector3 vertex in grid.Vertices)
            {
                float perlinX = vertex.x / heightData.MountainWidth.x + heightData.Seed.x;
                float perlinY = vertex.z / heightData.MountainWidth.y + heightData.Seed.y;

                float y = Mathf.PerlinNoise(perlinX, perlinY) * heightData.MountainHeight;
                verticesWithHeightData.Add(new Vector3(vertex.x, y, vertex.z));
            }

            return verticesWithHeightData;
        }
        
    }
}
