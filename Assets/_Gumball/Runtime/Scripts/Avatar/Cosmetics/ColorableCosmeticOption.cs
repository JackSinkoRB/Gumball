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
        [SerializeField, InitializationField] private bool isColorable;
        [SerializeField, InitializationField, ConditionalField(nameof(isColorable))] private int defaultColorIndex;
        [SerializeField, InitializationField, ConditionalField(nameof(isColorable))] private CollectionWrapperString colorMaterialProperties;
        [SerializeField, ConditionalField(nameof(isColorable))] private CollectionWrapperColor colors;
            
        public bool IsColorable => isColorable;
        public int DefaultColorIndex => defaultColorIndex;
        public string[] ColorMaterialProperties => colorMaterialProperties.Value;
        public Color[] Colors => colors.Value;
    }
}
