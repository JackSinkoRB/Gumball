using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettingFloatScalar : CarPerformanceSetting
    {
        
        [Space]
        [SerializeField] private float max;

        public CarPerformanceSettingFloatScalar(float max)
        {
            this.max = max;
        }

        public float GetValue(CarPerformanceProfile profile)
        {
            float finalWeight = GetFinalWeight(profile);

            float finalValue = Mathf.Lerp(0, max, finalWeight);
            return finalValue;
        }
        
    }
}
