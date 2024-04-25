using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class FloatUtils
    {

        public static float NormaliseAngle(this float angle)
        {
            angle %= 360; //ensure angle is within [0, 360)
            if (angle < 0)
                angle += 360; //ensure angle is positive
            return angle;
        }
        
        public static bool Approximately(this float a, float b, float tolerance = 0.01f) {
            return Mathf.Abs(a - b) < tolerance;
        }
        
    }
}
