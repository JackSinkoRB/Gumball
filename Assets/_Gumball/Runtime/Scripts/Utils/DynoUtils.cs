using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class DynoUtils
    {
        
        public static float CalculateHorsepower(float torqueFootPounds, float rpm)
        {
            const float conversion = 5252;
            return (torqueFootPounds * rpm) / conversion;
        }
        
        public static float ConvertNewtonMetresToFootPounds(float newtonMetres)
        {
            const float conversion = 0.7376f;
            return newtonMetres * conversion;
        }
        
        public static float ConvertFootPoundsToNewtonMetres(float footPounds)
        {
            const float conversion = 0.7376f;
            return footPounds / conversion;
        }
        
        public static float ConvertHorsepowerToKilowatts(float horsepower)
        {
            const float conversion = 0.745699872f;
            return horsepower * conversion;
        }
        
        public static float ConvertKilowattsToHorsepower(float kilowatts)
        {
            const float conversion = 0.745699872f;
            return kilowatts / conversion;
        }
        
    }
}
