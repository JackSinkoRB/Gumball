using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [System.Serializable]
    public struct SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializableColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }
    }

    public static class SerializableColorExtensions
    {

        public static SerializableColor ToSerializableColor(this Color color)
        {
            return new SerializableColor(color);
        }
        
        public static Color[] ToColors(this SerializableColor[] serializableColors)
        {
            Color[] colors = new Color[serializableColors.Length];
            for (int index = 0; index < serializableColors.Length; index++)
            {
                SerializableColor serializableColor = serializableColors[index];
                colors[index] = serializableColor.ToColor();
            }

            return colors;
        }
        
        public static SerializableColor[] ToSerializableColors(this Color[] colors)
        {
            SerializableColor[] serializableColors = new SerializableColor[colors.Length];
            for (int index = 0; index < colors.Length; index++)
            {
                Color color = colors[index];
                serializableColors[index] = color.ToSerializableColor();
            }

            return serializableColors;
        } 
        
    }
}
