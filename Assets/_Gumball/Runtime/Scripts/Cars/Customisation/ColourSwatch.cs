using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gumball
{
    [Serializable]
    public struct ColourSwatchSerialized : IEquatable<ColourSwatch>
    {
        [SerializeField] private SerializableColor color;
        [SerializeField] private SerializableColor specular;
        [SerializeField] private PaintMaterial.Type type;
        [SerializeField] private float smoothness;
        [SerializeField] private float clearCoat;
        [SerializeField] private float clearCoatSmoothness;

        public SerializableColor Color => color;
        public SerializableColor Specular => specular;
        public float Smoothness => type == PaintMaterial.Type.NONE ? smoothness : GlobalPaintPresets.Instance.GetMaterialFromType(type).Smoothness;
        public float ClearCoat => type == PaintMaterial.Type.NONE ? clearCoat : GlobalPaintPresets.Instance.GetMaterialFromType(type).ClearCoat;
        public float ClearCoatSmoothness => type == PaintMaterial.Type.NONE ? clearCoatSmoothness : GlobalPaintPresets.Instance.GetMaterialFromType(type).ClearCoatSmoothness;

        public ColourSwatchSerialized(ColourSwatch colourSwatch)
        {
            color = colourSwatch.Color.ToSerializableColor();
            specular = colourSwatch.Specular.ToSerializableColor();
            type = colourSwatch.MaterialType;
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
        
        public void SetMaterialType(PaintMaterial.Type type)
        {
            this.type = type;
        }
    }

    [Serializable]
    public class ColourSwatch
    {
        
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Color specular = Color.black;
        [SerializeField] private PaintMaterial.Type type;
        [SerializeField, ConditionalField(nameof(type), true)] private float smoothness = 0.5f;
        [SerializeField, ConditionalField(nameof(type), true), Range(0,1)] private float clearCoat = 1;
        [SerializeField, ConditionalField(nameof(type), true), Range(0,1)] private float clearCoatSmoothness = 1;
        
        public Color Color => color;
        public Color Specular => specular;
        public PaintMaterial.Type MaterialType => type;
        public float Smoothness => type == PaintMaterial.Type.NONE ? smoothness : GlobalPaintPresets.Instance.GetMaterialFromType(type).Smoothness;
        public float ClearCoat =>  type == PaintMaterial.Type.NONE ? clearCoat : GlobalPaintPresets.Instance.GetMaterialFromType(type).ClearCoat;
        public float ClearCoatSmoothness =>  type == PaintMaterial.Type.NONE ? clearCoatSmoothness : GlobalPaintPresets.Instance.GetMaterialFromType(type).ClearCoatSmoothness;

        public void SetColor(Color color)
        {
            this.color = color;
        }
        
        public void SetSpecular(Color specular)
        {
            this.specular = specular;
        }

        public void SetMaterialType(PaintMaterial.Type type)
        {
            this.type = type;
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
