using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = System.Random;

namespace Gumball
{
    [Serializable]
    public class TerrainHeightData
    {
        
        [SerializeField] public int Seed = 100;

        [Tooltip("How many layers of perlin noise is combined? This can add more detail to the terrain.")]
        [MinValue(1)]
        public int LayersOfDetail = 3;

        [Tooltip("Controls the increase in frequency of octaves.")]
        [PositiveValueOnly] public float MountainFrequency = 1;

        [Tooltip("Controls the decrease in amplitude of octaves. Higher value = bigger mountains/")]
        [PositiveValueOnly] public float ElevationAmount = 3;

        [Tooltip("How much is the terrain raised above ground versus lowered below ground.")]
        [Range(-1, 1)] public float ElevationPercent = 0.5f;

        [Tooltip("An additional modifier to the elevation settings.")]
        public AnimationCurve ElevationModifier = AnimationCurve.Linear(0, 0, 1, 1);
        
        public float Scale = 100;

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
}
