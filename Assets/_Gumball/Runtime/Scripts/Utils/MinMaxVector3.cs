using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class MinMaxVector3
    {
        [SerializeField] private Vector3 min;
        [SerializeField] private Vector3 max;

        public Vector3 Min => min;
        public Vector3 Max => max;

        public MinMaxVector3(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Return a vector where all 3 axis values are clamped to the min and max values.
        /// </summary>
        public Vector3 Clamp(Vector3 vector)
        {
            float xClamped = Mathf.Clamp(vector.x, min.x, max.x);
            float yClamped = Mathf.Clamp(vector.y, min.y, max.y);
            float zClamped = Mathf.Clamp(vector.z, min.z, max.z);
            return new Vector3(xClamped, yClamped, zClamped);
        }
    }
}
