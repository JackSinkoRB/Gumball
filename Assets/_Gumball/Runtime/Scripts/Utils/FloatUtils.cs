using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class FloatUtils
    {

        public static float NormaliseAngle(this float angle)
        {
            angle += 360;
            angle %= 360;
            return angle;
        }
        
        public static bool Approximately(this float a, float b, float tolerance = 0.01f) {
            return Mathf.Abs(a - b) < tolerance;
        }
        
    }
}
