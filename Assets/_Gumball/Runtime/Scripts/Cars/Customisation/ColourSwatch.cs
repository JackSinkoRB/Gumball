using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gumball
{
    [Serializable]
    public struct ColourSwatchSerialized : IEquatable<ColourSwatch>
    {
        [SerializeField] private SerializableColor color;
        [SerializeField] private SerializableColor specular;
        [SerializeField] private float smoothness;
        [SerializeField] private float clearCoat;
        [SerializeField] private float clearCoatSmoothness;

        public SerializableColor Color => color;
        public SerializableColor Specular => specular;
        public float Smoothness => smoothness;
        public float ClearCoat => clearCoat;
        public float ClearCoatSmoothness => clearCoatSmoothness;

        public ColourSwatchSerialized(ColourSwatch colourSwatch)
        {
            color = colourSwatch.Color.ToSerializableColor();
            specular = colourSwatch.Specular.ToSerializableColor();
            smoothness = colourSwatch.Smoothness;
            clearCoat = colourSwatch.ClearCoat;
            clearCoatSmoothness = colourSwatch.ClearCoatSmoothness;
        }

        public bool Equals(ColourSwatch other)
        {
            return color.ToColor().Equals(other.Color)
                   && specular.ToColor().Equals(other.Specular)
                   && smoothness.Equals(other.Smoothness)
                   && clearCoat.Equals(other.ClearCoat)
                   && clearCoatSmoothness.Equals(other.ClearCoatSmoothness);
        }
    }

    [Serializable]
    public class ColourSwatch
    {
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Color specular = Color.black;
        [SerializeField] private float smoothness = 0.5f;
        [SerializeField] private float clearCoat = 1;
        [SerializeField] private float clearCoatSmoothness = 1;
        
        public Color Color => color;
        public Color Specular => specular;
        public float Smoothness => smoothness;
        public float ClearCoat => clearCoat;
        public float ClearCoatSmoothness => clearCoatSmoothness;

        public void SetColor(Color color)
        {
            this.color = color;
        }
        
        public void SetSpecular(Color specular)
        {
            this.specular = specular;
        }

        public void SetSmoothness(float value)
        {
            smoothness = value;
        }
        
        public void SetClearcoat(float value)
        {
            clearCoat = value;
        }

        public ColourSwatchSerialized Serialize()
        {
            return new ColourSwatchSerialized(this);
        }
    }
}
