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

        private const string defaultTerrainMaterialPath = "Assets/_Gumball/Runtime/Materials/DefaultTerrain.mat";

        [SerializeField] private bool matchRoadHeight = true;
        [PositiveValueOnly, SerializeField, ConditionalField(nameof(matchRoadHeight), true)] private float terrainHeight;
        [Space(5)]
        [PositiveValueOnly, SerializeField] private float widthAroundRoad = 100;
        [PositiveValueOnly, SerializeField] private int resolution = 100;
        [PositiveValueOnly, SerializeField] private float roadFlattenDistance = 15;
        [PositiveValueOnly, SerializeField] private float roadBlendDistance = 20;

        [Tooltip("Should each side of the road have their own height data?")]
        [SerializeField] private bool splitHeightData;
        [SerializeField] private TerrainHeightData heightData;
        [ConditionalField(nameof(splitHeightData)), SerializeField] private TerrainHeightData heightDataOther;

        public float WidthAroundRoad => widthAroundRoad;
        public int Resolution => resolution;
        public ChunkGrid Grid { get; private set; }
        
        private Chunk chunk;

        private float maxPerlinHeight;
        private float minPerlinHeight;
        
        public GameObject Create(Chunk chunkToUse, Chunk.QualityLevel quality, Material[] materialsToUse = null)
        {
            chunk = chunkToUse;
            UpdateGrid(quality);
            return GenerateTerrainMeshFromGrid(quality, materialsToUse);
        }

        private void UpdateGrid(Chunk.QualityLevel quality)
        {
            Grid = new ChunkGrid(chunk, GetResolutionForQuality(quality), widthAroundRoad);
        }

        private GameObject GenerateTerrainMeshFromGrid(Chunk.QualityLevel qualityLevel, Material[] materialsToAssign = null)
        {
            //create the gameobject
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.SetParent(chunk.TerrainLODGroup.transform);
            terrain.transform.position = Grid.GridCenter;
            terrain.tag = ChunkUtils.TerrainTag;
            terrain.layer = LayerMask.NameToLayer(ChunkUtils.TerrainLayer);
            
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
            string path = $"{ChunkUtils.TerrainMeshAssetFolderPath}/{ChunkUtils.TerrainMeshPrefix}{chunk.UniqueID}_{qualityLevel.ToString()}.asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null)
                AssetDatabase.DeleteAsset(path);
            
            AssetDatabase.CreateAsset(mesh, path);
            Mesh newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            
            meshFilter.sharedMesh = newMesh;
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
            EditorUtility.SetDirty(meshFilter);
            AssetDatabase.SaveAssets();

            ChunkUtils.CleanupUnusedMeshes(chunk);

            //add a collider
            terrain.AddComponent<MeshCollider>();
            
            return terrain;
        }

        private int GetResolutionForQuality(Chunk.QualityLevel quality)
        {
            return quality switch
            {
                Chunk.QualityLevel.HIGH => resolution,
                Chunk.QualityLevel.MEDIUM => resolution / 2,
                Chunk.QualityLevel.LOW => resolution / 4,
                _ => throw new ArgumentOutOfRangeException()
            };
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
            
            foreach (Vector3 vertexPosition in Grid.Vertices)
            {
                TerrainHeightData heightDataAtPos = GetHeightData(vertexPosition);
                float perlinHeight = GetPerlinHeightForVertex(vertexPosition, heightDataAtPos);
                
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
            
            var (closestSample, distanceToSpline) = chunk.GetClosestSampleOnSpline(vertexPosition, true);

            return GetHeightData(vertexPosition, closestSample);
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
                
                if (matchRoadHeight)
                    return closestSample.position.y - amountToSitUnderRoad;

                return terrainHeight - amountToSitUnderRoad;
            }
            
            //check to flatten under chunk objects
            foreach (ChunkObject chunkObject in chunk.transform.GetComponentsInAllChildren<ChunkObject>())
            {
                if (!chunkObject.FlattenTerrain)
                    continue;

                //TODO: for multiple objects, the closest object takes priority
                
                float radiusSqr = chunkObject.FlattenTerrainRadius * chunkObject.FlattenTerrainRadius;
                Vector3 lowestPos = chunkObject.GetLowestPosition();
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
                float difference = desiredHeight < 0 ? minPerlinHeight : maxPerlinHeight;
                float heightPercent = desiredHeight / difference;
                desiredHeight *= heightDataAtPos.ElevationModifier.Evaluate(heightPercent);
            }
            
            //check to blend with the road
            bool canBlendWithRoad = roadBlendDistance > 0 && distanceToSpline < (roadFlattenDistance + roadBlendDistance);
            if (canBlendWithRoad)
            {
                float blendPercent = Mathf.Clamp01((distanceToSpline - roadFlattenDistance) / roadBlendDistance);
                float roadHeight = 0; //TODO - use vertex position instead
                float blendOffsetDifference = (roadHeight - desiredHeight) * (1-blendPercent);
                desiredHeight += blendOffsetDifference;
            }
            
            //check to blend with chunk objects 
            foreach (ChunkObject chunkObject in chunk.transform.GetComponentsInAllChildren<ChunkObject>())
            {
                if (!chunkObject.FlattenTerrain)
                    continue;

                if (chunkObject.FlattenTerrainBlendRadius <= 0)
                    continue;
                
                float blendRadiusSqr = chunkObject.FlattenTerrainBlendRadius * chunkObject.FlattenTerrainBlendRadius;
                Vector3 lowestPos = chunkObject.GetLowestPosition();
                float distanceToObjectSqr = (lowestPos.FlattenAsVector2() - vertexPosition.FlattenAsVector2()).sqrMagnitude;
                bool isWithinBlendRadius = distanceToObjectSqr < blendRadiusSqr;
                if (!isWithinBlendRadius)
                    continue;
                
                //desired height offset = desiredHeightDifference * blendPercent
                float flattenedRadiusSqr = chunkObject.FlattenTerrainRadius * chunkObject.FlattenTerrainRadius;
                float blendPercent = Mathf.Clamp01((distanceToObjectSqr - flattenedRadiusSqr) / blendRadiusSqr);
                float flattenedObjectHeight = chunkObject.GetLowestPosition().y;
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
                desiredHeight += terrainHeight;
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
                
                float perlinX = localVertexPosition.x / heightDataAtPos.Scale * octave.Frequency + heightDataAtPos.GetRandomPerlinOffset().x;
                float perlinY = localVertexPosition.z / heightDataAtPos.Scale * octave.Frequency + heightDataAtPos.GetRandomPerlinOffset().y;
                
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