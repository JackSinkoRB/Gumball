using System;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class SerializedVector3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public SerializedVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
