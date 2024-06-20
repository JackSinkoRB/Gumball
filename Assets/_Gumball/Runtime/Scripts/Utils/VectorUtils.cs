using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class VectorUtils
    {

        public static Vector3 Round(this Vector3 vector, int decimals)
        {
            return new Vector3((float)Math.Round(vector.x, decimals), (float)Math.Round(vector.y, decimals), (float)Math.Round(vector.z, decimals));
        }
        
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
        
        public static Vector2 FindIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
        {
            Vector2 intersection = Vector2.zero;

            // Calculate slopes of the lines
            float m1 = (line1End.y - line1Start.y) / (line1End.x - line1Start.x);
            float m2 = (line2End.y - line2Start.y) / (line2End.x - line2Start.x);

            // Check if lines are parallel
            if (Mathf.Approximately(m1, m2))
                return intersection;

            // Calculate y-intercepts
            float b1 = line1Start.y - m1 * line1Start.x;
            float b2 = line2Start.y - m2 * line2Start.x;

            // Calculate intersection point
            intersection.x = (b2 - b1) / (m1 - m2);
            intersection.y = m1 * intersection.x + b1;

            return intersection;
        }
        
        /// <summary>
        /// Multiplies each element in Vector3 v by the corresponding element of w.
        /// </summary>
        public static Vector3 Multiply(this Vector3 a, Vector3 b)
        {
            a.x *= b.x;
            a.y *= b.y;
            a.z *= b.z;

            return a;
        }

        public static Vector3 ClampValues(this Vector3 a, float min, float max)
        {
            float x = Mathf.Clamp(a.x, min, max);
            float y = Mathf.Clamp(a.y, min, max);
            float z = Mathf.Clamp(a.z, min, max);

            return new Vector3(x, y, z);
        }

    }
}
