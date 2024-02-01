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
        [PositiveValueOnly, SerializeField] private int resolutionLowLOD = 20;
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
        
        public ChunkGrid GridLowLOD { get; private set; }
        public ChunkGrid Grid { get; private set; }

        public Dictionary<Chunk.TerrainLOD, GameObject> Create(Chunk chunkToUse, Material[] materialsToUse = null)
        {
            chunk = chunkToUse;
            UpdateGrid();

            Dictionary<Chunk.TerrainLOD, GameObject> terrains = new()
            {
                [Chunk.TerrainLOD.LOW] = GenerateTerrainMeshFromGrid(Chunk.TerrainLOD.LOW, materialsToUse),
                [Chunk.TerrainLOD.HIGH] = GenerateTerrainMeshFromGrid(Chunk.TerrainLOD.HIGH, materialsToUse)
            };
            
            return terrains;
        }

        private void UpdateGrid()
        {
            GridLowLOD = new ChunkGrid(chunk, resolutionLowLOD, widthAroundRoad);
            Grid = new ChunkGrid(chunk, resolution, widthAroundRoad);
        }
        
        private GameObject GenerateTerrainMeshFromGrid(Chunk.TerrainLOD lod, Material[] materialsToAssign = null)
        {
            ChunkGrid grid = lod == Chunk.TerrainLOD.HIGH ? Grid : GridLowLOD;
            
            //create the gameobject
            GameObject terrain = new GameObject($"Terrain-{lod.ToString()}");
            terrain.transform.SetParent(chunk.transform);
            terrain.transform.position = grid.GridCenter;
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
            List<Vector3> verticesWithHeightData = ApplyHeightDataToVertices(grid);

            //offset the vertices so the origin is (0,0,0)
            for (int i = 0; i < verticesWithHeightData.Count; i++)
                verticesWithHeightData[i] -= grid.GridCenter;

            //setup the mesh
            mesh.SetVertices(verticesWithHeightData);
            mesh.SetTriangles(CreateTrianglesFromGrid(grid), 0);
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
            if (lod == Chunk.TerrainLOD.HIGH)
                terrain.AddComponent<MeshCollider>().sharedMesh = newMesh;
            
            return terrain;
        }

        private List<int> CreateTrianglesFromGrid(ChunkGrid grid)
        {
            List<int> triangleIndexes = new List<int>();
            
            //iterate over all the columns
            for (int column = 0; column < grid.GetNumberOfColumns(); column++)
            {
                for (int row = 0; row < grid.GetNumberOfRows(); row++)
                {
                    int vertexIndex = grid.GetVertexIndexAt(column, row);
                    if (vertexIndex == -1)
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
                }
            }

            return triangleIndexes;
        }

        private void CalculateMinMaxPerlinHeights(ChunkGrid grid)
        {
            perlinHeight.Max = Mathf.NegativeInfinity;
            perlinHeight.Min = Mathf.Infinity;
            
            foreach (Vector3 vertexPosition in grid.Vertices)
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
        
        private List<Vector3> ApplyHeightDataToVertices(ChunkGrid grid)
        {
            //need to calculate the perlin values FIRST, and then get the highest and lowest
            //then apply the modifier to the desiredHeights (only the ones that USE the perlin)
            CalculateMinMaxPerlinHeights(grid);
            
            List<Vector3> verticesWithHeightData = new List<Vector3>();
            
            ChunkObject[] chunkObjects = chunk.transform.GetComponentsInAllChildren<ChunkObject>().ToArray();
            //ensure chunk object flattening colliders are on chunk object layer before raycasting
            chunk.GetComponent<ChunkEditorTools>().EnsureChunkObjectsAreOnRaycastLayer();
            
            for (int vertexIndex = 0; vertexIndex < grid.Vertices.Count; vertexIndex++)
            {
                Vector3 vertex = grid.Vertices[vertexIndex];
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
            
            //check to match chunk object colliders
            float chunkObjectBlendHeight = GetHeightAtPositionWithChunkObjectBlending(vertexPosition.SetY(desiredHeight), chunkObjects);
            if (!chunkObjectBlendHeight.Approximately(desiredHeight))
            {
                return chunkObjectBlendHeight;
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
        
        /// <summary>
        /// Find the chunk objects that affect the height at the desired position to calculate the average of their offsets and apply it to the current position.
        /// </summary>
        /// <returns>A new height value that takes the chunk object blending into account.</returns>
        private float GetHeightAtPositionWithChunkObjectBlending(Vector3 currentPosition, ChunkObject[] chunkObjects)
        {
            if (chunkObjects.Length == 0)
                return currentPosition.y; //no chunk objects to blend with
            
            Dictionary<ChunkObject, float> desiredOffsets = new();
            float sumOfOffsets = 0;

            //raycast upwards from vertex point (minus 10,000 to start at bottom) to see if it's overlapping
            const int maxChunkObjectsPerPosition = 15;
            RaycastHit[] hits = new RaycastHit[maxChunkObjectsPerPosition];
            int numberOfHits = chunk.gameObject.scene.GetPhysicsScene().Raycast(currentPosition.OffsetY(10000), Vector3.down, hits, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.ChunkObject));
            for (int count = 0; count < numberOfHits; count++)
            {
                RaycastHit hit = hits[count];
                ChunkObject chunkObject = hit.transform.GetComponent<ChunkObject>();
                
                if (chunkObject == null || !chunkObject.CanFlattenTerrain)
                    continue;
                
                //TODO: draw ray upward from the current position
                
                float offset = hit.point.y - currentPosition.y;
                
                desiredOffsets[chunkObject] = offset;
                sumOfOffsets += offset;
            }

            //check to blend
            foreach (ChunkObject chunkObject in chunkObjects)
            {
                if (desiredOffsets.ContainsKey(chunkObject) || !chunkObject.CanFlattenTerrain || chunkObject.FlattenTerrainBlendDistance <= 0)
                    continue;
                
                float maxDistanceSqr = chunkObject.FlattenTerrainBlendDistance * chunkObject.FlattenTerrainBlendDistance;
                //get distance from collider to currentPosition
                var (closestPosition, distanceSqr) = chunkObject.ColliderToFlattenTo.ClosestVertex(currentPosition, true);
                
                bool isWithinBlendRadius = distanceSqr <= maxDistanceSqr;
                if (!isWithinBlendRadius)
                    continue;

                float blendPercent = distanceSqr / maxDistanceSqr;
                float blendPercentWithCurve = chunkObject.FlattenTerrainBlendCurve.Evaluate(blendPercent);
                float blendModifier = Mathf.Clamp01(blendPercentWithCurve); //furthest away = 0 : 1
                float offset = closestPosition.y - currentPosition.y;

                float offsetWithBlending = offset * blendModifier;
                
                desiredOffsets[chunkObject] = offsetWithBlending;
                sumOfOffsets += offsetWithBlending;
            }

            if (desiredOffsets.Count == 0)
                return currentPosition.y; //hasn't changed
            
            //calculate the average offset
            float offsetAverage = sumOfOffsets / desiredOffsets.Count;
            float newHeight = currentPosition.y + offsetAverage;
            
            return newHeight;
        }
        
        private static Material GetDefaultMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(defaultTerrainMaterialPath);
        }
        
    }
}
#endif