using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class SpeedUtils
    {

        /// <summary>
        /// Convert km/h to m/s
        /// </summary>
        public static float FromKmh(float speed)
        {
            return speed / 3.6f;
        }
        
        /// <summary>
        /// Convert km/h to m/s
        /// </summary>
        public static float ToKmh(float speed)
        {
            return speed * 3.6f;
        }
        
    }
}
