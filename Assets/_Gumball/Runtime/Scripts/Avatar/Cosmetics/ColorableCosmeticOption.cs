using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct ColorableCosmeticOption
    {
        [SerializeField] private bool isColorable;
        [SerializeField, ConditionalField(nameof(isColorable))] private int defaultColorIndex;
        [SerializeField, ConditionalField(nameof(isColorable))] private CollectionWrapperString colorMaterialProperties;
        [Tooltip("A list of strings where if a material name contains the string, it is not coloured.")]
        [SerializeField, ConditionalField(nameof(isColorable))] private CollectionWrapperString materialNamesToIgnore;
        [SerializeField, ConditionalField(nameof(isColorable))] private CollectionWrapperColor colors;
            
        public bool IsColorable => isColorable;
        public int DefaultColorIndex => defaultColorIndex;
        public string[] ColorMaterialProperties => colorMaterialProperties.Value;
        public string[] MaterialNamesToIgnore => materialNamesToIgnore.Value;
        public Color[] Colors => colors.Value;

        public bool CanIgnoreMaterial(Material material)
        {
            foreach (string materialNameToIgnore in MaterialNamesToIgnore)
            {
                if (material.name.Contains(materialNameToIgnore))
                    return true;
            }

            return false;
        }
    }
}
