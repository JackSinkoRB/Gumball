#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class TerrainTextureBlendSettings
    {
        
        [Header("Object surrounding texture")]
        [Tooltip("The distance around chunk objects to show the 'object surrrounding texture' at full strength.")]
        [SerializeField] private float objectSurroundingRadius = 5;
        [Tooltip("The distance that the texture is blended down.")]
        [SerializeField] private float objectSurroundingBlendRadius = 15;
        [SerializeField, Range(0, 1)] private float objectSurroundingMaxOpacity = 0.8f;

        [Header("Lighting")]
        [SerializeField] private float naturalLight;
        
        [Header("Slope texture")]
        [SerializeField] private MinMaxInt minMaxSlopeAngle = new(0, 50);
        [SerializeField, Range(0, 1)] private float slopeMaxOpacity = 0.8f;

        public Color[] GetVertexColors(Chunk chunk, Vector3[] vertexPositions, Transform terrainTransform, Mesh mesh)
        {
            //force update the physics system for the chunk object raycasts in case objects have moved
            Physics.SyncTransforms();
            
            PhysicsScene scene = chunk.gameObject.scene.GetPhysicsScene();
            
            //calculate the automatic vertex colours
            Color[] vertexColors = new Color[vertexPositions.Length];
            for (int i = 0; i < vertexPositions.Length; i++)
            {
                Vector3 vertexPosition = vertexPositions[i];
                Vector3 vertexPositionWorld = terrainTransform.TransformPoint(vertexPosition);

                //these are additional layers
                // - the total of all 3 should be between 0 and 1
                // - 1 meaning it completely overrides the base layer
                // - 0.5 means the base is half showing, and the other half is a combination of the blends
                float objectSurroundingWeight = GetObjectSurroundingWeight(chunk, vertexPositionWorld);
                float slopeWeight = GetSlopeWeight(i, mesh);

                float greenColor = slopeWeight;
                float blueColor = objectSurroundingWeight;
                float redColor = Mathf.Clamp01(1 - (greenColor + blueColor));
                
                Color finalColor = new Color(redColor, greenColor, blueColor, naturalLight);

                //apply chunk object colouring
                //raycast upwards from vertex point (add 10,000 to start at top) to see if it's overlapping
                const int maxChunkObjectsPerPosition = 15;
                RaycastHit[] hits = new RaycastHit[maxChunkObjectsPerPosition];
                int numberOfHits = scene.Raycast(vertexPositionWorld.OffsetY(10000), Vector3.down, hits, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.ChunkObject));
                for (int count = 0; count < numberOfHits; count++)
                {
                    RaycastHit hit = hits[count];
                    ChunkObject chunkObject = hit.transform.GetComponentInAllParents<ChunkObject>();
                    
                    if (chunkObject == null || !chunkObject.CanColourTerrain)
                        continue;

                    finalColor = chunkObject.ColorModifier.GetDesiredColor(finalColor);
                }
                
                vertexColors[i] = finalColor;
            }

            return vertexColors;
        }

        private float GetObjectSurroundingWeight(Chunk chunk, Vector3 vertexPositionWorld)
        {
            //surrounding objects are: any 'chunk object' in the chunk, plus the closest spline sample

            //for now, just do road
            //get the distance to the road
            if (!chunk.GetComponent<ChunkEditorTools>().TerrainData.MatchRoadHeight)
                return 0;
            
            var (closestSample, distanceToSplineSqr) = chunk.GetClosestSampleOnSpline(vertexPositionWorld);
            float objectSurroundingRadiusSqr = objectSurroundingRadius * objectSurroundingRadius;
            float objectSurroundingBlendRadiusSqr = objectSurroundingBlendRadius * objectSurroundingBlendRadius;
            float objectSurroundingWeight = 1 - Mathf.Clamp01((distanceToSplineSqr - objectSurroundingRadiusSqr) / objectSurroundingBlendRadiusSqr);
            objectSurroundingWeight *= objectSurroundingMaxOpacity;

            return objectSurroundingWeight;
        }
        
        private float GetSlopeWeight(int index, Mesh mesh)
        {
            Vector3 normal = mesh.normals[index];
            float angle = Vector3.Angle(normal, Vector3.up);
            float percent = (angle - minMaxSlopeAngle.Min) / (minMaxSlopeAngle.Max - minMaxSlopeAngle.Min);
            
            return Mathf.Clamp01(percent) * slopeMaxOpacity;
        }
        
    }
}
#endif
