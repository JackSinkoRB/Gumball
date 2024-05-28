using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class SpeedUtils
    {
        
        public static float FromKmhToMs(float speed)
        {
            return speed / 3.6f;
        }
        
        public static float FromMsToKmh(float speed)
        {
            return speed * 3.6f;
        }

        public static float FromKphToMph(float kmh)
        {
            return kmh * 0.621371f;
        }
        
    }
}
