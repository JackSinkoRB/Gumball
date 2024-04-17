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
        
        private static readonly Color noiseColor = new(0, 1, 0, 0);
        private static readonly Color objectSurroundingColor = new(0, 0, 1, 0);
        private static readonly Color slopeColor = new(0, 0, 0, 1);
        
        [Header("Object surrounding texture")]
        [Tooltip("The distance around chunk objects to show the 'object surrrounding texture' at full strength.")]
        [SerializeField] private float objectSurroundingRadius = 5;
        [Tooltip("The distance that the texture is blended down.")]
        [SerializeField] private float objectSurroundingBlendRadius = 15;
        [SerializeField, Range(0, 1)] private float objectSurroundingMaxOpacity = 0.8f;

        [Header("Noise texture")]
        [SerializeField] private float noiseScale = 120;
        [SerializeField, Range(0, 1)] private float noiseMaxOpacity = 0.8f;

        [Header("Slope texture")]
        [SerializeField] private MinMaxInt minMaxSlopeAngle = new(0, 50);
        [SerializeField, Range(0, 1)] private float slopeMaxOpacity = 0.8f;

        public Color[] GetVertexColors(Chunk chunk, Vector3[] vertexPositions, Transform terrainTransform, Mesh mesh)
        {
            Color[] vertexColors = new Color[vertexPositions.Length];
            for (int i = 0; i < vertexPositions.Length; i++)
            {
                Vector3 vertexPosition = vertexPositions[i];
                Vector3 vertexPositionWorld = terrainTransform.TransformPoint(vertexPosition);

                //these are additional layers
                // - the total of all 3 should be between 0 and 1
                // - 1 meaning it completely overrides the base layer
                // - 0.5 means the base is half showing, and the other half is a combination of the blends
                float noiseWeight = GetNoiseWeight(vertexPositionWorld);
                float objectSurroundingWeight = GetObjectSurroundingWeight(chunk, vertexPositionWorld);
                float slopeWeight = GetSlopeWeight(i, mesh);

                Color finalColor = noiseWeight * noiseColor + objectSurroundingWeight * objectSurroundingColor +
                                   slopeWeight * slopeColor;

                vertexColors[i] = finalColor;
            }

            return vertexColors;
        }

        private float GetNoiseWeight(Vector3 vertexPositionWorld)
        {
            float perlinValue = Mathf.Clamp01(Mathf.PerlinNoise(vertexPositionWorld.x / noiseScale, vertexPositionWorld.z / noiseScale));
            return perlinValue * noiseMaxOpacity;
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
