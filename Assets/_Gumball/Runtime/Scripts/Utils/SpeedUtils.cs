using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class SpeedUtils
    {

        public const int MetresInAKm = 1000;
        
        public static float FromKmhToMs(float speed)
        {
            return speed / 3.6f;
        }
        
        public static float FromMsToKmh(float speed)
        {
            return speed * 3.6f;
        }

        public static float FromKmToMiles(float km)
        {
            return km * 0.621371f;
        }

        /// <summary>
        /// Converts metres into a user friendly string, taking into consideration the unit of speed setting. Eg. metres=1000 returns 1km (or 0.6 miles)
        /// </summary>
        public static string GetDistanceUserFriendly(float metres, int decimalPlaces = 1)
        {
            float km = metres / MetresInAKm;
            
            if (!UnitOfSpeedSetting.UseMiles)
            {
                if (metres < 1000)
                    return $"{metres}m";
                return $"{Math.Round(km, decimalPlaces)}km";
            }
            float miles = FromKmToMiles(km);
            return $"{Math.Round(miles, decimalPlaces)}mi";
        } 
        
    }
}
