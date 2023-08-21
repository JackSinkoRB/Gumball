#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class ChunkTerrainData
    {

        private const string meshAssetFolderPath = "Assets/_Gumball/Runtime/Meshes/Terrains/";
        private const string chunkFolderPath = "Assets/_Gumball/Runtime/Prefabs/Chunks";
        private const string defaultTerrainMaterialPath = "Assets/_Gumball/Runtime/Materials/DefaultTerrain.mat";
        
        [PositiveValueOnly, SerializeField] private float widthAroundRoad = 100;
        [PositiveValueOnly, SerializeField] private int resolution = 100;
        [PositiveValueOnly, SerializeField] private float roadFlattenDistance = 15;
        [PositiveValueOnly, SerializeField] private float roadBlendDistance = 20;

        [SerializeField] private TerrainHeightData heightData;

        public float WidthAroundRoad => widthAroundRoad;
        public int Resolution => resolution;
        public ChunkGrid Grid { get; private set; }
        
        private Chunk chunk;

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
            Grid = new ChunkGrid(chunk, resolution, widthAroundRoad);
        }
        
        private GameObject GenerateTerrainMeshFromGrid(Material[] materialsToAssign = null)
        {
            //create the gameobject
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.SetParent(chunk.transform);
            terrain.transform.position = Grid.GridCenter;
            terrain.tag = ChunkUtils.TerrainTag;
            
            //apply materials
            MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materialsToAssign.HasValidMaterials()
                ? materialsToAssign
                : new[] { GetDefaultMaterial() }; //set a material with the default shader if none

            //apply mesh
            MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

            //apply height data
            List<Vector3> verticesWithHeightData = ApplyHeightDataToVertices();

            //offset the vertices so the origin is (0,0,0)
            for (int i = 0; i < verticesWithHeightData.Count; i++)
                verticesWithHeightData[i] -= Grid.GridCenter;

            //setup the mesh
            mesh.SetVertices(verticesWithHeightData);
            mesh.SetTriangles(CreateTrianglesFromGrid(), 0);
            mesh.SetUVs(0, ChunkUtils.GetTriplanarUVs(verticesWithHeightData, terrain.transform));
            
            //apply the changes to the mesh
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            //save the mesh asset
            AssetDatabase.CreateAsset(mesh, $"{meshAssetFolderPath}/ProceduralTerrain_{chunk.UniqueID}.asset");
            AssetDatabase.SaveAssets();
            meshFilter.sharedMesh = mesh;
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);

            CleanupUnusedMeshes();
            AssetDatabase.SaveAssets();


            return terrain;
        }

        private void CleanupUnusedMeshes()
        {
            //find all the used meshes
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { chunkFolderPath });
            HashSet<Mesh> usedMeshes = new HashSet<Mesh>();
            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                Chunk chunkPrefab = AssetDatabase.LoadAssetAtPath<Chunk>(prefabPath);
                if (chunkPrefab == null)
                    continue; 
                if (chunkPrefab.CurrentTerrain == null)
                    continue;
                
                MeshFilter meshFilter = chunkPrefab.CurrentTerrain.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh != null)
                    usedMeshes.Add(meshFilter.sharedMesh);
            }

            //find all the terrain meshes
            string[] meshGuids = AssetDatabase.FindAssets("t:Mesh", new[] { meshAssetFolderPath });
            List<Mesh> allMeshes = new List<Mesh>();
            foreach (string meshGuid in meshGuids)
            {
                string meshPath = AssetDatabase.GUIDToAssetPath(meshGuid);
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                allMeshes.Add(mesh);
            }

            //delete the assets that aren't used
            foreach (Mesh mesh in allMeshes)
            {
                if (mesh == chunk.CurrentTerrain.GetComponent<MeshFilter>().sharedMesh)
                    continue;
                
                if (!usedMeshes.Contains(mesh))
                {
                    string path = AssetDatabase.GetAssetPath(mesh);
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log($"Removed unused terrain mesh asset: {path}");
                }
            }
        }
        
        private List<int> CreateTrianglesFromGrid()
        {
            List<int> triangleIndexes = new List<int>();
            
            //iterate over all the columns
            for (int column = 0; column < Grid.GetNumberOfColumns(); column++)
            {
                for (int row = 0; row < Grid.GetNumberOfRows(); row++)
                {
                    int vertexIndex = Grid.GetVertexIndexAt(column, row);
                    if (vertexIndex == -1)
                        continue;
                    
                    int vertexIndexAbove = Grid.GetVertexAbove(column, row);
                    int vertexIndexBelow = Grid.GetVertexBelow(column, row);
                    int vertexIndexOnRight = Grid.GetVertexOnRight(column, row);
                    int vertexIndexOnLeft = Grid.GetVertexOnLeft(column, row);

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
                }
            }

            return triangleIndexes;
        }

        private void CalculateMinMaxPerlinHeights()
        {
            maxPerlinHeight = Mathf.NegativeInfinity;
            minPerlinHeight = Mathf.Infinity;
            
            foreach (Vector3 vertex in Grid.Vertices)
            {
                float perlinHeight = GetPerlinHeightForVertex(vertex);
                
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

            for (int vertexIndex = 0; vertexIndex < Grid.Vertices.Count; vertexIndex++)
            {
                Vector3 vertex = Grid.Vertices[vertexIndex];
                float desiredHeight = GetDesiredHeightForVertex(vertexIndex);
                
                verticesWithHeightData.Add(new Vector3(vertex.x, desiredHeight, vertex.z));
            }

            return verticesWithHeightData;
        }
        
        private float GetDesiredHeightForVertex(int vertexIndex)
        {
            Vector3 vertexPosition = Grid.Vertices[vertexIndex];

            float desiredHeight = vertexPosition.y;
            var (closestSample, distanceToSpline) = chunk.GetClosestSampleOnSpline(vertexPosition, true);

            //check to flatten under road
            bool canFlattenUnderRoad = distanceToSpline < roadFlattenDistance;
            if (canFlattenUnderRoad)
            {
                const float amountToSitUnderRoad = 0.5f; //let it sit just under the road, so it doesn't clip
                return closestSample.position.y - amountToSitUnderRoad;
            }

            //check to apply height data
            if (!heightData.ElevationAmount.Approximately(0))
            {
                //use perlin:
                desiredHeight = GetPerlinHeightForVertex(vertexPosition);
                
                //multiply by the modifier, depending on the height percent
                float difference = desiredHeight < 0 ? minPerlinHeight : maxPerlinHeight;
                float heightPercent = desiredHeight / difference;
                desiredHeight *= heightData.ElevationModifier.Evaluate(heightPercent);
            }
            
            //check to blend with the road
            bool canBlendWithRoad = roadBlendDistance > 0 && distanceToSpline < (roadFlattenDistance + roadBlendDistance);
            if (canBlendWithRoad)
            {
                float blendPercent = Mathf.Clamp01((distanceToSpline - roadFlattenDistance) / roadBlendDistance);
                float desiredHeightDifference = vertexPosition.y + desiredHeight;
                desiredHeight = vertexPosition.y + (desiredHeightDifference * blendPercent);
            }

            //minus the height difference from road
            float heightDifferenceFromRoad = vertexPosition.y - closestSample.position.y;
            desiredHeight -= heightDifferenceFromRoad;
            
            return desiredHeight;
        }

        private float GetPerlinHeightForVertex(Vector3 vertexPosition)
        {
            float noiseHeight = 0;
            foreach (TerrainHeightData.Octave octave in heightData.GetOctaves())
            {
                //use the vertex LOCAL position instead of global
                Vector3 localVertexPosition = chunk.transform.InverseTransformPoint(vertexPosition);
                
                float perlinX = localVertexPosition.x / heightData.Scale * octave.Frequency + heightData.GetRandomPerlinOffset().x;
                float perlinY = localVertexPosition.z / heightData.Scale * octave.Frequency + heightData.GetRandomPerlinOffset().y;
                
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
        
        private static Material GetDefaultMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(defaultTerrainMaterialPath);
        }
        
    }
}
#endif