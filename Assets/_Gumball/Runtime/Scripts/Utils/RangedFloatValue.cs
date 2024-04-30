using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct RangedFloatValue
    {
        
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float defaultValue;

        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float DefaultValue => defaultValue;

        public float Difference => maxValue - minValue;

        public float GetDefaultValueNormalized => NormalizeValue(DefaultValue);

        public RangedFloatValue(float minValue, float maxValue, float defaultValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.defaultValue = defaultValue;
        }
        
        /// <returns>Converts a value that is from 0 to 1 to the actual value.</returns>
        public float DenormalizeValue(float normalizedValue)
        {
            return minValue + Mathf.Clamp01(normalizedValue) * Difference;
        }

        /// <returns>A value from 0 to 1 representing where the value sits between the min and max.</returns>
        public float NormalizeValue(float value)
        {
            return Mathf.Clamp01((value - minValue) / Difference);
        }
        
    }
}
