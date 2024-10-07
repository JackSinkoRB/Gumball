using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class PaintMaterial
    {

        public enum Type
        {
            NONE,
            GLOSSY,
            MATTE,
            METALLIC
        }
        
        [SerializeField] private float smoothness = 0.5f;
        [SerializeField] private float clearCoat = 1f;
        [SerializeField] private float clearCoatSmoothness = 1f;
        
        public float Smoothness => smoothness;
        public float ClearCoat => clearCoat;
        public float ClearCoatSmoothness => clearCoatSmoothness;
        
    }
}
