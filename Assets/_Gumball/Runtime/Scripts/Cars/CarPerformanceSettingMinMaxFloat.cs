using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettingMinMaxFloat : CarPerformanceSetting
    {
        
        [Space]
        [SerializeField] private MinMaxFloat min;
        [SerializeField] private MinMaxFloat max;

        public CarPerformanceSettingMinMaxFloat(MinMaxFloat min, MinMaxFloat max)
        {
            this.min = min;
            this.max = max;
        }

        public MinMaxFloat GetValue(CarPerformanceProfile profile)
        {
            if (min.Equals(max))
                return min; //values can't be upgraded, no need to calculate
            
            float finalWeight = GetFinalWeight(profile);

            MinMaxFloat finalValue = new(Mathf.Lerp(min.Min, max.Min, finalWeight), Mathf.Lerp(min.Max, max.Max, finalWeight));
            return finalValue;
        }
        
    }
}
