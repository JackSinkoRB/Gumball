using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct ColourSwatchSerialized : IEquatable<ColourSwatch>
    {
        [SerializeField] private SerializableColor color;
        [SerializeField] private float metallic;
        [SerializeField] private float smoothness;
        [SerializeField] private float clearCoat;
        [SerializeField] private float clearCoatSmoothness;
        [SerializeField] private SerializableColor emission;

        public SerializableColor Color => color;
        public float Metallic => metallic;
        public float Smoothness => smoothness;
        public float ClearCoat => clearCoat;
        public float ClearCoatSmoothness => clearCoatSmoothness;
        public SerializableColor Emission => emission;

        public ColourSwatchSerialized(ColourSwatch colourSwatch)
        {
            color = colourSwatch.Color.ToSerializableColor();
            metallic = colourSwatch.Metallic;
            smoothness = colourSwatch.Smoothness;
            clearCoat = colourSwatch.ClearCoat;
            clearCoatSmoothness = colourSwatch.ClearCoatSmoothness;
            emission = colourSwatch.Emission.ToSerializableColor();
        }

        public bool Equals(ColourSwatch other)
        {
            return color.ToColor().Equals(other.Color)
                   && metallic.Equals(other.Metallic)
                   && smoothness.Equals(other.Smoothness)
                   && clearCoat.Equals(other.ClearCoat)
                   && clearCoatSmoothness.Equals(other.ClearCoatSmoothness)
                   && emission.ToColor().Equals(other.Emission);
        }
    }

    [Serializable]
    public class ColourSwatch
    {
        [SerializeField] private Color color = Color.white;
        [SerializeField] private float metallic = 0.8f;
        [SerializeField] private float smoothness = 0.5f;
        [SerializeField] private float clearCoat = 1;
        [SerializeField] private float clearCoatSmoothness = 1;
        [SerializeField] private Color emission = Color.black;
        
        public Color Color => color;
        public float Metallic => metallic;
        public float Smoothness => smoothness;
        public float ClearCoat => clearCoat;
        public float ClearCoatSmoothness => clearCoatSmoothness;
        public Color Emission => emission;
        
        public ColourSwatchSerialized Serialize()
        {
            return new ColourSwatchSerialized(this);
        }
    }
}
