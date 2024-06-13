using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class ChunkObjectColorModifier
    {

        [Tooltip("Assign a collider for the terrain to colour around.")]
        [SerializeField] private Collider colliderToColourAround;
        
        [Space(5)]
        [SerializeField] private bool useColor;
        [SerializeField, ConditionalField(nameof(useColor))] private Color color = new(1,0,0,0.5f);
        
        [Space(5)]
        [SerializeField] private bool useAlpha;
        [SerializeField, ConditionalField(nameof(useAlpha)), Range(0, 1)] private float alpha;
        
        private float colorStrength => color.a;
        
        public Collider ColliderToColourAround => colliderToColourAround;
        
        public Color GetDesiredColor(Color currentColor)
        {
            Color newColor = currentColor;
            if (useColor)
                //lerp from the current color to the new color but at the specified strength - keeping the alpha the same
                newColor = Color.Lerp(currentColor, new Color(color.r, color.g, color.g, currentColor.a), colorStrength);
            
            if (useAlpha)
                newColor = newColor.WithAlphaSetTo(alpha);

            return newColor;
        }
    }
}
