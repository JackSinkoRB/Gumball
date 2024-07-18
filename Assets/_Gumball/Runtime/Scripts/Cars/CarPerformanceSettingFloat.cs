using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettingFloat : CarPerformanceSetting
    {
        
        [Space]
        [SerializeField] private float min;
        [SerializeField] private float max;

        public CarPerformanceSettingFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float GetValue(CarPerformanceProfile profile)
        {
            if (min.Approximately(max))
                return min; //values can't be upgraded, no need to calculate

            float finalWeight = GetFinalWeight(profile);

            float finalValue = Mathf.Lerp(min, max, finalWeight);
            return finalValue;
        }
        
    }
}
