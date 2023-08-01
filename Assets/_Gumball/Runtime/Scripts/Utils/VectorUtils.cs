using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class VectorUtils
    {

        public static Vector3 Flatten(this Vector3 vector3)
        {
            return new Vector3(vector3.x, 0, vector3.z);
        }
        
        public static bool AreLinesIntersecting(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
        {
            Vector3 dir1 = line1End - line1Start;
            Vector3 dir2 = line2End - line2Start;

            float denominator = dir1.x * dir2.z - dir1.z * dir2.x;

            // Check if the line segments are parallel (denominator == 0)
            if (Mathf.Approximately(denominator, 0f))
                return false;

            float numerator1 = (line2Start.x - line1Start.x) * dir2.z - (line2Start.z - line1Start.z) * dir2.x;
            float numerator2 = (line2Start.x - line1Start.x) * dir1.z - (line2Start.z - line1Start.z) * dir1.x;

            float t1 = numerator1 / denominator;
            float t2 = numerator2 / denominator;

            // Check if the intersection point is within the bounds of both line segments
            if (t1 >= 0f && t1 <= 1f && t2 >= 0f && t2 <= 1f)
                return true;

            return false;
        }
        
    }
}
