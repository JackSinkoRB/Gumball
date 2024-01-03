#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dreamteck.Splines;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class ChunkTerrainData
    {

        private const string defaultTerrainMaterialPath = "Assets/_Gumball/Runtime/Materials/DefaultTerrain.mat";
        
        [PositiveValueOnly, SerializeField] private int resolution = 100;
        [PositiveValueOnly, SerializeField] private float chunkBlendDistance = 50;

        [Header("Road")]
        [Tooltip("Should the terrain match the road's height? Or should it be above (eg. a highway overpass)?")]
        [SerializeField] private bool matchRoadHeight = true;
        [SerializeField, ConditionalField(nameof(matchRoadHeight), true)] private float terrainHeightFromRoad;
        [PositiveValueOnly, SerializeField] private float widthAroundRoad = 100;
        [PositiveValueOnly, SerializeField] private float roadFlattenDistance = 15;
        [PositiveValueOnly, SerializeField] private float roadBlendDistance = 20;
        [Tooltip("Let it sit under the road just enough so it doesn't clip.")]
        [PositiveValueOnly, SerializeField] private float amountToSitUnderRoad = 0.2f;
        
        [Header("Height")]
        [Tooltip("Should each side of the road have their own height data?")]
        [SerializeField] private bool splitHeightData;
        [SerializeField] private TerrainHeightData heightData;
        [ConditionalField(nameof(splitHeightData)), SerializeField] private TerrainHeightData heightDataOther;
        [Space(5)]
        [SerializeField] private TerrainTextureBlendSettings textureBlendSettings;

        [SerializeField, HideInInspector] private Chunk chunk;
        [SerializeField, HideInInspector] private MinMaxFloat perlinHeight;

        public float WidthAroundRoad => widthAroundRoad;
        public float RoadFlattenDistance => roadFlattenDistance;
        public bool MatchRoadHeight => matchRoadHeight;
        public int Resolution => resolution;
        public float ChunkBlendDistance => chunkBlendDistance;
        public TerrainTextureBlendSettings TextureBlendSettings => textureBlendSettings;
        
        public ChunkGrid Grid { get; private set; }

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
            terrain.layer = (int) LayersAndTags.Layer.Terrain;
            
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
            
            //set the vertex colours AFTER calculating normals
            Color[] vertexColors = textureBlendSettings.GetVertexColors(chunk, verticesWithHeightData, terrain.transform, mesh);
            mesh.SetColors(vertexColors);

            //save the mesh asset
            string chunkDirectory = $"{ChunkUtils.ChunkMeshAssetFolderPath}/{chunk.UniqueID}";
            string path = $"{chunkDirectory}/{meshFilter.gameObject.name}.asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null)
                AssetDatabase.DeleteAsset(path);
            if (!Directory.Exists(chunkDirectory))
                Directory.CreateDirectory(chunkDirectory);
            AssetDatabase.CreateAsset(mesh, path);
            Mesh newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            
            meshFilter.sharedMesh = newMesh;
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
            EditorUtility.SetDirty(meshFilter);
            AssetDatabase.SaveAssets();

            //add a collider
            terrain.AddComponent<MeshCollider>().sharedMesh = newMesh;
            
            return terrain;
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
            perlinHeight.Max = Mathf.NegativeInfinity;
            perlinHeight.Min = Mathf.Infinity;
            
            foreach (Vector3 vertexPosition in Grid.Vertices)
            {
                TerrainHeightData heightDataAtPos = GetHeightData(vertexPosition);
                float actualPerlinHeight = GetPerlinHeightForVertex(vertexPosition, heightDataAtPos);
                
                //check if it is highest or lowest
                if (actualPerlinHeight > perlinHeight.Max)
                    perlinHeight.Max = actualPerlinHeight;
                if (actualPerlinHeight < perlinHeight.Min)
                    perlinHeight.Min = actualPerlinHeight;
            }
        }
        
        private List<Vector3> ApplyHeightDataToVertices()
        {
            //need to calculate the perlin values FIRST, and then get the highest and lowest
            //then apply the modifier to the desiredHeights (only the ones that USE the perlin)
            CalculateMinMaxPerlinHeights();
            
            List<Vector3> verticesWithHeightData = new List<Vector3>();
            ChunkObject[] chunkObjects = chunk.transform.GetComponentsInAllChildren<ChunkObject>().ToArray();
            
            for (int vertexIndex = 0; vertexIndex < Grid.Vertices.Count; vertexIndex++)
            {
                Vector3 vertex = Grid.Vertices[vertexIndex];
                float desiredHeight = GetDesiredHeightAtPosition(vertex, chunkObjects);
                
                verticesWithHeightData.Add(new Vector3(vertex.x, desiredHeight, vertex.z));
            }

            return verticesWithHeightData;
        }

        /// <summary>
        /// Gets the specific height settings at the given position.
        /// </summary>
        private TerrainHeightData GetHeightData(Vector3 vertexPosition, SplineSample closestSplineSample)
        {
            if (!splitHeightData)
                return heightData;

            Vector3 rightTangent = closestSplineSample.right;
            Vector3 leftTangent = -closestSplineSample.right;
            
            float distanceToRightTangent = (vertexPosition - closestSplineSample.position + rightTangent).sqrMagnitude;
            float distanceToLeftTangent = (vertexPosition - closestSplineSample.position + leftTangent).sqrMagnitude;

            return distanceToRightTangent < distanceToLeftTangent ? heightData : heightDataOther;
        }

        private TerrainHeightData GetHeightData(Vector3 vertexPosition)
        {
            if (!splitHeightData)
                return heightData;
            
            var (closestSample, distanceToSplineSqr) = chunk.GetClosestSampleOnSpline(vertexPosition);

            return GetHeightData(vertexPosition, closestSample);
        }

        public float GetDesiredHeightAtPosition(Vector3 vertexPosition, ChunkObject[] chunkObjects)
        {
            float desiredHeight = vertexPosition.y;
            var (closestSample, distanceToSplineSqr) = chunk.GetClosestSampleOnSpline(vertexPosition);

            //check to flatten under road
            float roadFlattenDistanceSqr = roadFlattenDistance * roadFlattenDistance;
            bool canFlattenUnderRoad = distanceToSplineSqr < roadFlattenDistanceSqr;
            if (canFlattenUnderRoad)
            {
                if (matchRoadHeight)
                    return closestSample.position.y - amountToSitUnderRoad;

                return terrainHeightFromRoad - amountToSitUnderRoad;
            }
            
            //check to flatten under chunk objects
            foreach (ChunkObject chunkObject in chunkObjects)
            {
                if (!chunkObject.FlattenTerrain)
                    continue;

                //TODO: for multiple objects, the closest object takes priority
                
                float radiusSqr = chunkObject.FlattenTerrainRadius * chunkObject.FlattenTerrainRadius;
                Vector3 lowestPos = chunkObject.transform.position;
                float distanceToObjectSqr = (lowestPos.FlattenAsVector2() - vertexPosition.FlattenAsVector2()).sqrMagnitude;
                bool isWithinFlattenRadius = distanceToObjectSqr < radiusSqr;
                if (isWithinFlattenRadius)
                    return lowestPos.y;
            }
            
            //check to apply height data
            TerrainHeightData heightDataAtPos = GetHeightData(vertexPosition, closestSample);
            if (!heightDataAtPos.ElevationAmount.Approximately(0))
            {
                //use perlin:
                desiredHeight = GetPerlinHeightForVertex(vertexPosition, heightDataAtPos);

                //multiply by the modifier, depending on the height percent
                float difference = desiredHeight < 0 ? perlinHeight.Min : perlinHeight.Max;
                float heightPercent = desiredHeight / difference;
                desiredHeight *= heightDataAtPos.ElevationModifier.Evaluate(heightPercent);
            }

            //check to blend with the road
            float roadBlendDistanceSqr = roadBlendDistance * roadBlendDistance;
            bool canBlendWithRoad = roadBlendDistance > 0 && distanceToSplineSqr < (roadFlattenDistanceSqr + roadBlendDistanceSqr);
            if (canBlendWithRoad)
            {
                float blendPercent = Mathf.Clamp01((distanceToSplineSqr - roadFlattenDistanceSqr) / roadBlendDistanceSqr);
                float roadHeight = 0; //TODO - use vertex position at the road position instead
                float blendOffsetDifference = (roadHeight - desiredHeight) * (1-blendPercent);
                desiredHeight += blendOffsetDifference;
            }
            
            //check to blend with chunk objects 
            foreach (ChunkObject chunkObject in chunkObjects)
            {
                if (!chunkObject.FlattenTerrain)
                    continue;

                if (chunkObject.FlattenTerrainBlendRadius <= 0)
                    continue;
                
                float blendRadiusSqr = chunkObject.FlattenTerrainBlendRadius * chunkObject.FlattenTerrainBlendRadius;
                Vector3 lowestPos = chunkObject.transform.position;
                float distanceToObjectSqr = (lowestPos.FlattenAsVector2() - vertexPosition.FlattenAsVector2()).sqrMagnitude;
                bool isWithinBlendRadius = distanceToObjectSqr < blendRadiusSqr;
                if (!isWithinBlendRadius)
                    continue;
                
                //desired height offset = desiredHeightDifference * blendPercent
                float flattenedRadiusSqr = chunkObject.FlattenTerrainRadius * chunkObject.FlattenTerrainRadius;
                float blendPercent = Mathf.Clamp01((distanceToObjectSqr - flattenedRadiusSqr) / blendRadiusSqr);
                float flattenedObjectHeight = chunkObject.transform.position.y;
                float blendOffsetDifference = (flattenedObjectHeight - desiredHeight) * (1-blendPercent);
                desiredHeight += blendOffsetDifference;
            }

            if (matchRoadHeight)
            {
                //minus the height difference from road
                float heightDifferenceFromRoad = vertexPosition.y - closestSample.position.y;
                desiredHeight -= heightDifferenceFromRoad;
            }
            else
            {
                desiredHeight += terrainHeightFromRoad;
            }

            return desiredHeight;
        }

        private float GetPerlinHeightForVertex(Vector3 vertexPosition, TerrainHeightData heightDataAtPos)
        {
            float noiseHeight = 0;
            foreach (TerrainHeightData.Octave octave in heightDataAtPos.GetOctaves())
            {
                //use the vertex LOCAL position instead of global
                Vector3 localVertexPosition = chunk.transform.InverseTransformPoint(vertexPosition);
                
                float perlinX = localVertexPosition.x / heightDataAtPos.Scale.x * octave.Frequency + heightDataAtPos.GetRandomPerlinOffset().x + heightDataAtPos.Offset.x;
                float perlinY = localVertexPosition.z / heightDataAtPos.Scale.y * octave.Frequency + heightDataAtPos.GetRandomPerlinOffset().y + heightDataAtPos.Offset.y;
                
                float perlinValue = Mathf.PerlinNoise(perlinX, perlinY);
                
                //formula:
                //if ElevationPercent = 1, it should = perlinValue (between 0 and 1)
                //if ElevationPercent = -1, it should = -perlinValue (between 0 and -1)
                //if ElevationPercent = 0, it should = perlinValue * 2 - 1 (between -1 and 1)
                float elevationPerlinValue = (heightDataAtPos.ElevationPercent * perlinValue) + (1 - Mathf.Abs(heightDataAtPos.ElevationPercent)) * (perlinValue * 2 - 1);
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