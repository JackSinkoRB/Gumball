using System;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct SerializedVector3
    {
        [SerializeField] private float x;
        [SerializeField] private float y;
        [SerializeField] private float z;

        public float X => x;
        public float Y => y;
        public float Z => z;

        public SerializedVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
