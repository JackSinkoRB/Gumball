using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class VectorUtils
    {
        
        public static SerializedVector3 ToSerializedVector(this Vector3 vector)
        {
            return new SerializedVector3(vector.x, vector.y, vector.z);
        }

        public static Vector3 Flatten(this Vector3 vector3)
        {
            return new Vector3(vector3.x, 0, vector3.z);
        }

        public static Vector2 FlattenAsVector2(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }
        
        public static Vector3 Unflatten(this Vector2 vector2)
        {
            return new Vector3(vector2.x, 0, vector2.y);
        }

        public static bool IsFurtherInDirection(this Vector3 vector, Vector3 other, Vector3 direction)
        {
            float distanceA = Vector3.Dot(direction, vector);
            float distanceB = Vector3.Dot(direction, other);

            bool isFurtherInDirection = distanceA > distanceB;
            return isFurtherInDirection;
        }

    }
}
